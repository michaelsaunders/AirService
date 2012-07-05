using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirService.Model
{
    /// <summary>
    ///   Records relationship between device admin and the venue's ipad
    /// </summary>
    public class DeviceAdmin : SimpleModel
    {
        [Required]
        [MaxLength(256)]
        public string Email
        {
            get;
            set;
        }

        [Required]
        [MaxLength(256)]
        public string UserName
        {
            get;
            set;
        }

        [Required]
        [MaxLength(256)]
        public string Password
        {
            get;
            set;
        }

        public virtual IList<iPad> iPads
        {
            get;
            set;
        }

        [Required]
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
    }
}