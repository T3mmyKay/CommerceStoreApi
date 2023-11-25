﻿using System.ComponentModel.DataAnnotations;

namespace T3mmyStoreApi.Models
{
    public class ContactDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = "";
        [Required, MaxLength(100)]
        public string LastName { get; set; } = "";
        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; } = "";
        [MaxLength(100)]
        public string? Phone { get; set; }
        public int SubjectId { get; set; }
        [Required, MinLength(20), MaxLength(4000)]
        public string Message { get; set; } = "";
    }
}
