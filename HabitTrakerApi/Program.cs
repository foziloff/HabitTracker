using FluentValidation;
using HabitTrakerApi.Common;
using HabitTrakerApi.DbContext;
using HabitTrakerApi.FluentValidation;
using HabitTrakerApi.Repositories;
using HabitTrakerApi.Repositories.Interfaces;
using HabitTrakerApi.Services;
using HabitTrakerApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HabitTrakerApi.Data;
using ILogger = Microsoft.Extensions.Logging.ILogger;


var builder = WebApplication.CreateBuilder(args); // Это создает новый экземпляр класса WebApplicationBuilder, который используется для настройки и создания веб-приложения. Он предоставляет доступ к различным сервисам и настройкам, которые можно использовать для конфигурации приложения.

builder.Services.AddControllers(); 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // Уровень детализации
            .WriteTo.Console()    // Дублировать в консоль
            .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day) 
            .CreateLogger();

builder.Services.AddScoped<ICategoryRepository , CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHabitLogRepository , HabitLogRepository>();
builder.Services.AddScoped<IHabitRepository, HabitRepository>();
builder.Services.AddScoped<IJwtServiceRepository, JwtServiceRepository>();
builder.Services.AddScoped <IReminderRepository, ReminderRepository>();
builder.Services.AddScoped<IAuthService,AuthServiceJwt >();
builder.Services.AddScoped<ICategoryService ,  CategoryService>();
builder.Services.AddScoped<IHabitService, HabitService>();
builder.Services.AddScoped<IHabitLogService, HabitLogService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IUserService,UserService>();


using var loggerFactory = LoggerFactory.Create(builder =>
{
            builder.AddSerilog(); 
});

        ILogger logger = loggerFactory.CreateLogger<Program>();

builder.Services.AddValidatorsFromAssemblyContaining<UserValidation>();
// JWT Authentication configuration
var jwtKey = builder.Configuration["Jwt:key"];
if (string.IsNullOrEmpty(jwtKey))
{
    logger.LogWarning("JWT key is not configured (Jwt:key). Authentication will not validate tokens correctly.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? string.Empty)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Current user accessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

logger.LogInformation("APi Запущено!");
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
}
//app.UseMiddleware<GlobalExtentionHandler>();
// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();