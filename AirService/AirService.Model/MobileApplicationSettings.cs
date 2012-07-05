using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class MobileApplicationSettings : SimpleModel
    {
        [MaxLength(1024)]
        [DisplayName("Header Image")]
        [DataMember(Name = "headerImage")]
        public string HeaderImage
        {
            get;
            set;
        }

        [MaxLength(1024)]
        [DisplayName("Background Image")]
        [DataMember(Name = "backgroundImage")]
        public string BackgroundImage
        {
            get;
            set;
        }

        [DisplayName("Frosting")]
        [DataMember(Name = "frosting")]
        public bool FrostingActive
        {
            get;
            set;
        }

        [MaxLength(1024)]
        [DisplayName("Header Bar")]
        [DataMember(Name = "headerTheme")]
        public string Theme
        {
            get;
            set;
        }

        // theme options, light and dark
        [DisplayName("Font Colour")]
        [DataMember(Name = "fontColour")]
        public bool DarkTextColour
        {
            get;
            set;
        }

        [DisplayName("Button Colour")]
        [DataMember(Name = "darkButtonColour")]
        public bool DarkButtonColour
        {
            get;
            set;
        }

        [DataMember(Name = "advertisements")]
        public virtual IList<VenueAdvertisement> VenueAdvertisements
        {
            get;
            set;
        }
    }
}