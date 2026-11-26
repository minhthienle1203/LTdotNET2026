using HotelManagement.Core.DTOs;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Core.Services;

public sealed class ReportService
{
    private readonly IReportRepository _repository;

    public ReportService(IReportRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public MonthlyReportDto GetMonthlyReport(int year, int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month));
        if (year<1)
            throw new ArgumentOutOfRangeException(nameof(year));
        var report = _repository.GetMonthlyReport(year, month, DateTime.Now);
        report.DaysInMonth = DateTime.DaysInMonth(year, month);
        report.OccupiedDays = (report.OccupiedDays ?? new List<int>())
            .Where(day => day >= 1 && day <= report.DaysInMonth).Distinct().OrderBy(day => day).ToList();

        // số phòng * tổng ngày/tháng đó * 24h
        report.TotalRoomHours = report.ActiveRoomCount * report.DaysInMonth * 24m;
        report.OccupancyRate = report.TotalRoomHours <= 0
            ? 0m
            : Math.Round(report.OccupiedRoomHours / report.TotalRoomHours * 100m, 2, MidpointRounding.AwayFromZero);
        return report;
    }
}
