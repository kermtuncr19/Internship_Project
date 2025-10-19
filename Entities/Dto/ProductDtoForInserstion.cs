namespace Entities.Dto
{
    public record ProductDtoForInsertion : ProductDto
    {
        public bool RequiresSize { get; set; }
        public string? SizeOptionsCsv { get; set; }
    }
}