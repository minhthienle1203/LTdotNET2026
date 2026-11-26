using System;
using System.Collections.Generic;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Core.Services;

public sealed class StayServiceService
{
    private readonly IStayServiceRepository _repository;
    public StayServiceService(IStayServiceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
    public void AddServiceToStay(int stayId, int serviceId, decimal quantity, decimal unitPrice, DateTime? usedAt = null, string? note = null)
    {
        if (stayId <= 0)
            throw new ArgumentException("Luu trú (StayId) không hợp lệ.", nameof(stayId));
        if (serviceId <= 0)
            throw new ArgumentException("Dịch vụ (ServiceId) không hợp lệ.", nameof(serviceId));
        if (quantity <= 0)
            throw new ArgumentException("Số lượng phải lớn hơn 0.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Đơn giá không được âm.", nameof(unitPrice));

        var amount = decimal.Round(quantity * unitPrice, 0, MidpointRounding.AwayFromZero);
        var entity = new StayService
        {
            StayId = stayId,
            ServiceId = serviceId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Amount = amount,
            UsedAt = usedAt ?? DateTime.Now,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
        };
        _repository.Add(entity);
    }
    public void RemoveServiceFromStay(int stayServiceId)
    {
        if (stayServiceId <= 0)
            throw new ArgumentException("Mã bản ghi dịch vụ không hợp lệ.", nameof(stayServiceId));

        _repository.Delete(stayServiceId);
    }
    public IReadOnlyList<StayService> GetByStayId(int stayId)
    {
        if (stayId <= 0)
            return Array.Empty<StayService>();
        return _repository.GetByStayId(stayId);
    }
}
