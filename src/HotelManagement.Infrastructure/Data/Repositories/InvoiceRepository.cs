using System.Globalization;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Infrastructure.Data.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public InvoiceRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }
    //Xếp tu mới đến cũ
    public IReadOnlyList<Invoice> GetAll(bool includeReservations = false)
    {
        const string sql = @"
            SELECT
                InvoiceId,
                InvoiceCode,
                StayId,
                CreatedAt,
                ChargeBasis,
                ChargedUnits,
                AppliedRate,
                RoomCharge,
                ServiceCharge,
                SubTotal,
                DepositDeducted,
                AmountDue,
                RefundDue,
                Status,
                PrintedAt
            FROM Invoice
            ORDER BY InvoiceId DESC;";

        var invoices = new List<Invoice>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            invoices.Add(new Invoice
            {
                InvoiceId = reader.GetInt32(0),
                InvoiceCode = reader.GetString(1),
                StayId = reader.GetInt32(2),
                CreatedAt = DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture),
                ChargeBasis = Enum.Parse<ChargeBasis>(reader.GetString(4), ignoreCase: false),
                ChargedUnits = reader.GetInt32(5),
                AppliedRate = SqliteValueConverter.FromVnd(reader, 6),
                RoomCharge = SqliteValueConverter.FromVnd(reader, 7),
                ServiceCharge = SqliteValueConverter.FromVnd(reader, 8),
                SubTotal = SqliteValueConverter.FromVnd(reader, 9),
                DepositDeducted = SqliteValueConverter.FromVnd(reader, 10),
                AmountDue = SqliteValueConverter.FromVnd(reader, 11),
                RefundDue = SqliteValueConverter.FromVnd(reader, 12),
                Status = Enum.Parse<InvoiceStatus>(reader.GetString(13), ignoreCase: false),
                PrintedAt = reader.IsDBNull(14)
                    ? null
                    : DateTime.Parse(reader.GetString(14), CultureInfo.InvariantCulture)
            });
        }
        return invoices;
    }
    public void MarkAsPrinted(int invoiceId, DateTime printedAt)
    {
        const string sql = @"
        UPDATE Invoice
        SET PrintedAt = @PrintedAt
        WHERE InvoiceId = @InvoiceId;";

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        var pPrintedAt = command.CreateParameter();
        pPrintedAt.ParameterName = "@PrintedAt";
        pPrintedAt.Value = printedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        command.Parameters.Add(pPrintedAt);

        var pInvoiceId = command.CreateParameter();
        pInvoiceId.ParameterName = "@InvoiceId";
        pInvoiceId.Value = invoiceId;
        command.Parameters.Add(pInvoiceId);
        command.ExecuteNonQuery();
    }
}
