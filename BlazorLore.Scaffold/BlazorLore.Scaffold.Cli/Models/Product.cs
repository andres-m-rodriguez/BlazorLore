using System.ComponentModel.DataAnnotations;

namespace BlazorLore.Scaffold.Cli.Models;

public record Product(
    [property: Required(ErrorMessage = "Product name is required")]
    [property: StringLength(100, ErrorMessage = "Product name cannot be longer than 100 characters")]
    string Name,
    
    [property: StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
    string? Description,
    
    [property: Required(ErrorMessage = "Price is required")]
    [property: Range(0.01, 10000.00, ErrorMessage = "Price must be between $0.01 and $10,000.00")]
    decimal Price,
    
    [property: Required(ErrorMessage = "SKU is required")]
    [property: RegularExpression(@"^[A-Z]{3}-\d{4}$", ErrorMessage = "SKU must be in format XXX-0000")]
    string SKU,
    
    [property: Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    int StockQuantity,
    
    [property: Required(ErrorMessage = "Category is required")]
    string Category,
    
    bool IsAvailable = true,
    
    [property: DataType(DataType.Date)]
    DateTime? ReleaseDate = null,
    
    [property: Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    int? Rating = null
)
{
    public int Id { get; init; }
    
    [Url(ErrorMessage = "Invalid image URL")]
    public string? ImageUrl { get; init; }
    
    [Range(0.0, 100.0, ErrorMessage = "Discount percentage must be between 0 and 100")]
    public decimal? DiscountPercentage { get; init; }
}