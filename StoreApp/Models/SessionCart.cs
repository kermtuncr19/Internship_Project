using System.Text.Json.Serialization;
using Entities.Models;
using StoreApp.Infrastructure.Extensions;

namespace StoreApp.Models
{
    public class SessionCart : Cart
    {
        [JsonIgnore]
        public ISession? Session { get; set; }
        public static Cart GetCart(IServiceProvider services)
        {
            ISession? session = services.GetRequiredService<IHttpContextAccessor>().HttpContext?.Session;

            SessionCart cart = session?.GetJson<SessionCart>("cart") ?? new SessionCart();
            cart.Session = session;
            return cart;

        }
        public override void AddItem(Product product, int quantity, string? size)
        {
            base.AddItem(product, quantity, size);
            Session?.SetJson("cart", this);
        }


        public override void Clear()
        {
            base.Clear();
            Session?.Remove("cart");
        }
        public override void RemoveLine(Product product, string? size)
        {
            base.RemoveLine(product, size);
            Session?.SetJson<SessionCart>("cart", this);
        }
        public override void DecrementItem(Product product, string? size, int quantity = 1)
        {
            base.DecrementItem(product, size, quantity);
            Session?.SetJson<SessionCart>("cart", this);
        }
        public override void RemoveLineById(int productId)
        {
            base.RemoveLineById(productId);
            Session?.SetJson<SessionCart>("cart", this);
        }

        public void ChangeSize(int productId, string oldSize, string newSize)
        {
            var oldLine = Lines.FirstOrDefault(l =>
                l.Product.ProductId == productId &&
                string.Equals(l.Size ?? "", oldSize ?? "", StringComparison.OrdinalIgnoreCase));

            if (oldLine != null)
            {
                // Eğer aynı ürün yeni bedende zaten varsa miktarları birleştir
                var existing = Lines.FirstOrDefault(l =>
                    l.Product.ProductId == productId &&
                    string.Equals(l.Size ?? "", newSize ?? "", StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.Quantity += oldLine.Quantity;
                    Lines.Remove(oldLine);
                }
                else
                {
                    oldLine.Size = newSize;
                }

                // 🔥 Güncellenmiş sepeti session’a yaz
                Session?.SetJson("cart", this);
            }
        }

    }
}