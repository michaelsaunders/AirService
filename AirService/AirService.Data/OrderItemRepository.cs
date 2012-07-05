using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{ 
    public class OrderItemRepository : SimpleRepository<OrderItem>
    {
        public OrderItemRepository(IAirServiceContext context)
            : base(context)
        { 
        }
    }
}