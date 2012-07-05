using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{ 
    public class OrderRepository : SimpleRepository<Order>
    {
        public OrderRepository(IAirServiceContext context)
            : base(context)
        { 
        }
    }
}