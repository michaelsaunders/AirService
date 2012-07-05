using System;
using System.Collections.Generic;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Model.Eway;
using AirService.Services.Contracts;
using AirService.Services.Eway;
using AutoMapper;

namespace AirService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IEwayWrapper _ewayWrapper;
        private readonly IRepository<Venue> _venueRepository;

        public PaymentService(IEwayWrapper ewayWrapper, IRepository<Venue> venueRepository)
        {
            this._ewayWrapper = ewayWrapper;
            this._venueRepository = venueRepository;
        }

        #region IPaymentService Members

        public IRebillEventDetails CreateRebillingEvent(int venueId, CreditCard creditCard, out string errorMessage)
        {
            Venue venue = this._venueRepository.Find(venueId);
            if (venue.EwayCustomerId == null)
            {
                if (!this.CreateEwayCustomer(venue, out errorMessage))
                {
                    return null;
                }
            }

            DateTime initialPaymentDate = DateTime.Now;
            var createRequest = new CreateRebillEventRequest();
            createRequest.SetPaymentSchedule(venue, creditCard, initialPaymentDate);
            CreateRebillEventResponse eventResponse;
            try
            {
                eventResponse = this._ewayWrapper.CreateRebillEvent(createRequest);
                if (eventResponse.CreateRebillEventResult.Result != "Success")
                {
                    var failedPayment = new FailedPayment();
                    failedPayment.ErrorMessage = eventResponse.CreateRebillEventResult.ErrorDetails;
                    failedPayment.VenueId = venue.Id;
                    this._venueRepository.Set<FailedPayment>().Add(failedPayment);
                    this._venueRepository.Save();
                    errorMessage = failedPayment.ErrorMessage;
                    return null;
                }
            }
            catch(Exception e)
            {
                Logger.Log("CreateRebillingEvent", e);
                errorMessage = Resources.PaymentServerDown;
                return null;
            }

            venue.EwayRebillId = eventResponse.CreateRebillEventResult.RebillID;
            venue.VenueAccountType = venue.VenueAccountType & ~(int) VenueAccountTypes.AccountTypeEvaluation;
            this._venueRepository.Update(venue);
            this._venueRepository.Save();
            errorMessage = null;

            return eventResponse.CreateRebillEventResult;
        }

        public IRebillEventDetails GetRebillEvents(int venueId)  
        {
            Venue venue = this._venueRepository.Find(venueId);
            if (venue.EwayRebillId == null)
            {
                return null;
            }

            var request = new QueryRebillEventRequest
                              {
                                  RebillCustomerID = venue.EwayCustomerId,
                                  RebillID = venue.EwayRebillId
                              };
            try
            {
                var response = this._ewayWrapper.GetRebillEventDetails(request);
                return response.QueryRebillEventResult;
            }  
            catch
            {
                return null;
            }
        }

        public IRebillEventDetails UpdateRebillingEvent(int venueId, CreditCard creditCard, out string errorMessage)
        { 
            Venue venue = this._venueRepository.Find(venueId);
            if (venue.EwayCustomerId == null)
            {
                if (!this.CreateEwayCustomer(venue, out errorMessage))
                {
                    return null;
                } 
            }

            if (venue.EwayRebillId == null)
            {
                return this.CreateRebillingEvent(venueId, creditCard, out errorMessage);
            }

            var details = this.GetRebillEvents(venueId);
            if (details == null)
            {
                errorMessage = Resources.PaymentServerDown;
                return null;
            }

            var request = new UpdateRebillEventRequest();
            request.SetPaymentSchedule(venue, creditCard, details);
            UpdateRebillEventResponse response;
            try
            {
                response = new EwayWrapper().UpdateRebillEvent(request);
                if (response.UpdateRebillEventResult.Result != "Success")
                {
                    var failedPayment = new FailedPayment
                                            {
                                                ErrorMessage = response.UpdateRebillEventResult.ErrorDetails,
                                                VenueId = venue.Id
                                            };

                    this._venueRepository.Set<FailedPayment>().Add(failedPayment);
                    this._venueRepository.Save();
                    errorMessage = failedPayment.ErrorMessage;
                    return null;
                }
            }
            catch
            {
                errorMessage = Resources.PaymentServerDown;
                return null;
            }

            venue.EwayRebillId = response.UpdateRebillEventResult.RebillID;
            venue.EwayCustomerId = response.UpdateRebillEventResult.RebillCustomerID;
            this._venueRepository.Update(venue);
            this._venueRepository.Save();
            
            errorMessage = null;
            return response.UpdateRebillEventResult;
        }

        public IEnumerable<IRebillTransaction> QueryTransactions(int venueId, out string errorMessage)
        {
            Venue venue = this._venueRepository.Find(venueId);
            if (venue.EwayRebillId == null || venue.EwayCustomerId == null)
            {
                var records = (from transaction in this._venueRepository.Set<VenueTransaction>()
                               where transaction.VenueId == venueId
                               orderby transaction.TransactionDate descending
                               select transaction).Take(100).ToList();
                errorMessage = records.Count == 0 ? Resources.ErrorNoPaymentTransactions : null;
                return records;
            }

            try
            {
                errorMessage = null;
                var records = (from transaction in this._venueRepository.Set<VenueTransaction>()
                               where transaction.VenueId == venueId
                               orderby transaction.TransactionDate descending
                               select transaction).Take(100).ToList();
                var lastRecordDate = records.Select(t => (DateTime?) t.TransactionDate).FirstOrDefault();
                var response = new EwayWrapper().CreateQueryTransactionRequest(venue.EwayCustomerId,
                                                                               venue.EwayRebillId,
                                                                               lastRecordDate);
                int newRecords = 0;
                foreach (var transaction in response.QueryTransactionsResult)
                {
                    if (transaction.Status == TransactionStatus.Successful ||
                        transaction.Status == TransactionStatus.Failed)
                    {
                        var existingRecord = records.FirstOrDefault(
                            r =>
                            (transaction.TransactionNumber != "0" &&
                             transaction.TransactionNumber == r.TransactionNumber) ||
                            (transaction.TransactionNumber == "0" &&
                             r.TransactionNumber == "0" &&
                             r.TransactionDate == transaction.TransactionDate &&
                             r.TransactionAmount == transaction.TransactionAmount));
                        if (existingRecord != null)
                        {
                            records.Remove(existingRecord);
                            continue;
                        }

                        var record = Mapper.Map<rebillTransaction, VenueTransaction>(transaction);
                        record.VenueId = venueId;
                        record.Venue = venue;
                        record.RebillId = venue.EwayRebillId;
                        this._venueRepository.Insert(record);
                        newRecords++;
                    }
                }

                if (newRecords > 0)
                {
                    this._venueRepository.Save();
                }

                IEnumerable<IRebillTransaction> result = response.QueryTransactionsResult;
                return result.Concat(records);
            }
            catch (Exception e)
            {
                Logger.Log("Error querying transaction", e);
                errorMessage = Resources.PaymentServerDown;
                return null;
            }
        }

        public bool CancelRebill(int venueId, out string errorMessage)
        {
            try
            {
                var venue = this._venueRepository.Find(venueId);
                var response = this._ewayWrapper.CancelRebill(venue.EwayCustomerId, venue.EwayRebillId);
                errorMessage = response.DeleteRebillEventResult.ErrorDetails;
                return response.DeleteRebillEventResult.Result == "Success";
            }
            catch(Exception e)
            {
                Logger.Log("CancelRebill", e);
                errorMessage = Resources.PaymentServerDown;
                return false;
            }
        }

        #endregion

        private bool CreateEwayCustomer(Venue venue, out string errorMessage)
        {
            var customerDetail = new CreateRebillCustomerRequest();
            customerDetail.customerFirstName = venue.ContactFirstName;
            customerDetail.customerLastName = venue.ContactLastName;
            CreateRebillCustomerResponse customerResponse;
            try
            {
                customerResponse = this._ewayWrapper.CreateCustomer(customerDetail);
                if (customerResponse.CreateRebillCustomerResult.Result != "Success")
                {
                    var failedPayment = new FailedPayment();
                    failedPayment.ErrorMessage = customerResponse.CreateRebillCustomerResult.ErrorDetails;
                    failedPayment.VenueId = venue.Id;
                    this._venueRepository.Set<FailedPayment>().Add(failedPayment);
                    this._venueRepository.Save();
                    errorMessage = failedPayment.ErrorMessage;
                    return false;
                }
            }
            catch(Exception e)
            {
                Logger.Log("CreateEwayCustomer", e);
                errorMessage = Resources.PaymentServerDown;
                return false;
            }

            venue.EwayCustomerId = customerResponse.CreateRebillCustomerResult.RebillCustomerID;
            this._venueRepository.Update(venue);
            this._venueRepository.Save();
            errorMessage = null;
            return true;
        }
    }
}