using System.Collections.Generic;
using System.Linq;
using AirService.Model;

namespace AirService.WebTest.ViewModels
{
    public class CustomerOrderViewModel
    {
        private readonly int _venueId;
        private readonly IEnumerable<Order> orders;

        public CustomerOrderViewModel(int venueId,
                                      IEnumerable<Order> orders)
        {
            this._venueId = venueId;
            this.orders = orders.ToList();
        }

        public int VenueId
        {
            get
            {
                return this._venueId;
            }
        }

        public bool CanFinalize
        {
            get
            {
                if (this.orders == null)
                {
                    return false;
                }

                return (from order in this.orders
                        where
                            order.OrderStatus == Order.OrderStatusProcessed &&
                            order.OrderStatus != Order.OrderStatusCancelled
                        select order).Count() ==
                       this.orders.Count(order => order.OrderStatus != Order.OrderStatusCancelled);
            }
        }

        public Customer Customer
        {
            get
            {
                Order order = this.orders.FirstOrDefault();
                if (order == null)
                {
                    return null;
                }

                return order.Customer;
            }
        }

        public IEnumerable<Order> Orders
        {
            get
            {
                return this.orders;
            }
        }
    }
}