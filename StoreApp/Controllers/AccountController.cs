using Entities.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using StoreApp.Models;
using StoreApp.Services;

namespace StoreApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
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
        public IActionResult ChangePassword()
        {
            return View(); // Views/Account/ResetPassword.cshtml
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
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

        // Şifremi Unuttum - Form Gösterme
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Şifremi Unuttum - Token Gönderme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Güvenlik: Kullanıcı bulunamasa bile başarılı mesajı göster
            // (Hesap varlığı bilgisi sızdırma)
            if (user == null)
            {
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            // Şifre sıfırlama token'ı oluştur
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Şifre sıfırlama linki oluştur
            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { email = model.Email, token = token },
                protocol: Request.Scheme);

            // E-posta içeriği
            var emailBody = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background: linear-gradient(135deg, #001f54 0%, #ffea00 100%); 
                           padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                .header h1 {{ color: white; margin: 0; }}
                .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                .button {{ display: inline-block; padding: 12px 30px; background: #001f54; 
                          color: white; text-decoration: none; border-radius: 5px; 
                          margin: 20px 0; font-weight: bold; }}
                .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>🔐 Şifre Sıfırlama</h1>
                </div>
                <div class='content'>
                    <h2>Merhaba,</h2>
                    <p>Hesabınız için şifre sıfırlama talebinde bulundunuz.</p>
                    <p>Şifrenizi sıfırlamak için aşağıdaki butona tıklayın:</p>
                    <p style='text-align: center;'>
                        <a href='{resetLink}' class='button'>Şifremi Sıfırla</a>
                    </p>
                    <p><strong>Önemli:</strong> Bu link 1 saat geçerlidir.</p>
                    <p>Eğer bu talebi siz oluşturmadıysanız, bu e-postayı görmezden gelebilirsiniz.</p>
                    <hr>
                    <p style='font-size: 12px; color: #666;'>
                        Buton çalışmıyorsa aşağıdaki linki tarayıcınıza kopyalayın:<br>
                        <a href='{resetLink}'>{resetLink}</a>
                    </p>
                </div>
                <div class='footer'>
                    <p>© 2025 Fenerium - Tüm hakları saklıdır</p>
                </div>
            </div>
        </body>
        </html>";

            try
            {
                await _emailService.SendEmailAsync(
                    model.Email,
                    "Şifre Sıfırlama - Fenerium",
                    emailBody);
            }
            catch (Exception ex)
            {
                // Log hatası
                ModelState.AddModelError("", "E-posta gönderilirken bir hata oluştu. Lütfen tekrar deneyin.");
                return View(model);
            }

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // Onay Sayfası
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // Şifre Sıfırlama Formu
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;

            return View();
        }

        // Şifre Sıfırlama İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(
    [FromForm] string email,
    [FromForm] string token,
    [FromForm] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Email = email;
                ViewBag.Token = token;
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View(model);
        }

        // Şifre Sıfırlama Başarılı
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

    }

}