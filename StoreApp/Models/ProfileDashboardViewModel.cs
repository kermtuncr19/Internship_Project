using Entities.Models;

namespace StoreApp.Models
{
    public class ProfileDashboardViewModel
    {
        public UserProfile Profile { get; set; } = default!;
        public int AddressCount { get; set; }
        public int FavoriteCount { get; set; }
        public List<Order> LastOrders { get; set; } = new();
    }
}
