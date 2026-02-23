using Microsoft.AspNetCore.Mvc;
using Northwind.Services.Repositories;
using ModelsAddOrder = Northwind.Orders.WebApi.Models.AddOrder;
using ModelsBriefOrder = Northwind.Orders.WebApi.Models.BriefOrder;
using ModelsBriefOrderDetail = Northwind.Orders.WebApi.Models.BriefOrderDetail;
using ModelsCustomer = Northwind.Orders.WebApi.Models.Customer;
using ModelsEmployee = Northwind.Orders.WebApi.Models.Employee;
using ModelsFullOrder = Northwind.Orders.WebApi.Models.FullOrder;
using ModelsShipper = Northwind.Orders.WebApi.Models.Shipper;
using ModelsShippingAddress = Northwind.Orders.WebApi.Models.ShippingAddress;

namespace Northwind.Orders.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderRepository orderRepository;
    private readonly ILogger<OrdersController> logger;

    public OrdersController(IOrderRepository orderRepository, ILogger<OrdersController> logger)
    {
        this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{orderId:long}")]
    public async Task<ActionResult<ModelsFullOrder>> GetOrderAsync(long orderId)
    {
        try
        {
            var order = await this.orderRepository.GetOrderAsync(orderId).ConfigureAwait(false);
            var model = MapToFullOrder(order);
            return this.Ok(model);
        }
        catch (OrderNotFoundException ex)
        {
            this.logger.LogWarning(ex, "Order with id {OrderId} was not found.", orderId);
            return this.NotFound();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error while getting order {OrderId}.", orderId);
            return this.StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModelsBriefOrder>>> GetOrdersAsync(int? skip, int? count)
    {
        int actualSkip = skip ?? 0;
        int actualCount = count ?? 10;

        if (actualSkip < 0 || actualCount <= 0)
        {
            return this.BadRequest();
        }

        try
        {
            var orders = await this.orderRepository.GetOrdersAsync(actualSkip, actualCount).ConfigureAwait(false);
            var result = orders.Select(MapToBriefOrder).ToList();
            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error while getting orders.");
            return this.StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ModelsAddOrder>> AddOrderAsync(ModelsBriefOrder order)
    {
        try
        {
            var repositoryOrder = MapFromBriefOrder(order, 0);
            long id = await this.orderRepository.AddOrderAsync(repositoryOrder).ConfigureAwait(false);
            var result = new ModelsAddOrder { OrderId = id };
            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error while adding order.");
            return this.StatusCode(500);
        }
    }

    [HttpDelete("{orderId:long}")]
    public async Task<ActionResult> RemoveOrderAsync(long orderId)
    {
        try
        {
            await this.orderRepository.RemoveOrderAsync(orderId).ConfigureAwait(false);
            return this.NoContent();
        }
        catch (OrderNotFoundException ex)
        {
            this.logger.LogWarning(ex, "Order with id {OrderId} was not found for removal.", orderId);
            return this.NotFound();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error while removing order {OrderId}.", orderId);
            return this.StatusCode(500);
        }
    }

    [HttpPut("{orderId:long}")]
    public async Task<ActionResult> UpdateOrderAsync(long orderId, ModelsBriefOrder order)
    {
        try
        {
            var repositoryOrder = MapFromBriefOrder(order, orderId);
            await this.orderRepository.UpdateOrderAsync(repositoryOrder).ConfigureAwait(false);
            return this.NoContent();
        }
        catch (OrderNotFoundException ex)
        {
            this.logger.LogWarning(ex, "Order with id {OrderId} was not found for update.", orderId);
            return this.NotFound();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error while updating order {OrderId}.", orderId);
            return this.StatusCode(500);
        }
    }

    private static ModelsFullOrder MapToFullOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new ModelsFullOrder
        {
            Id = order.Id,
            Customer = new ModelsCustomer
            {
                Code = order.Customer.Code.Code,
                CompanyName = order.Customer.CompanyName,
            },
            Employee = new ModelsEmployee
            {
                Id = order.Employee.Id,
                FirstName = order.Employee.FirstName,
                LastName = order.Employee.LastName,
                Country = order.Employee.Country,
            },
            OrderDate = order.OrderDate,
            RequiredDate = order.RequiredDate,
            ShippedDate = order.ShippedDate,
            Freight = order.Freight,
            ShipName = order.ShipName,
            Shipper = new ModelsShipper
            {
                Id = order.Shipper.Id,
                CompanyName = order.Shipper.CompanyName,
            },
            ShippingAddress = new ModelsShippingAddress
            {
                Address = order.ShippingAddress.Address,
                City = order.ShippingAddress.City,
                Region = order.ShippingAddress.Region,
                PostalCode = order.ShippingAddress.PostalCode,
                Country = order.ShippingAddress.Country,
            },
        };
    }

    private static ModelsBriefOrder MapToBriefOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new ModelsBriefOrder
        {
            Id = order.Id,
            CustomerId = order.Customer.Code.Code,
            EmployeeId = order.Employee.Id,
            OrderDate = order.OrderDate,
            RequiredDate = order.RequiredDate,
            ShippedDate = order.ShippedDate,
            ShipperId = order.Shipper.Id,
            Freight = order.Freight,
            ShipName = order.ShipName,
            ShipAddress = order.ShippingAddress.Address,
            ShipCity = order.ShippingAddress.City,
            ShipRegion = order.ShippingAddress.Region,
            ShipPostalCode = order.ShippingAddress.PostalCode,
            ShipCountry = order.ShippingAddress.Country,
            OrderDetails = new List<ModelsBriefOrderDetail>(),
        };
    }

    private static Order MapFromBriefOrder(ModelsBriefOrder order, long id)
    {
        ArgumentNullException.ThrowIfNull(order);

        var repositoryOrder = new Order(id)
        {
            Customer = new Customer(new CustomerCode(order.CustomerId))
            {
                CompanyName = string.Empty,
            },
            Employee = new Employee(order.EmployeeId)
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                Country = string.Empty,
            },
            Shipper = new Shipper(order.ShipperId)
            {
                CompanyName = string.Empty,
            },
            ShippingAddress = new ShippingAddress(
                order.ShipAddress,
                order.ShipCity,
                order.ShipRegion,
                order.ShipPostalCode,
                order.ShipCountry),
            OrderDate = order.OrderDate,
            RequiredDate = order.RequiredDate,
            ShippedDate = order.ShippedDate,
            Freight = order.Freight,
            ShipName = order.ShipName,
        };

        if (order.OrderDetails is not null)
        {
            foreach (var detail in order.OrderDetails)
            {
                var product = new Product(detail.ProductId)
                {
                    ProductName = string.Empty,
                    CategoryId = 0,
                    Category = string.Empty,
                    SupplierId = 0,
                    Supplier = string.Empty,
                };

                var repositoryDetail = new OrderDetail(repositoryOrder)
                {
                    Product = product,
                    UnitPrice = detail.UnitPrice,
                    Quantity = detail.Quantity,
                    Discount = detail.Discount,
                };

                repositoryOrder.OrderDetails.Add(repositoryDetail);
            }
        }

        return repositoryOrder;
    }
}
