using System.Text;
using Microsoft.AspNetCore.Mvc;
using My_Project.Models;
using Newtonsoft.Json;

namespace My_Project.Controllers
{
    [CheckAccess]
    public class CategoryController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IHttpClientFactory httpClientFactory, ILogger<CategoryController> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7270/api/CategoryAPI/");
            _logger = logger;
        }

        #region Get all Categoty
        public async Task<IActionResult> GetAllCategory()
        {
            var response = await _client.GetAsync("All");
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<CategoryModel>>(json);
            return View(list);
        }
        #endregion

        #region Get Category Details
        public async Task<IActionResult> CategoryDetails(int id)
        {
            var response = await _client.GetAsync($"{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var category = JsonConvert.DeserializeObject<CategoryModel>(json);
                return View(category);
            }

            return NotFound("Not Found");
        }
        #endregion

        #region AddEdit
        public async Task<IActionResult> AddEdit(int? id)
        {
            CategoryModel model;

            if (id != null)
            {
                var response = await _client.GetAsync($"{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction("GetAllCategory");
                }

                var json = await response.Content.ReadAsStringAsync();
                model = JsonConvert.DeserializeObject<CategoryModel>(json) ?? new CategoryModel();
            }
            else
            {
                model = new CategoryModel();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(CategoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (model.CategoryId == 0)
                {
                    response = await _client.PostAsync("", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Create failed: {errorMsg}");
                        TempData["Error"] = "Failed to create Category.";

                        return View(model);
                    }

                    TempData["Success"] = "Category created successfully.";
                }
                else
                {
                    response = await _client.PutAsync($"{model.CategoryId}", content);
                    var createdUserResponse = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Update failed: {errorMsg}");
                        TempData["Error"] = "Failed to update category.";

                        return View(model);
                    }

                    TempData["Success"] = "Category updated successfully.";
                }

                return RedirectToAction("GetAllCategory");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving category with ID {model.CategoryId}");
                TempData["Error"] = "Unable to save category.";
            }
            return View(model);
        }
        #endregion

        #region Delete Category
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Category deleted successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Delete failed for Category {id}: {errorContent}");
                    TempData["Error"] = "Something went wrong while deleting the Category!!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while deleting Category {id}");
                TempData["Error"] = "Something went wrong while deleting the Category.";
            }

            return RedirectToAction("GetAllCategory");
        }
        #endregion
    }
}
