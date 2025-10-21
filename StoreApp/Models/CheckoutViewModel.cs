using Entities.Models;

namespace StoreApp.Models
{
    /// <summary>
    /// Checkout sayfası için view model
    /// Hem sipariş bilgilerini hem de kullanıcının kayıtlı adreslerini taşır
    /// </summary>
    public class CheckoutViewModel
    {
        /// <summary>
        /// Oluşturulacak sipariş bilgileri
        /// </summary>
        public Order? Order { get; set; }

        /// <summary>
        /// Kullanıcının kayıtlı adresleri
        /// </summary>
        public List<UserAddress> SavedAddresses { get; set; } = new List<UserAddress>();
    }
}