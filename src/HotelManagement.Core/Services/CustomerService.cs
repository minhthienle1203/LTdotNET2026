using System.Text.RegularExpressions;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Exceptions;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Core.Services;

public sealed class CustomerService
{
    private static readonly Regex CccdRegex = new(@"^[0-9]{12}$");
    private static readonly Regex PassportRegex = new(@"^[A-Z0-9]{6,9}$");
    private static readonly Regex PhoneRegex = new(@"^0\d{9}$");

    private readonly ICustomerRepository _repo;

    public CustomerService(ICustomerRepository repo)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
    }

    public IEnumerable<Customer> SearchCustomers(string? keyword) =>
        _repo.Search((keyword ?? string.Empty).Trim());

    public Customer? GetById(int customerId) => _repo.GetById(customerId);

    public string GetSuggestedCode() => $"KH-{_repo.PeekNextId():000}";

    public bool IsIdentityDuplicate(string identityNumber, int? excludeCustomerId = null) =>
        _repo.IdentityExists(identityNumber.Trim().ToUpperInvariant(), excludeCustomerId);

    public void Save(Customer customer, bool isNew)
    {
        if (isNew) CreateCustomer(customer);
        else UpdateCustomer(customer);
    }

    public void CreateCustomer(Customer customer)
    {
        Normalize(customer);
        Validate(customer);
        if (_repo.IdentityExists(customer.IdentityNumber, null))
            throw new BusinessRuleException("Số định danh đã tồn tại trong hệ thống.");

        customer.IsActive = true;
        customer.CreatedAt = DateTime.Now;
        customer.CustomerId = _repo.Insert(customer);
    }

    public void UpdateCustomer(Customer customer)
    {
        if (_repo.GetById(customer.CustomerId) is null)
            throw new BusinessRuleException("Không tìm thấy khách hàng cần cập nhật.");

        Normalize(customer);
        Validate(customer);
        if (_repo.IdentityExists(customer.IdentityNumber, customer.CustomerId))
            throw new BusinessRuleException("Số định danh đã tồn tại ở khách hàng khác.");

        _repo.Update(customer);
    }

    private static void Normalize(Customer c)
    {
        c.FullName = Regex.Replace((c.FullName ?? string.Empty).Trim(), @"\s+", " ");
        c.IdentityNumber = (c.IdentityNumber ?? string.Empty).Trim().ToUpperInvariant();
        c.Phone = Regex.Replace(c.Phone ?? string.Empty, @"[\s\-.]", "");
        c.Email = string.IsNullOrWhiteSpace(c.Email) ? null : c.Email.Trim();
        c.Address = string.IsNullOrWhiteSpace(c.Address) ? null : c.Address.Trim();
    }

    private static void Validate(Customer c)
    {
        if (string.IsNullOrWhiteSpace(c.FullName))
            throw new BusinessRuleException("Vui lòng nhập họ và tên khách hàng.");

        bool passport = c.IdentityType == IdentityType.Passport;
        if (!(passport ? PassportRegex : CccdRegex).IsMatch(c.IdentityNumber))
            throw new BusinessRuleException(passport
                ? "Số hộ chiếu gồm 6–9 ký tự chữ hoặc số."
                : "CCCD phải gồm đúng 12 chữ số.");

        if (!PhoneRegex.IsMatch(c.Phone))
            throw new BusinessRuleException("Số điện thoại phải gồm 10 chữ số, bắt đầu bằng 0.");

        if (c.Email is not null && !IsValidEmail(c.Email))
            throw new BusinessRuleException("Email không hợp lệ.");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            return new System.Net.Mail.MailAddress(email).Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
