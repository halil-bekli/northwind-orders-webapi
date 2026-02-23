using System;
using System.Collections.Generic;

namespace Northwind.Services.EntityFramework.Entities;

public class Employee
{
    public long EmployeeID { get; set; }

    public string LastName { get; set; } = default!;

    public string FirstName { get; set; } = default!;

    public string? Title { get; set; }

    public string? TitleOfCourtesy { get; set; }

    public DateTime? BirthDate { get; set; }

    public DateTime? HireDate { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Region { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? HomePhone { get; set; }

    public string? Extension { get; set; }

    public string? Notes { get; set; }

    public long? ReportsTo { get; set; }

    public string? PhotoPath { get; set; }

    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
}

