using System.ComponentModel.DataAnnotations;

namespace AirService.Model
{
    [DisplayColumn("Title")]
    public class Country : SimpleModel
    {
        [MaxLength(10)]
        public string Code
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(256)]
        public string Title
        {
            get;
            set;
        }
    }
}