using Microsoft.AspNetCore.Identity;

namespace Entities.Models
{
    // Entities/Models/UserFavoriteProduct.cs
public class UserFavoriteProduct
{
    public string UserId { get; set; } = default!;
    public IdentityUser User { get; set; } = default!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

}