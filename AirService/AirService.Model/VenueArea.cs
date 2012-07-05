using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class VenueArea : SimpleModel
    {
        [Required]
        [DataMember(Name = "title")]
        [MaxLength(256)]
        public string Title
        {
            get;
            set;
        }

        [DataMember(Name = "description")]
        [MaxLength(1024)]
        public string Description
        {
            get;
            set;
        }

        public int VenueId
        {
            get;
            set;
        }

        public Venue Venue
        {
            get;
            set;
        }
    }
}