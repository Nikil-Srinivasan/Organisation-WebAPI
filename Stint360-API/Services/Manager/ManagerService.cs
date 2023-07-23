using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Organisation_WebAPI.Data;
using Organisation_WebAPI.Dtos.EmployeeDto;
using Organisation_WebAPI.Dtos.ManagerDto;
using Organisation_WebAPI.InputModels;
using Organisation_WebAPI.Models;
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

        public async Task<ServiceResponse<List<GetManagerDto>>> DeleteManager(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetManagerDto>>();
            try
            {
                var manager = await _context.Managers.FirstOrDefaultAsync(c => c.ManagerId == id);
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

        public async Task<
            ServiceResponse<List<ManagerDepartmentDto>>
        > GetAllDepartmentsAssociatedWithManager()
        {
            var serviceResponse = new ServiceResponse<List<ManagerDepartmentDto>>();
            try
            {
                var dbManagers = await _context.Managers.ToListAsync();
                var managerDepartmentList = dbManagers
                    .Select(
                        m =>
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

        public async Task<ServiceResponse<GetManagerDto>> GetManagerByDepartmentId(int departmentId)
        {
            var serviceResponse = new ServiceResponse<GetManagerDto>();
            try
            {
                var manager = await _context.Managers
                    .Include(m => m.Department)
                    .FirstOrDefaultAsync(m => m.DepartmentID == departmentId);

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

        public async Task<
            ServiceResponse<GetEmployeesAndManagerDto>
        > GetEmployeesAndManagerByDepartmentId(int departmentId)
        {
            var serviceResponse = new ServiceResponse<GetEmployeesAndManagerDto>();
            try
            {
                var manager = await _context.Managers
                    .Include(m => m.Department)
                    .Include(m => m.Employees)
                    .FirstOrDefaultAsync(m => m.DepartmentID == departmentId);
                var department = await _context.Departments.FirstOrDefaultAsync(
                    d => d.DepartmentID == departmentId
                );

                var managerDto = _mapper.Map<GetEmployeesAndManagerDto>(manager);

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

        public async Task<ServiceResponse<GetManagerDto>> GetManagerById(int id)
        {
            var serviceResponse = new ServiceResponse<GetManagerDto>();
            try
            {
                var dbManager = await _context.Managers
                    .Include(m => m.Department) // Eagerly load the associated department
                    .FirstOrDefaultAsync(m => m.ManagerId == id);
                if (dbManager is null)
                    throw new Exception($"Manager with id '{id}' not found");

                serviceResponse.Data = _mapper.Map<GetManagerDto>(dbManager);
                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetManagerDto>> UpdateManager(
            UpdateManagerDto updatedManager,
            int id
        )
        {
            var serviceResponse = new ServiceResponse<GetManagerDto>();
            try
            {
                var manager = await _context.Managers.FirstOrDefaultAsync(c => c.ManagerId == id);

                if (manager is null)
                    throw new Exception($"Manager with id '{id}' not found");

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
