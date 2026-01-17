using Entities.Models;
using System.Collections.Generic;

namespace StoreApp.Models
{
    public class ShowcaseViewModel
    {
        public List<Product> ShowcaseProducts { get; set; } = new();
        public List<Product> MostFavorited { get; set; } = new();
        public List<Product> NewArrivals { get; set; } = new();
        public List<Product> BestSellers { get; set; } = new();
    }
}