// Models/ResetPasswordModel.cs
using System.ComponentModel.DataAnnotations;

namespace StoreApp.Models
{
    public class ResetPasswordModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre gereklidir")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır")]
        [RegularExpression(@"^(?=.*\p{Ll})(?=.*\p{Lu})(?=.*\d)[^\s]+$",
            ErrorMessage = "Şifre en az bir küçük harf, bir büyük harf ve bir rakam içermeli; boşluk içeremez.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}