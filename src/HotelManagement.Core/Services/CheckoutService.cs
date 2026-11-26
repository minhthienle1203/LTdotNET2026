using System;
using System.Collections.Generic;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Exceptions;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Core.Services;

public sealed class CheckoutService
{
    private readonly ICheckoutRepository _repo;
    public CheckoutService(ICheckoutRepository repo) => _repo = repo ?? throw new ArgumentNullException(nameof(repo));
    public Stay GetStay(int stayId)
    {
        if (stayId <= 0) throw new ArgumentException("StayId không hợp lệ.", nameof(stayId));
        return _repo.GetStayById(stayId) ?? throw new BusinessRuleException("Không tìm thấy lượt lưu trú.");
    }
    public Room GetRoom(int roomId)
    {
        if (roomId <= 0) throw new ArgumentException("RoomId không hợp lệ.", nameof(roomId));
        return _repo.GetRoomById(roomId) ?? throw new BusinessRuleException("Không tìm thấy thông tin phòng.");
    }
    public string GetCustomerName(int customerId) => _repo.GetCustomerNameById(customerId);
    public decimal GetDepositAmount(int? reservationId) => _repo.GetDepositByReservationId(reservationId);
    public IReadOnlyList<StayService> GetUsedServices(int stayId) => _repo.GetUsedServicesByStayId(stayId);
    public string GetServiceName(int serviceId) => _repo.GetServiceNameById(serviceId);
    public bool IsPaid(int stayId) => _repo.IsPaid(stayId);
    public decimal CalculateServiceTotal(IEnumerable<StayService> services)
    {
        decimal total = 0m;
        if (services == null) return total;
        foreach (var s in services) total += s.Amount;
        return total;
    }
    public (int ChargedUnits, decimal AppliedRate, decimal RoomCharge) CalculateRoomCharge(
        Stay stay, DateTime checkOutAt, ChargeBasis chargeBasis)
    {
        if (stay == null) throw new ArgumentNullException(nameof(stay));
        if (checkOutAt < stay.ActualCheckIn) throw new BusinessRuleException("Thời gian check-out không được nhỏ hơn thời gian check-in.");
        var span = checkOutAt - stay.ActualCheckIn;
        int units = chargeBasis == ChargeBasis.Hour ? Math.Max(1, (int)Math.Ceiling(span.TotalHours)) : Math.Max(1, (int)Math.Ceiling(span.TotalDays));
        decimal rate = chargeBasis == ChargeBasis.Hour ? stay.HourlyRateAtCheckIn : stay.DailyRateAtCheckIn;
        decimal roomCharge = decimal.Round(units * rate, 0, MidpointRounding.AwayFromZero);
        return (units, rate, roomCharge);
    }

    public (decimal SubTotal, decimal DepositDeducted, decimal AmountDue, decimal RefundDue) CalculateTotals(
        decimal roomCharge, decimal serviceCharge, decimal depositAmount)
    {
        decimal subTotal = Math.Max(0m, roomCharge) + Math.Max(0m, serviceCharge);
        decimal deposit = Math.Max(0m, depositAmount);
        decimal depositDeducted = Math.Min(deposit, subTotal);
        decimal amountDue = Math.Max(0m, subTotal - deposit);
        decimal refundDue = Math.Max(0m, deposit - subTotal);
        return (subTotal, depositDeducted, amountDue, refundDue);
    }
    public int PayAndCheckOut(
        int stayId, DateTime checkOutAt, ChargeBasis chargeBasis,
        decimal serviceCharge, PaymentMethod paymentMethod, string? note = null)
    {
        var stay = GetStay(stayId);
        if (stay.Status == StayStatus.Completed || _repo.IsPaid(stayId)) throw new BusinessRuleException("Lượt lưu trú này đã được thanh toán.");
        var room = GetRoom(stay.RoomId);
        var (units, rate, roomCharge) = CalculateRoomCharge(stay, checkOutAt, chargeBasis);
        decimal deposit = GetDepositAmount(stay.ReservationId);
        var (subTotal, depositDeducted, amountDue, refundDue) = CalculateTotals(roomCharge, serviceCharge, deposit);
        return _repo.CompleteCheckOut(
            stayId: stay.StayId,
            roomId: room.RoomId,
            actualCheckOut: checkOutAt,
            chargeBasis: chargeBasis,
            chargedUnits: units,
            appliedRate: rate,
            roomCharge: roomCharge,
            serviceCharge: serviceCharge,
            subTotal: subTotal,
            depositDeducted: depositDeducted,
            amountDue: amountDue,
            refundDue: refundDue,
            paymentMethod: paymentMethod,
            note: string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            targetRoomStatus: RoomStatus.Cleaning);
    }
}
