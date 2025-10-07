using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Extensions
{
    public static class ProductRepositoryExtension
    {
        public static IQueryable<Product> FilteredByCategoryId(this IQueryable<Product> products, int? categoryId)
        {
            if (categoryId == 0)
                return products;
            else
                return products.Where(prd => prd.CategoryId.Equals(categoryId));
        }

        public static IQueryable<Product> FilteredBySearchTerm(this IQueryable<Product> products, String? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return products;
            var term = $"%{searchTerm.Trim()}%";

            var tokens = searchTerm
        .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(t => $"%{t}%")
        .ToArray();

            foreach (var like in tokens)
            {
                products = products.Where(p =>
                    EF.Functions.Like(p.ProductName, like) ||
                    EF.Functions.Like(p.Category.CategoryName, like) ||
                    EF.Functions.Like(p.Summary!, like)
                );
            }

            return products;
        }
    }
}