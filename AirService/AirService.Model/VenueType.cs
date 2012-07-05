using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirService.Model
{
    public class VenueType : SimpleModel
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(256)]
        public string Title
        {
            get;
            set;
        }

        public virtual IList<Venue> Venues
        {
            get;
            set;
        }
    }
}