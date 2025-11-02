using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;

namespace Repositories
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T>
        where T : class, new()
    {
        protected readonly RepositoryContext _context;

        protected RepositoryBase(RepositoryContext context)
        {
            _context = context;
        }

        public IQueryable<T> FindAll(bool trackChange) =>
            trackChange
                ? _context.Set<T>()
                : _context.Set<T>().AsNoTracking();

        // Sorgulanabilir versiyon (koleksiyon)
        public IQueryable<T> QueryByCondition(Expression<Func<T, bool>> expression, bool trackChange) =>
            trackChange
                ? _context.Set<T>().Where(expression)
                : _context.Set<T>().Where(expression).AsNoTracking();

        // Tek kayıt – kolay kullanım
        public T? FindOne(Expression<Func<T, bool>> expression, bool trackChange) =>
            trackChange
                ? _context.Set<T>().SingleOrDefault(expression)
                : _context.Set<T>().AsNoTracking().SingleOrDefault(expression);

        // (Opsiyonel) Geri uyumluluk: tek kayıt döndüren eski isim
        public T? FindByCondition(Expression<Func<T, bool>> expression, bool trackChange) =>
            FindOne(expression, trackChange);

        public void Create(T entity) => _context.Set<T>().Add(entity);
        public void Update(T entity) => _context.Set<T>().Update(entity);
        public void Remove(T entity) => _context.Set<T>().Remove(entity);
    }
}
