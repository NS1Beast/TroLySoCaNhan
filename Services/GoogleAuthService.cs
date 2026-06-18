using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TroLySoCaNhan.Models
{
    public class GoogleAuthService
    {
        private const string ClientId = "clientID";
        private const string ClientSecret = "SlientSecret";
        private readonly string[] Scopes = { "openid", "email", "profile" };
        //private const string ClientId = "860492625504-p5iu36kaa0eq9uuv2g8p51ksr4ic78gl.apps.googleusercontent.com";
        //private const string ClientSecret = "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";
        //private readonly string[] Scopes = { "openid", "email", "profile" };

        /// <summary>
        /// Trả về (Email, Tên hiển thị) nếu đăng nhập thành công
        /// </summary>
        public async Task<(string Email, string Name)?> LoginAndGetUserInfoAsync()
        {
            try
            {
                // 1. Mở trình duyệt để đăng nhập Google
                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets { ClientId = ClientId, ClientSecret = ClientSecret },
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new NullDataStore() // Buộc chọn lại tài khoản mỗi lần bấm
                );

                // 2. Dùng AccessToken gọi API của Google để lấy Email và Tên
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credential.Token.AccessToken);

                var response = await client.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                if (!response.IsSuccessStatusCode) return null;

                string json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                string email = root.GetProperty("email").GetString() ?? "";
                string name = root.GetProperty("name").GetString() ?? "Người dùng Google";

                return (email, name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đăng nhập Google: {ex.Message}");
                return null;
            }
        }
    }
}