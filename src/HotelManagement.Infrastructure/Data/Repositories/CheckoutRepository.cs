using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Infrastructure.Data.Repositories;

public sealed class CheckoutRepository : ICheckoutRepository
{
    private readonly DbConnectionFactory _factory;
    public CheckoutRepository(DbConnectionFactory factory) => _factory = factory;

    public Stay? GetStayById(int stayId)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Stay WHERE StayId = @id;";
        AddParam(cmd, "@id", stayId);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Stay
        {
            StayId = Convert.ToInt32(r["StayId"]),
            ReservationId = r["ReservationId"] == DBNull.Value ? null : Convert.ToInt32(r["ReservationId"]),
            CustomerId = Convert.ToInt32(r["CustomerId"]),
            RoomId = Convert.ToInt32(r["RoomId"]),
            ActualCheckIn = DateTime.Parse(r["ActualCheckIn"].ToString()!),
            HourlyRateAtCheckIn = Convert.ToDecimal(r["HourlyRateAtCheckIn"]),
            DailyRateAtCheckIn = Convert.ToDecimal(r["DailyRateAtCheckIn"]),
            Status = Enum.Parse<StayStatus>(r["Status"].ToString()!, true)
        };
    }

    public Room? GetRoomById(int roomId)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT RoomId, RoomCode, Status FROM Room WHERE RoomId = @id;";
        AddParam(cmd, "@id", roomId);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Room
        {
            RoomId = Convert.ToInt32(r["RoomId"]),
            RoomCode = r["RoomCode"].ToString()!,
            Status = Enum.Parse<RoomStatus>(r["Status"].ToString()!, true)
        };
    }
    public string GetCustomerNameById(int customerId) =>
        Scalar("SELECT FullName FROM Customer WHERE CustomerId = @id;", "@id", customerId)?.ToString() ?? "";
    public decimal GetDepositByReservationId(int? resId) =>
        resId == null ? 0m : Convert.ToDecimal(Scalar("SELECT DepositAmount FROM Reservation WHERE ReservationId = @id;", "@id", resId) ?? 0m);
    public string GetServiceNameById(int serviceId) =>
        Scalar("SELECT ServiceName FROM HotelService WHERE ServiceId = @id;", "@id", serviceId)?.ToString() ?? $"DV #{serviceId}";
    public bool IsPaid(int stayId) =>
        Scalar("SELECT 1 FROM Invoice WHERE StayId = @id AND Status = 'Paid' LIMIT 1;", "@id", stayId) != null;
    public IReadOnlyList<StayService> GetUsedServicesByStayId(int stayId)
    {
        var list = new List<StayService>();
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM StayService WHERE StayId = @id ORDER BY UsedAt;";
        AddParam(cmd, "@id", stayId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new StayService
            {
                StayServiceId = Convert.ToInt32(r["StayServiceId"]),
                StayId = stayId,
                ServiceId = Convert.ToInt32(r["ServiceId"]),
                Quantity = Convert.ToDecimal(r["Quantity"]),
                UnitPrice = Convert.ToDecimal(r["UnitPrice"]),
                Amount = Convert.ToDecimal(r["Amount"]),
                UsedAt = DateTime.Parse(r["UsedAt"].ToString()!)
            });
        }
        return list;
    }

    public int CompleteCheckOut(
        int stayId, int roomId, DateTime actualCheckOut, ChargeBasis chargeBasis,
        int chargedUnits, decimal appliedRate, decimal roomCharge, decimal serviceCharge,
        decimal subTotal, decimal depositDeducted, decimal amountDue, decimal refundDue,
        PaymentMethod paymentMethod, string? note, RoomStatus targetRoomStatus)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string codeSuffix = $"{DateTime.Now:yyyyMM}-{stayId}-{DateTime.Now:HHmmss}";

            // 1. Tạo Invoice
            using var invCmd = conn.CreateCommand();
            invCmd.Transaction = tx;
            invCmd.CommandText = @"
                INSERT INTO Invoice (InvoiceCode, StayId, CreatedAt, ChargeBasis, ChargedUnits, AppliedRate, RoomCharge, ServiceCharge, SubTotal, DepositDeducted, AmountDue, RefundDue, Status)
                VALUES (@code, @stayId, @now, @basis, @units, @rate, @room, @svc, @sub, @dep, @due, @ref, 'Paid');
                SELECT last_insert_rowid();";
            AddParam(invCmd, "@code", $"INV-{codeSuffix}");
            AddParam(invCmd, "@stayId", stayId);
            AddParam(invCmd, "@now", now);
            AddParam(invCmd, "@basis", chargeBasis.ToString());
            AddParam(invCmd, "@units", chargedUnits);
            AddParam(invCmd, "@rate", appliedRate);
            AddParam(invCmd, "@room", roomCharge);
            AddParam(invCmd, "@svc", serviceCharge);
            AddParam(invCmd, "@sub", subTotal);
            AddParam(invCmd, "@dep", depositDeducted);
            AddParam(invCmd, "@due", amountDue);
            AddParam(invCmd, "@ref", refundDue);
            int invoiceId = Convert.ToInt32(invCmd.ExecuteScalar());
            // 2. Tạo Payment
            ExecTx(conn, tx,
                "INSERT INTO Payment (InvoiceId, PaymentCode, Amount, Method, Status, PaidAt, Note) VALUES (@invId, @code, @amt, @method, 'Completed', @now, @note);",
                ("@invId", invoiceId), ("@code", $"PAY-{codeSuffix}"), ("@amt", amountDue),
                ("@method", paymentMethod.ToString()), ("@now", now), ("@note", note));

            // 3. Đóng Stay & chuyển Rom thanành Cleaning
            ExecTx(conn, tx, "UPDATE Stay SET ActualCheckOut = @out, Status = 'Completed' WHERE StayId = @id;",
                ("@out", actualCheckOut.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)), ("@id", stayId));

            ExecTx(conn, tx, "UPDATE Room SET Status = @st WHERE RoomId = @id;",
                ("@st", targetRoomStatus.ToString()), ("@id", roomId));
            tx.Commit();
            return invoiceId;
        }
        catch { tx.Rollback(); throw; }
    }

    private object? Scalar(string sql, string paramName, object val)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParam(cmd, paramName, val);
        return cmd.ExecuteScalar();
    }

    private static void ExecTx(IDbConnection conn, IDbTransaction tx, string sql, params (string, object?)[] ps)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        foreach (var (k, v) in ps) AddParam(cmd, k, v);
        cmd.ExecuteNonQuery();
    }

    private static void AddParam(IDbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }
}
