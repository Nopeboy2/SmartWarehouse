using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Application.Warehouse;
using SmartWarehouse.Web.ViewModels;

namespace SmartWarehouse.Web.Controllers;

[Authorize(Roles = "Admin")]
public sealed class CategoriesController(IWarehouseOperations operations) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await operations.GetCategoryRowsAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() => View(new CreateCategoryViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await operations.CreateCategoryAsync(new CreateCategoryRequest(model.Code, model.Name), cancellationToken);
            TempData["Success"] = "Danh mục đã được tạo.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }
}

