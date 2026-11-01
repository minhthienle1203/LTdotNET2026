using Microsoft.Data.SqlClient;

namespace HotelManagement.Infrastructure.Data;

/// <summary>
/// Tạo SqlConnection cho các Repository. Connection string được truyền từ
/// tầng khởi tạo ứng dụng; không đặt thông tin kết nối trực tiếp trong Repository.
/// </summary>
public sealed class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string không được để trống.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public SqlConnection CreateConnection() => new(_connectionString);
}
