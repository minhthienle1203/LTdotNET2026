using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface IActiveStayRepository
{
    ActiveStaySnapshotDto Load(DateTime now);
}
