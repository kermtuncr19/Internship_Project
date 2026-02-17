using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductQuestionsController : Controller
    {
        private readonly IServiceManager _manager;
        private readonly UserManager<IdentityUser> _um;

        public ProductQuestionsController(IServiceManager manager, UserManager<IdentityUser> um)
        {
            _manager = manager;
            _um = um;
        }

        public async Task<IActionResult> Index()
        {
            var pending = await _manager.ProductQaService.GetPendingAsync();
            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Answer(int questionId, string answerText)
        {
            var adminId = _um.GetUserId(User)!;

            try
            {
                await _manager.ProductQaService.AnswerAsync(questionId, adminId, answerText);
                TempData["success"] = "Cevap kaydedildi.";
            }
            catch (Exception ex)
            {
                TempData["Danger"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
