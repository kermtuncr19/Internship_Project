using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Contracts;
using StoreApp.Infrastructure.Extensions;
using System.Linq;

namespace StoreApp.Pages
{
    public class CartModel : PageModel
    {
        private readonly IServiceManager _manager;

        public Cart Cart { get; set; } //IoC
        public string ReturnUrl { get; set; } = "/";

        public CartModel(IServiceManager manager, Cart cartService)
        {
            _manager = manager;
            Cart = cartService;
        }


        public void OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl ?? "/";

        }

        public IActionResult OnPost(int productId, string returnUrl, string? size)
        {
            Product? product = _manager.PoductService.GetOneProduct(productId, false);

            if (product is null) return RedirectToPage(new { returnUrl });

            // Sunucu tarafı koruması: beden zorunluysa boş gelmesin
            if (product.RequiresSize && string.IsNullOrWhiteSpace(size))
            {
                TempData["CartError"] = "Lütfen beden seçin.";
                return RedirectToAction("Get", "Product", new { id = productId });
            }

            Cart.AddItem(product, 1, size);
            return RedirectToPage(new { returnUrl });
        }

        public IActionResult OnPostRemove(int id, string returnUrl, string? size)
        {
            var product = _manager.PoductService.GetOneProduct(id, trackChanges: false);
            if (product is not null)
            {
                // 🔹 sadece bu beden satırını sil
                Cart.RemoveLine(product, size);
            }
            return RedirectToPage(new { returnUrl });
        }


        public IActionResult OnPostIncrement(int id, string returnUrl, string? size)
        {
            var product = _manager.PoductService.GetOneProduct(id, trackChanges: false);
            if (product is not null)

                Cart.AddItem(product, 1, size);


            return RedirectToPage(new { returnUrl });
        }

        public IActionResult OnPostDecrement(int id, string returnUrl, string? size)
        {
            var product = _manager.PoductService.GetOneProduct(id, trackChanges: false);
            if (product is not null)

                Cart.DecrementItem(product, size, 1);

            return RedirectToPage(new { returnUrl });
        }
    }
}