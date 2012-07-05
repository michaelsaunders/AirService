using System.ComponentModel.DataAnnotations;

namespace AirService.Model
{
    /// <summary>
    /// POCO only it is not stored to/from database
    /// </summary>
    public class CreditCard
    {
        [Required(ErrorMessage = "Please enter name on card")]
        [StringLength(200)]
        public string AccountName { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Please enter creditcard number")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Please select creditcard type")]
        public string CardType { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Please enter security number")]
        public string CcNumber { get; set; }

        public int ExpiryMonth { get; set; }

        public int ExpiryYear { get; set; }
    }
}