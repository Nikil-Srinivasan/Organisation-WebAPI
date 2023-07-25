using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Organisation_WebAPI.Dtos.EmployeeDto;
using Organisation_WebAPI.InputModels;
using Organisation_WebAPI.Services.AuthRepo;
using Organisation_WebAPI.Services.Employees;

namespace Organisation_WebAPI.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IJwtUtils _jwtUtils;
        private readonly IMapper _mapper;
        public EmployeeController(IEmployeeService employeeService, IJwtUtils jwtUtils, IMapper mapper)
        {
            _employeeService = employeeService;
            _jwtUtils = jwtUtils;
            _mapper = mapper;
        }

        //Endpoint to retrieves all employees from the database

        [HttpPost("GetAllEmployees")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeDto>>> GetEmployees(PaginationInput paginationInput)
        {
            var response = await _employeeService.GetAllEmployees(paginationInput);
            if(!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }


        //Endpoint to retrieves a employee from the database based on the provided employeeID

        [HttpGet("GetEmployeeById")]
        [Authorize(Roles = nameof(UserRole.Employee) + "," + nameof(UserRole.Manager))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeDto>>> GetEmployee(int id)
        {
            var response = await _employeeService.GetEmployeeById(id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to retrieves all employees from the database based on the provided ManagerID 

        [HttpGet("GetAllEmployeesByManagerId")]
        [Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.Manager))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeDto>>> GetAllEmployeesByManagerId()
        {
            int managerId = _jwtUtils.GetUserId();

            var response = await _employeeService.GetAllEmployeesByManagerId(managerId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        
        
        //Endpoint to updates an employee from the database based on the provided employeeID

        [HttpPut("UpdateEmployee")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<ServiceResponse<UpdateEmployeeDto>>> UpdateEmployee(UpdateEmployeeDto updatedEmployee,int id){
            var response = await _employeeService.UpdateEmployee(updatedEmployee,id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        
        //Endpoint to delete an employee from the database based on the provided employeeID

        [HttpDelete("DeleteEmployee")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<ServiceResponse<GetEmployeeDto>>> DeleteEmployee(int id){

            var response = await _employeeService.DeleteEmployee(id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);

        }



    }
}