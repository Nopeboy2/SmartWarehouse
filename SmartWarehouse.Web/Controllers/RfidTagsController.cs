using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Application.Warehouse;
using SmartWarehouse.Web.ViewModels;

namespace SmartWarehouse.Web.Controllers;

[Authorize(Roles = "Admin")]
public sealed class RfidTagsController(IWarehouseOperations operations) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await operations.GetRfidTagsAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() => View(new CreateRfidTagViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRfidTagViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await operations.CreateRfidTagAsync(new CreateRfidTagRequest(model.Epc, model.Sku), cancellationToken);
            TempData["Success"] = "Thẻ EPC đã được gán cho sản phẩm.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }
}

