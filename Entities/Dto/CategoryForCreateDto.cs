namespace Entities.Dto
{
    public class CategoryForCreateDto
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Kategori adı zorunludur.")]
        [System.ComponentModel.DataAnnotations.StringLength(50, ErrorMessage = "Kategori adı 50 karakterden uzun olamaz.")]
        public string CategoryName { get; set; } = string.Empty;
    }
}
