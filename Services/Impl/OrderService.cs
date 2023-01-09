using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orders.DAL;
using Orders.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lesson6.Services.Impl
{
    public class OrderService : IOrderService
    {
        #region Services

        private readonly ILogger<OrderService> _logger;
        private readonly OrdersDbContext _context;

        #endregion

        #region Constructors

        public OrderService(OrdersDbContext context,
            ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #endregion

        public async Task<Order> CreateAsync(int buyerId, string address, string phone, IEnumerable<(int productId, int quantity)> products)
        {
            var buyer = await _context.Buyers.FirstOrDefaultAsync(buyer => buyer.Id == buyerId);
            if (buyer == null)
                throw new Exception("Buyer not found.");

            Dictionary<Product, int> productCollection = new Dictionary<Product, int>();
            foreach (var p in products)
            {
                var productEntyty = await _context.Products.FirstOrDefaultAsync(product => product.Id == p.productId);
                if (productEntyty == null)
                    throw new Exception("Product not found.");
                if (productCollection.ContainsKey(productEntyty))
                    productCollection[productEntyty] += p.quantity;
                else
                    productCollection.Add(productEntyty, p.quantity);
            }

            var order = new Order
            {
                Buyer = buyer,
                Address = address,
                Phone = phone,
                OrderDate = DateTime.Now,
                Items = productCollection.Select(p => new OrderItem
                {
                    Product = p.Key,
                    Quantity = p.Value
                }).ToArray()
            };

            await _context.Orders.AddAsync(order);

            await _context.SaveChangesAsync();

            return order;
        }
    }
}
