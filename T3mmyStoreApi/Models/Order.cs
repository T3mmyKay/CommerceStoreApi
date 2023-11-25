using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace T3mmyStoreApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Precision(16, 2)]
        public Decimal ShippingFee { get; set; }
        [MaxLength(100)]
        public string ShippingAddress { get; set; } = "";
        [MaxLength(100)]
        public string PaymentMethod { get; set; } = "";
        [MaxLength(100)]
        public string PaymentStatus { get; set; } = "";
        [MaxLength(100)]
        public string OrderStatus { get; set; } = "";

        // Navigation Properties
        public User User { get; set; } = null!;

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
