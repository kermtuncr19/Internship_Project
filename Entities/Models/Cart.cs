namespace Entities.Models
{
    public class Cart
    {
        public List<CartLine> Lines { get; set; }
        public Cart()
        {
            Lines = new List<CartLine>();
        }

        public virtual void AddItem(Product product, int quantity, string? size)
        {
            // eğer ürün varsa sayısını artır yoksa listeye ekle.
            CartLine? line = Lines.Where(l => l.Product.ProductId == product.ProductId && l.Size == size).FirstOrDefault();

            if (line is null)
            {
                Lines.Add(new CartLine()
                {
                    Product = product,
                    Quantity = quantity,
                    Size = size
                });
            }
            else
            {
                line.Quantity += quantity;
            }


        }
        public virtual void DecrementItem(Product product, string? size, int quantity = 1)
        {
            var line = Lines.FirstOrDefault(p => p.Product.ProductId == product.ProductId && p.Size == size);
            if (line == null) return;

            line.Quantity -= quantity;
            if (line.Quantity <= 0)
                Lines.Remove(line);
        }

        public virtual void RemoveLine(Product product, string? size)
        {
            var line = Lines.FirstOrDefault(l =>
                l.Product.ProductId == product.ProductId &&
                l.Size == size
            );
            if (line is not null)
                Lines.Remove(line);
        }

        public virtual void RemoveLineById(int productId)
        {
            Lines.RemoveAll(l => l.Product.ProductId == productId);
        }


        public decimal ComputeTotalValue() => Lines.Sum(e => e.Product.Price * e.Quantity);

        public virtual void Clear() => Lines.Clear();
    }
}