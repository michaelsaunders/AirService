using System;
using System.Configuration;
using System.Globalization;
using System.ServiceModel;
using System.Xml.Serialization;
using AirService.Model;
using AirService.Model.Eway;
using AirService.Services.Eway;

namespace AirService.Services.Eway
{
    public interface IEwayHeader
    {
        eWAYHeader EwayHeader { set; }
    }
    
    [MessageContract]
    public class RebillEventRequestBase: IEwayHeader
    { 
        public readonly string LiteInit = ConfigurationManager.AppSettings["LiteInit"] ?? "1";
        public readonly string LiteRecur = ConfigurationManager.AppSettings["LiteRecur"] ?? "1";
        public readonly string PremInit = ConfigurationManager.AppSettings["PremInit"] ?? "1";
        public readonly string PremRecur = ConfigurationManager.AppSettings["PremRecur"] ?? "1";

        [System.ServiceModel.MessageHeaderAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill")]
        public eWAYHeader eWAYHeader;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 0)]
        public string RebillCustomerID;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 1)]
        public string RebillID;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 2)]
        public string RebillInvRef;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 3)]
        public string RebillInvDes;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 4)]
        public string RebillCCName;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 5)]
        public string RebillCCNumber;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 6)]
        public string RebillCCExpMonth;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 7)]
        public string RebillCCExpYear;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 8)]
        public string RebillInitAmt;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 9)]
        public string RebillInitDate;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 10)]
        public string RebillRecurAmt;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 11)]
        public string RebillStartDate;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 12)]
        public string RebillInterval;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 13)]
        public string RebillIntervalType;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill", Order = 14)]
        public string RebillEndDate;

        public eWAYHeader EwayHeader
        {
            set { this.eWAYHeader = value; }
        }

        public void SetPaymentSchedule(Venue venue, CreditCard creditCard, DateTime initDate)
        {
            SetCreditCardDetails(venue, creditCard);
            this.RebillInitDate = initDate.ToString("dd/MM/yyyy");
            this.RebillStartDate = initDate.Add(new TimeSpan(30, 0, 0, 0)).ToString("dd/MM/yyyy");
            this.RebillInterval = "1";
            this.RebillIntervalType = "3"; // 3 = month.
            // Bill for 100 years.
            this.RebillEndDate = initDate.Add(new TimeSpan(3650, 0, 0, 0)).ToString("dd/MM/yyyy");

            if (venue.VenueAccountType == (int) VenueAccountTypes.AccountTypeFull)
            {
                this.RebillInitAmt = this.PremInit;
                this.RebillRecurAmt = this.PremRecur;
            }
            else
            {
                this.RebillInitAmt = this.LiteInit;
                this.RebillRecurAmt = this.LiteRecur;
            }
        }

        public void SetPaymentSchedule(Venue venue, CreditCard creditCard, IRebillEventDetails details)
        {
            this.SetCreditCardDetails(venue, creditCard);
            this.RebillInitDate = details.RebillInitDate;
            this.RebillInitAmt = details.RebillInitAmt;
            this.RebillStartDate = details.RebillStartDate;
            this.RebillRecurAmt = details.RebillRecurAmt;
            this.RebillInterval = details.RebillInterval;
            this.RebillIntervalType = details.RebillIntervalType;
            this.RebillEndDate = details.RebillEndDate;
        }

        private void SetCreditCardDetails(Venue venue, CreditCard creditCard)
        {
            this.RebillCustomerID = venue.EwayCustomerId;
            this.RebillID = venue.EwayRebillId;
            this.RebillCCName = creditCard.AccountName;
            this.RebillCCNumber = creditCard.CardNumber;
            this.RebillCCExpYear = creditCard.ExpiryYear.ToString().PadLeft(2, '0');
            this.RebillCCExpMonth = creditCard.ExpiryMonth.ToString().PadLeft(2, '0');
        }
    }
}

public partial class CreateRebillCustomerRequest : IEwayHeader
{
    #region IEwayHeader Members

    public eWAYHeader EwayHeader
    {
        set { this.eWAYHeader = value; }
    }

    #endregion
}
 

public partial class QueryRebillEventRequest : IEwayHeader
{
    #region IEwayHeader Members

    public eWAYHeader EwayHeader
    {
        set { this.eWAYHeader = value; }
    }

    #endregion
} 

public partial class QueryTransactionsRequest: IEwayHeader
{
    public eWAYHeader EwayHeader
    {
        set { this.eWAYHeader = value; }
    }
}

public partial class DeleteRebillEventRequest : IEwayHeader
{
    public eWAYHeader EwayHeader
    {
        set { this.eWAYHeader = value; }
    }
}

public partial class rebillTransaction : IRebillTransaction
{
    /// <summary>
    /// Amount in Dollar
    /// </summary>
    [XmlIgnore]
    public decimal TransactionAmount
    {
        get
        {
            decimal value;
            if (decimal.TryParse(this.Amount, out value))
            {
                return value/100;
            }

            return 0;
        }
        set { this.Amount = (value*100).ToString(); }
    }
}