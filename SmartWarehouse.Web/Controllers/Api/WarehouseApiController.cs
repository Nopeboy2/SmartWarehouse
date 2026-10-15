using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Application.Warehouse;

namespace SmartWarehouse.Web.Controllers.Api;

[ApiController, Route("api/warehouse")]
public sealed class WarehouseApiController(IWarehouseOperations operations) : ControllerBase
{
    [HttpGet("dashboard")]
    [ProducesResponseType<DashboardSummary>(StatusCodes.Status200OK)]
    public Task<DashboardSummary> Dashboard(CancellationToken cancellationToken) => operations.GetDashboardAsync(cancellationToken);

    [HttpGet("products")]
    [ProducesResponseType<IReadOnlyList<ProductRow>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<ProductRow>> Products(CancellationToken cancellationToken) => operations.GetProductsAsync(cancellationToken);

    [HttpGet("inventory")]
    [ProducesResponseType<IReadOnlyList<InventoryRow>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<InventoryRow>> Inventory(CancellationToken cancellationToken) => operations.GetInventoryAsync(cancellationToken);

    [HttpPost("products")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken)
    {
        await operations.CreateProductAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("rfid/read")]
    public Task<RfidReadResult> ReadRfid([FromBody] RfidReadRequest request, CancellationToken cancellationToken) =>
        operations.ProcessRfidReadAsync(request.Epc, request.ReaderCode, request.ReadPoint, request.EventKey, cancellationToken);
    [HttpPost("inbound/receive")]
    public async Task<IActionResult> Receive(StockOperationRequest request, CancellationToken cancellationToken) =>
        Ok(new { processed = await operations.ReceiveStockAsync(request, cancellationToken) });

    [HttpPost("outbound/reserve")]
    public async Task<IActionResult> Reserve(StockOperationRequest request, CancellationToken cancellationToken) =>
        Ok(new { processed = await operations.ReserveStockAsync(request, cancellationToken) });

    [HttpPost("outbound/dispatch")]
    public async Task<IActionResult> Dispatch(StockOperationRequest request, CancellationToken cancellationToken) =>
        Ok(new { processed = await operations.DispatchStockAsync(request, cancellationToken) });
}

public sealed record RfidReadRequest(string Epc, string ReaderCode, string ReadPoint, string EventKey);
