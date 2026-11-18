using System.Reflection.Emit;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Core.Services;

public sealed partial class FloorService
{
    private readonly  IFloorRepository _repository;

    public FloorService(IFloorRepository repository)
    {
        _repository = repository ?? throw new ArgumentException(nameof(repository));
    }

    public IReadOnlyList<Floor> GetFloors(bool includeInactive = false)
    {
        return _repository.GetFloors(includeInactive);
    }
}
