using Microsoft.EntityFrameworkCore;
using Northwind.Services.EntityFramework.Entities;
using Northwind.Services.Repositories;
using RepositoryOrder = Northwind.Services.Repositories.Order;
using RepositoryCustomer = Northwind.Services.Repositories.Customer;
using RepositoryCustomerCode = Northwind.Services.Repositories.CustomerCode;
using RepositoryEmployee = Northwind.Services.Repositories.Employee;
using RepositoryOrderDetail = Northwind.Services.Repositories.OrderDetail;
using RepositoryProduct = Northwind.Services.Repositories.Product;
using RepositoryShipper = Northwind.Services.Repositories.Shipper;

namespace Northwind.Services.EntityFramework.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly NorthwindContext context;

    public OrderRepository(NorthwindContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<RepositoryOrder> GetOrderAsync(long orderId)
    {
        var entity = await this.context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Shipper)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                    .ThenInclude(p => p.Category)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                    .ThenInclude(p => p.Supplier)
            .FirstOrDefaultAsync(o => o.OrderID == orderId)
            .ConfigureAwait(false);

        if (entity is null)
        {
            throw new OrderNotFoundException($"Order with id {orderId} was not found.");
        }

        return MapToRepositoryOrder(entity, includeDetails: true);
    }

    public async Task<IList<RepositoryOrder>> GetOrdersAsync(int skip, int count)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var entities = await this.context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Shipper)
            .OrderBy(o => o.OrderID)
            .Skip(skip)
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);

        return entities
            .Select(o => MapToRepositoryOrder(o, includeDetails: false))
            .ToList();
    }

    public async Task<long> AddOrderAsync(RepositoryOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);

        ValidateOrderDetails(order);

        var entity = MapToEntityOrderForAdd(order);

        this.context.Orders.Add(entity);

        try
        {
            await this.context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new RepositoryException("An error occurred while adding an order.", ex);
        }

        return entity.OrderID;
    }

    public async Task RemoveOrderAsync(long orderId)
    {
        var entity = await this.context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderID == orderId)
            .ConfigureAwait(false);

        if (entity is null)
        {
            throw new OrderNotFoundException($"Order with id {orderId} was not found.");
        }

        this.context.OrderDetails.RemoveRange(entity.OrderDetails);
        this.context.Orders.Remove(entity);
        await this.context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task UpdateOrderAsync(RepositoryOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var entity = await this.context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderID == order.Id)
            .ConfigureAwait(false);

        if (entity is null)
        {
            throw new OrderNotFoundException($"Order with id {order.Id} was not found.");
        }

        ValidateOrderDetails(order);

        entity.CustomerID = order.Customer.Code.Code;
        entity.EmployeeID = order.Employee.Id;
        entity.OrderDate = order.OrderDate;
        entity.RequiredDate = order.RequiredDate;
        entity.ShippedDate = order.ShippedDate;
        entity.ShipVia = order.Shipper.Id;
        entity.Freight = order.Freight;
        entity.ShipName = order.ShipName;
        entity.ShipAddress = order.ShippingAddress.Address;
        entity.ShipCity = order.ShippingAddress.City;
        entity.ShipRegion = order.ShippingAddress.Region;
        entity.ShipPostalCode = order.ShippingAddress.PostalCode;
        entity.ShipCountry = order.ShippingAddress.Country;

        this.context.OrderDetails.RemoveRange(entity.OrderDetails);
        entity.OrderDetails.Clear();

        foreach (var detail in order.OrderDetails)
        {
            var detailEntity = new Entities.OrderDetail
            {
                Order = entity,
                ProductID = detail.Product.Id,
                UnitPrice = detail.UnitPrice,
                Quantity = detail.Quantity,
                Discount = detail.Discount,
            };

            entity.OrderDetails.Add(detailEntity);
        }

        await this.context.SaveChangesAsync().ConfigureAwait(false);
    }

    private static void ValidateOrderDetails(RepositoryOrder order)
    {
        foreach (var detail in order.OrderDetails)
        {
            if (detail.Product is null || detail.Product.Id <= 0)
            {
                throw new RepositoryException("Product id must be greater than zero.");
            }

            if (detail.UnitPrice < 0)
            {
                throw new RepositoryException("Unit price must be greater than or equal to zero.");
            }

            if (detail.Quantity <= 0)
            {
                throw new RepositoryException("Quantity must be greater than zero.");
            }

            if (detail.Discount < 0 || detail.Discount > 1)
            {
                throw new RepositoryException("Discount must be between 0 and 1.");
            }
        }
    }

    private static RepositoryOrder MapToRepositoryOrder(Entities.Order entity, bool includeDetails)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var order = new RepositoryOrder(entity.OrderID)
        {
            Customer = new RepositoryCustomer(new RepositoryCustomerCode(entity.Customer.CustomerID))
            {
                CompanyName = entity.Customer.CompanyName,
            },
            Employee = new RepositoryEmployee(entity.Employee.EmployeeID)
            {
                FirstName = entity.Employee.FirstName,
                LastName = entity.Employee.LastName,
                Country = entity.Employee.Country ?? string.Empty,
            },
            Shipper = new RepositoryShipper(entity.Shipper.ShipperID)
            {
                CompanyName = entity.Shipper.CompanyName,
            },
            ShippingAddress = new ShippingAddress(
                entity.ShipAddress,
                entity.ShipCity,
                entity.ShipRegion,
                entity.ShipPostalCode,
                entity.ShipCountry),
            OrderDate = entity.OrderDate,
            RequiredDate = entity.RequiredDate,
            ShippedDate = entity.ShippedDate,
            Freight = entity.Freight,
            ShipName = entity.ShipName,
        };

        if (includeDetails)
        {
            foreach (var detailEntity in entity.OrderDetails.OrderBy(d => d.ProductID))
            {
                var product = detailEntity.Product;

                var repositoryProduct = new RepositoryProduct(product.ProductID)
                {
                    ProductName = product.ProductName,
                    CategoryId = product.CategoryID,
                    Category = product.Category.CategoryName,
                    SupplierId = product.SupplierID,
                    Supplier = product.Supplier.CompanyName,
                };

                var detail = new RepositoryOrderDetail(order)
                {
                    Product = repositoryProduct,
                    UnitPrice = detailEntity.UnitPrice,
                    Quantity = detailEntity.Quantity,
                    Discount = detailEntity.Discount,
                };

                order.OrderDetails.Add(detail);
            }
        }

        return order;
    }

    private static Entities.Order MapToEntityOrderForAdd(RepositoryOrder order)
    {
        var entity = new Entities.Order
        {
            CustomerID = order.Customer.Code.Code,
            EmployeeID = order.Employee.Id,
            OrderDate = order.OrderDate,
            RequiredDate = order.RequiredDate,
            ShippedDate = order.ShippedDate,
            ShipVia = order.Shipper.Id,
            Freight = order.Freight,
            ShipName = order.ShipName,
            ShipAddress = order.ShippingAddress.Address,
            ShipCity = order.ShippingAddress.City,
            ShipRegion = order.ShippingAddress.Region,
            ShipPostalCode = order.ShippingAddress.PostalCode,
            ShipCountry = order.ShippingAddress.Country,
        };

        foreach (var detail in order.OrderDetails)
        {
            var detailEntity = new Entities.OrderDetail
            {
                Order = entity,
                ProductID = detail.Product.Id,
                UnitPrice = detail.UnitPrice,
                Quantity = detail.Quantity,
                Discount = detail.Discount,
            };

            entity.OrderDetails.Add(detailEntity);
        }

        return entity;
    }
}
