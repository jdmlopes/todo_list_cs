using System.ComponentModel.DataAnnotations;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Repositories;
using TodoApi.Utils;

var builder = WebApplication.CreateBuilder(args);
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
DbInitializer.Initialize(connectionString);
builder.Services.AddSingleton<UserRepository>(
    new UserRepository(connectionString)
);
builder.Services.AddSingleton<TodoRepository>(
    new TodoRepository(connectionString)
);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/register", (RegisterRequest request, UserRepository users) =>
{
    var existingUser = users.GetByUsername(request.Username);

    if (existingUser != null)
        return Results.BadRequest("Username already exists");

    var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

    var user = new User
    {
        Username = request.Username,
        PasswordHash = hash
    };

    users.Create(user);

    return Results.Ok("User created");
});

app.MapPost("/login", (RegisterRequest request, UserRepository users) =>
{
    var user = users.GetByUsername(request.Username);

    if (user == null)
        return Results.Unauthorized();

    bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

    if (!valid)
        return Results.Unauthorized();

    return Results.Ok("Login successful");
});

app.MapGet("/todos", (TodoRepository repo) =>
{
    return Results.Ok(repo.GetAll());
});

app.MapGet("/todos/{id}", (int id, TodoRepository repo) =>
{
    Todo? todo = repo.GetById(id);
    return todo is null ? Results.NotFound() : Results.Ok(todo);

});

app.MapPost("/todos", (Todo newTodo, TodoRepository repo) =>
{
    var errors = ValidationHelper.Validate(newTodo);
    if (errors != null)
        return Results.BadRequest(errors);

    repo.Create(newTodo);
    return Results.Created($"/todos/{newTodo.Id}", newTodo);
});

app.MapPut("/todos/{id}", (int id, Todo updatedTodo, TodoRepository repo) =>
{
    var errors = ValidationHelper.Validate(updatedTodo);
    if (errors != null)
        return Results.BadRequest(errors);
    Todo? todo = repo.Update(id, updatedTodo);
    return todo is null ? Results.NotFound() : Results.Ok(todo);

});

app.MapDelete("/todos/{id}", (int id, TodoRepository repo) =>
{
    if (repo.Delete(id))
    {
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.Run();

