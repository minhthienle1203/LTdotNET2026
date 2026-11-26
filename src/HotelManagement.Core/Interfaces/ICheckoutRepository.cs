using System;
using System.Collections.Generic;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;

namespace HotelManagement.Core.Interfaces;

public interface ICheckoutRepository
{
    Stay? GetStayById(int stayId);
    Room? GetRoomById(int roomId);
    string GetCustomerNameById(int customerId);
    decimal GetDepositByReservationId(int? reservationId);
    IReadOnlyList<StayService> GetUsedServicesByStayId(int stayId);
    string GetServiceNameById(int serviceId);
    bool IsPaid(int stayId);
    int CompleteCheckOut(
        int stayId,
        int roomId,
        DateTime actualCheckOut,
        ChargeBasis chargeBasis,
        int chargedUnits,
        decimal appliedRate,
        decimal roomCharge,
        decimal serviceCharge,
        decimal subTotal,
        decimal depositDeducted,
        decimal amountDue,
        decimal refundDue,
        PaymentMethod paymentMethod,
        string? note,
        RoomStatus targetRoomStatus);
}
