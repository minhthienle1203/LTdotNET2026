using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using HotelManagement.Infrastructure.Data;

namespace HotelManagement.Infrastructure.Data.Repositories;

public sealed partial class FloorRepository : IFloorRepository
{
    private readonly DbConnectionFactory  _connectionFactory;
    public FloorRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public List<Floor> GetFloors(bool includeInactive = false)
    {
        const string sql =@"SELECT FloorId, FloorNumber, FloorName, SortOrder, IsActive
                    FROM Floor
                    Where ($includeInactive = 1 OR IsActive = 1)
                    ORDER BY SortOrder, FloorNumber;";
        var floor = new List<Floor>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$includeInactive", includeInactive ? 1 : 0);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            floor.Add(new Floor
            {
                FloorId = reader.GetInt32(0),
                FloorNumber = reader.GetInt32(1),
                FloorName = reader.GetString(2),
                SortOrder = reader.GetInt32(3),
                IsActive = reader.GetInt64(4) == 1
            });
        }
        return floor;
    }
}
