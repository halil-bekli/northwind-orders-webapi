using System.Collections.Generic;

namespace Northwind.Services.EntityFramework.Entities;

public class Product
{
    public long ProductID { get; set; }

    public string ProductName { get; set; } = default!;

    public long SupplierID { get; set; }

    public Supplier Supplier { get; set; } = default!;

    public long CategoryID { get; set; }

    public Category Category { get; set; } = default!;

    public string? QuantityPerUnit { get; set; }

    public double UnitPrice { get; set; }

    public int UnitsInStock { get; set; }

    public int UnitsOnOrder { get; set; }

    public int ReorderLevel { get; set; }

    public int Discontinued { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
}

