using System.Collections.Generic;

namespace Northwind.Services.EntityFramework.Entities;

public class Category
{
    public long CategoryID { get; set; }

    public string CategoryName { get; set; } = default!;

    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new HashSet<Product>();
}

