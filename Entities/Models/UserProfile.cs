using Microsoft.AspNetCore.Identity;

namespace Entities.Models
{
    // Entities/Models/UserProfile.cs
public class UserProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public IdentityUser User { get; set; } = default!;

    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? PhoneNumber { get; set; }  // Order’da da var; profil için de tutmak isteyebilirsiniz.
}

}