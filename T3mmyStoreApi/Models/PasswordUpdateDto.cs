using System.ComponentModel.DataAnnotations;

namespace T3mmyStoreApi.Models
{
    public class PasswordUpdateDto
    {
        [Required, MaxLength(100)]
        public string OldPassword { get; set; } = "";
        [Required, MinLength(8), MaxLength(100)]
        public string NewPassword { get; set; } = "";
        [Required, Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = "";
    }
}
