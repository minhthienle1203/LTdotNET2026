using System;
using System.Collections.Generic;
using System.Globalization;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using HotelManagement.Infrastructure.Data;

namespace HotelManagement.Infrastructure.Data.Repositories;

public sealed class StayServiceRepository : IStayServiceRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public StayServiceRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public void Add(StayService item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        const string sql = @"
            INSERT INTO StayService (StayId, ServiceId, Quantity, UnitPrice, Amount, UsedAt, Note)
            VALUES (@StayId, @ServiceId, @Quantity, @UnitPrice, @Amount, @UsedAt, @Note);";

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParam(command, "@StayId", item.StayId);
        AddParam(command, "@ServiceId", item.ServiceId);
        AddParam(command, "@Quantity", item.Quantity.ToString("0.###", CultureInfo.InvariantCulture));
        AddParam(command, "@UnitPrice", item.UnitPrice);
        AddParam(command, "@Amount", item.Amount);
        AddParam(command, "@UsedAt", item.UsedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        AddParam(command, "@Note", (object?)item.Note ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void Delete(int stayServiceId)
    {
        const string sql = "DELETE FROM StayService WHERE StayServiceId = @StayServiceId;";
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParam(command, "@StayServiceId", stayServiceId);
        command.ExecuteNonQuery();
    }

    public StayService? GetById(int stayServiceId)
    {
        const string sql = @"
            SELECT StayServiceId, StayId, ServiceId, Quantity, UnitPrice, Amount, UsedAt, Note
            FROM StayService
            WHERE StayServiceId = @StayServiceId;";

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParam(command, "@StayServiceId", stayServiceId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        return new StayService
        {
            StayServiceId = reader.GetInt32(0),
            StayId = reader.GetInt32(1),
            ServiceId = reader.GetInt32(2),
            Quantity = decimal.Parse(reader.GetString(3), CultureInfo.InvariantCulture),
            UnitPrice = reader.GetDecimal(4),
            Amount = reader.GetDecimal(5),
            UsedAt = DateTime.ParseExact(reader.GetString(6), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            Note = reader.IsDBNull(7) ? null : reader.GetString(7)
        };
    }

    public IReadOnlyList<StayService> GetByStayId(int stayId)
    {
        const string sql = @"
            SELECT StayServiceId, StayId, ServiceId, Quantity, UnitPrice, Amount, UsedAt, Note
            FROM StayService
            WHERE StayId = @StayId
            ORDER BY UsedAt DESC, StayServiceId DESC;";

        var result = new List<StayService>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParam(command, "@StayId", stayId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new StayService
            {
                StayServiceId = reader.GetInt32(0),
                StayId = reader.GetInt32(1),
                ServiceId = reader.GetInt32(2),
                Quantity = decimal.Parse(reader.GetString(3), CultureInfo.InvariantCulture),
                UnitPrice = reader.GetDecimal(4),
                Amount = reader.GetDecimal(5),
                UsedAt = DateTime.ParseExact(reader.GetString(6), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                Note = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        }
        return result;
    }
    private static void AddParam(System.Data.IDbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}
