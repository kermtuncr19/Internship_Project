using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Components
{
    public class RecentOrdersViewComponent : ViewComponent
    {
        private readonly IServiceManager _manager;

        public RecentOrdersViewComponent(IServiceManager manager)
        {
            _manager = manager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var recentOrders = await _manager.OrderService.GetRecentOrdersAsync(10);
            return View(recentOrders);
        }
    }
}