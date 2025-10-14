using System.ComponentModel.DataAnnotations;

namespace Entities.Dto
{
    public record UserDtoForCreation : UserDto
    {
        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Şifre Gerekli!")]
        public String? Password { get; init; }
    }
}