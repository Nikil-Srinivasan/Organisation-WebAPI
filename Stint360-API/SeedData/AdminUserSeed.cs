using Microsoft.EntityFrameworkCore;
using Organisation_WebAPI.Data;

namespace Organisation_WebAPI.DataSeed
{
    public class AdminUserSeed
    {
        public static void SeedAdminUser(IServiceProvider serviceProvider)
        {
            using (var context = new OrganizationContext(
                serviceProvider.GetRequiredService<DbContextOptions<OrganizationContext>>()))
            {
                // Check if admin user already exists
                if (context.Users.Any(u => u.UserName == "Vikram"))
                {
                    return; // Admin user already seeded, no need to proceed
                }

                // Generate password hash and salt for admin user's password
                CreatePasswordHash("Admin@2001", out byte[] passwordHash, out byte[] passwordSalt);

                // Create admin user instance
                var adminUser = new User
                {
                    UserName = "Vikram",
                    Email = "vikram@gmail.com",
                    IsVerified = true,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Role = UserRole.Admin // Set the admin role for the user
                };

                // Add admin user to the database
                context.Users.Add(adminUser);
                context.SaveChanges();
            }
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}

