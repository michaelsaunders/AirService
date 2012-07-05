namespace AirService.Model.Eway
{
    public interface IRebillEventDetails
    {
        string Result { get; set; }

        string ErrorSeverity { get; set; }

        string ErrorDetails { get; set; }

        string RebillCustomerID { get; set; }

        string RebillID { get; set; }

        string RebillInvRef { get; set; }

        string RebillInvDesc { get; set; }

        string RebillCCName { get; set; }

        string RebillCCNumber { get; set; }

        string RebillCCExpMonth { get; set; }

        string RebillCCExpYear { get; set; }

        string RebillInitAmt { get; set; }

        string RebillInitDate { get; set; }

        string RebillRecurAmt { get; set; }

        /// <remarks />
        string RebillStartDate { get; set; }

        string RebillInterval { get; set; }

        string RebillIntervalType { get; set; }

        string RebillEndDate { get; set; }
    }
}
