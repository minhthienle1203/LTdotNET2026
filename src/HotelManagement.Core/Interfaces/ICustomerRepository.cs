using HotelManagement.Core.Entities;

namespace HotelManagement.Core.Interfaces;

public interface ICustomerRepository
{
    List<Customer> Search(string keyword);
    Customer? GetById(int customerId);
    bool IdentityExists(string identityNumber, int? excludeCustomerId);
    int Insert(Customer customer);
    void Update(Customer customer);
    int PeekNextId();
}
