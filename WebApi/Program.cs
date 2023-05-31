using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prometheus;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApi.Database;
using WebApi.Features.Accounts.Registration;
using WebApi.Features.Auth.Registration;
using WebApi.Features.Users.Registration;
using WebApi.Observability;
using WebApi.Pipeline;
using WebApi.Pipeline.Behaviors;
using WebApi.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
    // Can be merged if necessary
    .AddOpenBehavior(typeof(LoggingBehavior<,>))
    .AddOpenBehavior(typeof(MetricsBehavior<,>))
    .AddOpenBehavior(typeof(TracingBehavior<,>))
    .AddOpenBehavior(typeof(ValidationBehavior<,>)));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddSingleton<Dispatcher>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.CustomSchemaIds(s => s.FullName.Replace("+", ".")));
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();


// todo Features
builder.AddUsers();
builder.AddAuth();
builder.AddAccounts();

var app = builder.Build();

Telemetry.Init("WebApi");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    RunMigration(app);
}

app.MapMetrics();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// void RegisterUserAPIs()
// {
//     app.MapPost("/register", async (CancellationToken token, IValidator<CreateUserDto> validator, CreateUserDto userDto, IUserService userService) =>
//     {
//         var validationResult = await validator.ValidateAsync(userDto);
//         if (!validationResult.IsValid) {
//             return Results.ValidationProblem(validationResult.ToDictionary());
//         }
//         
//         var createdUserResult = await userService.Register(userDto, token);
//
//         if (createdUserResult.IsT1)
//         {
//             return Results.BadRequest(createdUserResult.AsT1.GetErrorsString());
//         }
//         return Results.Ok(createdUserResult.AsT0);
//     });
//     
//     app.MapPost("/login", async (CancellationToken token, IValidator<CreateUserDto> validator, CreateUserDto userDto, IUserService userService) =>
//     {
//         var validationResult = await validator.ValidateAsync(userDto);
//         if (!validationResult.IsValid) {
//             return Results.ValidationProblem(validationResult.ToDictionary());
//         }
//         
//         var createdUserResult = await userService.LoginAsync(userDto, token);
//
//         if (createdUserResult.IsT1)
//         {
//             return Results.BadRequest(createdUserResult.AsT1.GetErrorsString());
//         }
//         return Results.Ok(createdUserResult.AsT0);
//     });
//     
//     app.MapGet("/check", [Authorize]async (CancellationToken token) =>
//     {
//         return Results.Ok("createdUserResult.AsT0");
//     });
//
// }

void RunMigration(WebApplication webApplication)
{
    using (var scope = webApplication.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        if (dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            dbContext.Database.Migrate();
    }
}

public partial class Program { }