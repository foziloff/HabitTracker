using FluentValidation;
using HabitTrakerApi.Common;
using HabitTrakerApi.Data;
using HabitTrakerApi.DbContext;
using HabitTrakerApi.FluentValidation;
using HabitTrakerApi.Repositories;
using HabitTrakerApi.Repositories.Interfaces;
using HabitTrakerApi.Services;
using HabitTrakerApi.Services.BackraundServices;
using HabitTrakerApi.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using HabitTrakerApi.Messaging.Consumers;
using MassTransit;

// 1. НАСТРОЙКА ЛОГГЕРА (Serilog) ДО СТАРТА ПРИЛОЖЕНИЯ
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Уровень детализации логов
    .Enrich.FromLogContext()
    .WriteTo.Console() // Вывод в консоль IDE / терминала
    .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day) // Запись в файл каждый день
    .CreateLogger();

try
{
    Log.Information("Запуск веб-хоста приложения...");

    var builder = WebApplication.CreateBuilder(args);

    // Подключаем Serilog к инфраструктуре ASP.NET Core
    builder.Host.UseSerilog();

    // 2. РЕГИСТРАЦИЯ СЕРВИСОВ (DI)
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Настройка подключения к SQL Server
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Регистрация фоновых служб (Hosted Services)
// 1. Регистрируем TelegramService как Singleton, чтобы NotificationService мог получить его в конструкторе
    builder.Services.AddSingleton<TelegramService>();

// 2. Регистрируем его же как фоновую службу, забирая экземпляр из контейнера
    builder.Services.AddHostedService<TelegramService>(provider => provider.GetRequiredService<TelegramService>());

// 3. Явно регистрируем NotificationService (если Scrutor не делает это за вас автоматически как Singleton)
    builder.Services.AddSingleton<NotificationService>();    builder.Services.AddSingleton<NotificationService>();

    // Регистрация MediatR для реализации паттерна CQRS
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    // Регистрация репозиториев (Data Access Layer)
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IHabitLogRepository, HabitLogRepository>();
    builder.Services.AddScoped<IHabitRepository, HabitRepository>();
    builder.Services.AddScoped<IJwtServiceRepository, JwtServiceRepository>();
    builder.Services.AddScoped<IReminderRepository, ReminderRepository>();

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<TelegramNotificationConsumer>();

        // In-memory шина — без RabbitMQ и прочей инфраструктуры. Живёт внутри процесса.
        x.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    });
    // Регистрация бизнес-сервисов (Business Logic Layer)
    builder.Services.AddScoped<IAuthService, AuthServiceJwt>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IHabitService, HabitService>();
    builder.Services.AddScoped<IHabitLogService, HabitLogService>();
    builder.Services.AddScoped<IReminderService, ReminderService>();
    builder.Services.AddScoped<IUserService, UserService>();

    // Регистрация валидаторов FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<UserValidation>();

    // Настройка авторизации на базе JWT-токенов
    var jwtKey = builder.Configuration["Jwt:key"];
    if (string.IsNullOrEmpty(jwtKey))
    {
        Log.Warning("JWT key IS NOT configured (Jwt:key). Authentication will not validate tokens correctly.");
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

    // Доступ к текущему HTTP-контексту (идентификация пользователя)
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // Сборка приложения
    var app = builder.Build();

    // 3. БЕЗОПАСНАЯ ИНИЦИАЛИЗАЦИЯ И СИДИНГ БАЗЫ ДАННЫХ
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            
            Log.Information("Анализ миграций и запуск наполнения базы данных (DbSeeder)...");
            
            // Жестко блокируем поток и ждем завершения миграции и сидера ДО старта API
            await DbSeeder.SeedAsync(context);
            
            Log.Information("База данных успешно проверена и заполнена тестовыми данными!");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "КРИТИЧЕСКАЯ ОШИБКА: Не удалось применить миграции или заполнить БД сидерами!");
            // Приложение не запустится дальше, если база повреждена или падает с ошибкой каскада
            throw; 
        }
    }

    // 4. НАСТРОЙКА MIDDLEWARE (HTTP Pipeline)
    app.UseSwagger();
    app.UseSwaggerUI();
// app.UseMiddleware<GlobalExtentionHandler>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("API успешно сконфигурировано и начинает слушать порты...");
    
    // Запуск приложения
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение аварийно завершило свою работу во время инициализации хоста!");
}
finally
{
    // Гарантированно записываем оставшиеся логи в файл перед закрытием процесса
    Log.CloseAndFlush();
}

