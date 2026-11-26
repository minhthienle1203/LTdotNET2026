namespace HotelManagement.Core.Interfaces;

public interface ICheckInRepository
{
    T Read<T>(Func<ICheckInSession, T> work);
    T Write<T>(Func<ICheckInSession, T> work);
}
