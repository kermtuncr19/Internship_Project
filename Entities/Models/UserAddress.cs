using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Entities.Models
{
    public class UserAddress : IValidatableObject
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public IdentityUser User { get; set; } = default!;

        // DB'de tutulacak asıl alanlar
        [Required, StringLength(60)]
        public string FirstName { get; set; } = default!;

        [Required, StringLength(60)]
        public string LastName  { get; set; } = default!;

        // Formun bağlanacağı tek alan (DB'ye yazılmaz)
        [NotMapped]
        [Display(Name = "Ad Soyad")]
        public string RecipientName
        {
            get => $"{FirstName} {LastName}".Trim();
            set
            {
                var s = (value ?? "").Trim();
                if (string.IsNullOrEmpty(s))
                {
                    FirstName = "";
                    LastName = "";
                    return;
                }

                // İlk kelimeyi ad, kalanını soyad yap (çok parçalı soyad destekler)
                var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                FirstName = parts[0];
                LastName  = parts.Length > 1 ? string.Join(' ', parts[1..]) : "";
            }
        }

        public string Label { get; set; } = "Varsayılan";
        [Required] public string City { get; set; } = default!;
        [Required] public string District { get; set; } = default!;
        [Required] public string Neighborhood { get; set; } = default!;
        [Required] public string AddressLine { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Sunucu doğrulaması: tek alan boşsa veya sadece 1 kelimeyse uyarı ver
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                yield return new ValidationResult("Ad kısmı boş olamaz.", new[] { nameof(RecipientName) });

            if (string.IsNullOrWhiteSpace(LastName))
                yield return new ValidationResult("Lütfen soyadınızı da ekleyin.", new[] { nameof(RecipientName) });

            if (!string.IsNullOrEmpty(FirstName) && FirstName.Length > 60)
                yield return new ValidationResult("Ad 60 karakteri geçemez.", new[] { nameof(RecipientName) });

            if (!string.IsNullOrEmpty(LastName) && LastName.Length > 60)
                yield return new ValidationResult("Soyad 60 karakteri geçemez.", new[] { nameof(RecipientName) });
        }
    }
}
