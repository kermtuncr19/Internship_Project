using Microsoft.AspNetCore.Mvc;

namespace StoreApp.Components
{
    public class ProductFilterViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}