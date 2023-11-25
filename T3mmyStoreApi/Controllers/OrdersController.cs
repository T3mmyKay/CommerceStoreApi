using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T3mmyStoreApi.Models;
using T3mmyStoreApi.Services;

namespace T3mmyStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        /// <summary>
        /// Creates an Order.
        /// </summary>
        /// <param name="orderDto"></param>
        /// <returns>A newly created Order</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todo
        ///     {
        ///        "productIdentifiers": "4-9-8-7-9-23',
        ///        "shippingAddress": "Federal University Technology, Akure",
        ///        "paymentMethod": "Cash on Delivery" || "Paypal" || "Stripe"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created order</response>
        [Authorize]
        [HttpPost]
        public IActionResult CreateOrder(OrderDto orderDto)
        {
            if (!OrderHelper.PaymentMethods.ContainsKey(orderDto.PaymentMethod))
            {
                ModelState.AddModelError("Payment Method", "Please select a valid payment method");
                return BadRequest(ModelState);
            }

            int userId = JWTReader.GetUserId(User);
            var user = _context.Users.Find(userId);

            if (user == null)
            {
                ModelState.AddModelError("Order", "Unable to create the order");
                return BadRequest(ModelState);
            }

            var productDictionary = OrderHelper.GetProductDictionary(orderDto.ProductIdentifiers);

            if (productDictionary.Count == 0)
            {
                ModelState.AddModelError("Order", "Unable to create the order");
                return BadRequest(ModelState);
            }

            var order = new Order();
            order.UserId = userId;
            order.PaymentMethod = orderDto.PaymentMethod;
            order.CreatedAt = DateTime.Now;
            order.ShippingFee = OrderHelper.ShippingFee;
            order.ShippingAddress = orderDto.ShippingAddress;
            order.PaymentMethod = orderDto.PaymentMethod;
            order.PaymentStatus = OrderHelper.PaymentStatuses[0]; //Pending
            order.OrderStatus = OrderHelper.OrderStatuses[0]; //Created

            foreach (var pair in productDictionary)
            {
                int productId = pair.Key;
                var product = _context.Products.Find(productId);

                if (product == null)
                {
                    ModelState.AddModelError("Product", $"Product with id {productId} is not available");
                    return BadRequest(ModelState);
                }

                var orderItem = new OrderItem();
                orderItem.ProductId = productId;
                orderItem.Quantity = pair.Value;
                orderItem.UnitPrice = product.Price;

                order.OrderItems.Add(orderItem);
            }

            if (order.OrderItems.Count < 1)
            {
                ModelState.AddModelError("Order", "Unable to create the order");
                return BadRequest(ModelState);
            }

            //save the order
            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in order.OrderItems)
            {
                item.Order = null;
            }
            // hide the user password.
            order.User.Password = null;
            return Ok(order);
        }

        /// <summary>
        /// This returns all the orders depending on the user role
        /// </summary>
        /// <param name="page"></param>
        /// <returns>A response containing the page information and the orders</returns>
        [Authorize]
        [HttpGet("GetOrders")]
        public IActionResult GetOrders(int? page)
        {

            int userId = JWTReader.GetUserId(User);
            string role = JWTReader.GetUserRole(User);

            IQueryable<Order> query = _context.Orders.Include(o => o.User)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product);

            if (role != "admin")
            {
                query = query.Where(o => o.UserId == userId);
            }

            query = query.OrderByDescending(o => o.Id);

            if (page == null || page < 1)
            {
                page = 1;
            }

            int pageSize = 5;
            int totalPages = 0;

            decimal count = query.Count();
            totalPages = (int)(Math.Ceiling(count / pageSize));

            query = query.Skip((int)((page - 1) * pageSize)).Take(pageSize);

            var orders = query.ToList();

            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    item.Order = null;
                }
                order.User.Password = "";
            }

            var response = new
            {
                Orders = orders,
                TotalPages = totalPages,
                Page = page,
                PageSize = pageSize,

            };
            return Ok(response);
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            int userId = JWTReader.GetUserId(User);
            string role = JWTReader.GetUserRole(User);

            Order? order = null;

            if (role == "admin")
            {
                order = _context.Orders
                     .Include(o => o.User)
                     .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                     .FirstOrDefault(o => o.Id == id);
            }
            else
            {
                order = _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                    .FirstOrDefault(o => o.Id == id && o.UserId == userId);
            }

            if (order == null)
            {
                return NotFound();
            }

            foreach (var item in order.OrderItems)
            {
                item.Order = null;
            }

            order.User.Password = "";
            return Ok(order);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateOrder(int id, string? paymentStatus, string? orderStatus)
        {
            if (paymentStatus == null && orderStatus == null)
            {
                ModelState.AddModelError("Order", "Unable to update the order");
                return BadRequest(ModelState);
            }

            if (paymentStatus != null && !OrderHelper.PaymentStatuses.Contains(paymentStatus))
            {
                ModelState.AddModelError("Payment Status", "Please select a valid payment status");
                return BadRequest(ModelState);

            }

            if (orderStatus != null && !OrderHelper.OrderStatuses.Contains(orderStatus))
            {
                ModelState.AddModelError("Order Status", "Please select a valid order status");
                return BadRequest(ModelState);

            }

            var order = _context.Orders.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            if (paymentStatus != null)
            {
                order.PaymentStatus = paymentStatus;
            }
            if (orderStatus != null)
            {
                order.OrderStatus = orderStatus;
            }

            _context.SaveChanges();

            return Ok(order);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            _context.SaveChanges();

            return Ok();
        }
    }
}
