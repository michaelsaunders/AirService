using System.Runtime.Serialization;

namespace AirService.WebServices.Models
{
    [DataContract]
    public class DeviceOptions
    {
        [DataMember(Name = "pin")]
        public string Pin
        {
            get;
            set;
        }

        [DataMember(Name = "isDeliveryEnabled")]
        public bool IsDeliveryEnabled
        {
            get;
            set;
        }

        [DataMember(Name = "isPickupEnabled")]
        public bool IsPickupEnabled
        {
            get;
            set;
        }
    }
}