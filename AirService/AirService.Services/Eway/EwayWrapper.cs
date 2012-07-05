using System;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;
using AirService.Model;

namespace AirService.Services.Eway
{
    public interface IEwayWrapper
    {
        CreateRebillCustomerResponse CreateCustomer(CreateRebillCustomerRequest request);

        CreateRebillEventResponse CreateRebillEvent(CreateRebillEventRequest request);
        
        QueryRebillEventResponse GetRebillEventDetails(QueryRebillEventRequest request); 
        
        DeleteRebillEventResponse CancelRebill(string ewayCustomerId, string ewayRebillId);
    }

    public class EwayWrapper : IEwayWrapper
    {
        private readonly ChannelFactory<manageRebillSoap> _rebillClientFactory;

        private readonly eWAYHeader _credentialHeader;


        public EwayWrapper()
        {
            var rebillSoapUrl = ConfigurationManager.AppSettings["RebillSoapUrl"] ??
                "https://www.eway.com.au/gateway/rebill/test/manageRebill_test.asmx";

            _credentialHeader = new eWAYHeader
                                    {
                                        eWAYCustomerID = ConfigurationManager.AppSettings["EwayId"] ?? "87654321",
                                        Username = ConfigurationManager.AppSettings["EwayUsername"] ?? "test@eway.com.au",
                                        Password = ConfigurationManager.AppSettings["EwayPassword"] ?? "test123"
                                    };

            _rebillClientFactory = new ChannelFactory<manageRebillSoap>(
                new BasicHttpBinding(BasicHttpSecurityMode.Transport), rebillSoapUrl);
        }

        private TResult OperateClient<TRequest, TResult>(Func<manageRebillSoap, TRequest, TResult> selector, TRequest request)
            where TRequest : IEwayHeader
        {
            // Cannot have null for string fields.
            var properties = typeof(TRequest)
                .GetFields().Where(a => a.FieldType == typeof(string));
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.GetValue(request) == null)
                {
                    propertyInfo.SetValue(request, "");
                }
            }

            // Set credential.
            request.EwayHeader = _credentialHeader;

            // Request.
            var client = _rebillClientFactory.CreateChannel();

            return selector(client, request);
        }

        public CreateRebillCustomerResponse CreateCustomer(CreateRebillCustomerRequest request)
        {
            Contract.Requires(!string.IsNullOrEmpty(request.customerFirstName));
            Contract.Requires(!string.IsNullOrEmpty(request.customerLastName));

            return OperateClient((client, req) => client.CreateRebillCustomer(req), request);
        }

        public CreateRebillEventResponse CreateRebillEvent(CreateRebillEventRequest request)
        {
            Contract.Requires(!string.IsNullOrEmpty(request.RebillCustomerID));
            Contract.Requires(!string.IsNullOrEmpty(request.RebillCCExpMonth));
            Contract.Requires(new Func<bool>(() =>
                                                 {
                                                     int month;
                                                     if (!int.TryParse(request.RebillCCExpMonth, out month))
                                                         return false;
                                                     return (month >= 1 && month <= 12);
                                                 })());
            Contract.Requires(!string.IsNullOrEmpty(request.RebillCCExpYear));
            Contract.Requires(request.RebillCCExpYear.Length == 2);
            Contract.Requires(new Func<bool>(() =>
                                                 {
                                                     int year;
                                                     return !int.TryParse(request.RebillCCExpYear, out year);
                                                 })());
            Contract.Requires(!string.IsNullOrEmpty(request.RebillCCName));
            Contract.Requires(!string.IsNullOrEmpty(request.RebillCCNumber));
            Contract.Requires(!string.IsNullOrEmpty(request.RebillInitAmt));
            Contract.Requires(!string.IsNullOrEmpty(request.RebillInitDate));
            Contract.Requires(!string.IsNullOrEmpty(request.RebillStartDate));
            Contract.Requires(!string.IsNullOrEmpty(request.RebillRecurAmt));
            Contract.Requires(!string.IsNullOrEmpty(request.RebillStartDate));
            Contract.Requires(!string.IsNullOrEmpty(request.RebillEndDate));
            Contract.Requires(!string.IsNullOrEmpty(request.RebillStartDate));
            Contract.Requires(!string.IsNullOrEmpty(request.RebillInterval));
            Contract.Requires(!string.IsNullOrEmpty(request.RebillIntervalType));

            return OperateClient((client, req) => client.CreateRebillEvent(req), request);
        }

        public QueryRebillEventResponse GetRebillEventDetails(QueryRebillEventRequest request)
        {
            return OperateClient((client, req) => client.QueryRebillEvent(req), request);
        }

        public DeleteRebillEventResponse CancelRebill(string ewayCustomerId, string ewayRebillId)
        { 
            var request = new DeleteRebillEventRequest 
                              {
                                  RebillCustomerID = ewayCustomerId,
                                  RebillID = ewayRebillId
                              };
            return this.OperateClient((client, req) => client.DeleteRebillEvent(request), request);
        }

        public UpdateRebillEventResponse UpdateRebillEvent(UpdateRebillEventRequest request)
        {
            return this.OperateClient((client, req)=>client.UpdateRebillEvent(request), request);
        }

        public QueryTransactionsResponse CreateQueryTransactionRequest(string customerId, string rebillId, DateTime? dateFrom)
        {
            var request = new QueryTransactionsRequest
                              {
                                  RebillCustomerID = customerId,
                                  RebillID = rebillId,
                                  startDate = dateFrom,
                                  endDate = DateTime.Now.AddMonths(1)
                              };
            return this.OperateClient((client, req) => client.QueryTransactions(request), request);
        }
    } 
}
