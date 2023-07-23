using Microsoft.AspNetCore.Mvc;
using Organisation_WebAPI.Dtos.Admin;
using Organisation_WebAPI.Dtos.ManagerDto;
using Organisation_WebAPI.Models;
using System.Security.Claims;

namespace Organisation_WebAPI.Services.AuthRepo
{
    public interface IAuthRepository
    {
        Task<ServiceResponse<string>> Register(UserRegisterDto model);
        Task<ServiceResponse<string>> Login(string username, string password);
        Task<ServiceResponse<string>> Verify(string email, string otp);
        Task<ServiceResponse<string>> ForgotPassword(string email);
        Task<ServiceResponse<ResetPasswordDto>> ResetPassword(ResetPasswordDto email);
        Task<ServiceResponse<string>> ResendOtp(string email);
        Task<ServiceResponse<GetUserDto>> GetUserById(int id);
        Task<ServiceResponse<string>> DeleteUserById(int id);
        Task<ServiceResponse<List<GetUserDto>>> GetAllUsers();
        Task<bool> UserExists(string username);
        Task<ServiceResponse<string>> AppointNewManager(int managerId, NewManagerDto model);
    }
}
