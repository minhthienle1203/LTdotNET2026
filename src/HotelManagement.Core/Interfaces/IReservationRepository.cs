using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;

namespace HotelManagement.Core.Interfaces;

public interface IReservationRepository
{

    List<ReservationListItemDto> Search(ReservationFilter filter);
    List<RoomTypeOptionDto> GetRoomTypeOptions(bool activeOnly);
    List<CustomerOptionDto> GetActiveCustomers();

    List<RoomOptionDto> GetFreeRooms(int roomTypeId, DateTime from, DateTime to, int? excludeReservationId);
    int CountActiveRooms(int roomTypeId);
    int CountUnassignedReservations(int roomTypeId, DateTime from, DateTime to, int? excludeReservationId);


    Reservation? GetById(int reservationId);
    int Insert(Reservation reservation);
    bool Update(Reservation reservation);
    bool Cancel(int reservationId);
}
