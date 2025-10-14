using System.ComponentModel.DataAnnotations;

namespace Entities.Dto
{
    public record UserDto
    {
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Kullanıcı Adı Gerekli!")]
        public String? UserName { get; init; }
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "E-Posta Gerekli!")]
        public String? Email { get; init; }
        [DataType(DataType.PhoneNumber)]
        public String? PhoneNumber { get; init; }
        public HashSet<String> Roles { get; set; } = new HashSet<string>();    
        }
}