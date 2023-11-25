using System.ComponentModel.DataAnnotations;

namespace T3mmyStoreApi.Models
{
    public class OrderDto
    {
        [Required]
        public string ProductIdentifiers { get; set; } = "";
        [Required, MinLength(30), MaxLength(100)]
        public string ShippingAddress { get; set; } = "";

        [Required]
        public string PaymentMethod { get; set; } = "";

    }
}
