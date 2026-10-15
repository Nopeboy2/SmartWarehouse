using System.ComponentModel.DataAnnotations;

namespace SmartWarehouse.Web.ViewModels;

public sealed class CreateProductViewModel
{
    [Required, StringLength(64)] public string Sku { get; set; } = "";
    [Required, StringLength(200)] public string Name { get; set; } = "";
    [Required] public Guid CategoryId { get; set; }
    [Required, StringLength(16)] public string UnitOfMeasure { get; set; } = "PCS";
    [StringLength(500)] public string? Description { get; set; }
}

public sealed class StockOperationViewModel
{
    [Required, StringLength(64)] public string Sku { get; set; } = "";
    [Required, StringLength(40)] public string LocationCode { get; set; } = "A-01-01";
    [Range(1, 1000000)] public int Quantity { get; set; } = 1;
    [Required, StringLength(64)] public string ReferenceNumber { get; set; } = "";
    [Required, StringLength(128)] public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString("N");
}
public sealed class CreateRfidTagViewModel
{
    [Required, StringLength(64)] public string Epc { get; set; } = "";
    [Required, StringLength(64)] public string Sku { get; set; } = "";
}


public sealed class CreateCategoryViewModel
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(32)] public string Code { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(120)] public string Name { get; set; } = "";
}

public sealed class CreateLocationViewModel
{
    [System.ComponentModel.DataAnnotations.Required] public Guid ZoneId { get; set; }
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(40)] public string Code { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(120)] public string Name { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Range(1, 100000)] public int CapacityUnits { get; set; } = 100;
}
