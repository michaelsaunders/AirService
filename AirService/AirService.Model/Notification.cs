using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class Notification
    {
        [Key]
        public int Id
        {
            get;
            set;
        }

        [Required]
        public DateTime DateTime
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

        public int? CustomerId
        {
            get;
            set;
        }

        [Required]
        public int NotificationType
        {
            get;
            set;
        }

        [Required]
        [MaxLength(256)]
        public string Message
        {
            get;
            set;
        }
    }
}