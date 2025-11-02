// Services/CategoryManager.cs
using Entities.Dto;
using Entities.Models;
using Repositories.Contracts;
using Services.Contracts;
using System.Linq;

namespace Services
{
    public class CategoryManager : ICategoryService
    {
        private readonly IRepositoryManager _manager;

        public CategoryManager(IRepositoryManager manager)
        {
            _manager = manager;
        }

        public IEnumerable<Category> GetAllCategories(bool trackChanges) =>
            _manager.Category.FindAll(trackChanges).OrderBy(c => c.CategoryName);

        // ❌ Eskisi: FindByCondition(...).SingleOrDefault()
        // ✅ Doğrusu: tek kayıt dönen metodu doğrudan çağır
        public Category? GetOneCategory(int id, bool trackChanges) =>
            _manager.Category.FindByCondition(c => c.CategoryId == id, trackChanges);
            // veya: _manager.Category.FindOne(c => c.CategoryId == id, trackChanges);

        public Category CreateCategory(CategoryForCreateDto dto)
        {
            var category = new Category
            {
                CategoryName = dto.CategoryName
            };

            _manager.Category.Create(category);
            _manager.Save();
            return category;
        }

        public void UpdateCategory(CategoryForUpdateDto dto)
        {
            // Aynı isim var mı? (bu id hariç)
            if (ExistsByName(dto.CategoryName, excludeId: dto.CategoryId))
                throw new InvalidOperationException("Bu kategori adı zaten mevcut.");

           var entity = _manager.Category.FindByCondition(c => c.CategoryId == dto.CategoryId, trackChange: true);

            if (entity is null)
                throw new KeyNotFoundException("Kategori bulunamadı.");

            entity.CategoryName = dto.CategoryName.Trim();
            _manager.Category.Update(entity);
            _manager.Save();
        }

        public bool TryDeleteCategory(int id, bool trackChanges, out string? error)
    {
        error = null;

        var category = _manager.Category.FindByCondition(c => c.CategoryId == id, trackChanges);
        if (category is null)
        {
            error = "Kategori bulunamadı.";
            return false;
        }

        // 🔍 Bu kategoride ürün var mı?
        var hasProducts = _manager.Product
            .QueryByCondition(p => p.CategoryId == id, trackChange: false)
            .Any();

        if (hasProducts)
        {
            error = "Bu kategoriye bağlı ürünler olduğu için silinemez.";
            return false;
        }

        _manager.Category.Remove(category);
        _manager.Save();
        return true;
    }

        public bool ExistsByName(string name, int? excludeId = null)
        {
            name = name.Trim();

            // ❌ Eskisi: FindByCondition(...).Any()  // FindByCondition tek kayıt döndürür, Any yok.
            // ✅ Doğrusu: koleksiyon isteyen işlerde QueryByCondition + Any
            return _manager.Category
                .QueryByCondition(
                    c => c.CategoryName == name && (excludeId == null || c.CategoryId != excludeId),
                    trackChange: false)
                .Any();
        }
    }
}
