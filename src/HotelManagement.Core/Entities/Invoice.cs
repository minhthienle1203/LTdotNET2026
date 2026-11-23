using HotelManagement.Core.Enums;

namespace HotelManagement.Core.Entities;

public class Invoice
{
    public int InvoiceId { get; set; }
    public string InvoiceCode { get; set; } = string.Empty;
    public int StayId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ChargeBasis ChargeBasis { get; set; }
    public int ChargedUnits { get; set; }
    public decimal AppliedRate { get; set; }
    public decimal RoomCharge { get; set; }
    public decimal ServiceCharge { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DepositDeducted { get; set; }
    public decimal AmountDue { get; set; }
    public decimal RefundDue { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
    public DateTime? PrintedAt { get; set; }
}
