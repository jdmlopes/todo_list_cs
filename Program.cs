using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

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

List<Todo> todoList = new()
    {
        new Todo{Id = 1, Title = "Do Groceries", Description = "1. 2kg potatos\n2. 1kg Onions\n3. Pasta", Completed = false},
        new Todo{Id = 2, Title = "Walk the dog", Description = "Toto needs his afternoon walk", Completed = false},
        new Todo{Id = 3, Title = "Morning jog", Description = "Jog in the park for at least 30 mins", Completed = true }
    };
int maxId = todoList.Count == 0 ? 0 : todoList.Max(t => t.Id);

app.MapGet("/todos", () =>
{
    return Results.Ok(todoList);
});

app.MapGet("/todos/{id}", (int id) =>
{
    Todo? todo = todoList.Find(t => t.Id == id);
    if (todo == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(todo);
});

app.MapPost("/todos", (Todo newTodo) =>
{
    ValidationContext validationContext = new ValidationContext(newTodo);
    List<ValidationResult> errors = new();
    if (!Validator.TryValidateObject(newTodo,validationContext,errors,true))
    {
        return Results.BadRequest(errors);
    }
    maxId++;
    newTodo.Id = maxId;
    todoList.Add(newTodo);
    return Results.Created($"/todos/{newTodo.Id}", newTodo);
});

app.MapPut("/todos/{id}", (int id, Todo updatedTodo) =>
{
    ValidationContext validationContext = new ValidationContext(updatedTodo);
    List<ValidationResult> errors = new();
    if (!Validator.TryValidateObject(updatedTodo,validationContext,errors,true))
    {
        return Results.BadRequest(errors);
    }
    Todo? todo = todoList.Find(t => t.Id == id);
    if (todo is null)
    {
        return Results.NotFound();
    }
    todo.Title = updatedTodo.Title;
    todo.Description = updatedTodo.Description;
    todo.Completed = updatedTodo.Completed;
    return Results.Ok(todo);
});

app.MapDelete("/todos/{id}", (int id) =>
{
    Todo? todo = todoList.Find(t => t.Id == id);
    if (todo is null)
    {
        return Results.NotFound();
    }
    todoList.Remove(todo);
    return Results.NoContent();
});

app.Run();

