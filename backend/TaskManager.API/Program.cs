using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Services;
using TaskManager.Application.Services.Interfaces;
using TaskManager.API.Middlewares;
using TaskManager.Infrastructure.Data;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options => {
        options.InvalidModelStateResponseFactory = context => {
            var problemDetails = new ValidationProblemDetails(context.ModelState) {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "One or more request fields are invalid.",
                Instance = context.HttpContext.Request.Path
            };

            return new BadRequestObjectResult(problemDetails);
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();