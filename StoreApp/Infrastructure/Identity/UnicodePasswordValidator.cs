using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class UnicodePasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
{
    // En az 1 Unicode küçük, 1 Unicode büyük, 1 rakam; boşluk YOK
    private static readonly Regex Rule =
        new Regex(@"^(?=.*\p{Ll})(?=.*\p{Lu})(?=.*\d)[^\s]+$",
                  RegexOptions.CultureInvariant);

    public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Şifre boş olamaz." }));

        if (password.Length < 8)
            return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Şifre en az 8 karakter olmalı." }));

        if (!Rule.IsMatch(password))
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Description = "Şifre en az bir küçük harf, bir büyük harf ve bir rakam içermeli; boşluk içeremez."
            }));
        }

        return Task.FromResult(IdentityResult.Success);
    }
}
