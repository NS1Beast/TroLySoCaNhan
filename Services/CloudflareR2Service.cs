using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;

namespace TroLySoCaNhan.Services
{
    public class CloudflareR2Service
    {
        // THÔNG TIN KẾT NỐI CLOUDFLARE R2 CỦA BẠN
        private const string AccessKey = "8fd44418cd191cd0c46e7e9618aff2fc";
        private const string SecretKey = "913a770d28171472f8f8ba605af9da684ea2e64873438d2112426bb327891e67";
        private const string ServiceUrl = "https://959bc83b08a72a9ba0460da85ed47ac8.r2.cloudflarestorage.com";
        private const string BucketName = "trolysocanhan";

        // Hàm hỗ trợ khởi tạo kết nối S3 tương thích với Cloudflare R2
        private static AmazonS3Client GetS3Client()
        {
            var config = new AmazonS3Config
            {
                ServiceURL = ServiceUrl,
                ForcePathStyle = true,
                AuthenticationRegion = "auto"
            };
            return new AmazonS3Client(AccessKey, SecretKey, config);
        }
        public static async Task DeleteFileAsync(string objectKey)
        {
            using var client = GetS3Client();
            var request = new DeleteObjectRequest
            {
                BucketName = BucketName,
                Key = objectKey
            };

            // Gửi lệnh Xóa vĩnh viễn Object khỏi Bucket
            await client.DeleteObjectAsync(request);
        }

        // ====================================================
        // HÀM UPLOAD FILE LÊN R2 (Sử dụng PutObjectRequest)
        // ====================================================
        public static async Task<string> UploadFileAsync(string filePath, string objectKey)
        {
            using var client = GetS3Client();

            // Dùng PutObjectRequest thay vì TransferUtility để kiểm soát hoàn toàn giao thức
            var putRequest = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = objectKey,
                FilePath = filePath,

                // ĐÂY LÀ "CHÌA KHÓA" ĐỂ KHẮC PHỤC TRIỆT ĐỂ LỖI STREAMING CỦA CLOUDFLARE R2
                DisablePayloadSigning = true
            };

            // Bắn file .enc từ Local Vault lên Cloudflare R2
            await client.PutObjectAsync(putRequest);

            return objectKey;
        }

        // ====================================================
        // HÀM DOWNLOAD FILE TỪ R2
        // ====================================================
        public static async Task DownloadFileAsync(string objectKey, string downloadPath)
        {
            using var client = GetS3Client();

            // Yêu cầu lấy file từ Bucket bằng ObjectKey
            var request = new GetObjectRequest
            {
                BucketName = BucketName,
                Key = objectKey
            };

            // Lấy response stream từ Cloudflare
            using var response = await client.GetObjectAsync(request);

            // Lưu stream tải về thành file vật lý (.enc) tại thư mục Local Vault
            await response.WriteResponseStreamToFileAsync(downloadPath, false, System.Threading.CancellationToken.None);
        }
    }
}