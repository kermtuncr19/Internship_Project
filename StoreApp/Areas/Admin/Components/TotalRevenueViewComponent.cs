using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Components
{
    public class TotalRevenueViewComponent : ViewComponent
    {
        private readonly IServiceManager _manager;

        public TotalRevenueViewComponent(IServiceManager manager)
        {
            _manager = manager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var totalRevenue = await _manager.OrderService.GetTotalRevenueAsync();
            return View(totalRevenue);
        }
    }
}