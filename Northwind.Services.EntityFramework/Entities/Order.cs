using System;
using System.Collections.Generic;

namespace Northwind.Services.EntityFramework.Entities;

public class Order
{
    public long OrderID { get; set; }

    public string CustomerID { get; set; } = default!;

    public Customer Customer { get; set; } = default!;

    public long EmployeeID { get; set; }

    public Employee Employee { get; set; } = default!;

    public DateTime OrderDate { get; set; }

    public DateTime RequiredDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public long ShipVia { get; set; }

    public Shipper Shipper { get; set; } = default!;

    public double Freight { get; set; }

    public string ShipName { get; set; } = default!;

    public string ShipAddress { get; set; } = default!;

    public string ShipCity { get; set; } = default!;

    public string? ShipRegion { get; set; }

    public string ShipPostalCode { get; set; } = default!;

    public string ShipCountry { get; set; } = default!;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
}

