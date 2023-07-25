using Organisation_WebAPI.InputModels;
using Organisation_WebAPI.ViewModels;

namespace Organisation_WebAPI.Services.Pagination
{
    public interface IPaginationServices<T, R> where T : class
    {
        PaginationResultVM<T> GetPagination(List<R> source, PaginationInput paginationInput);
    }
}
