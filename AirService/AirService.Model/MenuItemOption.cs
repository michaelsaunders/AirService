using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    [Serializable]
    public class MenuItemOption : SimpleModel
    {
        public MenuItemOption()
        {
            
        }

        public MenuItemOption(MenuItemOption option)
        {
            this.Title = option.Title;
            this.Price = option.Price;
            this.SortOrder = option.SortOrder;
        }

        [DataMember(Name = "title")]
        [Required]
        [MaxLength(256)]
        public string Title
        {
            get;
            set;
        }

        [Required, RegularExpression(@"^[0-9]*(\.[0-9]{1,2})?$", ErrorMessage = "Please enter a valid amount.")]
        [DataMember(Name = "price")]
        public decimal Price
        {
            get;
            set;
        }

        public int MenuItemId
        {
            get;
            set;
        }

        public MenuItem MenuItem
        {
            get;
            set;
        }

        [NotMapped]
        [DataMember(Name = "sortOrder")]
        public int SortOrder
        {
            get;
            set;
        }
    }
}