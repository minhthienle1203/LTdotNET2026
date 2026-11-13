using System.Reflection;
using Microsoft.Data.Sqlite;

namespace HotelManagement.Infrastructure.Data;

/// init schema và dữ liệu demo khi run với SQLite lần đầu
public sealed class DbInitializer
{
    private const string ResourcePrefix = "HotelManagement.Infrastructure.Database.";

    public void Initialize(DbConnectionFactory connectionFactory)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        ExecuteScript(connection, "01_CreateDatabase.sql");
        var isNewDatabase = !TableExists(connection, "Floor");

        using var transaction = connection.BeginTransaction();
        ExecuteScript(connection, "02_CreateTables.sql", transaction);
        ExecuteScript(connection, "03_CreateConstraintsAndIndexes.sql", transaction);

        if (isNewDatabase)
            ExecuteScript(connection, "04_SeedData.sql", transaction);

        transaction.Commit();
    }

    private static bool TableExists(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $name LIMIT 1;";
        command.Parameters.AddWithValue("$name", tableName);
        return command.ExecuteScalar() is not null;
    }

    private static void ExecuteScript(SqliteConnection connection, string fileName, SqliteTransaction? transaction = null)
    {
        var resourceName = ResourcePrefix + fileName;
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Không tìm thấy SQL resource: {resourceName}");
        using var reader = new StreamReader(stream);
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = reader.ReadToEnd();
        command.ExecuteNonQuery();
    }
}
