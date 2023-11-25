using System.ComponentModel.DataAnnotations;

namespace T3mmyStoreApi.Models
{
    public class UserProfileUpdateDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = "";
        [Required, MaxLength(100)]
        public string LastName { get; set; } = "";
        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = "";
        [MaxLength(20)]
        public string? Phone { get; set; }
        [Required, MaxLength(100)]
        public string Address { get; set; } = "";
    }
}
