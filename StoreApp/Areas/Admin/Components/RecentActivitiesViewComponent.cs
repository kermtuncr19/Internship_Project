using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Components
{
    public class RecentActivitiesViewComponent : ViewComponent
    {
        private readonly IServiceManager _manager;

        public RecentActivitiesViewComponent(IServiceManager manager)
        {
            _manager = manager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var activities = await _manager.AuthService.GetRecentActivitiesAsync(10);
            return View(activities);
        }
    }
}