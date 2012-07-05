using System.Collections.Generic;
using AirService.Model;
using AirService.Model.Eway;

namespace AirService.Services.Contracts
{
    public interface IPaymentService
    {
        IRebillEventDetails CreateRebillingEvent(int venueId,
                                                 CreditCard creditCard,
                                                 out string errorMessage);

        IRebillEventDetails GetRebillEvents(int venueId);

        IRebillEventDetails UpdateRebillingEvent(int venueId, CreditCard creditCard, out string errorMessage);

        IEnumerable<IRebillTransaction> QueryTransactions(int venueId, out string errorMessage);

        bool CancelRebill(int venueId, out string errorMessage);
    }
}
