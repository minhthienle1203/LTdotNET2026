using HotelManagement.Core.Entities;

namespace HotelManagement.Core.Interfaces;

public interface IInvoiceRepository
{
    IReadOnlyList<Invoice> GetAll(bool includeReservations = false);
}
