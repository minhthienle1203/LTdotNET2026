using HotelManagement.Core.DTOs;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Core.Services;

public sealed class ActiveStayService(IActiveStayRepository repository, Func<DateTime>? clock = null)
{
    private readonly Func<DateTime> _clock = clock ?? (() => DateTime.Now);

    public DateTime Now => _clock();

    public ActiveStaySnapshotDto Load() => repository.Load(Now);

    public static bool IsOverdue(ActiveStayDto stay, DateTime now) => stay.ExpectedCheckOut < now;

    public static IReadOnlyList<ActiveStayDto> Filter(ActiveStaySnapshotDto data, ActiveStayFilter filter, DateTime now)
    {
        var keyword = filter.Keyword?.Trim();
        bool Has(string value) => value.Contains(keyword!, StringComparison.CurrentCultureIgnoreCase);

        return data.Stays
            .Where(s => filter.FloorId is null || s.FloorId == filter.FloorId)
            .Where(s => filter.RoomTypeId is null || s.RoomTypeId == filter.RoomTypeId)
            .Where(s => filter.Due switch
            {
                DueFilter.Today => s.ExpectedCheckOut.Date == now.Date,
                DueFilter.Overdue => IsOverdue(s, now),
                DueFilter.Later => s.ExpectedCheckOut.Date > now.Date,
                _ => true
            })
            .Where(s => string.IsNullOrEmpty(keyword) || Has(s.RoomCode) || Has(s.CustomerName)
                        || Has(s.Phone) || Has(s.IdentityNumber) || Has(s.ReservationCode ?? ""))
            .ToList();
    }

    public static ActiveStaySummaryDto Summarize(ActiveStaySnapshotDto data, DateTime now)
    {
        var occupied = data.Stays.Select(s => s.RoomId).Distinct().Count();
        return new ActiveStaySummaryDto(
            occupied, data.ActiveRoomCount,
            data.ActiveRoomCount == 0 ? 0 : Math.Round(occupied * 100m / data.ActiveRoomCount, 1),
            data.Stays.Sum(s => s.GuestCount),
            data.Stays.Sum(s => s.ServiceTotal),
            data.Stays.Sum(s => s.ServiceLines),
            data.Stays.Count(s => s.ExpectedCheckOut.Date == now.Date),
            data.Stays.Count(s => IsOverdue(s, now)),
            data.CheckoutPendingCount);
    }

    public static ChargeEstimateDto Estimate(ActiveStayDto stay, ChargeBasis basis, DateTime now)
    {
        var ticks = Math.Max(0, (now - stay.ActualCheckIn).Ticks);
        var hours = (ticks + TimeSpan.TicksPerHour - 1) / TimeSpan.TicksPerHour;
        var units = (int)Math.Max(1, basis == ChargeBasis.Hour ? hours : (hours + 23) / 24);
        var rate = basis == ChargeBasis.Hour ? stay.HourlyRate : stay.DailyRate;
        var roomCharge = units * rate;
        var subTotal = roomCharge + stay.ServiceTotal;

        return new ChargeEstimateDto(basis, units, rate, roomCharge, stay.ServiceTotal, subTotal,
            stay.DepositAmount,
            Math.Max(0, subTotal - stay.DepositAmount),
            Math.Max(0, stay.DepositAmount - subTotal));
    }

    public static string DurationText(DateTime from, DateTime now)
    {
        var span = now > from ? now - from : TimeSpan.Zero;
        return span.TotalDays >= 1
            ? $"{(int)span.TotalDays}n {span.Hours:00}h"
            : $"{span.Hours:00}h {span.Minutes:00}m";
    }
}
