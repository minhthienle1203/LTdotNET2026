using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;

namespace HotelManagement.Core.Interfaces;

public interface IHotelServiceRepository
{
    IReadOnlyList<HotelService> GetAll(bool includeInactive = true);

}
