
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

    public User? GetByUsername(string username)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Username, PasswordHash
                FROM Users
                WHERE Username = $username;
            ";
            command.Parameters.AddWithValue("$username", username);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2)
            };
        }
    }

    public int Create(User user)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users (Username,PasswordHash) 
                VALUES($username,$passwordhash);
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$username", user.Username);
            command.Parameters.AddWithValue("$passwordhash", user.PasswordHash);
            var id = command.ExecuteScalar();
            user.Id = (int)(long)id!;
            return user.Id;
        }
    }
}