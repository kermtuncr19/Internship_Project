namespace Entities.Dto
{
    public record ProductDtoForUpdate : ProductDto
    {
        public bool ShowCase { get; set; }
    }
}