using Scalar.AspNetCore;
using TaskBoard.Api.Interfaces;
using TaskBoard.Api.Models;
using TaskBoard.Api.Services;
using TaskBoard.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<INotificationChannel, NotificationService>();
builder.Services.AddHostedService<NotificationWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

var tasks = new List<FreelanceTask>();


app.MapGet("/tasks", () =>
{
    return Results.Ok(tasks);
});

app.MapPost("/tasks", async (CreateTaskCommand command, INotificationChannel notificationChannel, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(command.Title))
        return Results.BadRequest("Title is required");
    var newTask = new FreelanceTask
    {
        Id = Guid.NewGuid(),
        Name = command.Title,
        Description = command.Description,
        CreatedAt = DateTime.UtcNow,
        Status = "New"
    };
    tasks.Add(newTask);
    await notificationChannel.WriteAsync(new Notification
    {
        Type = "TaskCreated",
        Payload = newTask.Id.ToString(),
        CreatedAt = DateTime.UtcNow
    }, ct);
    return Results.Created($"/tasks/{newTask.Id}", newTask);
});

app.MapGet("/tasks/{id:guid}", (Guid id) =>
{
    var task = tasks.FirstOrDefault(x => x.Id == id);
    return task is null ? Results.NotFound() : Results.Ok(task);
});

app.Run();
