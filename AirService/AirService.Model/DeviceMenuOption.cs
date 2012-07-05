using System.ComponentModel.DataAnnotations;

namespace AirService.Model
{
    /// <summary>
    /// iPadMenus table was never created as Model before. 
    /// Now we need to store Print option per menu per device, 
    /// however it involves many coding changes. 
    /// </summary>
    public class DeviceMenuOption
    {
        [Key, Column(Order = 0)]
        public int iPadId { get; set; }

        [Key, Column(Order = 1)]
        public int MenuId { get; set; }

        public bool Print { get; set; }

        [ForeignKey("iPadId")]
        public virtual iPad iPad { get; set; }

        [ForeignKey("MenuId")]
        public virtual Menu Menu { get; set; }
    }
}
