using AutoMapper;
using ECommerce.Api.Orders.Db;
using ECommerce.Api.Orders.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Api.Orders.Providers
{
    public class OrdersProvider : IOrdersProvider
    {
        private readonly OrdersDbContext dbContext;
        private readonly ILogger<OrdersProvider> logger;
        private readonly IMapper mapper;

        public OrdersProvider(OrdersDbContext dbContext, ILogger<OrdersProvider> logger, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.mapper = mapper;

            SeedData();
        }

        private void SeedData()
        {
            if (!dbContext.Orders.Any())
            {
                dbContext.Orders.Add(
                    new Order()
                    {
                        Id = 1,
                        CustomerId = 2,
                        OrderDate = DateTime.UtcNow,
                        Items = new List<OrderItem>() {
                        new OrderItem() { Id = 1, OrderId = 1, ProductId = 3, Quantity = 2, UnitPrice = 100},
                        new OrderItem() { Id = 2, OrderId = 1, ProductId = 1, Quantity = 5, UnitPrice = 30}
                    }
                    }
                    );
                dbContext.Orders.Add(
                    new Order()
                    {
                        Id = 2,
                        CustomerId = 1,
                        OrderDate = DateTime.UtcNow,
                        Items = new List<OrderItem>() {
                        new OrderItem() { Id = 3, OrderId = 2, ProductId = 2, Quantity = 2, UnitPrice = 55},
                        new OrderItem() { Id = 4, OrderId = 2, ProductId = 3, Quantity = 4, UnitPrice = 80}
                    }
                    }
                    );
                dbContext.SaveChanges();
            };
        }

        public async Task<(bool IsSuccess, Models.Order Order, string ErrorMessage)> GetOrderAsync(int customerId)
        {
            try
            {
                var order = await dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(x => x.CustomerId == customerId);
                if (order != null)
                {
                    var result = mapper.Map<Db.Order, Models.Order>(order);
                    return (true, result, null);
                }
                return (false, null, "Not found");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool IsSuccess, IEnumerable<Models.Order> Orders, string ErrorMessage)> GetOrdersAsync(int customerId)
        {
            try
            {
                var orders = await dbContext.Orders
                    .Where(x => x.CustomerId == customerId)
                    .Include(o => o.Items)
                    .ToListAsync();

                if (orders != null && orders.Any())
                {
                    var result = mapper.Map<IEnumerable<Db.Order>, IEnumerable<Models.Order>>(orders);
                    return (true, result, null);
                }
                return (false, null, "Not found");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
                return (false, null, ex.Message);
            }
        }
    }
}
