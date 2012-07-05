using System.Collections.Generic;
using AirService.Model;
using AirService.Model.Eway;

namespace AirService.Web.Areas.Admin.Models
{
    public class TransactionViewModel
    {
        private readonly string _errorMessage;
        private readonly IEnumerable<IRebillTransaction> _transactions;
        private readonly Venue _venue;

        public TransactionViewModel(Venue venue, IEnumerable<IRebillTransaction> transactions, string errorMessage)
        {
            this._venue = venue;
            this._transactions = transactions;
            this._errorMessage = errorMessage;
        }

        public string ErrorMessage
        {
            get { return this._errorMessage; }
        }

        public IEnumerable<IRebillTransaction> Transactions
        {
            get { return this._transactions; }
        }

        public Venue Venue
        {
            get { return this._venue; }
        }
    }
}
