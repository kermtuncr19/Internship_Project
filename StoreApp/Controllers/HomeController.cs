using Services.Contracts;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly IServiceManager _manager;
    public HomeController(IServiceManager manager) { _manager = manager; }

    public IActionResult Index()
    {
        ViewData["Title"] = "Hoşgeldiniz";
        ViewBag.Categories = _manager.CategoryService.GetAllCategories(false); // IEnumerable<Category>
        ViewBag.ActiveCategoryId = null; // ana sayfada “Tümü” aktif
        return View();
    }
}