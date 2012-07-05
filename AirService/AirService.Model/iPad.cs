using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class iPad : SimpleModel
    {
        public const int ServiceOptionNone = 0; // This shouldn't be used for iPad object. 
        public const int ServiceOptionDelivery = 1;
        public const int ServiceOptionPickup = 2;

        public iPad()
        {
            this.AssignedMenus = new List<DeviceMenuOption>();
            this.ServiceOption = ServiceOptionDelivery | ServiceOptionPickup;
        }

        [Required]
        [DataMember(Name = "title")]
        [MaxLength(256)] 
        public string Name
        {
            get;
            set;
        }

        [Required]
        [MaxLength(256)]
        public string Location
        {
            get;
            set;
        }

        [DataMember(Name = "image")]
        [MaxLength(1024)] 
        public string Image
        {
            get;
            set;
        }

        [DataMember(Name = "udid")]
        [MaxLength(50)]
        public string Udid
        {
            get;
            set;
        }

        [DisplayName("PIN Code")]
        [MaxLength(10)]
        public string Pin
        {
            get;
            set;
        }

        public virtual IList<DeviceMenuOption> AssignedMenus
        {
            get;
            set;
        }
         
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

        public virtual IList<DeviceAdmin> DeviceAdmins
        {
            get;
            set;
        }

        /// <summary>
        ///   Value must be 1 or 2, or both
        /// </summary>
        [DefaultValue(ServiceOptionDelivery)]
        public int ServiceOption
        {
            get;
            set;
        }

        [NotMapped]
        [DataMember(Name = "isDeliveryEnabled")]
        public bool IsDeliveryEnabled
        {
            get
            {
                return (this.ServiceOption & ServiceOptionDelivery) == ServiceOptionDelivery;
            }
            set
            {
                if (value)
                {
                    this.ServiceOption |= ServiceOptionDelivery;
                }
                else
                {
                    this.ServiceOption &= ~ServiceOptionDelivery;
                }
            }
        }

        [NotMapped]
        [DataMember(Name = "isPickupEnabled")]
        public bool IsPickupEnabled
        {
            get
            {
                return (this.ServiceOption & ServiceOptionPickup) == ServiceOptionPickup;
            }
            set
            {
                if (value)
                {
                    this.ServiceOption |= ServiceOptionPickup;
                }
                else
                {
                    this.ServiceOption &= ~ServiceOptionPickup;
                }
            }
        }
    }
}