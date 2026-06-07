using TroLySoCaNhan.MVVM;

namespace TroLySoCaNhan.Models
{
    /// <summary>
    /// Model Tài liệu — bind cho DataGrid danh sách file.
    /// </summary>
    public class TaiLieu : ViewModelBase
    {
        private string _id = string.Empty;
        private string _tenFile = string.Empty;
        private string _dinhDang = "pdf";
        private long _dungLuong;
        private string _danhMuc = string.Empty;
        private string _trangThai = "Đã sẵn sàng";
        private System.DateTime _ngayTao = System.DateTime.Now;
        private string _nguoiTao = string.Empty;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string TenFile { get => _tenFile; set => SetProperty(ref _tenFile, value); }
        public string DinhDang { get => _dinhDang; set => SetProperty(ref _dinhDang, value); }
        public long DungLuong { get => _dungLuong; set => SetProperty(ref _dungLuong, value); }
        public string DanhMuc { get => _danhMuc; set => SetProperty(ref _danhMuc, value); }
        public string TrangThai { get => _trangThai; set => SetProperty(ref _trangThai, value); }
        public System.DateTime NgayTao { get => _ngayTao; set => SetProperty(ref _ngayTao, value); }
        public string NguoiTao { get => _nguoiTao; set => SetProperty(ref _nguoiTao, value); }

        /// <summary>Hiển thị dung lượng dạng KB/MB/GB.</summary>
        public string DungLuongHienThi
        {
            get
            {
                if (_dungLuong >= 1_073_741_824L)
                    return $"{_dungLuong / 1_073_741_824.0:0.##} GB";
                if (_dungLuong >= 1_048_576L)
                    return $"{_dungLuong / 1_048_576.0:0.##} MB";
                if (_dungLuong >= 1024L)
                    return $"{_dungLuong / 1024.0:0.##} KB";
                return $"{_dungLuong} B";
            }
        }
    }
}
