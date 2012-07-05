using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class NotificationToken
    {
        [Key]
        [DataMember(Name = "udid")]
        public string Udid
        {
            get;
            set;
        }

        [Required]
        [DataMember(Name = "token")]
        public string Token
        {
            get;
            set;
        }

        public DateTime CreateDate
        {
            get;
            set;
        }
         
        public DateTime UpdateDate
        {
            get;
            set;
        }

        public byte[] GetDeviceTokenBytes()
        {
            return Enumerable.Range(0,
                                    this.Token.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(this.Token.Substring(x,
                                                                 2),
                                            16)).ToArray();
        }
    }
}