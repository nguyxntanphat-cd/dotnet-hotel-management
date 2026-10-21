using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyRazorApp.Models;

public class Category // 1 vs 3
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public ICollection<Product> Products { get; set; } = new List<Product>();

}

// [Table("Categories")]
// public class Category
// {
//     [Key]
//     public int CategoryId { get; set; }

//     [Required]
//     [MaxLength(100)]
//     public string CategoryName { get; set; } = string.Empty;

//     public ICollection<Product> Products { get; set; } = new List<Product>();
// }
