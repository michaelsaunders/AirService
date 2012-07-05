using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class MenuCategory : SimpleModel
    {
        public MenuCategory()
        {
            this.IsLive = true;
        }

        [Required]
        [DisplayName("Category Name")]
        [DataMember(Name = "title")]
        [MaxLength(256)]
        public string Title
        {
            get;
            set;
        }

        [DisplayName("Status")]
        [DataMember(Name = "isActive")]
        public bool IsLive
        {
            get;
            set;
        }

        [DataMember(Name = "menuItems")]
        public virtual IList<MenuItem> MenuItems
        {
            get;
            set;
        }

        [DisplayName("Photo")]
        [DataMember(Name = "image")]
        [MaxLength(1024)]
        public string CategoryImage
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

        public int MenuId
        {
            get;
            set;
        }

        public virtual Menu Menu
        {
            get;
            set;
        }
    }
}