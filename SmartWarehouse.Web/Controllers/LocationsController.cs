using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartWarehouse.Application.Warehouse;
using SmartWarehouse.Web.ViewModels;

namespace SmartWarehouse.Web.Controllers;

[Authorize(Roles = "Admin")]
public sealed class LocationsController(IWarehouseOperations operations) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await operations.GetWarehouseLocationsAsync(cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadZones(cancellationToken);
        return View(new CreateLocationViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLocationViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) { await LoadZones(cancellationToken); return View(model); }
        try
        {
            await operations.CreateWarehouseLocationAsync(new CreateLocationRequest(model.ZoneId, model.Code, model.Name, model.CapacityUnits), cancellationToken);
            TempData["Success"] = "Vị trí kho đã được tạo.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadZones(cancellationToken);
            return View(model);
        }
    }

    private async Task LoadZones(CancellationToken cancellationToken)
    {
        var zones = await operations.GetWarehouseZonesAsync(cancellationToken);
        ViewBag.Zones = new SelectList(zones.Select(x => new { x.Id, Label = $"{x.WarehouseCode} · {x.Code} · {x.Name}" }), "Id", "Label");
    }
}

