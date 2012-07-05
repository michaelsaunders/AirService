using System.Collections.Generic;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class OrderItemViewModel
    {
        public OrderItem OrderItem { get; set; }
        public IList<Order> Orders { get; set; }
        public IList<MenuItem> MenuItems { get; set; }
    }
}