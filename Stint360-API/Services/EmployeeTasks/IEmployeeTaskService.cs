using Organisation_WebAPI.Dtos.EmployeeTaskDto;
using Organisation_WebAPI.InputModels;
using Organisation_WebAPI.ViewModels;

namespace Organisation_WebAPI.Services.EmployeeTasks
{
    public interface IEmployeeTaskService
    {
        Task<ServiceResponse<List<GetEmployeeTaskDto>>> GetAllEmployeeTasks();
        Task<ServiceResponse<GetEmployeeTaskDto>> GetEmployeeTasksById(int id);
        Task<ServiceResponse<PaginationResultVM<GetEmployeeTaskDto>>> GetAllEmployeeTasksByEmployeeId(int managerid,int employeeid,PaginationInput paginationInput);
        Task<ServiceResponse<List<GetEmployeeTaskDto>>> GetEmployeeNewTaskByEmployeeId(int id);
        Task<ServiceResponse<List<GetEmployeeTaskDto>>> GetEmployeeInProgressTaskByEmployeeId(int id);
        Task<ServiceResponse<List<GetEmployeeTaskDto>>> GetEmployeeCompletedTaskByEmployeeId(int id);
        Task<ServiceResponse<List<GetEmployeeTaskDto>>> GetEmployeePendingTaskByEmployeeId(int id);
        Task<ServiceResponse<int>>  CalculateNewEmployeeTasksByEmployeeId(int employeeId);
        Task<ServiceResponse<List<GetEmployeeTaskDto>>> AddEmployeeTask(AddEmployeeTaskDto addEmployeeTask);
        Task<ServiceResponse<GetEmployeeTaskDto>> UpdateEmployeeTask(UpdateEmployeeTaskDto updateEmployeeTask,int id);
        Task<ServiceResponse<GetEmployeeTaskDto>> UpdateEmployeeTaskStatus(UpdateEmployeeTaskStatusDto updateEmployeeTaskStatus,int id);

        Task<ServiceResponse<List<GetEmployeeTaskDto>>> DeleteEmployeeTask(int id);

    }
}