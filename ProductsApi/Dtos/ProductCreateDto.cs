using System.ComponentModel.DataAnnotations;

namespace ProductsApi.Dtos;

public class ProductCreateDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, 999999999)]
    public decimal Price { get; set; }
}
