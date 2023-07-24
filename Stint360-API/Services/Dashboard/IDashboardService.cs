


namespace Organisation_WebAPI.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<ServiceResponse<Dictionary<string,int>>> GetTotalEmployeeCount();
        Task<ServiceResponse<Dictionary<Status,int>>> GetEmployeeTaskCount(int id);
        Task<ServiceResponse<Dictionary<Status,int>>> GetEmployeeTasksByManager(int id);
    }
}