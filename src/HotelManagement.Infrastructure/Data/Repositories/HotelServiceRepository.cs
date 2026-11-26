using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Infrastructure.Data.Repositories;

public sealed class HotelServiceRepository : IHotelServiceRepository
{
    private readonly DbConnectionFactory _connectionFactory;
    public HotelServiceRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }
    public IReadOnlyList<HotelService> GetAll(bool includeInactive = true)
    {
        const string sql = @"
            SELECT ServiceId, ServiceCode, ServiceName, Category, UnitName, UnitPrice, Description, IsActive
            FROM HotelService
            WHERE ($includeInactive = 1 OR IsActive = 1)
            ORDER BY ServiceId;";

        var services = new List<HotelService>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$includeInactive", includeInactive ? 1 : 0);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            services.Add(new HotelService
            {
                ServiceId = reader.GetInt32(0),
                ServiceCode = reader.GetString(1),
                ServiceName = reader.GetString(2),
                Category = Enum.Parse<ServiceCategory>(reader.GetString(3), ignoreCase: false),
                UnitName = reader.GetString(4),
                UnitPrice = SqliteValueConverter.FromVnd(reader, 5),
                Description = reader.IsDBNull(6) ? null : reader.GetString(6),
                IsActive = reader.GetInt64(7) == 1
            });
        }
        return services;
    }
}

