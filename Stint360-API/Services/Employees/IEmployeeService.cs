using Organisation_WebAPI.Dtos.EmployeeDto;
using Organisation_WebAPI.InputModels;
using Organisation_WebAPI.ViewModels;

namespace Organisation_WebAPI.Services.Employees
{
    public interface IEmployeeService
    {
        Task<ServiceResponse<PaginationResultVM<GetEmployeeDto>>> GetAllEmployees(PaginationInput paginationInput);
        Task<ServiceResponse<GetEmployeeDto>> GetEmployeeById(int id);
        Task<ServiceResponse<List<GetEmployeeDto>>> AddEmployee(AddEmployeeDto newDepartment);
        Task<ServiceResponse<UpdateEmployeeDto>> UpdateEmployee(UpdateEmployeeDto department,int id);
        Task<ServiceResponse<List<GetEmployeeDto>>> DeleteEmployee(int id);
        Task<ServiceResponse<List<GetEmployeeDto>>> GetAllEmployeesByManagerId(int managerId);
    }
}