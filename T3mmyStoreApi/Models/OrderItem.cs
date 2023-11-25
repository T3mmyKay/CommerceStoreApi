using Microsoft.EntityFrameworkCore;

namespace T3mmyStoreApi.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        [Precision(16, 2)]
        public decimal UnitPrice { get; set; }


        // Navigation Properties
        public Product Product { get; set; } = new Product();

        public Order Order { get; set; } = new Order();
    }
}
