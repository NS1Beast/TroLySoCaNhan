using System;
using System.IO;

namespace TroLySoCaNhan.Services
{
    public class LocalVaultService
    {
        // Hàm lấy đường dẫn file cấu hình nơi lưu thư mục tùy chỉnh
        private static string GetConfigFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configDir = Path.Combine(appDataPath, "TroLySoCaNhan");
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
            return Path.Combine(configDir, "vault_config.txt");
        }

        /// <summary>
        /// Lấy đường dẫn kho lưu trữ Local của người dùng. Nếu chưa có thì tự động tạo.
        /// </summary>
        public static string GetVaultPath(string userId)
        {
            string configPath = GetConfigFilePath();
            string basePath;

            // Nếu người dùng đã chọn nơi lưu trữ riêng, lấy đường dẫn đó
            if (File.Exists(configPath))
            {
                basePath = File.ReadAllText(configPath).Trim();
            }
            else // Nếu không, dùng mặc định trong AppData
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                basePath = Path.Combine(appDataPath, "TroLySoCaNhan", "Vault");
            }

            string vaultPath = Path.Combine(basePath, userId);

            // Tự động tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(vaultPath))
            {
                Directory.CreateDirectory(vaultPath);
                string manifestPath = Path.Combine(vaultPath, "manifest.json");
                File.WriteAllText(manifestPath, "[]");
            }

            return vaultPath;
        }

        /// <summary>
        /// Đổi thư mục lưu trữ Local sang đường dẫn mới
        /// </summary>
        public static void SetCustomVaultPath(string newBasePath)
        {
            File.WriteAllText(GetConfigFilePath(), newBasePath);
        }

        /// <summary>
        /// Lấy đường dẫn an toàn để lưu Private Key (Không cho ai đụng vào)
        /// </summary>
        public static string GetPrivateKeyPath(string userId)
        {
            string vaultPath = GetVaultPath(userId);
            return Path.Combine(vaultPath, "private.key.enc");
        }
    }
}