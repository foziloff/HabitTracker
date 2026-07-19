using FluentValidation;
using HabitTrakerApi.DbContext;
using HabitTrakerApi.FluentValidation;
using HabitTrakerApi.Repositories;
using HabitTrakerApi.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;


var builder = WebApplication.CreateBuilder(args); // Это создает новый экземпляр класса WebApplicationBuilder, который используется для настройки и создания веб-приложения. Он предоставляет доступ к различным сервисам и настройкам, которые можно использовать для конфигурации приложения.

builder.Services.AddControllers(); 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();


builder.Services.AddDbContext<AppDbContext>
    ( o=> o.UseSqlServer
        (builder.Configuration.GetConnectionString("DefaultConnection")));

 Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // Уровень детализации
            .WriteTo.Console()    // Дублировать в консоль
            .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day) 
            .CreateLogger();


 builder.Services.AddScoped<IHabitTrackerRepository,HabitTrackerRepository>();
 builder.Services.AddScoped<IServiceHabits, ServiceHabit>();
 builder.Services.AddScoped<IAuthService, AuthServiceJwt>();
 builder.Services.AddScoped<IJwtServiceRepository, JwtServiceRepository>();
 
using var loggerFactory = LoggerFactory.Create(builder =>
{
            builder.AddSerilog(); 
});

        ILogger logger = loggerFactory.CreateLogger<Program>();

builder.Services.AddValidatorsFromAssemblyContaining<UserValidation>();
 logger.LogInformation("APi Запущено!");
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
}
//app.UseMiddleware<GlobalExtentionHandler>();
app.MapControllers();
app.Run();