using Microsoft.Data.Sqlite;

namespace Portfolio.Api
{
    public class SQLiteDatabaseInitialiser
    {
        public static void Initialise(string connectionString)
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            var dbFilePath = builder.DataSource;

            // Ensure the directory exists
            var directory = Path.GetDirectoryName(dbFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Initialize the database schema
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL
                );
            ";
            createTableCmd.ExecuteNonQuery();

            // Seed initial data
            var seedDataCmd = connection.CreateCommand();
            seedDataCmd.CommandText = @"
                INSERT INTO Users (Name, Email)
                SELECT 'John Doe', 'john.doe@example.com'
                WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'john.doe@example.com');
            ";
            seedDataCmd.ExecuteNonQuery();
        }
    }
}

