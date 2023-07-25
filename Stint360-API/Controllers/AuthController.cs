using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using organisation_webapi.dtos.admin;
using Organisation_WebAPI.Dtos.Admin;
using Organisation_WebAPI.Dtos.ManagerDto;
using Organisation_WebAPI.Services.AuthRepo;


namespace Organisation_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        //Endpoint to Register method for employee and manager
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponse<int>>> Register(UserRegisterDto request)
        {
            var response = await _authRepository.Register(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to Login method for employee , manager and admin
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponse<int>>> Login(UserLoginDto request)
        {
            var response = await _authRepository.Login(request.UserName, request.Password);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to Verifiy OTP Method for forgot password
        [HttpPost("Verify")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponse<string>>> Verify(string email, string otp)
        {
            var response = await _authRepository.Verify(email, otp);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to Retrieves User by userId
        [HttpGet("GetUserById")]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> GetUserById(int id)
        {
            var response = await _authRepository.GetUserById(id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to Forgot password method by OTP verification
        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponse<string>>> ForgotPassword(string email)
        {
            var response = await _authRepository.ForgotPassword(email);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to  ResetPassword method after successfull verification of emailId
        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponse<string>>> ResetPassword(
            ResetPasswordDto request
        )
        {
            var response = await _authRepository.ResetPassword(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to ResendOTP for forgot password method
        [HttpPost("ResendOtp")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponse<string>>> ResendOtp(string email)
        {
            var response = await _authRepository.ResendOtp(email);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to Delete user by userId
        [HttpDelete("DeleteUserById")]
        public async Task<ActionResult<ServiceResponse<string>>> DeleteUserById(int id)
        {
            var response = await _authRepository.DeleteUserById(id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to  Retrieves all users from database
        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<ServiceResponse<string>>> GetAllUsers()
        {
            var response = await _authRepository.GetAllUsers();
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Endpoint to Appoint new manager method after removing current manager
        [HttpPost("AppointNewManager")]
        public async Task<ActionResult<ServiceResponse<string>>> AppointNewManager(
            NewManagerDto newManager,
            int id
        )
        {
            var response = await _authRepository.AppointNewManager(id, newManager);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
