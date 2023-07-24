using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Organisation_WebAPI.Dtos.EmployeeTaskDto;
using Organisation_WebAPI.InputModels;
using Organisation_WebAPI.Services.AuthRepo;
using Organisation_WebAPI.Services.EmployeeTasks;

namespace Organisation_WebAPI.Controllers
{   
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeTaskController : ControllerBase
    {
        private readonly IEmployeeTaskService _employeeTaskService;
        private readonly IJwtUtils _jwtUtils;

        public EmployeeTaskController(IEmployeeTaskService employeeTaskService, IJwtUtils jwtUtils)
        {
            _employeeTaskService = employeeTaskService;
            _jwtUtils = jwtUtils;
        }

        //Endpoint to get All employee Tasks
        [HttpGet("GetEmployeeTasks")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeTaskDto>>> GetEmployeeTasks()
        {
            var response = await _employeeTaskService.GetAllEmployeeTasks(); 

            if(!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }
        
        //Endpoint to get  All Tasks of an employee by employeeId
        [HttpPost("GetEmployeeTasksByEmployeeId")]  
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeTaskDto>>> GetEmployeeTasksByEmployeeId(int employeeid, PaginationInput paginationInput)
        {

            int managerId = _jwtUtils.GetUserId();
            var response =  await _employeeTaskService.GetAllEmployeeTasksByEmployeeId(managerId,employeeid, paginationInput);


            if (response.Message == "Unauthorized")
            {
               return Unauthorized();
            }

            if(!response.Success){
                return BadRequest(response);
            }

            return Ok(response);
        }

        //Endpoint to get New Tasks of an employee by employeeId
        [HttpGet("GetNewEmployeeTasksByEmployeeId")]
        [Authorize(Roles = nameof(UserRole.Employee))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeTaskDto>>> GetNewEmployeeTasks(int id)
        {
            var response = await _employeeTaskService.GetEmployeeNewTaskByEmployeeId(id);
            if(!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to get Inprogress Tasks of an employee by employeeId
        [HttpGet("GetInProgressEmployeeTasksByEmployeeId")]
        [Authorize(Roles = nameof(UserRole.Employee))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeTaskDto>>> GetInProgressEmployeeTasks(int id)
        {
            var response =  await _employeeTaskService.GetEmployeeInProgressTaskByEmployeeId(id);

            if(!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to get Completed Task of an employee by employeeId
        [HttpGet("GetCompletedEmployeeTasksByEmployeeId")] 
        [Authorize(Roles = nameof(UserRole.Employee))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeTaskDto>>> GetCompletedEmployeeTasks(int id)
        {
            var response = await _employeeTaskService.GetEmployeeCompletedTaskByEmployeeId(id);

            if(!response.Success) {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to get the Pending Tasks of an employee
        [HttpGet("GetPendingEmployeeTasksByEmployeeId")]
        [Authorize(Roles = nameof(UserRole.Employee))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeTaskDto>>> GetPendingEmployeeTasksByEmployeeId(int id)
        {   
            var response = await _employeeTaskService.GetEmployeePendingTaskByEmployeeId(id);

            if(!response.Success) {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to get count of new Task 
        [HttpGet("GetNewTaskCount")]
        [Authorize(Roles = nameof(UserRole.Employee))]
        public async Task<ActionResult<ServiceResponse<int>>> GetNewEmployeeTaskCount(int id){

            var response =  await _employeeTaskService.CalculateNewEmployeeTasksByEmployeeId(id);

            if(!response.Success) {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to create Task for employee
        [HttpPost("CreateEmployeeTasks")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeTaskDto>>> AddEmployeeTask(AddEmployeeTaskDto newEmployeeTask)
        {
            var response = await _employeeTaskService.AddEmployeeTask(newEmployeeTask);

            if(!response.Success) { 
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to delete Task by employeeTaskId
        [HttpDelete("DeleteEmployeeTask")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeTaskDto>>> DeleteEmployeeTask(int id){
            var response = await _employeeTaskService.DeleteEmployeeTask(id);

            if(!response.Success) {
                return BadRequest(response);
            }
            return Ok(response);
        }
        
        //Endpoint to get Task by employeeTaskId
        [HttpGet("GetEmployeeTasksById")]
        [Authorize(Roles = nameof(UserRole.Employee))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeTaskDto>>> GetEmployeeTask(int id)
        {
            var response = await _employeeTaskService.GetEmployeeTasksById(id);

            if(!response.Success) {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to update Task of Employee by employeeTaskId
        [HttpPut("UpdateEmployeeTask")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeTaskDto>>> UpdateEmployeeTask(UpdateEmployeeTaskDto updatedEmployeeTask,int id){

            var response = await _employeeTaskService.UpdateEmployeeTask(updatedEmployeeTask,id);

            if(!response.Success) {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to update employeeTaskStatus by employeeTaskId
        [HttpPut("UpdateEmployeeTaskStatus")]
        [Authorize(Roles = nameof(UserRole.Employee))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeTaskDto>>> UpdateEmployeeTaskStatus(UpdateEmployeeTaskStatusDto updatedEmployeeTaskStatus,int id){
            var response = await _employeeTaskService.UpdateEmployeeTaskStatus(updatedEmployeeTaskStatus,id);

            if(!response.Success) {
                return BadRequest(response);
            }
            return Ok(response);
            
        }
    }
}