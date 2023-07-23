using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Organisation_WebAPI.Data;
using Microsoft.EntityFrameworkCore;


namespace Organisation_WebAPI.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {   
        private readonly OrganizationContext _context;

        public DashboardService(OrganizationContext context )
        {
            _context = context;
        
        }
    
    // Get the count of employee tasks for each status based on the employee's ID.
     public async Task<ServiceResponse<Dictionary<Status, int>>> GetEmployeeTaskCount(int id)
     {
        var serviceResponse = new ServiceResponse<Dictionary<Status, int>>();

        try
        {   
            // Get all possible Status enum values
            var allStatuses = Enum.GetValues(typeof(Status)).Cast<Status>();

            // Group and count employee tasks by TaskStatus and convert it to a dictionary
            var taskStatusCounts = await _context.EmployeeTasks
            .Where(employee => employee.EmployeeId == id)
            .GroupBy(employee => employee.TaskStatus)
            .ToDictionaryAsync(group => group.Key, group => group.Count());

            var finalStatusCounts = allStatuses.ToDictionary(status => status, status => taskStatusCounts.GetValueOrDefault(status, 0));

            serviceResponse.Data = finalStatusCounts;
            serviceResponse.Message = "Task status counts retrieved successfully.";
        }
        catch (Exception ex)
        {
            serviceResponse.Success = false;
            serviceResponse.Message = "Failed to retrieve task status counts: " + ex.Message;
        }

            return serviceResponse;
        }


        //Retrieves the total count of EmployeeTasks based on managerId
        public async Task<ServiceResponse<Dictionary<Status, int>>> GetEmployeeTasksByManager(int id)
        {
            var serviceResponse = new ServiceResponse<Dictionary<Status, int>>();
            var taskCounts = new Dictionary<Status, int>();

            try
            {   
                //Get EmployeeTasks with ManagerId
                var tasks = await _context.EmployeeTasks
                    .Include(t => t.Employee)
                    .Where(t => t.Employee!.ManagerID == id)
                    .ToListAsync();

                // Initialize the dictionary with all possible Status enum values
                foreach (Status status in Enum.GetValues(typeof(Status)))
                {
                    taskCounts.Add(status, 0);
                }

                // Count the tasks for each status
                foreach (var task in tasks)
                {
                    if (taskCounts.ContainsKey(task.TaskStatus))
                    {
                        taskCounts[task.TaskStatus]++;
                    }
                }

                serviceResponse.Data = taskCounts;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }


    //Retrieves the total count of employees in each department.
    public async Task<ServiceResponse<Dictionary<string, int>>> GetTotalEmployeeCount()
    {
        var serviceResponse = new ServiceResponse<Dictionary<string, int>>();

        try
        {
            // Retrieve all departments from the database.
            var departments = await _context.Departments.ToListAsync();

            var tableCounts = new Dictionary<string, int>();

            // Loop through each department to calculate the total number of employees.
            foreach (var department in departments)
            {
                var employeesCount = await _context.Employees.CountAsync(e => e.Manager!.DepartmentID == department.DepartmentID);
                tableCounts.Add(department.DepartmentName!, employeesCount);
            }

            serviceResponse.Data = tableCounts;
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