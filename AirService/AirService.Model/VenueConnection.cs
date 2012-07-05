using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    /// <summary>
    ///   Temp table to keep track of customer venue connection
    /// </summary>
    [DataContract]
    public class VenueConnection: SimpleModel
    {
        public const int StatusWaiting = 0;
        //public const int StatusActive = 1;
        public const int StatusClosing = (int)CmsStatus.Frozen;
        public const int Disconnected = (int)CmsStatus.Deleted;

        public VenueConnection()
        {
            this.Status = StatusWaiting;
        }

        /// <summary>
        ///   Null if connection wasn't confirmed by the Venue's Staff yet.
        /// </summary>
        public DateTime? ConnectedSince
        {
            get;
            set;
        } 

        [ForeignKey("CustomerId")]
        public virtual Customer Customer
        {
            get;
            set;
        }

        [DataMember(Name = "customerId")]
        public int CustomerId
        {
            get;
            set;
        }

        [NotMapped]
        [DataMember(Name = "customerLocation")]
        public string CustomerLocation
        {
            get
            {
                if (this.VenueArea == null)
                {
                    return null;
                }

                return this.VenueArea.Title;
            }
            private set
            {
                /* intentially left blank. Required by DataContractSerializer */
            }
        }

        [NotMapped]
        [DataMember(Name = "customerName")]
        public string CustomerName
        {
            get
            {
                return this.Customer.FirstName + " " + this.Customer.LastName;
            }
            set
            {
                /* intentially left blank. Required by DataContractSerializer */
            }
        }

        [NotMapped]
        [DataMember(Name = "customerPhoto")]
        public string CustomerPhoto
        {
            get
            {
                return this.Customer.Image;
            }
            set
            {
                /* intentially left blank. Required by DataContractSerializer */
            }
        }

        public DateTime? FreezeUtil
        {
            get;
            set;
        }

        [DataMember(Name = "status")]
        public int ConnectionStatus
        {
            get
            {
                return this.Status;
            }
            private set
            {
                this.Status = value;
            }
        } 

        [NotMapped]
        [DataMember(Name = "connectedSince")]
        public string UtcConnectedSince
        {
            get
            {
                return this.ConnectedSince.HasValue ? this.ConnectedSince.Value.ToIso8061DateString() : null;
            }
            private set
            {
                this.ConnectedSince = value == null
                                          ? (DateTime?) null
                                          : DateUtility.FromIso8061FormattedDateString(value);
            }
        }
         
        [NotMapped]
        [DataMember(Name = "freezeUtil")]
        public string UtcFreezeUtil
        {
            get
            {
                return this.FreezeUtil.HasValue ? this.FreezeUtil.Value.ToIso8061DateString() : null;
            }
            set
            {
                if (value == null)
                {
                    this.FreezeUtil = null;
                }
                else
                {
                    this.FreezeUtil = DateUtility.FromIso8061FormattedDateString(value);
                }
            }
        }
         
        [ForeignKey("VenueId")]
        public virtual Venue Venue
        {
            get;
            set;
        }

        public virtual VenueArea VenueArea
        {
            get;
            set;
        }

        [DataMember(Name = "customerLocationId")]
        public int? VenueAreaId
        {
            get;
            set;
        }

        [DataMember(Name = "venueId")]
        public int VenueId
        {
            get;
            set;
        }

        public virtual IList<Order> Orders
        {
            get;
            set;
        }
    }
}