using System;
using System.Collections.Generic;
using HotelManagement.Core.Entities;

namespace HotelManagement.Core.Interfaces;

public interface IStayServiceRepository
{
    void Add(StayService item);
    void Delete(int stayServiceId);
    StayService? GetById(int stayServiceId);
    IReadOnlyList<StayService> GetByStayId(int stayId);
}
