using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Services
{
    public class OrderItemService : SimpleService<OrderItem>
    {
        public OrderItemService(IRepository<OrderItem> orderItemRepository)
        {
            this.Repository = orderItemRepository;
        }

    }
}