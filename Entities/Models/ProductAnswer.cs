using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class ProductAnswer
    {
        public int ProductAnswerId { get; set; }

        [Required]
        public int ProductQuestionId { get; set; }
        public ProductQuestion? Question { get; set; }

        [Required]
        public string AdminUserId { get; set; } = default!;
        public IdentityUser? AdminUser { get; set; }

        [Required]
        [MaxLength(2000)]
        public string AnswerText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
