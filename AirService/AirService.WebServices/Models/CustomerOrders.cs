using System.Collections.Generic;
using System.Runtime.Serialization;
using AirService.Model;

namespace AirService.WebServices.Models
{
    [DataContract]
    public class CustomerOrders
    {
        [DataMember(Name = "customer")]
        public Customer Customer
        {
            get;
            set;
        }

        [DataMember(Name = "orders")]
        public List<Order> Orders
        {
            get;
            set;
        }
    }
}