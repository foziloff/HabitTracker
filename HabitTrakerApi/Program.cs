using FluentValidation;
using HabitTrakerApi.DI;
using HabitTrakerApi.FluentValidation;
using HabitTrakerApi.Midlware;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.Enums;
using HabitTrakerApi.Repositories;
using HabitTrakerApi.Services;
using  Swashbuckle.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IHabitRepository, HabitRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHabitService, HabitService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

builder.Services.AddAutoMapper(cfg => 
{
    cfg.AddProfile<MappingProfile>();
});


builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
}
//app.UseMiddleware<ExceptionMiddleware>();


using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

    if (repo.GetByLogin("admin") == null)
    {
        repo.Add(new User
        {
            Login = "admin",
            Password = "123",
            Role = UserRole.Admin
        });
    }
}

app.MapControllers();
app.Run();