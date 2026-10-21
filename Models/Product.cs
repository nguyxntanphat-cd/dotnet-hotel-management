using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyRazorApp.Models;

public class Product //C1 vs C3
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int StockQuantity { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public string PriceText => $"{Price:N0} VNĐ";

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

// [Table("Products")]  // C2
// public class Product
// {
//     [Key]
//     public int ProductId { get; set; }

//     [Required]
//     [MaxLength(200)]
//     public string ProductName { get; set; } = string.Empty;

//     [Column(TypeName = "decimal(18, 2)")]
//     public decimal Price { get; set; }

//     public string? Description { get; set; }

//     public int? CategoryId { get; set; }
//     [ForeignKey("CategoryId")]
//     public Category? Category { get; set; }

//     [NotMapped]
//     public string PriceText => $"{Price:N0} VNĐ";
// }