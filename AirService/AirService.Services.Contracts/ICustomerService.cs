using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface ICustomerService
    {
        void Update(Customer customerToUpdate);

        Customer Insert(Customer newCustomer);

        Customer FindByUdid(string uuid);
    }
}