global using Organisation_WebAPI.Dtos;
global using Organisation_WebAPI.Models;
global using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using EmailService;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Organisation_WebAPI.Data;
using Organisation_WebAPI.Services.AuthRepo;
using Organisation_WebAPI.Services.Departments;
using Organisation_WebAPI.Services.Employees;
using Swashbuckle.AspNetCore.Filters;
using Serilog;
using Organisation_WebAPI.Services.EmployeeTasks;
using Organisation_WebAPI.Services.Managers;
using Organisation_WebAPI.Services.Dashboard;
using Organisation_WebAPI.DataSeed;
using Organisation_WebAPI.Services.Pagination;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Add services to the container.

builder.Services.AddDbContext<OrganizationContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddScoped<IJwtUtils, JwtUtils>();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IManagerService, ManagerService>();
builder.Services.AddScoped<IEmployeeTaskService, EmployeeTaskService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();

#region Configure Authentication



builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(
                    builder.Configuration.GetSection("AppSettings:Token").Value
                )
            ),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });
#endregion

#region Configure Swagger

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(
        "oauth2",
        new OpenApiSecurityScheme
        {
            Description =
                "Standard Authorization header using the Bearer Scheme, e.g. \"bearer {token}\"",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        }
    );

    c.OperationFilter<SecurityRequirementsOperationFilter>();
});

#endregion


static void SeedData(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var serviceProvider = scope.ServiceProvider;
    var context = serviceProvider.GetRequiredService<OrganizationContext>();

    // Seed the admin user
    AdminUserSeed.SeedAdminUser(serviceProvider);

    // Add more seed methods for other entities if needed

    // Save changes to the database
    context.SaveChanges();
}

builder.Services.AddTransient(typeof(IPaginationServices<,>), typeof(PaginationService<,>));

#region Configure Serilog

builder.Host.UseSerilog(
    (context, config) =>
    {
        config.WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day);

        if (context.HostingEnvironment.IsProduction() == false)
        {
            config.WriteTo.Console();
        }
    }
);

#endregion


var app = builder.Build();

var serviceProvider = app.Services;

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();

//}


app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

SeedData(serviceProvider);

app.Run();
