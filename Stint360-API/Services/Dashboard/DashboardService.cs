using Organisation_WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace Organisation_WebAPI.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly OrganizationContext _context;

        public DashboardService(OrganizationContext context)
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

                var finalStatusCounts = allStatuses.ToDictionary(
                    status => status,
                    status => taskStatusCounts.GetValueOrDefault(status, 0)
                );

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
        public async Task<ServiceResponse<Dictionary<Status, int>>> GetEmployeeTasksByManager(
            int id
        )
        {
            var serviceResponse = new ServiceResponse<Dictionary<Status, int>>();

            try
            {
                // Group and count employee tasks by TaskStatus and convert it to a dictionary
                var taskStatusCounts = await _context.EmployeeTasks
                    .Where(employee => employee.Employee!.ManagerID == id)
                    .GroupBy(employee => employee.TaskStatus)
                    .ToDictionaryAsync(group => group.Key, group => group.Count());

                // Get all possible Status enum values
                var allStatuses = Enum.GetValues(typeof(Status)).Cast<Status>();

                // Create a dictionary with all possible Status enum values and their counts
                var finalStatusCounts = allStatuses.ToDictionary(
                    status => status,
                    status => taskStatusCounts.GetValueOrDefault(status, 0)
                );

                serviceResponse.Data = finalStatusCounts;
                serviceResponse.Message = "Task status counts retrieved successfully.";
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
                // Group and count employees by department and convert it to a dictionary
                var tableCounts = await _context.Employees
                    .GroupBy(e => e.Manager!.Department.DepartmentName!)
                    .ToDictionaryAsync(group => group.Key, group => group.Count());

                serviceResponse.Data = tableCounts!;
                serviceResponse.Message =
                    "Total employee counts by department retrieved successfully.";
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
