using Microsoft.Data.Sqlite;

namespace HotelManagement.Infrastructure.Data;

///Tạo kết nối ADO.NET đến file SQLite
public sealed class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException(
                "Connection string không được để trống.",
                nameof(connectionString));

        var builder = new SqliteConnectionStringBuilder(connectionString);

        builder.DataSource = Environment.ExpandEnvironmentVariables(
            builder.DataSource);

        if (builder.DataSource != ":memory:")
        {
            builder.DataSource = Path.GetFullPath(builder.DataSource);

            string? directory = Path.GetDirectoryName(builder.DataSource);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
        }

        builder.Mode = SqliteOpenMode.ReadWriteCreate;
        builder.ForeignKeys = true;
        builder.Pooling = true;
        builder.DefaultTimeout = 5;

        _connectionString = builder.ToString();
    }

    public SqliteConnection CreateConnection() => new(_connectionString);
}
