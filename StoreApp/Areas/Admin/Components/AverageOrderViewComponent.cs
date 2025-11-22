using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Components
{
    public class AverageOrderViewComponent : ViewComponent
    {
        private readonly IServiceManager _manager;

        public AverageOrderViewComponent(IServiceManager manager)
        {
            _manager = manager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var avgOrder = await _manager.OrderService.GetAverageOrderValueAsync();
            return View(avgOrder);
        }
    }
}