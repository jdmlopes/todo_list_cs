
using Microsoft.Data.Sqlite;
using TodoApi.Models;
namespace TodoApi.Repositories;

public class UserRepository
{
    private readonly string connectionString;

    public UserRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public User? GetByEmail(string email)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Name, Email, PasswordHash
                FROM Users
                WHERE Email = $email;
            ";
            command.Parameters.AddWithValue("$email", email);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            return new User
            {
                Name = reader.GetString(0),
                Email = reader.GetString(1),
                PasswordHash = reader.GetString(2)
            };
        }
    }

    public string Create(User user)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users (Name, Email, PasswordHash) 
                VALUES($name, $email, $passwordhash);
            ";
            command.Parameters.AddWithValue("$name", user.Name);
            command.Parameters.AddWithValue("$email", user.Email);
            command.Parameters.AddWithValue("$passwordhash", user.PasswordHash);
            command.ExecuteNonQuery();
            return user.Email;
        }
    }
}