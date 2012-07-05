using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class MenuItem : SimpleModel
    {
        public MenuItem()
        {
            this.MenuItemStatus = true;
            this.Price = 0;
            this.MenuItemOptions = new List<MenuItemOption>();
        }

        protected MenuItem(MenuItem menuItem)
        {
            this.Title = "Copy of " + menuItem.Title;
            this.Description = menuItem.Description;
            this.Price = menuItem.Price;
            this.MenuItemStatus = menuItem.MenuItemStatus;
            this.MenuCategoryId = menuItem.MenuCategoryId;
            if (menuItem.MenuItemOptions != null)
            {
                this.MenuItemOptions = menuItem.MenuItemOptions.Select(option => new MenuItemOption(option)
                                                                                     {
                                                                                         MenuItem = this
                                                                                     }).ToList(); 
            }
            else
            {
                this.MenuItemOptions = new List<MenuItemOption>();
            }
        }

        [Required]
        [DataMember(Name = "title")]
        [DisplayName("Item Name")]
        [MaxLength(256)]
        public string Title
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

        [DataMember(Name = "image")]
        [DisplayName("Photo")]
        [MaxLength(1024)]
        public string Image
        {
            get;
            set;
        }

        [Required, RegularExpression(@"^[0-9]*(\.[0-9]{1,2})?$", ErrorMessage = "Please enter a valid amount.")]
        [DataMember(Name = "price")]
        [DisplayName("Item Price")]
        public Decimal Price
        {
            get;
            set;
        }

        [DisplayName("Status")]
        [DataMember(Name = "isActive")]
        public bool MenuItemStatus
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

        public int MenuCategoryId
        {
            get;
            set;
        }

        public virtual MenuCategory MenuCategory
        {
            get;
            set;
        }

        [DataMember(Name = "options")]
        public virtual IList<MenuItemOption> MenuItemOptions
        {
            get;
            set;
        }

        [NotMapped]
        [DataMember(Name = "print")]
        public bool Print { get; set; }

        public object Clone()
        {
            return new MenuItem(this);
        }
    }
}