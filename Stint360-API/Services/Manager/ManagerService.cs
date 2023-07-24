using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Organisation_WebAPI.Data;
using Organisation_WebAPI.Dtos.EmployeeDto;
using Organisation_WebAPI.Dtos.ManagerDto;
using Organisation_WebAPI.InputModels;
using Organisation_WebAPI.Services.Pagination;
using Organisation_WebAPI.ViewModels;

namespace Organisation_WebAPI.Services.Managers
{
    public class ManagerService : IManagerService
    {
        private readonly IMapper _mapper; // Provides object-object mapping
        private readonly OrganizationContext _context; // Represents the database context
        private readonly IPaginationServices<GetManagerDto, GetManagerDto> _paginationServices; //Provides Pagination service
        private readonly IHttpContextAccessor _httpContextAccessor; // Provides HttpContextAccessor Serivce

        public ManagerService(
            OrganizationContext context,
            IMapper mapper,
            IPaginationServices<GetManagerDto, GetManagerDto> paginationServices,
            IHttpContextAccessor httpContextAccessor
        )
        { 
            _mapper = mapper;
            _context = context;
            _paginationServices = paginationServices;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId() =>
            int.Parse(
                _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

        // Adds a new Manager to the database
        public async Task<ServiceResponse<List<GetManagerDto>>> AddManager(AddManagerDto newManager)
        {
            var serviceResponse = new ServiceResponse<List<GetManagerDto>>();
            try
            {
                var manager = _mapper.Map<Manager>(newManager);
                _context.Managers.Add(manager);
                await _context.SaveChangesAsync();
                serviceResponse.Data = await _context.Managers
                    .Select(c => _mapper.Map<GetManagerDto>(c))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        //Delete a Manager based on managerId
        public async Task<ServiceResponse<List<GetManagerDto>>> DeleteManager(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetManagerDto>>();
            try
            {   
                //Fetch manager record based on managerId
                var manager = await _context.Managers.FirstOrDefaultAsync(c => c.ManagerId == id);

                //If manager is null throw exception of managerId not found
                if (manager is null)
                    throw new Exception($"Manager with id '{id}' not found");

                _context.Managers.Remove(manager);
                await _context.SaveChangesAsync();
                serviceResponse.Data = _context.Managers
                    .Select(c => _mapper.Map<GetManagerDto>(c))
                    .ToList();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        //Retrieves all managers from database
        public async Task<ServiceResponse<PaginationResultVM<GetManagerDto>>> GetAllManagers(
            PaginationInput paginationInput
        )
        {
            var serviceResponse = new ServiceResponse<PaginationResultVM<GetManagerDto>>();
            try
            {
                var dbManagers = await _context.Managers.ToListAsync();
                var managerDTOs = dbManagers
                    .Select(e =>
                    {
                        var managerDTO = _mapper.Map<GetManagerDto>(e);
                        managerDTO.DepartmentName = _context.Departments
                            .FirstOrDefault(d => d.DepartmentID == e.DepartmentID)
                            ?.DepartmentName;
                        return managerDTO;
                    })
                    .ToList();
                var managers = _mapper.Map<List<GetManagerDto>>(managerDTOs);

                var result = _paginationServices.GetPagination(managers, paginationInput);

                serviceResponse.Data = result;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        //Retrieves all departments from database based on appointed managers
        public async Task<ServiceResponse<List<ManagerDepartmentDto>>> GetAllDepartmentsAssociatedWithManager()
        {
            var serviceResponse = new ServiceResponse<List<ManagerDepartmentDto>>();
            try
            {
                var dbManagers = await _context.Managers.ToListAsync();
                var managerDepartmentList = dbManagers.Select(m =>
                            new ManagerDepartmentDto
                            {
                                ManagerId = m.ManagerId,
                                DepartmentName = _context.Departments
                                    .FirstOrDefault(d => d.DepartmentID == m.DepartmentID)
                                    ?.DepartmentName,
                                IsAppointed = m.IsAppointed
                            }
                    )
                    .ToList();

                serviceResponse.Data = managerDepartmentList;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        //Retrieve an manager record from database based on departmentId
        public async Task<ServiceResponse<GetManagerDto>> GetManagerByDepartmentId(int departmentId)
        {
            var serviceResponse = new ServiceResponse<GetManagerDto>();
            try
            {   
                //Fetch manager record based on departmentId
                var manager = await _context.Managers
                    .Include(m => m.Department)
                    .FirstOrDefaultAsync(m => m.DepartmentID == departmentId);

                //If manager is null return service response as false with message
                if (manager == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Manager not found.";
                    return serviceResponse;
                }

                var managerDto = _mapper.Map<GetManagerDto>(manager);
                managerDto.DepartmentName = manager.Department?.DepartmentName;

                serviceResponse.Data = managerDto;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        //Retrieves all employees and manager based on departmentId
        public async Task<ServiceResponse<GetEmployeesAndManagerDto>> GetEmployeesAndManagerByDepartmentId(int departmentId)
        {
            var serviceResponse = new ServiceResponse<GetEmployeesAndManagerDto>();
            try
            {   
                // Retrieve manager with the specified departmentId including their associated department and employees.
                var manager = await _context.Managers
                    .Include(m => m.Department)
                    .Include(m => m.Employees)
                    .FirstOrDefaultAsync(m => m.DepartmentID == departmentId);
                var department = await _context.Departments.FirstOrDefaultAsync(
                    d => d.DepartmentID == departmentId
                );

                var managerDto = _mapper.Map<GetEmployeesAndManagerDto>(manager);

                //If manager is null throw exception of managerId not found
                if (manager == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Manager not found.";
                    return serviceResponse;
                }

                // Retrieve the department name using the department ID
                if (department != null)
                {
                    managerDto.DepartmentName = department.DepartmentName;
                }

                serviceResponse.Data = managerDto;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        //Retrieve manager from database by managerId
        public async Task<ServiceResponse<GetManagerDto>> GetManagerById(int id)
        {
            var serviceResponse = new ServiceResponse<GetManagerDto>();
            try
            {
                var manager = await _context.Managers
                    .Include(m => m.Department) // Eagerly load the associated department
                    .FirstOrDefaultAsync(m => m.ManagerId == id);

                //If manager is null throw exception of employeeId not found    
                if (manager is null)
                    throw new Exception($"Manager with id '{id}' not found");

                serviceResponse.Data = _mapper.Map<GetManagerDto>(manager);
                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        //Update Manager record based on managerId
        public async Task<ServiceResponse<GetManagerDto>> UpdateManager(UpdateManagerDto updatedManager,int id)
        {
            var serviceResponse = new ServiceResponse<GetManagerDto>();
            try
            {
                var manager = await _context.Managers.FirstOrDefaultAsync(c => c.ManagerId == id);

                //If manager is null throw exception of managerId not found
                if (manager is null)
                    throw new Exception($"Manager with id '{id}' not found");
                
                //Update the manager details to the current manager
                manager.ManagerName = updatedManager.ManagerName;
                manager.ManagerSalary = updatedManager.ManagerSalary;
                manager.ManagerAge = updatedManager.ManagerAge;
                manager.Address = updatedManager.Address;
                manager.Phone = updatedManager.Phone;

                await _context.SaveChangesAsync();
                serviceResponse.Data = _mapper.Map<GetManagerDto>(manager);

                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }
    }
}
