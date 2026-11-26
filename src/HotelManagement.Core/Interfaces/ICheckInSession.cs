using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;

namespace HotelManagement.Core.Interfaces;

public interface ICheckInSession
{
    IReadOnlyList<CheckInReservationDto> GetReservations(ReservationStatus? status, string? keyword);
    CheckInReservationDto? GetReservation(int reservationId);
    IReadOnlyList<CheckInRoomDto> GetRooms();
    CheckInRoomDto? GetRoom(int roomId);

    Customer? GetCustomer(int customerId);
    IReadOnlyList<Customer> SearchCustomers(string keyword);
    bool IdentityExists(IdentityType type, string number);
    int InsertCustomer(Customer customer);

    bool HasActiveStay(int roomId);
    bool HasRoomBookingOverlap(int roomId, DateTime start, DateTime end, int? excludeReservationId);
    int CountUnassignedBookings(int roomTypeId, DateTime start, DateTime end, int? excludeReservationId);
    bool HasPendingBooking(int roomId, DateTime now);
    int InsertStay(Stay stay);
    bool MarkCheckedIn(int reservationId, int roomId);
    bool MarkCancelled(int reservationId, string? note);
    bool MarkRoomOccupied(int roomId);
    void SetRoomStatus(int roomId, RoomStatus status);
}
