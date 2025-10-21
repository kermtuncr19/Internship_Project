using Entities.Dto;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View(); // Views/Account/ResetPassword.cshtml
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Yeni şifreler eşleşmiyor.");
                return View();
            }

            // 🔸 RegisterDto’daki regex ile aynı kontrolü uygula
            var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*\p{Ll})(?=.*\p{Lu})(?=.*\d)[^\s]+$");
            if (newPassword.Length < 8 || !passwordRegex.IsMatch(newPassword))
            {
                ModelState.AddModelError("", "Şifre en az 8 karakter olmalı, bir küçük, bir büyük harf ve bir rakam içermeli; boşluk içeremez.");
                return View();
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction("Index", "Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }

    }

}