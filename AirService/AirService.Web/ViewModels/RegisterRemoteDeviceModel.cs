using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AirService.Web.Infrastructure;

namespace AirService.Web.ViewModels
{
    public class RegisterRemoteDeviceModel
    {
        [Required]
        [Display(Name = "User Code")]
        public string UserName { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}