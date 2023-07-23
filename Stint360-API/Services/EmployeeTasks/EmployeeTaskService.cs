using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EmailService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Organisation_WebAPI.Data;
using Organisation_WebAPI.Dtos.EmployeeDto;
using Organisation_WebAPI.Dtos.EmployeeTaskDto;
using Organisation_WebAPI.InputModels;
using Organisation_WebAPI.Models;
using Organisation_WebAPI.Services.Pagination;
using Organisation_WebAPI.ViewModels;

namespace Organisation_WebAPI.Services.EmployeeTasks
{
    public class EmployeeTaskService : IEmployeeTaskService
    {
        private readonly IMapper _mapper; // Provides object-object mapping
        private readonly OrganizationContext _context; // Represents the database context
        private readonly IEmailSender _emailSender;
        private readonly IPaginationServices<
            GetEmployeeTaskDto,
            GetEmployeeTaskDto
        > _paginationServices;

        public EmployeeTaskService(
            IMapper mapper,
            OrganizationContext context,
            IEmailSender emailSender,
            IPaginationServices<GetEmployeeTaskDto, GetEmployeeTaskDto> paginationServices
        )
        {
            _mapper = mapper; //Injects Mapper instance
            _context = context; //Injects DbContext instance
            _emailSender = emailSender; //Injects emailSender instance
            _paginationServices = paginationServices; // Injects pagination service instance
        }

        public async Task<ServiceResponse<List<GetEmployeeTaskDto>>> AddEmployeeTask(
            [FromBody] AddEmployeeTaskDto addEmployeeTask
        )
        {
            var serviceResponse = new ServiceResponse<List<GetEmployeeTaskDto>>();

            var ExistingEmployee = await _context.Employees.FirstOrDefaultAsync(
                e => e.EmployeeID == addEmployeeTask.EmployeeId
            );

            try
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(
                    u => u.EmployeeID == addEmployeeTask.EmployeeId
                );

                if (employee is null)
                {
                    throw new Exception($"Employee not found");
                }
                var employeeTask = _mapper.Map<EmployeeTask>(addEmployeeTask);
                employeeTask.TaskCreatedDate = DateTime.Now;

                _context.EmployeeTasks.Add(employeeTask);

                await _context.SaveChangesAsync();

                var employeeMessage = new Message(
                    new string[] { employee.Email },
                    "New Task Assignment",
                    $"Dear {employee.EmployeeName},\n\nYou have been assigned a new task:\n\nTask Description:"
                        + $" {addEmployeeTask.TaskDescription}\nStart Date: {addEmployeeTask.TaskCreatedDate}\nEnd Date: "
                        + $"{addEmployeeTask.TaskDueDate}\n\nPlease take necessary actions accordingly.\n\nThank you!"
                );

                _emailSender.SendEmail(employeeMessage);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        // Delete employeetask based on its EmployeeTaskId
        public async Task<ServiceResponse<List<GetEmployeeTaskDto>>> DeleteEmployeeTask(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetEmployeeTaskDto>>();
            try
            {
                //Fetch an employeeTask with employeeTaskId
                var employeeTask = await _context.EmployeeTasks.FirstOrDefaultAsync(
                    c => c.TaskID == id
                );

                //If employeeTask is null throw exception of employeeTaskId not found
                if (employeeTask is null)
                    throw new Exception($"EmployeeTask with id '{id}' not found");

                //Remove employeeTask from the database
                _context.EmployeeTasks.Remove(employeeTask);
                await _context.SaveChangesAsync();
                serviceResponse.Data = _context.EmployeeTasks
                    .Select(c => _mapper.Map<GetEmployeeTaskDto>(c))
                    .ToList();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        //Retrieves all employeeTasks from the database
        public async Task<ServiceResponse<List<GetEmployeeTaskDto>>> GetAllEmployeeTasks()
        {
            var serviceResponse = new ServiceResponse<List<GetEmployeeTaskDto>>();
            try
            {
                //Fetch all records of employeeTasks from database
                var dbEmployeeTasks = await _context.EmployeeTasks.ToListAsync();

                //Calculate DueDate deadline with currentDate
                var currentDate = DateTime.Today;
                foreach (var employeeTask in dbEmployeeTasks)
                {
                    DateTime TaskDueDate = (DateTime)employeeTask.TaskDueDate!;
                    DateTime dueDate = TaskDueDate.Date;
                    if (dueDate < currentDate)
                    {
                        employeeTask.TaskStatus = Status.Pending;
                        _context.EmployeeTasks.Update(employeeTask);
                    }
                }
                await _context.SaveChangesAsync();
                serviceResponse.Data = dbEmployeeTasks
                    .Select(c => _mapper.Map<GetEmployeeTaskDto>(c))
                    .ToList();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        //Retrieves an employeeTask with employeetaskId
        public async Task<ServiceResponse<GetEmployeeTaskDto>> GetEmployeeTasksById(int id)
        {
            var serviceResponse = new ServiceResponse<GetEmployeeTaskDto>();
            try
            {
                var dbEmployeeTask = await _context.EmployeeTasks.FirstOrDefaultAsync(
                    c => c.TaskID == id
                );
                if (dbEmployeeTask is null)
                    throw new Exception($"EmployeeTask with id '{id}' not found");

                serviceResponse.Data = _mapper.Map<GetEmployeeTaskDto>(dbEmployeeTask);
                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetEmployeeTaskDto>> UpdateEmployeeTask(
            UpdateEmployeeTaskDto updateEmployeeTask,
            int id
        )
        {
            var serviceResponse = new ServiceResponse<GetEmployeeTaskDto>();
            try
            {
                var employeeTask = await _context.EmployeeTasks.FirstOrDefaultAsync(
                    c => c.TaskID == id
                );
                if (employeeTask is null)
                    throw new Exception($"EmployeeTask with id '{id}' not found");

                var ExistingEmployee = await _context.Employees.FirstOrDefaultAsync(
                    e => e.EmployeeID == updateEmployeeTask.EmployeeId
                );

                if (ExistingEmployee is null)
                    throw new Exception(
                        $"Employee with id '{updateEmployeeTask.EmployeeId}' not found"
                    );

                employeeTask.TaskName = updateEmployeeTask.TaskName;
                employeeTask.TaskDueDate = updateEmployeeTask.TaskDueDate;
                employeeTask.TaskDescription = updateEmployeeTask.TaskDescription;
                employeeTask.EmployeeId = updateEmployeeTask.EmployeeId;

                await _context.SaveChangesAsync();

                serviceResponse.Data = _mapper.Map<GetEmployeeTaskDto>(employeeTask);

                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<GetEmployeeTaskDto>> UpdateEmployeeTaskStatus(
            UpdateEmployeeTaskStatusDto updateEmployeeTaskStatus,
            int id
        )
        {
            var serviceResponse = new ServiceResponse<GetEmployeeTaskDto>();
            try
            {
                var employeeTask = await _context.EmployeeTasks.FirstOrDefaultAsync(
                    c => c.TaskID == id
                );

                if (employeeTask is null)
                    throw new Exception($"Employee task with id '{id}' not found");

                var existingEmployee = await _context.Employees.FirstOrDefaultAsync(
                    e => e.EmployeeID == updateEmployeeTaskStatus.EmployeeId
                );

                if (existingEmployee is null)
                    throw new Exception(
                        $"Employee with id '{updateEmployeeTaskStatus.EmployeeId}' not found"
                    );

                var manager = await _context.Managers.FirstOrDefaultAsync(
                    m => m.ManagerId == existingEmployee.ManagerID
                );

                if (manager is null)
                    throw new Exception(
                        $"Manager not found for employee with id '{updateEmployeeTaskStatus.EmployeeId}'"
                    );

                employeeTask.TaskStatus = updateEmployeeTaskStatus.TaskStatus;

                serviceResponse.Data = _mapper.Map<GetEmployeeTaskDto>(employeeTask);

                await _context.SaveChangesAsync();

                if (updateEmployeeTaskStatus.TaskStatus == Status.Completed)
                {
                    var managerMessage = new Message(
                        new string[] { manager.Email },
                        "Task Completed",
                        $"Dear {manager.ManagerName},\n\nThe task '{employeeTask.TaskName}' assigned to"
                            + $" {existingEmployee.EmployeeName} has been completed.\n\nPlease review and take"
                            + $" any necessary actions.\n\nThank you!"
                    );

                    _emailSender.SendEmail(managerMessage);
                }

                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<
            ServiceResponse<List<GetEmployeeTaskDto>>
        > GetEmployeeOngoingTaskByEmployeeId(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetEmployeeTaskDto>>();
            try
            {
                var dbEmployeeTasks = await _context.EmployeeTasks
                    .Where(e => e.EmployeeId == id && e.TaskStatus == Status.InProgress)
                    .ToListAsync();
                var currentDate = DateTime.Today;
                foreach (var employeeTask in dbEmployeeTasks)
                {
                    DateTime TaskDueDate = (DateTime)employeeTask.TaskDueDate!;
                    DateTime dueDate = TaskDueDate.Date;
                    if (dueDate <= currentDate)
                    {
                        employeeTask.TaskStatus = Status.Pending;
                        _context.EmployeeTasks.Update(employeeTask);
                    }
                }

                await _context.SaveChangesAsync();

                serviceResponse.Data = dbEmployeeTasks
                    .Select(c => _mapper.Map<GetEmployeeTaskDto>(c))
                    .ToList();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<
            ServiceResponse<List<GetEmployeeTaskDto>>
        > GetEmployeeCompletedTaskByEmployeeId(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetEmployeeTaskDto>>();
            try
            {
                var dbEmployeeTasks = await _context.EmployeeTasks
                    .Where(e => e.EmployeeId == id && e.TaskStatus == Status.Completed)
                    .ToListAsync();

                serviceResponse.Data = dbEmployeeTasks
                    .Select(c => _mapper.Map<GetEmployeeTaskDto>(c))
                    .ToList();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<
            ServiceResponse<List<GetEmployeeTaskDto>>
        > GetEmployeePendingTaskByEmployeeId(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetEmployeeTaskDto>>();
            try
            {
                var dbEmployeeTasks = await _context.EmployeeTasks
                    .Where(e => e.EmployeeId == id && e.TaskStatus == Status.Pending)
                    .ToListAsync();

                serviceResponse.Data = dbEmployeeTasks
                    .Select(c => _mapper.Map<GetEmployeeTaskDto>(c))
                    .ToList();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<
            ServiceResponse<PaginationResultVM<GetEmployeeTaskDto>>
        > GetAllEmployeeTasksByEmployeeId(
            int managerid,
            int employeeid,
            PaginationInput paginationInput
        )
        {
            var serviceResponse = new ServiceResponse<PaginationResultVM<GetEmployeeTaskDto>>();
            try
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(
                    c => c.EmployeeID == employeeid
                );

                if (employee == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Employee not found";
                    return serviceResponse;
                }

                if (employee.ManagerID != managerid)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Unauthorized";
                    return serviceResponse;
                }

                var dbEmployeeTasks = await _context.EmployeeTasks
                    .Where(c => c.EmployeeId == employeeid)
                    .ToListAsync();

                if (dbEmployeeTasks.Count == 0)
                    throw new Exception($"Employee has no tasks.");

                var employeeTasks = _mapper.Map<List<GetEmployeeTaskDto>>(dbEmployeeTasks);

                var result = _paginationServices.GetPagination(employeeTasks, paginationInput);

                serviceResponse.Data = result;

                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetEmployeeTaskDto>>> GetEmployeeNewTaskByEmployeeId(
            int id
        )
        {
            var serviceResponse = new ServiceResponse<List<GetEmployeeTaskDto>>();
            try
            {
                var currentDate = DateTime.Now;
                Console.WriteLine(currentDate);
                var dbEmployeeTasks = await _context.EmployeeTasks
                    .Where(t => t.EmployeeId == id && t.TaskStatus == Status.New)
                    .ToListAsync();

                foreach (var employeeTask in dbEmployeeTasks)
                {
                    DateTime TaskDueDate = (DateTime)employeeTask.TaskDueDate!;
                    DateTime dueDate = TaskDueDate.Date;

                    if (dueDate <= currentDate)
                    {
                        employeeTask.TaskStatus = Status.Pending;
                        _context.EmployeeTasks.Update(employeeTask);
                    }
                }

                await _context.SaveChangesAsync();

                serviceResponse.Data = dbEmployeeTasks
                    .Where(t => t.TaskStatus == Status.New)
                    .Select(c => _mapper.Map<GetEmployeeTaskDto>(c))
                    .ToList();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<int>> CalculateNewEmployeeTasksByEmployeeId(
            int employeeId
        )
        {
            var serviceResponse = new ServiceResponse<int>();
            try
            {
                var newTasksCount = await _context.EmployeeTasks.CountAsync(
                    e => e.EmployeeId == employeeId && e.TaskStatus == Status.New
                );

                serviceResponse.Data = newTasksCount;
                serviceResponse.Message =
                    $"New EmployeeTasks count calculated successfully for Employee ID: {employeeId}.";
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
