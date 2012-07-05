using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    [DataContract]
    public class OrderItem : SimpleModel
    {
        private string _assignedTo;

        public DateTime OrderTime
        {
            get;
            set;
        }

        [MaxLength(512)]
        [DataMember(Name = "title")]
        public string Name
        {
            get;
            set;
        }

        [DataMember(Name = "menuItemId")]
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

        [DataMember(Name = "optionId")]
        public int? MenuItemOptionId
        {
            get;
            set;
        }

        public MenuItemOption MenuItemOption
        {
            get;
            set;
        }

        [DataMember(Name = "orderId")]
        public int OrderId
        {
            get;
            set;
        }

        public Order Order
        {
            get;
            set;
        }

        /// <summary>
        /// Line total price at the time of order
        /// </summary>
        [DataMember(Name = "price")]
        public Decimal Price
        {
            get;
            set;
        }
        
        [DataMember(Name = "quantity")]
        public int Quantity
        {
            get;
            set;
        }

        /// <summary>
        /// This is a temp storage that indicates wehther or not an iPad can assign with this order item
        /// Only valid if this data is used by an ipad app. 
        /// </summary>
        [NotMapped]
        [DataMember(Name = "canAssign")]
        public bool CanAssign
        {
            get;
            set;
        }

        /// <summary>
        /// iPad app shouldn't display this item if hidden
        /// </summary>
        [NotMapped]
        [DataMember(Name = "hidden")]
        public bool Hidden
        {
            get;
            set;
        }

        [NotMapped]
        [DataMember(Name = "assignedTo")]
        public string AssignedDeviceName
        {
            get
            {
                if (this.iPad != null)
                {
                    return this.iPad.Name;
                }

                return this._assignedTo;
            }
            private set
            {
                this._assignedTo = value;
            }
        }

        [DataMember(Name = "orderStatus")]
        public int OrderStatus
        {
            get;
            set;
        }

        public int? iPadId
        {
            get;
            set;
        }

        /// <summary>
        /// iPad that is assigned to this order item
        /// </summary>
        [ForeignKey("iPadId")]
        public virtual iPad iPad
        {
            get;
            set;
        }

        [DataMember(Name = "serviceOption")]
        public int ServiceOption
        {
            get;
            set;
        }

        /// <summary>
        /// Set to true if delivered, or customer picked up after processed 
        /// </summary>
        public bool Delivered
        {
            get;
            set;
        }

        /// <summary>
        /// This field is only available in certain cases
        /// It was added to help IOS dev to quickly be able to navigate Menu from 
        /// an order item
        /// </summary>
        [DataMember(Name = "menuId")]
        private int? MenuId
        {
            get
            {
                if (this.MenuItem != null)
                {
                    var category = this.MenuItem.MenuCategory;
                    if (category != null)
                    {
                        return category.MenuId;
                    }
                }

                return null;
            }
            set
            { 
            }
        }

        /// <summary>
        /// This field is only available in certain cases
        /// It was added to help IOS dev to quickly be able to navigate Menu from 
        /// an order item
        /// </summary>
        [DataMember(Name = "categoryId")]
        private int? MenuCategoryId
        {
            get
            {
                if (this.MenuItem != null)
                {
                    var category = this.MenuItem.MenuCategory;
                    if (category != null)
                    {
                        return category.Id;
                    }
                }

                return null;
            }
            set
            {
            }
        }
    }
}