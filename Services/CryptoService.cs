using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TroLySoCaNhan.Services
{
    public class CryptoService
    {
        // ====================================================
        // 1. SINH CẶP KHÓA PGP/RSA (Public & Private)
        // ====================================================
        public static (string PublicKey, string PrivateKey) GenerateKeyPair()
        {
            RsaKeyPairGenerator rsaKeyPairGenerator = new RsaKeyPairGenerator();
            rsaKeyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
            AsymmetricCipherKeyPair keyPair = rsaKeyPairGenerator.GenerateKeyPair();

            // Chuyển Public Key thành dạng chuỗi (String) để lưu lên SQL Server
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);
            string publicKeyStr = Convert.ToBase64String(publicKeyInfo.GetDerEncoded());

            // Chuyển Private Key thành dạng chuỗi
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
            string privateKeyStr = Convert.ToBase64String(privateKeyInfo.GetDerEncoded());

            return (publicKeyStr, privateKeyStr);
        }

        // ====================================================
        // 2. BẢO VỆ PRIVATE KEY BẰNG WINDOWS DPAPI
        // ====================================================
        /// <summary>
        /// Mã hóa Private Key bằng tài khoản Windows và lưu xuống Local Vault
        /// </summary>
        public static void SaveAndProtectPrivateKey(string userId, string privateKeyString)
        {
            byte[] secretBytes = Encoding.UTF8.GetBytes(privateKeyString);

            // Dùng DPAPI mã hóa (Chỉ máy tính này & user Windows này mới giải mã được)
            byte[] encryptedBytes = ProtectedData.Protect(secretBytes, null, DataProtectionScope.CurrentUser);

            string path = LocalVaultService.GetPrivateKeyPath(userId);
            File.WriteAllBytes(path, encryptedBytes);
        }

        /// <summary>
        /// Lấy và giải mã Private Key từ Local Vault lên RAM để chuẩn bị mở file
        /// </summary>
        public static string UnprotectPrivateKey(string userId)
        {
            string path = LocalVaultService.GetPrivateKeyPath(userId);
            if (!File.Exists(path)) throw new FileNotFoundException("Không tìm thấy Private Key trên máy tính này!");

            byte[] encryptedBytes = File.ReadAllBytes(path);

            // Giải mã bằng DPAPI
            byte[] secretBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(secretBytes);
        }

        // ====================================================
        // 3. CÁC HÀM AES SẼ DÙNG CHO GIAI ĐOẠN 2 (CHUẨN BỊ SẴN)
        // ===================================================
        /// <summary>
        /// Mã hóa khóa AES bằng Public Key của người dùng (Để lưu lên Database)
        /// </summary>
        public static string EncryptAesKey(byte[] aesKey, string publicKeyBase64)
        {
            byte[] keyBytes = Convert.FromBase64String(publicKeyBase64);
            var publicKey = Org.BouncyCastle.Security.PublicKeyFactory.CreateKey(keyBytes);
            var cipher = new Org.BouncyCastle.Crypto.Encodings.OaepEncoding(new Org.BouncyCastle.Crypto.Engines.RsaEngine());
            
            cipher.Init(true, publicKey);
            byte[] encrypted = cipher.ProcessBlock(aesKey, 0, aesKey.Length);
            return Convert.ToBase64String(encrypted);
        }

        // ====================================================
        // 4. MÃ HÓA FILE VẬT LÝ BẰNG AES-256
        // ====================================================
        
        /// <summary>
        /// Sinh ra khóa AES ngẫu nhiên 32 byte (256-bit)
        /// </summary>
        public static byte[] GenerateAesKey()
        {
            byte[] key = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return key;
        }

        /// <summary>
        /// Mã hóa file gốc thành file .enc
        /// </summary>
        public static async System.Threading.Tasks.Task EncryptFileAsync(string inputFilePath, string outputEncryptedPath, byte[] aesKey)
        {
            using Aes aes = Aes.Create();
            aes.Key = aesKey;
            aes.GenerateIV();

            using FileStream fsOut = new FileStream(outputEncryptedPath, FileMode.Create);
            // Lưu IV (16 byte) ở đầu file để sau này dùng giải mã
            await fsOut.WriteAsync(aes.IV, 0, aes.IV.Length);

            using CryptoStream cs = new CryptoStream(fsOut, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using FileStream fsIn = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);
            
            await fsIn.CopyToAsync(cs);
        }
        // ====================================================
        // 5. GIẢI MÃ KHÓA AES BẰNG PRIVATE KEY (PGP)
        // ====================================================
        public static byte[] DecryptAesKey(string encryptedAesKeyBase64, string privateKeyBase64)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedAesKeyBase64);
            byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
            var privateKey = Org.BouncyCastle.Security.PrivateKeyFactory.CreateKey(privateKeyBytes);

            var cipher = new Org.BouncyCastle.Crypto.Encodings.OaepEncoding(new Org.BouncyCastle.Crypto.Engines.RsaEngine());
            cipher.Init(false, privateKey);

            return cipher.ProcessBlock(encryptedBytes, 0, encryptedBytes.Length);
        }

        // ====================================================
        // 6. GIẢI MÃ FILE .ENC VỀ LẠI FILE GỐC
        // ====================================================
        public static async System.Threading.Tasks.Task DecryptFileAsync(string inputEncryptedPath, string outputDecryptedPath, byte[] aesKey)
        {
            using FileStream fsIn = new FileStream(inputEncryptedPath, FileMode.Open, FileAccess.Read);

            // Đọc IV (16 byte đầu tiên của file .enc) mà ta đã lưu lúc mã hóa
            byte[] iv = new byte[16];
            await fsIn.ReadAsync(iv, 0, iv.Length);

            using Aes aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = iv;

            using CryptoStream cs = new CryptoStream(fsIn, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using FileStream fsOut = new FileStream(outputDecryptedPath, FileMode.Create, FileAccess.Write);

            await cs.CopyToAsync(fsOut);
        }
    }
}