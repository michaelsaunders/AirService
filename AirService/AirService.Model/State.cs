using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AirService.Model
{
    public class State : SimpleModel
    {
        [MaxLength(10)]
        public string Code
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200)]
        public string Title
        {
            get;
            set;
        }

        [Required]
        [DisplayName("Country")]
        public int CountryId
        {
            get;
            set;
        }

        //[ForeignKey("CountryId")]
        public virtual Country Country
        {
            get;
            set;
        }
    }
}