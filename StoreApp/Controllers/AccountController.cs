using Entities.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using StoreApp.Models;

namespace StoreApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Login([FromQuery(Name = "ReturnUrl")] string ReturnUrl = "/")
        {
            return View(new LoginModel()
            {
                ReturnUrl = ReturnUrl
            }
            );
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1) Kullanıcının girdiği değer (Identifier varsa onu, yoksa Email’i kullan)
            var identifier = (model.GetType().GetProperty("Identifier") != null)
                ? (string?)model.GetType().GetProperty("Identifier")!.GetValue(model)
                : model.GetType().GetProperty("Email")?.GetValue(model) as string;

            identifier = identifier?.Trim();

            if (string.IsNullOrWhiteSpace(identifier))
            {
                ModelState.AddModelError(string.Empty, "E-posta veya kullanıcı adı gerekli.");
                return View(model);
            }

            // 2) Kullanıcıyı bul (önce @ varsa email, yoksa username; bulunamazsa tersini dene)
            IdentityUser? user = null;

            if (identifier.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(identifier);
                if (user is null)
                    user = await _userManager.FindByNameAsync(identifier);
            }
            else
            {
                user = await _userManager.FindByNameAsync(identifier);
                if (user is null)
                    user = await _userManager.FindByEmailAsync(identifier);
            }

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz giriş bilgileri.");
                return View(model);
            }

            // 3) Şifre kontrolü ve oturum açma
            await _signInManager.SignOutAsync();

            // RememberMe varsa kullan; yoksa false
            bool rememberMe = (bool?)model.GetType().GetProperty("RememberMe")?.GetValue(model) ?? false;

            var password = model.GetType().GetProperty("Password")?.GetValue(model) as string ?? string.Empty;

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, password, rememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var returnUrl = model.ReturnUrl;
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Hesap geçici olarak kilitlendi.");
                return View(model);
            }

            // 2FA, email confirmation gibi ek akışlar varsa burada yönetin
            ModelState.AddModelError(string.Empty, "Geçersiz giriş bilgileri.");
            return View(model);
        }

        public async Task<IActionResult> Logout([FromQuery(Name = "ReturnUrl")] string ReturnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(ReturnUrl);
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                // Model geçersizse (örneğin şifre boşsa) formu aynı sayfada uyarıyla göster
                return View(model);
            }
            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email,
            };
            var result = await _userManager
                .CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleResult = await _userManager
                    .AddToRoleAsync(user, "User");

                if (roleResult.Succeeded)
                    return RedirectToAction("Login", new { ReturnUrl = "/" });
            }
            else
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View();
        }

        public IActionResult AccessDenied([FromQuery(Name = "ReturnUrl")] string ReturnUrl)
        {
            return View();
        }
    }

}