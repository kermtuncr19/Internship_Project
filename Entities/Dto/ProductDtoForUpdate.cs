namespace Entities.Dto
{
    public record ProductDtoForUpdate : ProductDto
    {
        public bool ShowCase { get; set; }
        public bool RequiresSize { get; set; }
        public string? SizeOptionsCsv { get; set; }
    }
}