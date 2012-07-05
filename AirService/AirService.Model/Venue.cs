using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class Venue : SimpleModel
    { 
        [Required(ErrorMessage = "Venue name is required")]
        [DisplayName("Venue Name")]
        [DataMember(Name = "title")]
        [MaxLength(256)]
        public string Title
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Venue description is required")]
        [DisplayName("Description")]
        [DataMember(Name = "description")]
        [MaxLength(1024)]
        public string Description
        {
            get;
            set;
        }

        [DisplayName("Contact First Name")]
        [Required(ErrorMessage = "First name is required")]
        [DataMember(Name = "contactFirstName")]
        [MaxLength(256)]
        public string ContactFirstName
        {
            get;
            set;
        }

        [DisplayName("Contact Last Name")]
        [Required(ErrorMessage = "Last name is required")]
        [DataMember(Name = "contractLastName")]
        [MaxLength(256)]
        public string ContactLastName
        {
            get;
            set;
        }

        [DisplayName("Contact Phone")]
        [Required(ErrorMessage = "Contact phone is required")]
        [DataMember(Name = "telephone")]
        [MaxLength(20)]
        public string Telephone
        {
            get;
            set;
        }

        [DisplayName("Mobile Application Settings")]
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

        public int VenueAccountType
        {
            get;
            set;
        }

        public virtual IList<VenueArea> VenueAreas
        {
            get;
            set;
        }

        public virtual IList<Menu> Menus
        {
            get;
            set;
        }

        [DisplayName("Type of Venue")]
        public virtual IList<VenueType> VenueTypes
        {
            get;
            set;
        }

        [NotMapped]
        [DataMember(Name = "state")]
        public string StateName
        {
            get
            {
                if (this.State == null)
                {
                    return null;
                }

                return this.State.Title;
            }
            private set
            {
                /* Intentionally left blank. Requires to be 'DataMember' */
            }
        }

        [NotMapped]
        [DataMember(Name = "country")]
        public string CountryName
        {
            get
            {
                if (this.Country == null)
                {
                    return null;
                }

                return this.Country.Title;
            }
            private set
            {
                /* Intentionally left blank. Requires to be 'DataMember' */
            }
        }

        /// <summary>
        ///   MenuOnly means customers cannot make orders. See PDD
        /// </summary>
        [NotMapped]
        [DataMember(Name = "isMenuOnly")]
        [Obsolete("There are no such thing as menu only subscriptions anymore. This method is here because iOS app still expect this property.")]
        public bool IsMenuOnly
        {
            get;set; 
        }

        public bool IsPaidAccount
        {
            get { return this.VenueAccountType != (int) VenueAccountTypes.AccountTypeEvaluation; }
        }

        /// <summary>
        ///   If false whole Air Service for this venue is disabled.
        /// </summary>
        [DataMember(Name = "isActive")]
        public bool IsActive
        {
            get;
            set;
        }

        public virtual IList<iPad> iPads
        {
            get;
            set;
        }

        #region Address

        [Required]
        [DisplayName("Address")]
        [DataMember(Name = "address1")]
        [MaxLength(256)]
        public string Address1
        {
            get;
            set;
        }

        [DisplayName("Address (2)")]
        [DataMember(Name = "address2")]
        [MaxLength(256)]
        public string Address2
        {
            get;
            set;
        }

        [Required]
        [DataMember(Name = "suburb")]
        [MaxLength(256)]
        public string Suburb
        {
            get;
            set;
        }

        [Required]
        [DataMember(Name = "postcode")]
        [MaxLength(10)]
        [StringLength(10)]
        public string Postcode
        {
            get;
            set;
        }

        [DataMember(Name = "lat")]
        public double LatitudePosition
        {
            get;
            set;
        }

        [DataMember(Name = "lng")]
        public double LongitudePosition
        {
            get;
            set;
        }

        [Required]
        [DisplayName("State")]
        public int StateId
        {
            get;
            set;
        }

        public virtual State State
        {
            get;
            set;
        }

        [DisplayName("Country")]
        public int CountryId
        {
            get;
            set;
        }

        public virtual Country Country
        {
            get;
            set;
        }

        #endregion

        [MaxLength(10)]
        public string EwayCustomerId { get; set; }

        [MaxLength(20)]
        public string EwayRebillId { get; set; }

        [MaxLength(100)]
        public string PromoCode { get; set; }
    }
}