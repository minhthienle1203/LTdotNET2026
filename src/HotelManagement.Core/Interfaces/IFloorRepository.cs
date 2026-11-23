using HotelManagement.Core.Entities;

namespace HotelManagement.Core.Interfaces;

public partial interface IFloorRepository
{
    List<Floor> GetFloors(bool includeReservations = false);
}
