using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Exceptions;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Core.Services;

public class ReservationService
{
    private readonly IReservationRepository reservationRepository;

    public ReservationService(IReservationRepository reservationRepository)
    {
        this.reservationRepository = reservationRepository
            ?? throw new ArgumentNullException(nameof(reservationRepository));
    }
    public List<ReservationListItemDto> SearchReservations(ReservationFilter filter)
    {
        if (filter.FromDate.HasValue && filter.ToDate.HasValue && filter.FromDate > filter.ToDate)
            throw new BusinessRuleException("'Từ ngày' không được lớn hơn 'Đến ngày'.");

        return reservationRepository.Search(filter);
    }

    public List<RoomTypeOptionDto> GetRoomTypeOptions(bool activeOnly)
        => reservationRepository.GetRoomTypeOptions(activeOnly);

    public List<CustomerOptionDto> GetActiveCustomers()
        => reservationRepository.GetActiveCustomers();

    public Reservation? GetReservation(int reservationId)
        => reservationRepository.GetById(reservationId);

    public List<RoomOptionDto> GetFreeRooms(int roomTypeId, DateTime from, DateTime to, int? excludeReservationId = null)
    {
        if (to <= from) return new List<RoomOptionDto>();
        return reservationRepository.GetFreeRooms(roomTypeId, from, to, excludeReservationId);
    }
    public AvailabilityDto CheckAvailability(int roomTypeId, DateTime from, DateTime to, int? excludeReservationId = null)
    {
        if (to <= from)
            throw new BusinessRuleException("Thời gian trả phòng phải sau thời gian nhận phòng.");

        return new AvailabilityDto
        {
            TotalRooms = reservationRepository.CountActiveRooms(roomTypeId),
            FreeRooms = reservationRepository.GetFreeRooms(roomTypeId, from, to, excludeReservationId).Count,
            HeldByType = reservationRepository.CountUnassignedReservations(roomTypeId, from, to, excludeReservationId)
        };
    }
    public int CreateReservation(Reservation reservation)
    {
        Validate(reservation, null);

        reservation.Status = ReservationStatus.Confirmed;
        reservation.CreatedAt = DateTime.Now;
        reservation.Note = string.IsNullOrWhiteSpace(reservation.Note) ? null : reservation.Note.Trim();
        return reservationRepository.Insert(reservation);
    }

    public void UpdateReservation(Reservation reservation)
    {
        var existing = reservationRepository.GetById(reservation.ReservationId)
            ?? throw new BusinessRuleException("Không tìm thấy đặt phòng.");
        if (existing.Status != ReservationStatus.Confirmed)
            throw new BusinessRuleException("Chỉ sửa được đặt phòng đang ở trạng thái 'Đã xác nhận'.");

        Validate(reservation, reservation.ReservationId);

        reservation.Note = string.IsNullOrWhiteSpace(reservation.Note) ? null : reservation.Note.Trim();
        if (!reservationRepository.Update(reservation))
            throw new BusinessRuleException("Không cập nhật được đặt phòng (trạng thái đã thay đổi).");
    }

    public void CancelReservation(int reservationId)
    {
        if (!reservationRepository.Cancel(reservationId))
            throw new BusinessRuleException("Chỉ hủy được đặt phòng đang ở trạng thái 'Đã xác nhận'.");
    }

    private void Validate(Reservation r, int? excludeReservationId)
    {
        if (r.CustomerId <= 0)
            throw new BusinessRuleException("Vui lòng chọn khách hàng.");
        if (!reservationRepository.GetActiveCustomers().Any(x => x.CustomerId == r.CustomerId))
            throw new BusinessRuleException("Khách hàng không tồn tại hoặc đã ngừng sử dụng.");

        if (!reservationRepository.GetRoomTypeOptions(true).Any(x => x.RoomTypeId == r.RoomTypeId))
            throw new BusinessRuleException("Vui lòng chọn loại phòng đang kinh doanh.");

        if (r.ExpectedCheckOut <= r.ExpectedCheckIn)
            throw new BusinessRuleException("Thời gian trả phòng phải sau thời gian nhận phòng.");

        if (r.DepositAmount < 0 || r.DepositAmount != decimal.Truncate(r.DepositAmount))
            throw new BusinessRuleException("Tiền cọc phải là số đồng nguyên, không âm.");

        var availability = CheckAvailability(r.RoomTypeId, r.ExpectedCheckIn, r.ExpectedCheckOut, excludeReservationId);
        if (!availability.IsAvailable)
            throw new BusinessRuleException("Loại phòng này đã hết phòng trong khoảng thời gian đã chọn.");

        if (r.RoomId.HasValue)
        {
            var freeRooms = reservationRepository.GetFreeRooms(r.RoomTypeId, r.ExpectedCheckIn, r.ExpectedCheckOut, excludeReservationId);
            if (!freeRooms.Any(x => x.RoomId == r.RoomId))
                throw new BusinessRuleException("Phòng đã chọn không thuộc loại phòng này hoặc đã trùng lịch.");
        }
    }
}
