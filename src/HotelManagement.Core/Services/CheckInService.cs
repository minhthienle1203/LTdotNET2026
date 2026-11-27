using System.Text.RegularExpressions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Exceptions;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Core.Services;

public sealed class CheckInService(ICheckInRepository repository, Func<DateTime>? clock = null)
{
    private readonly Func<DateTime> _clock = clock ?? (() => DateTime.Now);

    public DateTime Now
    {
        get { var n = _clock(); return n.AddTicks(-(n.Ticks % TimeSpan.TicksPerSecond)); }
    }
    public IReadOnlyList<CheckInReservationDto> GetReservations(ReservationStatus? status, string? keyword) =>
        repository.Read(s => s.GetReservations(status, keyword?.Trim()));

    public CheckInReservationDto? GetReservation(int id) => repository.Read(s => s.GetReservation(id));

    public CheckInRoomDto? GetRoom(int id) => repository.Read(s => s.GetRoom(id));

    public Customer? GetCustomer(int id) => repository.Read(s => s.GetCustomer(id));

    public IReadOnlyList<Customer> SearchCustomers(string keyword) =>
        repository.Read(s => s.SearchCustomers(keyword.Trim()));
    public int CountDueToday()
    {
        var now = Now;
        return GetReservations(ReservationStatus.Confirmed, null)
            .Count(r => r.ExpectedCheckIn.Date == now.Date && r.ExpectedCheckOut > now);
    }
    public IReadOnlyList<CheckInRoomDto> GetAssignableRooms(int? reservationId, DateTime expectedCheckOut)
    {
        var now = Now;
        return repository.Read(s =>
        {
            var booking = reservationId is int id ? s.GetReservation(id) : null;
            if (reservationId is not null && booking is null) return (IReadOnlyList<CheckInRoomDto>)[];
            return s.GetRooms()
                .Where(r => booking is null || r.RoomTypeId == booking.RoomTypeId)
                .Where(r => RoomProblem(s, r, booking, now, expectedCheckOut) is null)
                .ToList();
        });
    }

    public CheckInEvaluationDto Evaluate(CheckInDraftDto draft)
    {
        var now = Now;
        return repository.Read(s => Evaluate(s, draft, now));
    }
    public CheckInResultDto CheckInReservation(CheckInDraftDto draft) => Confirm(draft, CheckInMode.Reservation);
    public CheckInResultDto CheckInWalkIn(CheckInDraftDto draft) => Confirm(draft, CheckInMode.WalkIn);

    private CheckInResultDto Confirm(CheckInDraftDto draft, CheckInMode expected)
    {
        if (draft.Mode != expected) throw new BusinessRuleException("Sai loại nhận phòng.");

        var now = Now;
        return repository.Write(s =>
        {
            var evaluation = Evaluate(s, draft, now);
            if (!evaluation.CanConfirm)
                throw new BusinessRuleException("Chưa thể nhận phòng:\n" + evaluation.FailureText);

            var booking = draft.Mode == CheckInMode.Reservation
                ? s.GetReservation(draft.ReservationId!.Value)! : null;
            var room = s.GetRoom(draft.RoomId!.Value)!;
            var customerId = booking?.CustomerId ?? draft.CustomerId!.Value;

            var stayId = s.InsertStay(new Stay
            {
                ReservationId = booking?.ReservationId,
                CustomerId = customerId,
                RoomId = room.RoomId,
                ActualCheckIn = now,
                ExpectedCheckOut = draft.ExpectedCheckOut,
                ActualCheckOut = null,
                GuestCount = draft.GuestCount,
                HourlyRateAtCheckIn = room.HourlyRate,
                DailyRateAtCheckIn = room.DailyRate,
                Status = StayStatus.InProgress,
                Note = string.IsNullOrWhiteSpace(draft.Note) ? null : draft.Note.Trim()
            });

            if (!s.MarkRoomOccupied(room.RoomId))
                throw new BusinessRuleException("Trạng thái phòng vừa thay đổi. Hãy làm mới (F5).");

            if (booking is not null)
            {
                if (!s.MarkCheckedIn(booking.ReservationId, room.RoomId))
                    throw new BusinessRuleException("Đặt phòng vừa được xử lý ở nơi khác. Hãy làm mới (F5).");
                if (booking.RoomId is int oldRoomId && oldRoomId != room.RoomId)
                    ReleaseRoom(s, oldRoomId, now);
            }

            return new CheckInResultDto(stayId, room.RoomCode, now);
        });
    }

    public void CancelReservation(int reservationId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleException("Vui lòng nhập lý do hủy.");

        var now = Now;
        repository.Write(s =>
        {
            var booking = s.GetReservation(reservationId)
                ?? throw new BusinessRuleException("Không tìm thấy đặt phòng.");
            if (booking.Status != ReservationStatus.Confirmed || booking.StayId is not null)
                throw new BusinessRuleException(
                    $"Chỉ hủy được đặt phòng đang chờ nhận. Trạng thái hiện tại: {StatusText(booking.Status)}.");

            var line = $"[Hủy {now:dd/MM/yyyy HH:mm}] {reason.Trim()}";
            var note = string.IsNullOrWhiteSpace(booking.Note) ? line : $"{booking.Note}\n{line}";

            if (!s.MarkCancelled(reservationId, note))
                throw new BusinessRuleException("Đặt phòng vừa được xử lý ở nơi khác. Hãy làm mới (F5).");

            if (booking.RoomId is int roomId) ReleaseRoom(s, roomId, now);
            return true;
        });
    }
    public Customer CreateCustomer(Customer input)
    {
        var customer = new Customer
        {
            FullName = (input.FullName ?? "").Trim(),
            IdentityType = input.IdentityType,
            IdentityNumber = (input.IdentityNumber ?? "").Trim().ToUpperInvariant(),
            Phone = NormalizePhone(input.Phone),
            Email = Clean(input.Email),
            Address = Clean(input.Address),
            CreatedAt = Now,
            IsActive = true
        };

        if (customer.FullName.Length == 0) throw new BusinessRuleException("Họ tên không được để trống.");
        if (IdentityProblem(customer.IdentityType, customer.IdentityNumber) is { } problem)
            throw new BusinessRuleException(problem);
        if (PhoneProblem(customer.Phone) is { } phoneProblem) throw new BusinessRuleException(phoneProblem);
        if (EmailProblem(customer.Email) is { } emailProblem) throw new BusinessRuleException(emailProblem);

        return repository.Write(s =>
        {
            if (s.IdentityExists(customer.IdentityType, customer.IdentityNumber))
                throw new BusinessRuleException("Giấy tờ này đã có hồ sơ. Hãy dùng chức năng Tra cứu khách (F3).");
            customer.CustomerId = s.InsertCustomer(customer);
            return customer;
        });
    }

    public static string? IdentityProblem(IdentityType type, string? number) => type switch
    {
        IdentityType.CCCD when !Regex.IsMatch(number ?? "", "^[0-9]{12}$") => "CCCD phải đúng 12 chữ số.",
        IdentityType.Passport when !Regex.IsMatch(number ?? "", "^[A-Za-z0-9]{6,12}$") =>
            "Số hộ chiếu gồm 6–12 chữ cái hoặc chữ số.",
        _ => null
    };

    public static string NormalizePhone(string? phone) => Regex.Replace(phone ?? "", @"[\s-]", "");

    public static string? PhoneProblem(string? phone) =>
        Regex.IsMatch(NormalizePhone(phone), "^[0-9]{9,15}$") ? null : "Số điện thoại phải gồm 9–15 chữ số.";

    public static string? EmailProblem(string? email) =>
        string.IsNullOrWhiteSpace(email) || Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            ? null : "Email không đúng định dạng.";

    public static string StatusText(ReservationStatus status) => status switch
    {
        ReservationStatus.Confirmed => "Chờ nhận phòng",
        ReservationStatus.CheckedIn => "Đã nhận phòng",
        ReservationStatus.Completed => "Hoàn tất",
        ReservationStatus.Cancelled => "Đã hủy",
        ReservationStatus.NoShow => "Khách không đến",
        _ => status.ToString()
    };

    private static CheckInEvaluationDto Evaluate(ICheckInSession s, CheckInDraftDto d, DateTime now)
    {
        CheckInReservationDto? booking = null;
        if (d.Mode == CheckInMode.Reservation && d.ReservationId is int rid)
            booking = s.GetReservation(rid);

        var customerId = d.Mode == CheckInMode.Reservation ? booking?.CustomerId : d.CustomerId;
        var customer = customerId is int cid ? s.GetCustomer(cid) : null;
        var room = d.RoomId is int roomId ? s.GetRoom(roomId) : null;
        string? customerError =
            customer is null ? "Chưa chọn khách hàng." :
            !customer.IsActive ? "Hồ sơ khách đã ngừng hoạt động." :
            string.IsNullOrWhiteSpace(customer.Phone) ? "Hồ sơ thiếu số điện thoại." :
            IdentityProblem(customer.IdentityType, customer.IdentityNumber);
        string? sourceError = null;
        if (d.Mode == CheckInMode.Reservation)
        {
            sourceError =
                booking is null ? "Chưa chọn đặt phòng." :
                booking.Status != ReservationStatus.Confirmed ? $"Trạng thái: {StatusText(booking.Status)}." :
                booking.StayId is not null ? "Đặt phòng này đã tạo lượt lưu trú." :
                booking.ExpectedCheckIn.Date > now.Date ? $"Chưa đến ngày nhận ({booking.ExpectedCheckIn:dd/MM/yyyy})." :
                booking.ExpectedCheckOut <= now ? $"Đã quá hạn (ngày trả {booking.ExpectedCheckOut:dd/MM/yyyy HH:mm})." :
                null;
        }

        string? roomError =
            room is null ? "Chưa chọn phòng." :
            booking is not null && room.RoomTypeId != booking.RoomTypeId ? $"Phòng phải thuộc loại {booking.TypeName}." :
            RoomProblem(s, room, booking, now, d.ExpectedCheckOut);

        string? guestError =
            room is null ? "Chọn phòng để kiểm tra sức chứa." :
            d.GuestCount <= 0 ? "Số khách phải lớn hơn 0." :
            d.GuestCount > room.MaxOccupancy ? $"{d.GuestCount} khách vượt sức chứa {room.MaxOccupancy}." :
            null;

        string? timeError = d.ExpectedCheckOut <= now ? "Giờ trả dự kiến phải sau thời điểm hiện tại." : null;

        return new CheckInEvaluationDto(
        [
            Item("Khách hàng & giấy tờ", customerError,
                customer is null ? "" : $"{customer.IdentityType} {customer.IdentityNumber}"),
            Item(d.Mode == CheckInMode.Reservation ? "Trạng thái đặt phòng" : "Khách vãng lai", sourceError,
                booking is null ? "Không cần mã đặt phòng." : $"{booking.ReservationCode}: chờ nhận phòng."),
            Item("Phòng sẵn sàng, không trùng lịch", roomError,
                room is null ? "" : $"Phòng {room.RoomCode} có thể bàn giao."),
            Item("Sức chứa", guestError,
                room is null ? "" : $"{d.GuestCount}/{room.MaxOccupancy} khách."),
            Item("Thời gian lưu trú", timeError, $"Trả phòng {d.ExpectedCheckOut:dd/MM/yyyy HH:mm}.")
        ]);
    }

    private static CheckItemDto Item(string title, string? error, string okDetail) =>
        new(title, error is null, error ?? okDetail);

    private static string? RoomProblem(ICheckInSession s, CheckInRoomDto room, CheckInReservationDto? booking,
                                       DateTime now, DateTime checkOut)
    {
        if (!room.IsUsable) return "Phòng, tầng hoặc loại phòng đã ngừng hoạt động.";
        if (room.Status is RoomStatus.Occupied or RoomStatus.Cleaning)
            return $"Phòng {room.RoomCode} đang ở trạng thái {room.Status}.";
        if (room.Status == RoomStatus.Reserved && (booking is null || booking.RoomId != room.RoomId))
            return $"Phòng {room.RoomCode} đang được giữ cho đặt phòng khác.";
        if (s.HasActiveStay(room.RoomId)) return $"Phòng {room.RoomCode} còn lượt lưu trú chưa hoàn tất.";
        if (checkOut <= now) return null;

        var ownId = booking?.ReservationId;
        if (s.HasRoomBookingOverlap(room.RoomId, now, checkOut, ownId))
            return $"Phòng {room.RoomCode} trùng lịch với đặt phòng khác.";

        if (booking?.RoomId == room.RoomId) return null;
        var waiting = s.CountUnassignedBookings(room.RoomTypeId, now, checkOut, ownId);
        if (waiting == 0) return null;

        var free = s.GetRooms().Count(r =>
            r.RoomTypeId == room.RoomTypeId && r.IsUsable &&
            r.Status is not (RoomStatus.Occupied or RoomStatus.Cleaning) &&
            !s.HasActiveStay(r.RoomId) &&
            !s.HasRoomBookingOverlap(r.RoomId, now, checkOut, ownId));

        return free - 1 >= waiting ? null : $"Số phòng loại {room.TypeName} còn lại đã được giữ cho đặt phòng khác.";
    }

    private static void ReleaseRoom(ICheckInSession s, int roomId, DateTime now)
    {
        if (s.GetRoom(roomId)?.Status != RoomStatus.Reserved) return;
        s.SetRoomStatus(roomId, s.HasPendingBooking(roomId, now) ? RoomStatus.Reserved : RoomStatus.Available);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
