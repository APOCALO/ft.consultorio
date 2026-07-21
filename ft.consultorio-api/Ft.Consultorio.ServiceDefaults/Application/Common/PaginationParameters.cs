namespace Ft.Consultorio.ServiceDefaults.Application.Common
{
    public class PaginationParameters
    {
        private const int MaxPageSize = 100;
        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }
        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (value < 1)
                {
                    _pageSize = 1;
                    return;
                }

                _pageSize = value > MaxPageSize ? MaxPageSize : value;
            }
        }
    }
}
