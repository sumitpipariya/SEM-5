using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using My_Project.Models;   
using My_Project.Services; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_Project.Controllers
{
    public class LoginController : Controller
    {
        private readonly AuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginController(AuthService authService, IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var jsonData = await _authService.AuthenticateUserAsync(model.FullName, model.Password, model.Role);

            if (string.IsNullOrEmpty(jsonData))
            {
                ViewBag.Error = "Enter Valid Username or Password.";
                return View(model);
            }

            var data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonData);
            string token = data.ContainsKey("token") ? data["token"] : null;
            var userData = JsonConvert.DeserializeObject<LoginModel>(data["user"].ToString());

            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Invalid credentials.";
                return View(model);
            }

            // Save session values
            _httpContextAccessor.HttpContext.Session.SetString("JWTToken", token);
            _httpContextAccessor.HttpContext.Session.SetInt32("UserID", (int)userData.UserID);
            _httpContextAccessor.HttpContext.Session.SetString("FullName", (string)userData.FullName);
            _httpContextAccessor.HttpContext.Session.SetString("Role", (string)userData.Role);

            // ✅ Redirect based on role
            if (userData.Role == "Admin")
                return RedirectToAction("Index", "Home");
            else if (userData.Role == "User")
                return RedirectToAction("Index", "UserDashboard");

            // fallback
            return RedirectToAction("Login");
        }
        #endregion

        #region Registration

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var jsonData = await _authService.RegisterUserAsync(model);

            if (string.IsNullOrEmpty(jsonData))
            {
                ViewBag.Error = "Registration failed. Please try again.";
                return View(model);
            }

            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);

            bool isSuccess = data.ContainsKey("success") && (bool)data["success"];
            string message = data.ContainsKey("message") ? data["message"].ToString() : "Registration failed.";

            if (!isSuccess)
            {
                ViewBag.Error = message;
                return View(model);
            }

            TempData["Success"] = "Registration successful!";
            return RedirectToAction("Login");
        }

        #endregion

        #region Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Login");
        }
        #endregion



    }
}
