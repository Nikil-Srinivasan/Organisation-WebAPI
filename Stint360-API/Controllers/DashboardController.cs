using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Organisation_WebAPI.Services.Dashboard;

namespace Organisation_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        
        //Endpoint to retrieves Total Number of Employees in each Department 

        [HttpGet("GetTotalEmployeeCount")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<ServiceResponse<int>>> GetEmployeeCount()
        {
            var response = await _dashboardService.GetTotalEmployeeCount();
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to retrives Total Task counts with provided employeeID

        [HttpGet("GetEmployeeTasksCount")]
        [Authorize(Roles = nameof(UserRole.Employee))]
        public async Task<ActionResult<ServiceResponse<int>>> GetTaskCounts(int id)
        {
            var response= await _dashboardService.GetEmployeeTaskCount(id);

            if(!response.Success){
                return BadRequest(response);
            }

            return Ok(response);
        }

        //Endpoint to retrieves Total Task counts with provided managerID

        [HttpGet("GetEmployeeTasksByManager")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult<ServiceResponse<int>>> GetEmployeeTasks(int id)
        {
            var response = await _dashboardService.GetEmployeeTasksByManager(id);
            
            if(!response.Success) {
                return BadRequest(response);
            }

            return Ok(response);
        }
      
    }
}