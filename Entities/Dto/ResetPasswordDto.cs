using System.ComponentModel.DataAnnotations;

namespace Entities.Dto
{
    public record ResetPasswordDto
    {
        public String? UserName { get; init; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Şifre Gerekli!")]
        public String? Password { get; init; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Şifreyi Tekrar Girmeniz Gerekli!")]
        [Compare("Password", ErrorMessage ="Tekrar Girdiğiniz Şifre İlk Girdiğiniz ile Aynı Olmalı!")]
        public String? ConfirmPassword { get; init; }
    }
}