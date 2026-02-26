using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class CouponUsage
    {
        public int CouponUsageId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        public int CouponId { get; set; }
        public Coupon Coupon { get; set; } = default!;

        public int? OrderId { get; set; }
        public DateTime UsedAtUtc { get; set; } = DateTime.UtcNow;
    }
}