using System.ComponentModel.DataAnnotations;

namespace StoreApp.Models
{
    public class LoginModel
    {
        private string? _returnUrl;
        [Required(ErrorMessage = "E-Posta Gerekli!")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Şifre Gerekli!")]
        public string? Password { get; set; }

        public string ReturnUrl
        {
            get
            {
                if (_returnUrl is null)
                    return "/";
                else
                    return _returnUrl;
            }
            set
            {
                _returnUrl = value;
            }
        }

    }
}