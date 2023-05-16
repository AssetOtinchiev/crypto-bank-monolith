using Application;
using Application.Interfaces;
using Contracts.Dtos;
using Domain.Entitites;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddApplicationLayer();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    RunMigration(app);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

RegisterUserAPIs();
app.Run();

void RegisterUserAPIs()
{
    app.MapPost("/register", async (CancellationToken token, CreateUserDto userDto, IUserService userService) =>
    {
        var createdUserResult = await userService.Register(userDto, token);

        if (createdUserResult.IsT1)
        {
            return Results.BadRequest(createdUserResult.AsT1.GetErrorsString());
        }
        return Results.Ok(createdUserResult.AsT0);
    });

}

void RunMigration(WebApplication webApplication)
{
    using (var scope = webApplication.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        dbContext.Database.Migrate();
    }
}