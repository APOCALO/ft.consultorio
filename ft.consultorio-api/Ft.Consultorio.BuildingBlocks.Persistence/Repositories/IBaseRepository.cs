using Ft.Consultorio.ServiceDefaults.Application.Common;
using System.Linq.Expressions;

namespace Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories
{
    public interface IBaseRepository<T, TId>
        where T : class
    {
        Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
            PaginationParameters paginationParameters,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            bool asNoTracking = true,
            CancellationToken cancellationToken = default);

        Task AddAsync(T entity, CancellationToken cancellationToken);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken);
        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken);
        Task<T?> GetByIdAsync(TId id, bool asNoTracking, CancellationToken cancellationToken);
        Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
        Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> predicate, bool asNoTracking, CancellationToken cancellationToken);
        void Update(T entity);
        void Delete(T entity);
    }
}
