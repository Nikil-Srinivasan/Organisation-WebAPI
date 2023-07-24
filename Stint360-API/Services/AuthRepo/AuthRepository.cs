using AutoMapper;
using EmailService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Organisation_WebAPI.Data;
using Organisation_WebAPI.Dtos.Admin;
using Organisation_WebAPI.Dtos.ManagerDto;
using System.Text.RegularExpressions;


namespace Organisation_WebAPI.Services.AuthRepo
{
    public class AuthRepository : IAuthRepository
    {
        private readonly OrganizationContext _dbContext;
        private readonly IEmailSender _emailSender;
        private readonly Dictionary<string, string> _otpDictionary;
        private readonly IConfiguration _configuration;
        private readonly IJwtUtils _jwtUtils;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;

        public AuthRepository(
            OrganizationContext dbContext,
            IConfiguration configuration,
            IJwtUtils jwtUtils,
            IEmailSender emailSender,
            IMemoryCache memoryCache,
            IMapper mapper
        )
        {
            _dbContext = dbContext;
            _emailSender = emailSender;
            _otpDictionary = new Dictionary<string, string>();
            _configuration = configuration;
            _jwtUtils = jwtUtils;
            _memoryCache = memoryCache;
            _mapper = mapper;
        }

        //Login method for user and generate token if username and password matches
        public async Task<ServiceResponse<string>> Login(string username, string password)
        {
            var response = new ServiceResponse<string>();
            var user = await _dbContext.Users.FirstOrDefaultAsync(
                u => u.UserName!.ToLower() == username.ToLower()
            );

            //If user is null  response message as User not found
            if (user == null)
            {
                response.Success = false;
                response.Message = "User Not Found";
            }

            //If user is not null verify password and if returns false set response as Incorrect Password 
            else if (!VerifyPasswordHash(password, user.PasswordHash!, user.PasswordSalt!))
            {
                response.Success = false;
                response.Message = "Incorrect Password";
            }
            else
            {
                //If the manager is appointed generate JWT Token if not return false
                if (user.Role == UserRole.Manager)
                {
                    var manager = await _dbContext.Managers.FindAsync(user.UserID);
                    if (manager.IsAppointed == false)
                    {
                        response.Success = false;
                        response.Message = "User Not Found";
                        return response;
                    }
                }
                response.Data = _jwtUtils.GenerateJwtToken(user);
            }
            return response;
        }

        //Register method for employee and manager
        public async Task<ServiceResponse<string>> Register(UserRegisterDto model)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            //Check whether the given email is valid or not
            if (!IsEmailValid(model.Email))
            {
                response.Success = false;
                response.Message = "Invalid email address.";
                return response;
            }

            //Checks for the username in the Users
            if (await UserExists(model.UserName))
            {
                response.Success = false;
                response.Message = "User already exists";
                return response;
            }

            // Check if the email already exists in the Users table.
            var ExistingEmail = await _dbContext.Users.FirstOrDefaultAsync(
                e => model.Email == e.Email
            );
            if (ExistingEmail != null)
            {
                response.Success = false;
                response.Message = "Email already exists";
                return response;
            }

            //If username and email is unique generate OTP 
            OtpGenerator otpGenerator = new OtpGenerator();
            string otp = otpGenerator.GenerateOtp();

            DateTimeOffset indianTime = DateTimeOffset.UtcNow.ToOffset(
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time").BaseUtcOffset
            );
            DateTimeOffset otpExpiration = indianTime.AddMinutes(2);

             // Create password hash and salt for user password.
            CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // Begin a transaction to ensure atomicity of database operations.
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {   
                    //Set user details in User Table 
                    var user = new User
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        Role = model.Role,
                        IsVerified = true
                    };

                    await _dbContext.Users.AddAsync(user);
                    await _dbContext.SaveChangesAsync();

                    //If user role is employee , store it in employee table
                    if (model.Role == UserRole.Employee)
                    {
                        var manager = await _dbContext.Managers.FindAsync(model.ManagerID);

                        if (manager == null)
                        {
                            response.Success = false;
                            response.Message = "Invalid manager ID. Manager Not Found";
                            transaction.Rollback();
                            return response;
                        }

                        var employee = new Employee
                        {
                            EmployeeID = user.UserID,
                            Email = model.Email,
                            Phone = model.Phone,
                            Designation = model.Designation,
                            Address = model.Address,
                            EmployeeName = model.EmployeeName,
                            EmployeeSalary = model.EmployeeSalary,
                            EmployeeAge = model.EmployeeAge,
                            ManagerID = model.ManagerID,
                            User = user
                        };
                        var employeeMessage = new Message(
                            new string[] { model.Email },
                            "Welcome to Stint360 - Employee Registration",
                            $"Dear {model.UserName},\n\nCongratulations! You have been registered as an employee in Stint360.\n\nYour "
                                + $"credentials:\nUsername: {model.UserName}\nPassword: {model.Password}\n\nPlease keep this information confidential"
                                + $".\n\nThank you and welcome to Stint360!"
                        );

                        _emailSender.SendEmail(employeeMessage);

                        await _dbContext.Employees.AddAsync(employee);
                    }

                    //If user role is manager then store it in manager table
                    else if (model.Role == UserRole.Manager)
                    {
                        var department = await _dbContext.Departments.FindAsync(model.DepartmentID);

                        if (department == null)
                        {
                            response.Success = false;
                            response.Message = "Invalid department ID. Department Not Found";
                            transaction.Rollback();
                            return response;
                        }
                        var existingManagers = await _dbContext.Managers.FirstOrDefaultAsync(
                            m => m.DepartmentID == model.DepartmentID
                        );

                        if (existingManagers != null)
                        {
                            response.Success = false;
                            response.Message =
                                "Department ID is already associated with a manager.";
                            transaction.Rollback();
                            return response;
                        }

                        var manager = new Manager
                        {
                            ManagerId = user.UserID,
                            Email = model.Email,
                            Address = model.Address,
                            Phone = model.Phone,
                            ManagerName = model.ManagerName,
                            ManagerSalary = model.ManagerSalary,
                            ManagerAge = model.ManagerAge,
                            DepartmentID = model.DepartmentID,
                            IsAppointed = true,
                            User = user
                        };

                        var managerMessage = new Message(
                            new string[] { model.Email },
                            "Welcome to Stint360 - Manager Registration",
                            $"Dear {model.UserName},\n\nCongratulations! You have been registered as a manager in Stint360."
                                + $"\n\nYour credentials:\nUsername: {model.UserName}\nPassword: {model.Password}\n\nPlease keep this "
                                + $"information confidential.\n\nThank you and welcome to Stint360!"
                        );

                        _emailSender.SendEmail(managerMessage);

                        await _dbContext.Managers.AddAsync(manager);
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = "Invalid user role.";
                        return response;
                    }

                    await _dbContext.SaveChangesAsync();
                    transaction.Commit();

                    response.Data = "User Registered Successfully";
                    return response;
                }
                catch (Exception ex)
                {
                    // Handle exception if needed
                    transaction.Rollback();
                    response.Success = false;
                    response.Message = "An error occurred during registration.";
                    return response;
                }
            }
        }

        //Verify email and OTP for Registeration of user
        public async Task<ServiceResponse<string>> Verify(string email, string otp)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();
            
            // Check if the provided email is valid.
            if (!IsEmailValid(email))
            {
                response.Success = false;
                response.Message = "Invalid email address.";
                return response;
            }
            var user = await _dbContext.Users.FirstOrDefaultAsync(
                u => u.Email!.ToLower() == email.ToLower()
            );

            // Check if a user with the given email was found.
            if (user != null)
            {   
                // Check if the user has exceeded the maximum OTP resend limit.
                if (user.OtpResendCount >= 3)
                {
                    response.Success = false;
                    response.Message = "Maximum OTP resend limit reached.";
                    return response;
                }

                // Check if the OTP provided matches the OTP stored for the user.
                if (user.Otp == otp)
                {
                    if (user.OtpExpiration > DateTimeOffset.UtcNow)
                    {
                        user.Otp = null;
                        user.IsVerified = true;
                        await _dbContext.SaveChangesAsync();
                        response.Success = true;
                        response.Message = "OTP verification successful.";
                        return response;
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = "Your Otp has been expired , Please try again";
                    }
                }
                else
                {
                    response.Success = false;
                    response.Message = "Invalid OTP , Please try again";
                }
            }
            else
            {
                response.Success = false;
                response.Message = "Invalid Email , Please try again";
            }

            return response;
        }

        //Checks whether a user exists in the User Table
        public async Task<bool> UserExists(string username)
        {
            if (await _dbContext.Users.AnyAsync(u => u.UserName!.ToLower() == username.ToLower()))
            {
                return true;
            }
            return false;
        }

        //Forgot Password method for resetting the password
        public async Task<ServiceResponse<string>> ForgotPassword(string email)
        {
            var response = new ServiceResponse<string>();
            var user = await _dbContext.Users.FirstOrDefaultAsync(
                u => u.Email!.ToLower() == email.ToLower()
            );

            // Check if the provided email is valid.
            if (!IsEmailValid(email))
            {
                response.Success = false;
                response.Message = "Invalid email address.";
                return response;
            }
            // Check if the provided email exists in the database.
            if (!await EmailExists(email))
            {
                response.Success = false;
                response.Message = "Email does not exists";
                return response;
            }

            //If email exists generate OTP
            OtpGenerator otpGenerator = new OtpGenerator();
            string otp = otpGenerator.GenerateOtp();

            // Get the current Indian time
            DateTimeOffset indianTime = DateTimeOffset.UtcNow.ToOffset(
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time").BaseUtcOffset
            );

            // Add the expiration time in minutes
            DateTimeOffset otpExpiration = indianTime.AddMinutes(3);

            user.Otp = otp;
            user.OtpExpiration = otpExpiration;
            user.IsVerified = false;
            await _dbContext.SaveChangesAsync();

            var otpMessage = new Message(
                new string[] { email },
                "Stint360 - Password Reset OTP",
                $"Dear {email},\n\nYou have requested a password reset for your Stint360 account.\n\nYour OTP (One-Time Password) is: {otp}"
                    + $"\n\nPlease use this OTP to reset your password within the specified time limit.\n\nIf you did not request this password reset, "
                    + $"please ignore this message.\n\nThank you!"
            );

            _emailSender.SendEmail(otpMessage);
            response.Data = "Please check your email for OTP.";
            return response;
        }

        
        public async Task<ServiceResponse<ResetPasswordDto>> ResetPassword(ResetPasswordDto request)
        {
            ServiceResponse<ResetPasswordDto> response = new ServiceResponse<ResetPasswordDto>();

            // Check if the provided email is not null.
            if (request.Email is not null)
            {   
                // Try to find a user with the given email in the database.
                var user = await _dbContext.Users.FirstOrDefaultAsync(
                    u => u.Email!.ToLower() == request.Email.ToLower()
                );

                // Check if a user with the given email was found.
                if (user != null)
                {
                    if (user.IsVerified == false)
                    {
                        response.Success = false;
                        response.Message = "User Not Verified";
                        return response;
                    }

                    if (user.OtpExpiration > DateTimeOffset.UtcNow)
                    {
                        CreatePasswordHash(
                            request.NewPassword,
                            out byte[] passwordHash,
                            out byte[] passwordSalt
                        );
                        user.PasswordHash = passwordHash;
                        user.PasswordSalt = passwordSalt;
                        user.Otp = null;
                        user.OtpExpiration = null;
                        await _dbContext.SaveChangesAsync();
                        response.Message = "Password changed successfully.";
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = "Your OTP is expired.";
                        return response;
                    }
                }
                else
                {
                    response.Success = false;
                    response.Message = "Invalid Email Address.";
                }
            }
            else
            {
                response.Success = false;
                response.Message = "Email is Required.";
            }

            return response;
        }

        //Retrieve user from database by userId
        public async Task<ServiceResponse<GetUserDto>> GetUserById(int id)
        {
            var response = new ServiceResponse<GetUserDto>();
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found";
                return response;
            }

            response.Data = _mapper.Map<GetUserDto>(user);
            ;
            return response;
        }

        //Remove user from database by userId
        public async Task<ServiceResponse<string>> DeleteUserById(int id)
        {
            var response = new ServiceResponse<string>();

            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found";
                return response;
            }

            if (user.Role == UserRole.Employee)
            {
                var associatedEmployees = await _dbContext.Employees
                    .Where(e => e.UserID == id)
                    .ToListAsync();
                _dbContext.Employees.RemoveRange(associatedEmployees);
                _dbContext.Users.Remove(user);
            }
            else if (user.Role == UserRole.Manager)
            {
                // Update the manager's IsAppointed property to false
                var manager = await _dbContext.Managers.FirstOrDefaultAsync(m => m.ManagerId == id);
                if (manager != null)
                {
                    manager.IsAppointed = false;
                }
            }
            else
            {
                response.Success = false;
                response.Message = "Invalid user role";
                return response;
            }
            await _dbContext.SaveChangesAsync();
            response.Data = "User deleted successfully";
            return response;
        }

        //Retrieves all users from the database 
        public async Task<ServiceResponse<List<GetUserDto>>> GetAllUsers()
        {
            var response = new ServiceResponse<List<GetUserDto>>();
            var users = await _dbContext.Users.ToListAsync();

            response.Data = users.Select(c => _mapper.Map<GetUserDto>(c)).ToList();
            response.Message = "Users retrieved successfully";

            return response;
        }

        //Appoint new manager for available department 
        public async Task<ServiceResponse<string>> AppointNewManager(
            int managerId,
            NewManagerDto model
        )
        {
            var response = new ServiceResponse<string>();

            var manager = await _dbContext.Managers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);

            if (manager == null)
            {
                response.Success = false;
                response.Message = "Manager not found";
                return response;
            }

            // Update the user associated with the manager
            var user = await _dbContext.Users.FindAsync(managerId);

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found";
                return response;
            }

            CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // Update user properties
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.IsVerified = true;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            // Update other user fields as needed

            // Update manager properties

            manager.ManagerName = model.ManagerName;
            manager.ManagerSalary = model.ManagerSalary;
            manager.ManagerAge = model.ManagerAge;
            manager.IsAppointed = true;
            manager.User = user;
            manager.Phone = model.Phone;
            manager.Address = model.Address;
            manager.Email = model.Email;
            // Save changes to the database
            await _dbContext.SaveChangesAsync();

            // Send email with updated information

            var managerMessage = new Message(
                new string[] { model.Email },
                "Welcome to Stint360 - Manager Registration",
                $"Dear {model.UserName},\n\nCongratulations! You have been registered as a manager in Stint360."
                    + $"\n\nYour credentials:\nUsername: {model.UserName}\nPassword: {model.Password}\n\nPlease keep this "
                    + $"information confidential.\n\nThank you and welcome to Stint360!"
            );

            _emailSender.SendEmail(managerMessage);

            response.Data = "Manager updated successfully";
            return response;
        }

        public async Task<ServiceResponse<string>> ResendOtp(string email)
        {
            var response = new ServiceResponse<string>();

            // Check whether the given email is valid or not.
            if (!IsEmailValid(email))
            {
                response.Success = false;
                response.Message = "Invalid email address.";
                return response;
            }

            // Try to find a user with the given email in the database.
            var user = await _dbContext.Users.FirstOrDefaultAsync(
                u => u.Email!.ToLower() == email.ToLower()
            );

            // Check if a user with the given email exists.
            if (user == null)
            {
                response.Success = false;
                response.Message = "Email does not exist.";
                return response;
            }

             // Check if the user has reached the maximum OTP resend limit.
            if (user.OtpResendCount >= 3)
            {
                response.Success = false;
                response.Message = "Maximum OTP resend limit reached.";
                return response;
            }

            //Generate otp for reset password
            OtpGenerator otpGenerator = new OtpGenerator();
            string otp = otpGenerator.GenerateOtp();

            // Get the current Indian time
            DateTimeOffset indianTime = DateTimeOffset.UtcNow.ToOffset(
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time").BaseUtcOffset
            );

            // Add the expiration time in minutes
            DateTimeOffset otpExpiration = indianTime.AddMinutes(3);

            user.Otp = otp;
            user.OtpExpiration = otpExpiration;
            user.IsVerified = false;
            user.OtpResendCount++;

            await _dbContext.SaveChangesAsync();

            var message = new Message(
                new string[] { email },
                "OTP Resent",
                $"This is your new OTP: {otp}.\n\nIt will expire at {otpExpiration} IST."
            );
            _emailSender.SendEmail(message);

            response.Success = true;
            response.Message = "OTP has been resent.";
            return response;
        }

        //Check whether email exists in the database or not 
        public async Task<bool> EmailExists(string email)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email!.ToLower() == email.ToLower()))
            {
                return true;
            }
            return false;
        }

        //Create password Hash for the given password
        private static void CreatePasswordHash(
            string password,
            out byte[] passwordHash,
            out byte[] passwordSalt
        )
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        //Verify password has for password verification
        private static bool VerifyPasswordHash(
            string password,
            byte[] passwordHash,
            byte[] passwordSalt
        )
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);
            }
        }

        private static bool IsEmailValid(string email)
        {
            // Regular expression pattern for email validation
            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

            // Check if the email matches the pattern
            bool isValid = Regex.IsMatch(email, pattern);

            return isValid;
        }
    }
}
