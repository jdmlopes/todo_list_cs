using System.ComponentModel.DataAnnotations;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Repositories;
using TodoApi.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
DbInitializer.Initialize(connectionString);
builder.Services.AddSingleton<UserRepository>(
    new UserRepository(connectionString)
);
builder.Services.AddSingleton<TodoRepository>(
    new TodoRepository(connectionString)
);
builder.Services.AddSingleton<JwtService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var key = builder.Configuration["Jwt:Key"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/register", (RegisterRequest request, UserRepository users) =>
{
    var existingUser = users.GetByEmail(request.Email);

    if (existingUser != null)
        return Results.BadRequest("This email is already in use");

    var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

    var user = new User
    {
        Name = request.Name,
        Email = request.Email,
        PasswordHash = hash
    };

    users.Create(user);

    return Results.Ok("User created");
});

app.MapPost("/login", (RegisterRequest request, UserRepository users, JwtService jwt) =>
{
    var user = users.GetByEmail(request.Email);

    if (user == null)
        return Results.Unauthorized();

    bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

    if (!valid)
        return Results.Unauthorized();
    string token = jwt.GenerateToken(user);

    return Results.Ok(new { token });
});

app.MapGet("/todos", [Authorize] (
    TodoRepository repo,
    HttpContext context,
    int page = 1,
    int limit = 10,
    bool? completed = null,
    string? orderBy = null,
    string direction = "asc") =>
{
    var email = context.User.Identity!.Name!;
    var todos = repo.GetByUser(email,page,limit, completed,orderBy,direction);
    var total = repo.CountByUser(email,completed);

    return Results.Ok(new
        {
            data = todos,
            page,
            limit,
            total
        }
    );
});

app.MapGet("/todos/{id}", [Authorize] (int id, TodoRepository repo, HttpContext context) =>
{
    var email = context.User.Identity!.Name!;
    Todo? todo = repo.GetById(id,email);
    return todo is null ? Results.NotFound() : Results.Ok(todo);

});

app.MapPost("/todos", [Authorize] (Todo newTodo, TodoRepository repo, HttpContext context) =>
{
    var errors = ValidationHelper.Validate(newTodo);
    if (errors != null)
        return Results.BadRequest(errors);
    var email = context.User.Identity!.Name!;
    newTodo.UserEmail = email;
    repo.Create(newTodo);
    return Results.Created($"/todos/{newTodo.Id}", newTodo);
});

app.MapPut("/todos/{id}", [Authorize] (int id, Todo updatedTodo, TodoRepository repo, HttpContext context) =>
{
    var errors = ValidationHelper.Validate(updatedTodo);
    if (errors != null)
        return Results.BadRequest(errors);
    var email = context.User.Identity!.Name!;
    Todo? todo = repo.Update(id, email, updatedTodo);
    return todo is null ? Results.NotFound() : Results.Ok(todo);

});

app.MapDelete("/todos/{id}", [Authorize] (int id, TodoRepository repo, HttpContext context) =>
{
    var email = context.User.Identity!.Name!;
    if (repo.Delete(id,email))
    {
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.Run();

