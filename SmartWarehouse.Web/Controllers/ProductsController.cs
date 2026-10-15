using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartWarehouse.Application.Warehouse;
using SmartWarehouse.Web.ViewModels;

namespace SmartWarehouse.Web.Controllers;

[Authorize(Roles = "Admin")]
public sealed class ProductsController(IWarehouseOperations operations) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await operations.GetProductsAsync(cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadCategoriesAsync(cancellationToken);
        return View(new CreateProductViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(cancellationToken);
            return View(model);
        }
        try
        {
            await operations.CreateProductAsync(new CreateProductRequest(model.Sku, model.Name, model.CategoryId,
                model.UnitOfMeasure, model.Description), cancellationToken);
            TempData["Success"] = "Đã tạo sản phẩm.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadCategoriesAsync(cancellationToken);
            return View(model);
        }
    }

    private async Task LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = await operations.GetCategoriesAsync(cancellationToken);
        ViewBag.Categories = new SelectList(categories.Select(x => new { x.Id, Label = $"{x.Code} · {x.Name}" }), "Id", "Label");
    }
}

