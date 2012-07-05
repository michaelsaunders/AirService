using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{ 
    public class CustomerRepository : SimpleRepository<Customer>
    {
        public CustomerRepository(IAirServiceContext context)
            : base(context)
        { 
        }
    }
}