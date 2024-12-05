using Microsoft.Data.Sqlite;
using Portfolio.Api.Models;

namespace Portfolio.Api.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly SqliteConnection _sqliteConnection;

        public UserRepository(SqliteConnection sqliteConnection)
        {
            _sqliteConnection = sqliteConnection;
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            await _sqliteConnection.OpenAsync();
            var command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Email FROM Users";
            using var reader = await command.ExecuteReaderAsync();

            var users = new List<UserModel>();
            while (await reader.ReadAsync())
            {
                users.Add(new UserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2)
                });
            }

            return users;
        }

        public async Task<UserModel> GetUserByIdAsync(int id)
        {
            await _sqliteConnection.OpenAsync();
            var command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Email FROM Users WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2)
                };
            }

            return null;
        }

        public async Task AddUserAsync(UserModel user)
        {
            await _sqliteConnection.OpenAsync();
            var command = _sqliteConnection.CreateCommand();
            command.CommandText = "INSERT INTO Users (Name, Email) VALUES (@name, @email)";
            command.Parameters.AddWithValue("@name", user.Name);
            command.Parameters.AddWithValue("@email", user.Email);
            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateUserAsync(UserModel user)
        {
            await _sqliteConnection.OpenAsync();
            var command = _sqliteConnection.CreateCommand();
            command.CommandText = "UPDATE Users SET Name = @name, Email = @email WHERE Id = @id";
            command.Parameters.AddWithValue("@name", user.Name);
            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@email", user.Email);
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            await _sqliteConnection.OpenAsync();
            var command = _sqliteConnection.CreateCommand();
            command.CommandText = "DELETE FROM Users WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
        }
    }
}
