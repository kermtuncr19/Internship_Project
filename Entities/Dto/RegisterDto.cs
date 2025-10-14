using System.ComponentModel.DataAnnotations;

namespace Entities.Dto
{
    public record RegisterDto
    {
        [Required(ErrorMessage = "Kullanıcı Adı Gerekli")]
        public String? UserName { get; init; }
        [Required(ErrorMessage = "E-Posta Gerekli")]
        public String? Email { get; init; }
        [Required(ErrorMessage = "Şifre gerekli.")]
        [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalı.")]
        // En az 1 küçük (Unicode), 1 büyük (Unicode), 1 rakam, ve BOŞLUK YASAK
        [RegularExpression(@"^(?=.*\p{Ll})(?=.*\p{Lu})(?=.*\d)[^\s]+$",
        ErrorMessage = "Şifre en az bir küçük harf, bir büyük harf ve bir rakam içermeli; boşluk içeremez.")]
        [DataType(DataType.Password)]
        public string Password { get; init; } = string.Empty;
    }
}