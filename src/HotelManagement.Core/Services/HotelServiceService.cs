using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Core.Services;

public sealed class HotelServiceService
{
    private readonly IHotelServiceRepository _repository;
    public HotelServiceService(IHotelServiceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public IReadOnlyList<HotelService> GetAll(bool includeInactive = true)
    {
        return _repository.GetAll(includeInactive);
    }
}
