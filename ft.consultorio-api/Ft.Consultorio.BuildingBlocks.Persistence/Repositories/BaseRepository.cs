using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Application.Extensions;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Persistence.Repositories
{
    public class BaseRepository<T, TId> : IBaseRepository<T, TId>
        where T : class
    {
        private readonly IApplicationDbContext _dbContext;

        public BaseRepository(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
        {
            if (entities is null || !entities.Any()) throw new ArgumentNullException(nameof(entities));

            await _dbContext.Set<T>().AddRangeAsync(entities, cancellationToken);
        }

        public void Delete(T entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _dbContext.Set<T>().Remove(entity);
        }

        public async Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            return await FindByConditionAsync(predicate, asNoTracking: false, cancellationToken);
        }

        public async Task<List<T>> FindByConditionAsync(
            Expression<Func<T, bool>> predicate,
            bool asNoTracking,
            CancellationToken cancellationToken)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            IQueryable<T> query = _dbContext.Set<T>();
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }
            
        public async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken)
        {
            return await GetByIdAsync(id, asNoTracking: false, cancellationToken);
        }

        public async Task<T?> GetByIdAsync(TId id, bool asNoTracking, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<T>()
                .FindAsync(new object?[] { id }, cancellationToken);

            if (entity is null || !asNoTracking)
            {
                return entity;
            }

            // Detach to avoid tracking overhead for read-only queries.
            _dbContext.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
            PaginationParameters paginationParameters,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            bool asNoTracking = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (filter is not null)
                query = query.Where(filter);

            // El total es del query filtrado.
            var totalCount = await query.CountAsync(cancellationToken);

            if (include is not null)
                query = include(query);

            if (orderBy is not null)
            {
                query = orderBy(query);
            }
            else
            {
                // Fallback: ordenar por PK si existe, para orden estable.
                var entityType = _dbContext.Model.FindEntityType(typeof(T));
                var keyName = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;
                if (!string.IsNullOrEmpty(keyName))
                {
                    query = query.OrderBy(e => EF.Property<object>(e, keyName!));
                }
            }

            var items = await query
                .Paginate(paginationParameters)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            _dbContext.Entry(entity).State = EntityState.Modified;
        }
    }
}
