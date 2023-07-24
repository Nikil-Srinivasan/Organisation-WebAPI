using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Organisation_WebAPI.Dtos.EmployeeDto;
using Organisation_WebAPI.Dtos.ManagerDto;
using Organisation_WebAPI.InputModels;
using Organisation_WebAPI.Services.Managers;

namespace Organisation_WebAPI.Controllers
{   
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _managerService;

        public ManagerController(IManagerService managerService)
        {
             _managerService = managerService;
        }

        //Endpoint to get All managers from database by pagination
        [HttpPost("GetAllManagers")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<ServiceResponse<GetManagerDto>>> GetManagers(PaginationInput paginationInput)
        {
            var response = await _managerService.GetAllManagers(paginationInput);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to retrieve manager and employees by departmentId
        [HttpGet("GetEmployeesAndManagerByDepartmentId")]
        public async Task<ActionResult<ServiceResponse<GetEmployeesAndManagerDto>>> GetEmployeesAndManagerByDepartmentId(int id)
        {
            var response = await _managerService.GetEmployeesAndManagerByDepartmentId(id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }


        //Endpoint to retrieve a manager by managerId
        [HttpGet("GetManagerById")]
        public async Task<ActionResult<ServiceResponse<GetManagerDto>>> GetManagerById(int id)
        {
            var response = await _managerService.GetManagerById(id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to get manager by departmentId
        [HttpGet("GetManagerByDepartmentId")]
        public async Task<ActionResult<ServiceResponse<GetManagerDto>>> GetManagerByDepartmentId(int id)
        {
            var response = await _managerService.GetManagerByDepartmentId(id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to update a manager by managerId
        [HttpPut("UpdateManager")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<ServiceResponse<GetManagerDto>>> UpdateManager(UpdateManagerDto updatedManager,int id){
            var response = await _managerService.UpdateManager(updatedManager,id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to delete a manager by managerId
        [HttpDelete("DeleteManager")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<ServiceResponse<GetManagerDto>>> DeleteManager(int id){
            var response = await _managerService.DeleteManager(id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to retrieve all departments  associated with manager
        [HttpGet("GetAllDepartmentsAssociatedWithManager")]
        public async Task<ActionResult<ServiceResponse<string>>> GetAllDepartmentsAssociatedWithManager()
        {
            var response = await _managerService.GetAllDepartmentsAssociatedWithManager();
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}