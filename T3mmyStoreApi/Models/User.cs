using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace T3mmyStoreApi.Models
{
    [Index("Email", IsUnique = true)]
    public class User
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string FirstName { get; set; } = "";
        [MaxLength(100)]
        public string LastName { get; set; } = "";
        [MaxLength(100)]
        public string Email { get; set; } = ""; //unique in Database
        [MaxLength(20)]
        public string Phone { get; set; } = "";
        [MaxLength(100)]
        public string Address { get; set; } = "";
        [MaxLength(100)]
        public string Password { get; set; } = "";
        [MaxLength(20)]
        public string Role { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}
