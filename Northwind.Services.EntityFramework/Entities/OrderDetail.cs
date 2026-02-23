namespace Northwind.Services.EntityFramework.Entities;

public class OrderDetail
{
    public long OrderID { get; set; }

    public Order Order { get; set; } = default!;

    public long ProductID { get; set; }

    public Product Product { get; set; } = default!;

    public double UnitPrice { get; set; }

    public long Quantity { get; set; }

    public double Discount { get; set; }
}

