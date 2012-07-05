using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AirService.Model;
using AirService.Web.Infrastructure;
using AirService.Web.Infrastructure.Filters;

namespace AirService.Web.ViewModels
{
    public class AccountModel
    {
        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email again")]
        [Compare("Email", ErrorMessage = "The email and confirmation email do not match.")]
        public string ConfirmEmail { get; set; }

        public VenueViewModel VenueViewModel { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password again")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [MustBeTrue(ErrorMessage = "You must agree to the terms and conditions to register.")]
        public bool TermsAndConditionsAgreement { get; set; }

        public CreditCard CreditCard { get; set; }

        [StringLength(100)]
        public string PromotionCode { get; set; }

        public string ReCaptcha { get; set; }

        [Required(ErrorMessage = "Please type two words correctly as you see in the image at the bottom.")]
        public string recaptcha_response_field { get; set; }

        public string recaptcha_challenge_field { get; set; }
    }
}