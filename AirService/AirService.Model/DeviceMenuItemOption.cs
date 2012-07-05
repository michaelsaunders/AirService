using System.ComponentModel.DataAnnotations;

namespace AirService.Model
{
    public class DeviceMenuItemOption
    {
        [Key, Column(Order = 0)]
        public int iPadId { get; set; }

        [Key, Column(Order = 1)]
        public int MenuItemId { get; set; }

        public virtual iPad iPad { get; set; }

        public virtual MenuItem MenuItem { get; set; }

        public bool Print { get; set; }  
    }
}