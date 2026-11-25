using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Core.Services;

public sealed class InvoiceService
{
    private readonly IInvoiceRepository _repository;
    public InvoiceService(IInvoiceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
    public IReadOnlyList<Invoice> GetAll(bool includeReservations = false)
    {
        return _repository.GetAll(includeReservations);
    }
    public void MarkAsPrinted(int invoiceId, DateTime? printedAt = null)
    {
        if (invoiceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(invoiceId));

        _repository.MarkAsPrinted(invoiceId, printedAt ?? DateTime.Now);
    }
}
