using System;
using System.Runtime.Serialization;

namespace AirService.WebServices.Models
{
    [DataContract]
    public class CustomerVenueConnection
    {
        [DataMember(Name = "venueId")]
        public int VenueId { get; set; }

        [DataMember(Name = "sessionTotal")]
        public Decimal SessionTotalAmount { get; set; }

        [DataMember(Name = "dateConnected")]
        public string DateConnected { get; set; }
    }
}
