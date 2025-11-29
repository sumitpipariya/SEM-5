using System.Text;
using Microsoft.AspNetCore.Mvc;
using My_Project.Models;
using Newtonsoft.Json;

namespace My_Project.Controllers
{
    [CheckAccess]
    public class BrandController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<BrandController> _logger;

        public BrandController(IHttpClientFactory httpClientFactory, ILogger<BrandController> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7270/api/BrandAPI/");
            _logger = logger;
        }

        #region Get all Brand
        public async Task<IActionResult> GetAllBrand()
        {
            var response = await _client.GetAsync("All");
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<BrandModel>>(json);
            return View(list);
        }
        #endregion

        #region Get Brand Details
        public async Task<IActionResult> BrandDetails(int id)
        {
            var response = await _client.GetAsync($"{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var brand = JsonConvert.DeserializeObject<BrandModel>(json);
                return View(brand);
            }

            TempData["Error"] = "Brand not found.";
            return RedirectToAction("GetAllBrand");
        }
        #endregion

        #region AddEdit
        public async Task<IActionResult> AddEdit(int? id)
        {
            BrandModel model;

            if (id != null)
            {
                var response = await _client.GetAsync($"{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Brand not found.";
                    return RedirectToAction("GetAllBrand");
                }

                var json = await response.Content.ReadAsStringAsync();
                model = JsonConvert.DeserializeObject<BrandModel>(json) ?? new BrandModel();
            }
            else
            {
                model = new BrandModel();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(BrandModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (model.BrandId == 0)
                {
                    response = await _client.PostAsync("", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Create failed: {errorMsg}");
                        TempData["Error"] = "Failed to create Brand.";
                        return RedirectToAction("GetAllBrand");
                    }

                    TempData["Success"] = "Brand created successfully.";
                }
                else
                {
                    response = await _client.PutAsync($"{model.BrandId}", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Update failed: {errorMsg}");
                        TempData["Error"] = "Failed to update brand.";
                        return RedirectToAction("GetAllBrand");
                    }

                    TempData["Success"] = "Brand updated successfully.";
                }

                return RedirectToAction("GetAllBrand");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving brand with ID {model.BrandId}");
                TempData["Error"] = "Unable to save brand.";
                return RedirectToAction("GetAllBrand");
            }
        }
        #endregion

        #region Delete Brand
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Brand deleted successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Delete failed for user {id}: {errorContent}");
                    TempData["Error"] = "Something went wrong while deleting the Brand!!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while deleting Brand {id}");
                TempData["Error"] = "Something went wrong while deleting the Brand.";
            }

            return RedirectToAction("GetAllBrand");
        }


        #endregion
    }
}
