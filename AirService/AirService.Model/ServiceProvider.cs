using System;
using System.ComponentModel.DataAnnotations;

namespace AirService.Model
{
    public class ServiceProvider : SimpleModel
    {
        public Guid UserId
        {
            get;
            set;
        }

        public int? VenueId
        {
            get;
            set;
        }

        public bool IsSystemUser { get; set; }

        [ForeignKey("VenueId")]
        public virtual Venue Venue
        {
            get;
            set;
        }
    }
}