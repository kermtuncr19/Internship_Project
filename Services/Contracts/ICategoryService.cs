using Entities.Dto;
using Entities.Models;

namespace Services.Contracts
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAllCategories(bool trackChanges);
        Category? GetOneCategory(int id, bool trackChanges);
        Category CreateCategory(CategoryForCreateDto dto);
         void UpdateCategory(CategoryForUpdateDto dto);
         bool TryDeleteCategory(int id, bool trackChanges, out string? error);
        bool ExistsByName(string name, int? excludeId = null);
    }
}