using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Organisation_WebAPI.Dtos.EmployeeDto;
using Organisation_WebAPI.Dtos.ManagerDto;
using Organisation_WebAPI.InputModels;
using Organisation_WebAPI.ViewModels;

namespace Organisation_WebAPI.Services.Managers
{
    public interface IManagerService
    {
        Task<ServiceResponse<PaginationResultVM<GetManagerDto>>> GetAllManagers(PaginationInput paginationInput);
        Task<ServiceResponse<GetManagerDto>> GetManagerById(int id);
        Task<ServiceResponse<List<GetManagerDto>>> AddManager(AddManagerDto newManager);
        Task<ServiceResponse<GetManagerDto>> UpdateManager(UpdateManagerDto updatedManager ,int id);
        Task<ServiceResponse<List<GetManagerDto>>> DeleteManager(int id);
        Task<ServiceResponse<GetManagerDto>> GetManagerByDepartmentId(int departmentId); 
        Task<ServiceResponse<GetEmployeesAndManagerDto>> GetEmployeesAndManagerByDepartmentId(int departmentId);
        Task<ServiceResponse<List<ManagerDepartmentDto>>> GetAllDepartmentsAssociatedWithManager();
    }
}