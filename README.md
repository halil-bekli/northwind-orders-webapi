# Northwind Order Repository (Web API)

An ASP.NET Core Web API sample that exposes Northwind orders via a repository layer backed by Entity Framework Core.

The codebase uses an in-memory SQLite database for local runs and for the test suite.

## Prerequisites

You should be familiar with:
* You should be familiar with structured and object-oriented programming in C#.
* You should be familiar with .NET's string parsing and data formatting capabilities.
* You should be familiar with the [task asynchronous programming model (TAP)](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/task-asynchronous-programming-model).
* You should be familiar with the [Entity Framework](https://learn.microsoft.com/en-us/aspnet/entity-framework) object-relational mapper.
* You should be familiar with the [Repository design pattern](https://martinfowler.com/eaaCatalog/repository.html).
* You should know how to build [controller-based Web API applications](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-7.0) using ASP.NET Core.

The repository requires [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) to be installed.


## Repository Description

This repository contains a Web API application built with ASP.NET Core and Entity Framework.

The application consists of a few components:
1. [Entity types](https://learn.microsoft.com/en-us/ef/core/modeling/entity-types) mapped to the underlying database schema.
1. An order repository that searches and manages orders using Entity Framework.
1. An ASP.NET controller that handles incoming HTTP requests.

The application uses the [SQLite in-memory database](https://www.sqlite.org/inmemorydb.html) as a data source.


### Project Structure

The solution consists of five projects:
* The [Northwind.Orders.WebApi](Northwind.Orders.WebApi) project is a Web API application that is built using the ASP.NET Core framework.
* The repository interface and shared data classes are located in the [Northwind.Services](./Northwind.Services) project.
* The repository implementation is located in the [Northwind.Services.EntityFramework](Northwind.Services.EntityFramework) project.
* The [Northwind.Services.EntityFramework.Tests](Northwind.Services.EntityFramework.Tests) project contains unit tests for the repository implementation.
* The [Northwind.Orders.WebApi.Tests](Northwind.Orders.WebApi.Tests) project contains unit tests for the Web API controller class.

Diagram 1. Project Dependency Diagram.

![Project Dependency Diagram](images/project-dependency.png)


### Entity Types.

To enable the order repository to work with a database, this repository includes:

1. Entity types in the [Entities](Northwind.Services.EntityFramework/Entities) folder of the `Northwind.Services.EntityFramework` project.
1. The [NorthwindContext](Northwind.Services.EntityFramework/Entities/NorthwindContext.cs#L5) DbContext with a DbSet for each entity type.
1. Entity-to-table mappings configured via Fluent API (or data annotations).


### The OrderRepository Class

The [OrderRepository](Northwind.Services.EntityFramework/Repositories/OrderRepository.cs#L7) class provides methods for searching and returning order data, as well as methods for managing orders.
* [GetOrdersAsync](Northwind.Services/Repositories/IOrderRepository.cs#L15) returns a list of orders from a repository. The `skip` method parameter specifies the number of orders to skip before adding an order to the result list. The `count` method parameter specified the number of orders to return. The result list should be sorted by order ID, from smallest to largest.
* [GetOrderAsync](Northwind.Services/Repositories/IOrderRepository.cs#L22) returns an order with the specified identifier.
* [AddOrderAsync](Northwind.Services/Repositories/IOrderRepository.cs#L30) adds a new order to the repository as well as order details.
* [RemoveOrderAsync](Northwind.Services/Repositories/IOrderRepository.cs#L37) removes an order with the specified identifier as well as the order details.
* [UpdateOrderAsync](Northwind.Services/Repositories/IOrderRepository.cs#L44) updates order data as well as order details.

Diagram 2. OrderRepository Class Diagram.

![OrderRepository](images/order-repository.png)

An order is represented by the [Order](Northwind.Services/Repositories/Order.cs) class. There are a few related classes that represent order data and order details.

Diagram 3. Order Class Diagram.

![Order Class Diagram](images/order.png)


### The OrdersController Class

The [OrdersController](Northwind.Orders.WebApi/Controllers/OrdersController.cs#9) class is an ASP.NET WebAPI controller with the required action methods:

| Method Name      | HTTP Verb | URI                                  | HTTP Status Code |
|------------------|-----------|--------------------------------------|------------------|
| GetOrderAsync    | GET       | /api/orders/:orderId                 | OK               |
| GetOrdersAsync   | GET       | /api/orders?skip=:skip&count=:count  | OK               |
| AddOrderAsync    | POST      | /api/orders                          | OK               |
| RemoveOrderAsync | DELETE    | /api/orders/:orderId                 | No Content       |
| UpdateOrderAsync | PUT       | /api/orders/:orderId                 | No Content       |


### Northwind Database

The application and unit tests are configured to use the SQLite in-memory database as a data source. The database schema is shown on the Diagram 4.

Diagram 4. Northwind Database Schema.

![Northwind Database Schema](images/database-schema.png)


## Running and testing

From the solution root:

```bash
dotnet restore
dotnet build
dotnet test
```

To run the Web API locally:

```bash
dotnet run --project Northwind.Orders.WebApi
```
