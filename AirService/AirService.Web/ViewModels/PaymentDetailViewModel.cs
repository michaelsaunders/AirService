using AirService.Model;
using AirService.Model.Eway;

namespace AirService.Web.ViewModels
{
    public class PaymentDetailViewModel
    {
        public CreditCard CreditCard { get; set; }

        public IRebillEventDetails RebillDetails { get; set; }
    }
}