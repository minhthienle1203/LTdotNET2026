using HotelManagement.Core.Enums;
using HotelManagement.Core.Services;

namespace HotelManagement.Core.DTOs;
public class ReservationListItemDto
{
    public int ReservationId { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime ExpectedCheckIn { get; set; }
    public DateTime ExpectedCheckOut { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public int? RoomId { get; set; }
    public string? RoomCode { get; set; }
    public decimal DepositAmount { get; set; }
    public ReservationStatus Status { get; set; }
    public string? Note { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DailyRate { get; set; }


    public int TotalHours => RoomPriceCalculator.CountHours(ExpectedCheckIn, ExpectedCheckOut);
    public int TotalDays => RoomPriceCalculator.CountDays(ExpectedCheckIn, ExpectedCheckOut);
    public decimal RoomAmountByHour => RoomPriceCalculator.ByHour(ExpectedCheckIn, ExpectedCheckOut, HourlyRate);
    public decimal RoomAmountByDay => RoomPriceCalculator.ByDay(ExpectedCheckIn, ExpectedCheckOut, DailyRate);

    public string RoomText => RoomCode ?? "(Chưa gán - giữ theo loại)";

    public string StatusText => Status switch
    {
        ReservationStatus.Confirmed => "Đã xác nhận",
        ReservationStatus.CheckedIn => "Đã nhận phòng",
        ReservationStatus.Completed => "Hoàn tất",
        ReservationStatus.Cancelled => "Đã hủy",
        ReservationStatus.NoShow => "Không đến",
        _ => Status.ToString()
    };
}
