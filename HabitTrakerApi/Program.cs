using FluentValidation;
using HabitTrakerApi.DI;
using HabitTrakerApi.FluentValidation;
using HabitTrakerApi.Midlware;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.Enums;
using HabitTrakerApi.Repositories;
using HabitTrakerApi.Services;
using Microsoft.AspNetCore.Identity;
using  Swashbuckle.AspNetCore;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;


var builder = WebApplication.CreateBuilder(args);// это создает новый экземпляр класса WebApplicationBuilder, который используется для настройки и создания веб-приложения. Он предоставляет доступ к различным сервисам и настройкам, которые можно использовать для конфигурации приложения.

builder.Services.AddControllers(); 

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHabitService, IHabitService>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

 Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // Уровень детализации
            .WriteTo.Console()    // Дублировать в консоль
            .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day) 
            .CreateLogger();

using var loggerFactory = LoggerFactory.Create(builder =>
{
            builder.AddSerilog(); 
});

        ILogger logger = loggerFactory.CreateLogger<Program>();
builder.Services.AddAutoMapper(cfg => 
{
    cfg.AddProfile<MappingProfile>();
});


builder.Services.AddValidatorsFromAssemblyContaining<HabitValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UserValidation>();
 logger.LogInformation("APi Запущено!");
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
}
app.UseMiddleware<GlobalExtentionHandler>();

app.MapControllers();
app.Run();