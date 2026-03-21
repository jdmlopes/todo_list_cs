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

    // public List<Todo> GetAll()
    // {
    //     using (var connection = new SqliteConnection(connectionString))
    //     {
    //         connection.Open();
    //         var command = connection.CreateCommand();
    //         command.CommandText = @"
    //             SELECT Id, Title, Description, Completed FROM Todos
    //         ";
    //         using var reader = command.ExecuteReader();
    //         List<Todo> todos = new();
    //         while (reader.Read())
    //         {
    //             Todo todo = new Todo
    //             {
    //                 Id = reader.GetInt32(0),
    //                 Title = reader.GetString(1),
    //                 Description = reader.GetString(2),
    //                 Completed = reader.GetInt32(3) == 1
    //             };
    //             todos.Add(todo);
    //         }
    //         return todos;
    //     }
    // }
    public List<Todo> GetByUser(string email,
        int page,
        int limit,
        bool? completed,
        string? orderBy,
        string direction)
    {
        List<Todo> todos = new();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();

            string query = @"
                SELECT Id, Title, Description, Completed, UserEmail
                FROM Todos
                WHERE UserEmail = $useremail
                
            ";
            // filtro
            if (completed != null)
            {
                query += " AND Completed = $completed";
                command.Parameters.AddWithValue("$completed", completed);
            }
            // ordenação
            var sortableColumns = new Dictionary<string, string>
            {
                ["id"] = "Id",
                ["title"] = "Title",
                ["completed"] = "Completed"
            };
            if (!string.IsNullOrEmpty(orderBy) &&  sortableColumns.TryGetValue(orderBy.ToLower(), out var column))
            {
                direction = direction.ToLower() == "desc" ? "DESC" : "ASC";
                query += $" ORDER BY {column} COLLATE NOCASE {direction}";
            }
            else
            {
                query += " ORDER BY Id ASC";
            }
            query += " LIMIT $limit OFFSET $offset;";
            int offset = (page - 1) * limit;
            command.CommandText = query;
            command.Parameters.AddWithValue("$useremail", email);
            command.Parameters.AddWithValue("$limit", limit);
            command.Parameters.AddWithValue("$offset", offset);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                todos.Add(new Todo
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    Completed = reader.GetBoolean(3),
                    UserEmail = reader.GetString(4)
                });
            }
        }
        return todos;
    }

    public Todo? GetById(int id, string email)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Title, Description, Completed, UserEmail
                FROM Todos
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
                Completed = reader.GetInt32(3) == 1,
                UserEmail = reader.GetString(4)
            };

            return todo.UserEmail == email ? todo : null;

        }
    }



    public int Create(Todo todo)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Todos (Title,Description,Completed,UserEmail) 
                VALUES($title,$description,$completed,$useremail);
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$title", todo.Title);
            command.Parameters.AddWithValue("$description", todo.Description);
            command.Parameters.AddWithValue("$completed", todo.Completed);
            command.Parameters.AddWithValue("$useremail", todo.UserEmail);
            var id = command.ExecuteScalar();
            todo.Id = (int)(long)id!;
            return todo.Id;
        }
    }

    public Todo? Update(int id, string email, Todo todo)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Todos
                SET Title = $title,
                    Description = $description,
                    Completed = $completed
                WHERE Id = $id AND UserEmail = $useremail
            ";
            command.Parameters.AddWithValue("$title", todo.Title);
            command.Parameters.AddWithValue("$description", todo.Description);
            command.Parameters.AddWithValue("$completed", todo.Completed);
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$useremail", email);
            if (command.ExecuteNonQuery() == 0)
            {
                return null;
            }
            todo.Id = id;
            return todo;
        }

    }

    public bool Delete(int id, string email)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM Todos
                WHERE Id = $id AND UserEmail = $useremail
            ";
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$useremail", email);
            if (command.ExecuteNonQuery() == 0)
            {
                return false;
            }
            return true;
        }

    }

    public int CountByUser(string email, bool? completed)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            var query = @"
                SELECT COUNT(*)
                FROM Todos
                WHERE UserEmail = $email
            ";
            if (completed != null)
            {
                query += " AND Completed = $completed";
                command.Parameters.AddWithValue("$completed", completed);
            }
            command.CommandText = query;
            command.Parameters.AddWithValue("$email", email);
            return Convert.ToInt32(command.ExecuteScalar());

        }
    }
}