using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class Order : SimpleModel
    {
        public const int OrderStatusPending = 0; // new 
        public const int OrderStatusConfirmed = 1; // baking
        public const int OrderStatusProcessed = 2; // delivered
        public const int OrderStatusFinalized = 3; // paid
        public const int OrderStatusCancelled = 4;

        public DateTime OrderDate
        {
            get;
            set;
        }

        [DataMember(Name = "orderStatus")]
        public int OrderStatus
        {
            get;
            set;
        }

        [DataMember(Name = "total")]
        public Decimal TotalAmount
        {
            get;
            set;
        }

        [DataMember(Name = "customerId")]
        public int CustomerId
        {
            get;
            set;
        }

        public Customer Customer
        {
            get;
            set;
        }

        public int VenueConnectionId { get; set; }

        public VenueConnection VenueConnection { get; set; }

        public int VenueId
        {
            get;
            set;
        }

        public virtual Venue Venue
        {
            get;
            set;
        }

        [DataMember(Name = "items")]
        public virtual IList<OrderItem> OrderItems
        {
            get;
            set;
        }

        [NotMapped]
        [DataMember(Name = "orderDate")]
        public string UtcOrderDateTime
        {
            get
            {
                return this.OrderDate.ToIso8061DateString();
            }
            private set
            {
                this.OrderDate = DateUtility.FromIso8061FormattedDateString(value);
            }
        }
    }
}