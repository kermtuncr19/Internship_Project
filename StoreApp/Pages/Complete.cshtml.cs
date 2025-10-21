using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;   // <-- Include için
using Repositories;

public class CompleteModel : PageModel
{
    private readonly RepositoryContext _db;
    public CompleteModel(RepositoryContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public int OrderId { get; set; }

    public Order Order { get; private set; } = default!;

    public IActionResult OnGet()
    {
        Order = _db.Orders
            .Include(o => o.Lines)              // satırlar
                .ThenInclude(l => l.Product)    // ürünleri
            .AsNoTracking()
            .FirstOrDefault(o => o.OrderId == OrderId);

        if (Order == null)
            return RedirectToPage("/Index");

        return Page();
    }
}
