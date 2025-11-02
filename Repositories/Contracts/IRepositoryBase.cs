using System.Linq.Expressions;

namespace Repositories.Contracts
{
    public interface IRepositoryBase<T> where T : class, new()
    {
        // Koleksiyon (sorgulanabilir) dönenler
        IQueryable<T> FindAll(bool trackChange);
        IQueryable<T> QueryByCondition(Expression<Func<T, bool>> expression, bool trackChange);

        // Tek kayıt dönen (kolay kullanım)
        T? FindOne(Expression<Func<T, bool>> expression, bool trackChange);

        // (İstersen geri uyumluluk için tek kayıt döndüren FindByCondition da ekleyebilirsin)
        T? FindByCondition(Expression<Func<T, bool>> expression, bool trackChange);

        void Create(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}
