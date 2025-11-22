using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Components
{
    public class MonthlySalesViewComponent : ViewComponent
    {
        private readonly IServiceManager _manager;

        public MonthlySalesViewComponent(IServiceManager manager)
        {
            _manager = manager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var monthlySales = await _manager.OrderService.GetMonthlySalesAsync();
            return View(monthlySales);
        }
    }
}