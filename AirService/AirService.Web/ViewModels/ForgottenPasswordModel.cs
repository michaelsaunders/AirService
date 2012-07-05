using System.ComponentModel.DataAnnotations;

namespace AirService.Web.ViewModels
{
    public class ForgottenPasswordModel
    {
        [DataType(DataType.EmailAddress)]
        [Display(Name = "E-mail address")]
        [Required]
        public string EmailAddress { get; set; }
    }
}