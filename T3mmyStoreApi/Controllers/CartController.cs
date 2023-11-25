using Microsoft.AspNetCore.Mvc;
using T3mmyStoreApi.Models;
using T3mmyStoreApi.Services;

namespace T3mmyStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }
        [HttpGet]
        public IActionResult GetCart(string prouctIdentifiers)
        {
            CartDto cartDto = new CartDto();
            cartDto.CartItems = new List<CartItemDto>();
            cartDto.SubTotal = 0;
            cartDto.ShippingFee = OrderHelper.ShippingFee;
            cartDto.TotalPrice = 0;
            var productDictionary = OrderHelper.GetProductDictionary(prouctIdentifiers);

            foreach (var pair in productDictionary)
            {
                int productId = pair.Key;
                var product = _context.Products.Find(productId);

                if (product == null)
                {
                    continue;
                }

                var cartItemDto = new CartItemDto();
                cartItemDto.Product = product;
                cartItemDto.Quantity = pair.Value;

                cartDto.CartItems.Add(cartItemDto);

                cartDto.SubTotal += product.Price * pair.Value;
                cartDto.TotalPrice = cartDto.SubTotal + cartDto.ShippingFee;

            }
            return Ok(cartDto);

        }

        [HttpGet("PaymentMethods")]
        public IActionResult GetPaymentMethods()
        {
            Dictionary<string, string> methods = OrderHelper.PaymentMethods;
            return Ok(methods);
        }
    }
}
