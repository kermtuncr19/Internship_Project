using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class Coupon
    {
        public int CouponId { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal Percent { get; set; }  // 20 => %20

        public bool IsActive { get; set; } = true;

        public decimal? MinCartTotal { get; set; } // opsiyonel

        public DateTime? StartsAtUtc { get; set; }
        public DateTime? EndsAtUtc { get; set; }

        public int? UsageLimit { get; set; } // opsiyonel
        public int UsedCount { get; set; } = 0;
    }
}