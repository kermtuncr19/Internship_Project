using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Components
{
    public class WeeklySalesViewComponent : ViewComponent
    {
        private readonly IServiceManager _manager;

        public WeeklySalesViewComponent(IServiceManager manager)
        {
            _manager = manager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var weeklySales = await _manager.OrderService.GetWeeklySalesAsync();
            return View(weeklySales);
        }
    }
}