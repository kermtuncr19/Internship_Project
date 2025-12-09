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

            // Boşluklara göre ayır
            var tokens = searchTerm.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(t => t.Length >= 2) // "sa", "ka", "form" vs. gibi parçalar
                .ToList();

            if (tokens.Count == 0)
                return products;

            // Her token için hem orijinal hem normalize edilmiş versiyonu üret
            var allTokens = tokens
                .SelectMany(t => new[]
                {
            t.ToLowerInvariant(),
            NormalizeClient(t) // örn: "sarı" -> "sari"
                })
                .Distinct()
                .ToList();

            // OR mantığı: ÜRÜN, token'lardan EN AZ BİRİNİ içersin
            return products.Where(p =>
                allTokens.Any(token =>
                    ((p.ProductName ?? "").ToLower().Contains(token)) ||
                    ((p.Summary ?? "").ToLower().Contains(token))
                )
            );
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

        public static IQueryable<Product> SortBy(this IQueryable<Product> products, string? sortBy)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return products.OrderBy(p => p.ProductId); // varsayılan

            switch (sortBy)
            {
                case "price_asc":
                    return products.OrderBy(p => p.Price);

                case "price_desc":
                    return products.OrderByDescending(p => p.Price);

                case "reviews_desc":
                    // En çok onaylı yorum alan ürünler
                    return products.OrderByDescending(p =>
                        p.Reviews.Where(r => r.IsApproved).Count());

                case "rating_desc":
                    // En yüksek ortalama puan
                    return products.OrderByDescending(p =>
                        p.Reviews
                         .Where(r => r.IsApproved)
                         .Average(r => (double?)r.Rating) ?? 0);

                case "rating_asc":
                    // En düşük ortalama puan
                    return products.OrderBy(p =>
                        p.Reviews
                         .Where(r => r.IsApproved)
                         .Average(r => (double?)r.Rating) ?? 0);

                default:
                    return products.OrderBy(p => p.ProductId);
            }
        }

        public static IQueryable<Product> ToPaginate(this IQueryable<Product> products, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10; // varsayılan

            var skip = (pageNumber - 1) * pageSize;
            return products.Skip(skip).Take(pageSize);
        }
    }
}