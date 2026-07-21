namespace Ft.Consultorio.ServiceDefaults.Application.Common
{
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public double ResponseTime { get; set; }
        public PaginationMetadata? Pagination { get; set; }

        public ApiResponse(T data, bool success, PaginationMetadata? pagination = default)
        {
            Data = data;
            Success = success;
            Pagination = pagination;
        }
    }

    public class PaginationMetadata
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}
