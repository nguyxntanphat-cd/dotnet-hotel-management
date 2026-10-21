namespace MyRazorApp.Models;

public class Order //3
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}