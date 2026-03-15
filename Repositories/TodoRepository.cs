using System.Data.Common;
using Microsoft.Data.Sqlite;
using TodoApi.Models;

namespace TodoApi.Repositories;

public class TodoRepository
{
    private readonly string connectionString;

    public TodoRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public List<Todo> GetAll()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Title, Description, Completed FROM Todos
            ";
            using var reader = command.ExecuteReader();
            List<Todo> todos = new();
            while (reader.Read())
            {
                Todo todo = new Todo
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    Completed = reader.GetInt32(3) == 1
                };
                todos.Add(todo);
            }
            return todos;
        }
    }

    public Todo? GetById(int id)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Title, Description, Completed FROM Todos
                WHERE Id = $id;
            ";
            command.Parameters.AddWithValue("$id", id);
            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return null;

            Todo todo = new Todo
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.GetString(2),
                Completed = reader.GetInt32(3) == 1
            };
            return todo;

        }
    }

    public int Create(Todo todo)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Todos (Title,Description,Completed) 
                VALUES($title,$description,$completed);
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$title", todo.Title);
            command.Parameters.AddWithValue("$description", todo.Description);
            command.Parameters.AddWithValue("$completed", todo.Completed);
            var id = command.ExecuteScalar();
            todo.Id = (int)(long)id!;
            return todo.Id;
        }
    }

    public Todo? Update(int id, Todo todo)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Todos
                SET Title = $title, Description = $description, Completed = $completed
                WHERE Id = $id
            ";
            command.Parameters.AddWithValue("$title", todo.Title);
            command.Parameters.AddWithValue("$description", todo.Description);
            command.Parameters.AddWithValue("$completed", todo.Completed);
            command.Parameters.AddWithValue("$id", id);
            if (command.ExecuteNonQuery() == 0)
            {
                return null;
            }
            todo.Id = id;
            return todo;
        }

    }

    public bool Delete(int id)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM Todos
                WHERE Id = $id
            ";
            command.Parameters.AddWithValue("$id", id);
            if (command.ExecuteNonQuery() == 0)
            {
                return false;
            }
            return true;
        }

    }
}