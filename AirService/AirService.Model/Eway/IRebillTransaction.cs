using System;

namespace AirService.Model.Eway
{
    public interface IRebillTransaction
    {
        DateTime TransactionDate { get; set; }

        decimal TransactionAmount { get; set; }

        TransactionStatus Status { get; set; }

        string TransactionNumber { get; set; }

        string TransactionError { get; set; }
    }
}