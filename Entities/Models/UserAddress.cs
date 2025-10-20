using Microsoft.AspNetCore.Identity;

namespace Entities.Models
{// Entities/Models/UserAddress.cs
public class UserAddress
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public IdentityUser User { get; set; } = default!;

    public string Label { get; set; } = "Varsayılan"; // “Ev”, “İş” gibi
    public string City { get; set; } = default!;
    public string District { get; set; } = default!;
    public string Neighborhood { get; set; } = default!;
    public string AddressLine { get; set; } = default!; // detay
    public string? PhoneNumber { get; set; }

    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

    
}