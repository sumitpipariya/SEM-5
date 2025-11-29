using My_Project.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace My_Project.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"];
        }

        #region User Authenticate
        public async Task<string?> AuthenticateUserAsync(string username, string password, string? role = null)
        {
            var requestData = new
            {
                FullName = username,
                Password = password,
                Role = role
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/api/UserAPI/login", content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return null;
        }
        #endregion

        #region User Registration
        public async Task<string?> RegisterUserAsync(UserModel user)
        {
            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // ✅ This matches the API above
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/UserAPI/register", content);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }


        #endregion

    }
}
