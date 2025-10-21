// Entities/Models/PaymentInfo.cs
using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class PaymentInfo
    {
        [Required(ErrorMessage = "Kart üzerindeki isim gerekli")]
        [Display(Name = "Kart Üzerindeki İsim")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kart numarası gerekli")]
        [Display(Name = "Kart Numarası")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Kart numarası 16 haneli olmalı")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Son kullanma tarihi gerekli")]
        [Display(Name = "Son Kullanma (AA/YY)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Format: AA/YY")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV gerekli")]
        [Display(Name = "CVV")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV 3 veya 4 haneli olmalı")]
        public string CVV { get; set; } = string.Empty;
    }
}