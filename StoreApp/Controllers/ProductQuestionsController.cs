using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Controllers
{
    [Authorize]
    public class ProductQuestionsController : Controller
    {
        private readonly IServiceManager _manager;
        private readonly UserManager<IdentityUser> _um;

        public ProductQuestionsController(IServiceManager manager, UserManager<IdentityUser> um)
        {
            _manager = manager;
            _um = um;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, string questionText, string? returnUrl = null)
        {
            var userId = _um.GetUserId(User)!;

            try
            {
                await _manager.ProductQaService.CreateQuestionAsync(productId, userId, questionText);
                TempData["QaSuccess"] = "Sorunuz satıcıya iletildi. Cevaplandığında burada görünecek.";
            }
            catch (Exception ex)
            {
                TempData["QaError"] = ex.Message;
            }

            if (!string.IsNullOrWhiteSpace(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Get", "Product", new { id = productId });
        }
    }
}
