using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using TroLySoCaNhan.Models;
using TroLySoCaNhan.MVVM;
using Xceed.Words.NET;

namespace TroLySoCaNhan.ViewModels
{
    public class OcrViewModel : ViewModelBase
    {
        public UserDto CurrentUser { get; }

        public ObservableCollection<string> SelectedFiles { get; } = new ObservableCollection<string>();

        // Mảng chứa các dòng Log chạy trên UI Terminal
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();

        private string _saveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public string SaveDirectory { get => _saveDirectory; set => SetProperty(ref _saveDirectory, value); }

        private string _baseFileName = "KetQua_OCR";
        public string BaseFileName
        {
            get => _baseFileName;
            set { SetProperty(ref _baseFileName, value); StartOcrCommand?.RaiseCanExecuteChanged(); }
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (SetProperty(ref _isProcessing, value))
                {
                    SelectFilesCommand?.RaiseCanExecuteChanged();
                    RemoveFileCommand?.RaiseCanExecuteChanged();
                    ClearFilesCommand?.RaiseCanExecuteChanged();
                    SelectSaveDirCommand?.RaiseCanExecuteChanged();
                    StartOcrCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private double _progressValue;
        public double ProgressValue { get => _progressValue; set => SetProperty(ref _progressValue, value); }

        private string _progressText = string.Empty;
        public string ProgressText { get => _progressText; set => SetProperty(ref _progressText, value); }

        // ============================
        // COMMANDS
        // ============================
        public RelayCommand SelectFilesCommand { get; }
        public RelayCommand RemoveFileCommand { get; }
        public RelayCommand ClearFilesCommand { get; }
        public RelayCommand SelectSaveDirCommand { get; }
        public RelayCommand StartOcrCommand { get; }
        public RelayCommand ClearLogsCommand { get; }

        // MÃ API KEY CỦA GOOGLE GEMINI
        private readonly string GEMINI_API_KEY = "AQ.Ab8RN6LIGui00dPJ9p-UOgdKAHxX7sSPDpUD-jV6rOQs4yaDsA";

        public OcrViewModel(UserDto user)
        {
            CurrentUser = user;

            SelectFilesCommand = new RelayCommand(_ => DoSelectFiles(), _ => !IsProcessing);

            RemoveFileCommand = new RelayCommand(file =>
            {
                if (file is string path && SelectedFiles.Contains(path))
                {
                    SelectedFiles.Remove(path);
                    WriteLog($"- Đã gỡ file: {Path.GetFileName(path)}");
                    StartOcrCommand.RaiseCanExecuteChanged();
                }
            }, _ => !IsProcessing);

            ClearFilesCommand = new RelayCommand(_ =>
            {
                SelectedFiles.Clear();
                WriteLog("> Đã làm sạch danh sách chờ.");
                StartOcrCommand.RaiseCanExecuteChanged();
            }, _ => !IsProcessing && SelectedFiles.Count > 0);

            SelectSaveDirCommand = new RelayCommand(_ => DoSelectSaveDir(), _ => !IsProcessing);
            ClearLogsCommand = new RelayCommand(_ => LogMessages.Clear());

            StartOcrCommand = new RelayCommand(async _ => await DoOcrAsync(),
                _ => !IsProcessing && SelectedFiles.Count > 0 && !string.IsNullOrWhiteSpace(BaseFileName) && !string.IsNullOrWhiteSpace(SaveDirectory));

            WriteLog("> Hệ thống OCR AI Sẵn sàng. Chờ lệnh...");
        }

        private void WriteLog(string message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                LogMessages.Add($"[{time}] {message}");
            });
        }

        private void DoSelectFiles()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Thêm hình ảnh để quét AI",
                Filter = "Hình ảnh|*.jpg;*.jpeg;*.png;*.webp;*.heic|Tất cả|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                int count = 0;
                foreach (string file in openFileDialog.FileNames)
                {
                    if (!SelectedFiles.Contains(file))
                    {
                        SelectedFiles.Add(file);
                        count++;
                    }
                }
                WriteLog($"+ Đã thêm {count} file vào danh sách chờ.");
                StartOcrCommand.RaiseCanExecuteChanged();
            }
        }

        private void DoSelectSaveDir()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "Chọn thư mục lưu file Word" };
            if (dialog.ShowDialog() == true)
            {
                SaveDirectory = dialog.FolderName;
                WriteLog($"* Thư mục đích thay đổi: {SaveDirectory}");
            }
        }

        private async Task DoOcrAsync()
        {
            if (string.IsNullOrEmpty(GEMINI_API_KEY) || GEMINI_API_KEY.Contains("DÁN_API_KEY"))
            {
                WriteLog("! LỖI: Chưa cấu hình GEMINI_API_KEY.");
                MessageBox.Show("Vui lòng cấu hình GEMINI_API_KEY trong OcrViewModel.cs trước khi dùng.", "Lỗi cấu hình", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IsProcessing = true;
            ProgressValue = 0;
            WriteLog(">>> BẮT ĐẦU CHUỖI TÁC VỤ QUÉT (BATCH OCR) <<<");

            try
            {
                using var db = new TroLySoCaNhanContext();
                var userDb = db.NguoiDungs.FirstOrDefault(u => u.Id == CurrentUser.DbId);

                WriteLog("Đang xác thực thông tin tài khoản và số lượt AI...");
                if (userDb == null || userDb.LuotAisuDung < SelectedFiles.Count)
                {
                    WriteLog("! LỖI: Không đủ lượt AI khả dụng.");
                    MessageBox.Show($"Bạn chỉ còn {userDb?.LuotAisuDung ?? 0} lượt AI.\nKhông đủ để quét {SelectedFiles.Count} tài liệu. Vui lòng nạp VIP/Pro!", "Hết lượt sử dụng", MessageBoxButton.OK, MessageBoxImage.Warning);
                    IsProcessing = false;
                    return;
                }

                // KHỞI TẠO TÁC VỤ NỀN VÀO DATABASE
                Guid taskId = Guid.NewGuid();
                WriteLog($"Tạo Tác vụ nền (Job ID: {taskId.ToString().Substring(0, 8)})...");
                await db.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO TacVuNen (ID, MaNguoiDung, LoaiTacVu, NoiDungYeuCau, TongSoFile, SoFileHoanThanh, TiLeHoanThanh, TrangThai, NgayBatDau)
                    VALUES ({0}, {1}, 3, N'Trích xuất văn bản (OCR) bằng Gemini AI', {2}, 0, 0, 2, {3})",
                    taskId, CurrentUser.DbId, SelectedFiles.Count, DateTime.Now);

                using var httpClient = new HttpClient();
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={GEMINI_API_KEY}";
                int successCount = 0;

                for (int i = 0; i < SelectedFiles.Count; i++)
                {
                    string currentImage = SelectedFiles[i];
                    string shortImageName = Path.GetFileName(currentImage);

                    App.Current.Dispatcher.Invoke(() => { ProgressText = $"Đang xử lý: {shortImageName} ({i + 1}/{SelectedFiles.Count})"; });
                    WriteLog($"\n[File {i + 1}/{SelectedFiles.Count}] Bắt đầu phân tích: {shortImageName}");

                    // Đăng ký File gốc vào DB
                    Guid originalFileId = Guid.NewGuid();
                    Guid detailId = Guid.NewGuid();
                    await db.Database.ExecuteSqlRawAsync("INSERT INTO TaiLieu (ID, TenTaiLieu, MaChuSoHuu, DaXoa, NgayTao) VALUES ({0}, {1}, {2}, 0, {3})",
                        originalFileId, shortImageName, CurrentUser.DbId, DateTime.Now);

                    await db.Database.ExecuteSqlRawAsync(@"
                        INSERT INTO TacVuNen_ChiTiet (ID, MaTacVuNen, MaTaiLieuGoc, TrangThai, TienDoTungFile, NgayThucHien)
                        VALUES ({0}, {1}, {2}, 3, 0, {3})",
                        detailId, taskId, originalFileId, DateTime.Now);

                    try
                    {
                        WriteLog($"  ├ Đọc dữ liệu ảnh và nén Base64...");
                        byte[] imageBytes = await File.ReadAllBytesAsync(currentImage);
                        string base64Image = Convert.ToBase64String(imageBytes);
                        string mimeType = GetMimeType(currentImage);

                        var payload = new
                        {
                            contents = new[] { new { parts = new object[] {
                                new { text = "Hãy đóng vai chuyên gia nhập liệu. Trích xuất văn bản trong ảnh này, giữ nguyên bố cục. Chỉ trả về văn bản, không giải thích gì thêm." },
                                new { inline_data = new { mime_type = mimeType, data = base64Image } }
                            }}}
                        };

                        string jsonPayload = JsonSerializer.Serialize(payload);
                        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        WriteLog($"  ├ Đang gửi Packet đến Google Server...");
                        var response = await httpClient.PostAsync(url, content);

                        if (response.IsSuccessStatusCode)
                        {
                            WriteLog($"  ├ Nhận phản hồi thành công (HTTP 200). Đang bóc tách Text...");
                            string responseString = await response.Content.ReadAsStringAsync();
                            string extractedText = ParseGeminiResponse(responseString);

                            string suffix = SelectedFiles.Count > 1 ? $"_{i + 1}" : "";
                            string outFileName = $"{BaseFileName.Trim()}{suffix}.docx";
                            string outPath = Path.Combine(SaveDirectory, outFileName);

                            WriteLog($"  ├ Tạo file Word Output: {outFileName}");

                            await Task.Run(() =>
                            {
                                using (DocX document = DocX.Create(outPath))
                                {
                                    document.InsertParagraph(extractedText).FontSize(12);
                                    document.Save();
                                }
                            });

                            // Cập nhật Database
                            Guid resultFileId = Guid.NewGuid();
                            await db.Database.ExecuteSqlRawAsync("INSERT INTO TaiLieu (ID, TenTaiLieu, MaChuSoHuu, DaXoa, NgayTao) VALUES ({0}, {1}, {2}, 0, {3})",
                                resultFileId, outFileName, CurrentUser.DbId, DateTime.Now);

                            await db.Database.ExecuteSqlRawAsync(@"
                                UPDATE TacVuNen_ChiTiet 
                                SET TrangThai = 1, MaTaiLieuKetQua = {0}, TienDoTungFile = 100 
                                WHERE ID = {1}", resultFileId, detailId);

                            successCount++;
                            userDb.LuotAisuDung -= 1;
                            WriteLog($"  └ Hoàn tất. Đã trừ 1 lượt AI (Còn lại: {userDb.LuotAisuDung})");
                        }
                        else
                        {
                            string error = await response.Content.ReadAsStringAsync();
                            WriteLog($"  ! LỖI TỪ SERVER: {response.StatusCode} - {error}");
                            throw new Exception($"Lỗi từ Google: {response.StatusCode}");
                        }
                    }
                    catch (Exception loopEx)
                    {
                        WriteLog($"  ! LỖI FILE NÀY: {loopEx.Message}");
                        await db.Database.ExecuteSqlRawAsync(@"
                            UPDATE TacVuNen_ChiTiet 
                            SET TrangThai = 0, ChiTietLoi = {0} 
                            WHERE ID = {1}", loopEx.Message, detailId);
                    }

                    // CẬP NHẬT TIẾN ĐỘ JOB TỔNG (FIX: Dùng i + 1 thay vì successCount để đúng logic DB)
                    byte percent = (byte)((i + 1) * 100 / SelectedFiles.Count);
                    App.Current.Dispatcher.Invoke(() => { ProgressValue = percent; });

                    await db.Database.ExecuteSqlRawAsync(@"
                        UPDATE TacVuNen 
                        SET SoFileHoanThanh = {0}, TiLeHoanThanh = {1} 
                        WHERE ID = {2}", i + 1, percent, taskId);

                    await db.SaveChangesAsync(); // Commit trừ lượt AI

                    // [MỚI] TRÁNH BỊ GOOGLE CHẶN VÌ QUÁ TẢI (RATE LIMIT FREE TIER)
                    if (i < SelectedFiles.Count - 1)
                    {
                        WriteLog($"  ├ Đang dừng 2s để làm mát luồng và tránh Spam API...");
                        await Task.Delay(2000);
                    }
                } // Kết thúc vòng lặp

                // KẾT THÚC JOB
                byte finalStatus = successCount == SelectedFiles.Count ? (byte)1 : (successCount > 0 ? (byte)3 : (byte)0);
                await db.Database.ExecuteSqlRawAsync(@"
                    UPDATE TacVuNen 
                    SET TrangThai = {0}, NgayKetThuc = {1} 
                    WHERE ID = {2}", finalStatus, DateTime.Now, taskId);

                App.Current.Dispatcher.Invoke(() => { ProgressText = "Hoàn tất 100%!"; });
                WriteLog($"\n>>> KẾT THÚC: Thành công {successCount}/{SelectedFiles.Count} tác vụ <<<");

                MessageBox.Show($"Gemini AI đã trích xuất hoàn tất!\n- Thành công: {successCount}/{SelectedFiles.Count} file.\n- Lượt AI còn lại: {userDb.LuotAisuDung}\n\n*Chi tiết đã được lưu vào Nhật ký Hệ thống (TacVuNen).", "Xong", MessageBoxButton.OK, MessageBoxImage.Information);

                App.Current.Dispatcher.Invoke(() => { SelectedFiles.Clear(); });
            }
            catch (Exception ex)
            {
                WriteLog($"[FATAL ERROR] {ex.Message}");
                MessageBox.Show("Lỗi nghiêm trọng làm sập tiến trình: " + ex.Message, "Lỗi OCR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private string GetMimeType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".heic" => "image/heic",
                ".bmp" => "image/bmp",
                _ => "image/jpeg"
            };
        }

        private string ParseGeminiResponse(string jsonResponse)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                var textElement = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text");
                return textElement.GetString() ?? "";
            }
            catch (Exception ex)
            {
                WriteLog($"Lỗi Parse JSON: {ex.Message}");
                return "[Không thể bóc tách văn bản từ JSON của Google]";
            }
        }
    }
}