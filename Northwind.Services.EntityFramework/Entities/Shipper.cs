using System.Collections.Generic;

namespace Northwind.Services.EntityFramework.Entities;

public class Shipper
{
    public long ShipperID { get; set; }

    public string CompanyName { get; set; } = default!;

    public string? Phone { get; set; }

    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
}

