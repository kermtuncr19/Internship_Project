// StoreApp/Models/PaymentViewModel.cs
using Entities.Models;

namespace StoreApp.Models
{
    public class PaymentViewModel
    {
        public Order Order { get; set; } = new Order();
        public PaymentInfo PaymentInfo { get; set; } = new PaymentInfo();
        public decimal TotalAmount { get; set; }
        public List<CartLine> CartItems { get; set; } = new List<CartLine>();
    }
}