using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.ServiceDefaults.Application.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> query, PaginationParameters paginationParameters)
        {
            if (paginationParameters is null)
                throw new ArgumentNullException(nameof(paginationParameters));

            return query
                .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
                .Take(paginationParameters.PageSize);
        }
    }
}
