using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class Customer : SimpleModel
    {
        [DataMember(Name = "udid")]
        [MaxLength(50)]
        public string Udid
        {
            get;
            set;
        }

        [DataMember(Name = "firstName")]
        [MaxLength(256)]
        public string FirstName
        {
            get;
            set;
        }

        [DataMember(Name = "lastName")]
        [MaxLength(256)]
        public string LastName
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

        [DataMember(Name = "email")]
        [MaxLength(256)]
        public string Email
        {
            get;
            set;
        }

        [DataMember(Name = "facebookId")]
        [MaxLength(30)]
        public string FacebookId
        {
            get;
            set;
        }

        [DataMember(Name = "mobile")]
        [MaxLength(50)]
        public string Mobile
        {
            get;
            set;
        }

        public virtual ICollection<Order> Orders
        {
            get;
            set;
        }

        [DataMember(Name = "receiveSpecialOffers")]
        public bool ReceiveSpecialOffers
        {
            get;
            set;
        }

        [DataMember(Name = "receiveEmailNotification")]
        public bool ReceiveEmailNotification
        {
            get;
            set;
        }
    }
}