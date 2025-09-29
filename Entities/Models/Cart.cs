namespace Entities.Models
{
    public class Cart
    {
        public List<CartLine> Lines { get; set; }
        public Cart()
        {
            Lines = new List<CartLine>();
        }

        public void AddItem(Product product, int quantity)
        {
            // eğer ürün varsa sayısını artır yoksa listeye ekle.
            CartLine? line = Lines.Where(l => l.Product.ProductId == product.ProductId).FirstOrDefault();

            if (line is null)
            {
                Lines.Add(new CartLine()
                {
                    Product = product,
                    Quantity = quantity
                });
            }
            else
            {
                line.Quantity += quantity;
            }


        }
        public void DecrementItem(Product product, int quantity = 1)
        {
            var line = Lines.FirstOrDefault(p => p.Product.ProductId == product.ProductId);
            if (line == null) return;

            line.Quantity -= quantity;
            if (line.Quantity <= 0)
                Lines.Remove(line);
        }

        public void RemoveLine(Product product) => Lines.RemoveAll(l => l.Product.ProductId.Equals(product.ProductId));
        
        public void RemoveLineById(int productId)
        {
            Lines.RemoveAll(l => l.Product.ProductId == productId);
        }


        public decimal ComputeTotalValue() => Lines.Sum(e => e.Product.Price * e.Quantity);

        public void Clear() => Lines.Clear();
    }
}