using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Components
{
    public class SalesChartViewComponent : ViewComponent
    {
        private readonly IServiceManager _manager;

        public SalesChartViewComponent(IServiceManager manager)
        {
            _manager = manager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var salesData = await _manager.OrderService.GetMonthlySalesDataAsync();
            return View(salesData);
        }
    }
}