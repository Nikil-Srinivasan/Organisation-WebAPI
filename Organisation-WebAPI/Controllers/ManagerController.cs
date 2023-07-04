using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Organisation_WebAPI.Dtos.ManagerDto;
using Organisation_WebAPI.Services.Managers;

namespace Organisation_WebAPI.Controllers
{   
    [ApiController]
    [Route("api/[controller]")]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _managerService;

        public ManagerController(IManagerService managerService)
        {
             _managerService = managerService;
        }

        [HttpGet("GetAllManagers")]
        public async Task<ActionResult<ServiceResponse<GetManagerDto>>> GetManagers()
        {
            return Ok(await _managerService.GetAllManagers());
        }

        [HttpGet("GetManagerCount")]
        public async Task<ActionResult<ServiceResponse<int>>> GetEmployeeCount()
        {
            var serviceResponse = await _managerService.GetManagerCount();
            return Ok(serviceResponse);
        }

        
        [HttpGet("GetManagerById")]
        public async Task<ActionResult<ServiceResponse<GetManagerDto>>> GetEmployee(int id)
        {
            return Ok(await _managerService.GetManagerById(id));
        }

        [HttpPost("CreateManager")]
        public async Task<ActionResult<ServiceResponse<GetManagerDto>>> AddEmployee(AddManagerDto newManager)
        {
            return Ok(await _managerService.AddManager(newManager));
        }

       
        [HttpPut("UpdateManager")]
        public async Task<ActionResult<ServiceResponse<GetManagerDto>>> UpdateEmployee(UpdateManagerDto updatedManager,int id){
            return Ok(await _managerService.UpdateManager(updatedManager,id));
        }
        
        
        [HttpDelete("DeleteManager")]
        public async Task<ActionResult<ServiceResponse<GetManagerDto>>> DeleteEmployee(int id){
            return Ok(await _managerService.DeleteManager(id));
        }
    }
}