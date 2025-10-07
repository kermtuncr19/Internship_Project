using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Extensions
{
    public static class ProductRepositoryExtension
    {
        public static IQueryable<Product> FilteredByCategoryId(this IQueryable<Product> products, int? categoryId)
        {
            if (!categoryId.HasValue || categoryId.Value == 0)
                return products;

            return products.Where(prd => prd.CategoryId == categoryId.Value);
        }

        public static IQueryable<Product> FilteredBySearchTerm(this IQueryable<Product> products, string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return products;

            // Arama terimini client tarafında normalize edip tokenlara böl
            var tokens = NormalizeClient(searchTerm)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToArray();

            if (tokens.Length == 0)
                return products;

            // OR mantığı: her token için ayrı Where -> Concat, sonra Distinct
            IQueryable<Product> q = products.Where(p => false); // boş başlangıç

            foreach (var tk in tokens)
            {
                var like = tk; // capture için local değişken
                q = q.Concat(
                    products.Where(p =>
                        // ProductName
                        (((p.ProductName ?? "").ToLower())
                            .Replace("ı", "i").Replace("İ", "i").Replace("I", "i")
                            .Replace("ğ", "g").Replace("Ğ", "g")
                            .Replace("ş", "s").Replace("Ş", "s")
                            .Replace("ö", "o").Replace("Ö", "o")
                            .Replace("ü", "u").Replace("Ü", "u")
                            .Replace("ç", "c").Replace("Ç", "c")
                        ).Contains(like)
                        ||
                        // CategoryName (kategori null ise boş metin)
                        ((((p.Category != null ? p.Category.CategoryName : "")).ToLower())
                            .Replace("ı", "i").Replace("İ", "i").Replace("I", "i")
                            .Replace("ğ", "g").Replace("Ğ", "g")
                            .Replace("ş", "s").Replace("Ş", "s")
                            .Replace("ö", "o").Replace("Ö", "o")
                            .Replace("ü", "u").Replace("Ü", "u")
                            .Replace("ç", "c").Replace("Ç", "c")
                        ).Contains(like)
                        ||
                        // Summary
                        (((p.Summary ?? "").ToLower())
                            .Replace("ı", "i").Replace("İ", "i").Replace("I", "i")
                            .Replace("ğ", "g").Replace("Ğ", "g")
                            .Replace("ş", "s").Replace("Ş", "s")
                            .Replace("ö", "o").Replace("Ö", "o")
                            .Replace("ü", "u").Replace("Ü", "u")
                            .Replace("ç", "c").Replace("Ç", "c")
                        ).Contains(like)
                    )
                );
            }

            return q.Distinct();
        }

        // Sadece arama terimini client tarafında normalize eder (Expression Tree’nin dışında kaldığı için sorun olmaz)
        private static string NormalizeClient(string input) =>
            input.Trim().ToLowerInvariant()
                 .Replace('ı', 'i').Replace('İ', 'i').Replace('I', 'i')
                 .Replace('ğ', 'g').Replace('Ğ', 'g')
                 .Replace('ş', 's').Replace('Ş', 's')
                 .Replace('ö', 'o').Replace('Ö', 'o')
                 .Replace('ü', 'u').Replace('Ü', 'u')
                 .Replace('ç', 'c').Replace('Ç', 'c');
        public static IQueryable<Product> FilteredByPrice(this IQueryable<Product> products, int minPrice, int maxPrice, bool isValidPrice)
        {
            if (isValidPrice)
                return products.Where(prd => prd.Price >= minPrice && prd.Price <= maxPrice);
            else
                return products;
        }
        public static IQueryable<Product> ToPaginate(this IQueryable<Product> products, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 6; // varsayılan

            var skip = (pageNumber - 1) * pageSize;  
            return products.Skip(skip).Take(pageSize);
        }
    }
}