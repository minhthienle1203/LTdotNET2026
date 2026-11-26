using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface IReportRepository
{
    MonthlyReportDto  GetMonthlyReport(int year, int month, DateTime asOf);
}
