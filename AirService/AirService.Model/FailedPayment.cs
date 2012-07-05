using System;
using System.ComponentModel.DataAnnotations;

namespace AirService.Model
{
    public class FailedPayment
    {
        public FailedPayment()
        {
            Created = DateTime.Now;
        }

        [Key]
        public int Id { get; set; }

        public string ErrorMessage { get; set; }

        public int VenueId { get; set; }

        [ForeignKey("VenueId")]
        public virtual Venue Venue { get; set; }

        public DateTime Created { get; set; }
    }
}
