using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Components
{
    public class TopSellingProductsViewComponent : ViewComponent
    {
        private readonly IServiceManager _manager;

        public TopSellingProductsViewComponent(IServiceManager manager)
        {
            _manager = manager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var topProducts = await _manager.PoductService.GetTopSellingProductsAsync(5);
            return View(topProducts);
        }
    }
}