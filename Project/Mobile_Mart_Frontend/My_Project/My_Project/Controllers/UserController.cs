using System;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using My_Project.Models;
using Newtonsoft.Json;

namespace My_Project.Controllers
{
    [CheckAccess]
    public class UserController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<UserController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(IHttpClientFactory httpClientFactory, ILogger<UserController> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] + "/api/UserAPI/");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Get all Users
        public async Task<IActionResult> GetAllUser()
        {
            try
            {
                var token = HttpContext.Session.GetString("JWTToken");

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Login");
                }

                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _client.GetAsync("User");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Login");
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<UserModel>>(json);

                return View(list);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load users. " + ex.Message;
                return View(new List<UserModel>());
            }
        }

        #endregion

        #region Delete User
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["Error"] = "Session expired. Please login again.";
                    return RedirectToAction("Login", "Login");
                }

                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                _client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // ✅ Correct API endpoint
                var response = await _client.DeleteAsync($"{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "User deleted successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Delete failed for user {id}: {errorContent}");
                    TempData["Error"] = "Something went wrong while deleting the user!!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while deleting user {id}");
                TempData["Error"] = "Something went wrong while deleting the user.";
            }

            return RedirectToAction("GetAllUser");
        }


        #endregion

        #region Get User Details
        public async Task<IActionResult> UserDetails(int id)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Login");
            }

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _client.GetAsync($"{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserModel>(json);
                return View(user);
            }

            return NotFound();
        }
        #endregion

        #region Add & Edit User
        public async Task<IActionResult> AddEdit(int? id)
        {
            try
            {
                UserModel user = new UserModel();

                if (id != null)
                {
                    var token = HttpContext.Session.GetString("JWTToken");
                    if (string.IsNullOrEmpty(token))
                    {
                        TempData["Error"] = "Session expired. Please login again.";
                        return RedirectToAction("Login", "Login");
                    }

                    _client.DefaultRequestHeaders.Clear();
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // ✅ GET user by ID (api/UserAPI/{id})
                    var response = await _client.GetAsync($"{id}");

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = "User not found.";
                        return RedirectToAction("GetAllUser");
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    user = JsonConvert.DeserializeObject<UserModel>(json);
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while loading form for User ID {id}.");
                TempData["Error"] = "Unable to load form.";
                return RedirectToAction("GetAllUser");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(UserModel user)
        {
            if (!ModelState.IsValid)
                return View(user);

            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["Error"] = "Session expired. Please login again.";
                    return RedirectToAction("Login", "Login");
                }

                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(
                    JsonConvert.SerializeObject(user),
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response;

                if (user.UserID == 0) // INSERT
                {
                    // ✅ Calls POST api/UserAPI
                    response = await _client.PostAsync("", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Insert failed: {errorMsg}");
                        TempData["Error"] = "Failed to add user.";
                        return View(user);
                    }
                    TempData["Success"] = "User created successfully.";
                }
                else // UPDATE
                {
                    // ✅ Calls PUT api/UserAPI/{id}
                    response = await _client.PutAsync($"{user.UserID}", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Update failed: {errorMsg}");
                        TempData["Error"] = "Failed to update user.";
                        return View(user);
                    }
                    TempData["Success"] = "User updated successfully.";
                }

                return RedirectToAction("GetAllUser");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving user with ID {user.UserID}");
                TempData["Error"] = "Unable to save user.";
                return View(user);
            }
        }
        #endregion



    }
}


