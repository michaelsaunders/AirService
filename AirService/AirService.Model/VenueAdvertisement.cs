using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class VenueAdvertisement : SimpleModel
    {
        [Required]
        [DataMember(Name = "weekDay")]
        public int AdvertisedDay
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

        public int MobileApplicationSettingsId
        {
            get;
            set;
        }

        public virtual MobileApplicationSettings MobileApplicationSettings
        {
            get;
            set;
        }

        [NotMapped]
        public string Day
        {
            get
            {
                return Enum.GetName(typeof (DayOfWeek),
                                    this.AdvertisedDay);
            }
        }
    }
}