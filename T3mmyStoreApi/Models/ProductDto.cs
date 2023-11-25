using System.ComponentModel.DataAnnotations;

namespace T3mmyStoreApi.Models
{
    public class ProductDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]
        public string Brand { get; set; } = "";
        [Required, MaxLength(100)]
        public string Category { get; set; } = "";
        [Required]
        public decimal Price { get; set; }
        [MaxLength(4000)]
        public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
