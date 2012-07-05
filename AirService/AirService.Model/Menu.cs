using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class Menu : SimpleModel
    {
        public Menu()
        {
            this.Is24Hour = true;
            this.MenuStatus = true;
        }

        [Required]
        [DisplayName("Menu Title")]
        [DataMember(Name = "title")]
        [MaxLength(256)]
        public string Title
        {
            get;
            set;
        }

        [Required]
        [DisplayName("Display Title")]
        [DataMember(Name = "displayTitle")]
        [MaxLength(256)]
        public string DisplayTitle
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

        [MaxLength(4)]
        [DataMember(Name = "availableFrom")]
        [DisplayName("Time")]
        [Range(0, 2359, ErrorMessage = "Please enter a valid time (24 hour format)")]
        [RegularExpression("[0-2][0-9][0-5][0-9]", ErrorMessage = "Please enter a valid time (24 hour format - 0000)")]
        public string ShowFrom
        {
            get;
            set;
        }

        [MaxLength(4)]
        [DataMember(Name = "availableTo")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:D4}")]
        [Range(0, 2359, ErrorMessage = "Please enter a valid time (24 hour format)")]
        [RegularExpression("[0-2][0-9][0-5][0-9]", ErrorMessage = "Please enter a valid time (24 hour format - 0000)")]
        public string ShowTo
        {
            get;
            set;
        }

        [DataMember(Name = "alwaysAvailable")]
        [DisplayName("24 hours")]
        public bool Is24Hour
        {
            get;
            set;
        }

        [DataMember(Name = "isActive")]
        [DisplayName("Status")]
        public bool MenuStatus
        {
            get;
            set;
        }

        [DisplayName("Specials Menu?")]
        [DataMember(Name = "isSpecialsMenu")]
        public bool IsSpecialsMenu
        {
            get;
            set;
        }

        [DataMember(Name = "categories")]
        public virtual IList<MenuCategory> MenuCategories
        {
            get;
            set;
        }

        public virtual IList<DeviceMenuOption> AssignedDevices
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

        [DataMember(Name = "sortOrder")]
        public int SortOrder
        {
            get;
            set;
        }

        public virtual Venue Venue
        {
            get;
            set;
        }
        
        [NotMapped]
        [DataMember(Name = "print")]
        public bool Print { get; set; }
    }
}