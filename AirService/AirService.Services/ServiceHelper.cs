using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AirService.Model;
using AirService.Services.Security;

namespace AirService.Services
{
    public static class ServiceHelper
    {
        public static void ValidateCustomerStatus(this Customer customer)
        {
            if (customer == null)
            {
                throw new ServiceFaultException(1003,
                                                Resources.Err1003CustomerNotFound);
            }

            if (customer.Status != SimpleModel.StatusActive)
            {
                throw new ServiceFaultException(1005,
                                                Resources.Err1005CustomerDisabled);
            }
        }

        public static void ValidateVenueStatus(this Venue venue)
        {
            if (venue == null)
            {
                throw new ServiceFaultException(1012,
                                                Resources.Err1012VenueNotFound);
            }

            if (!venue.IsActive)
            {
                throw new ServiceFaultException(1011,
                                                Resources.Err1011VenueTemporarilyNotAvailable);
            }
        }

        public static void ValidateVenueConnection(this VenueConnection connection, bool mustNotFreezed = true)
        {
            if (connection == null)
            {
                throw new ServiceFaultException(1026,
                                                Resources.Err1026CustomerNotConnected);
            }

            if (connection.ConnectionStatus == VenueConnection.StatusWaiting)
            {
                throw new ServiceFaultException(1027,
                                                Resources.Err1027WaitingToConnectToTheVenue);
            }

            if (mustNotFreezed && connection.FreezeUtil.HasValue && DateTime.Now < connection.FreezeUtil)
            {
                throw new ServiceFaultException(1025,
                                                Resources.Err1025FleezedCustomerCannotMakeAnOrder);
            }
        }

        /// <returns>
        /// Please use return value as if it were local time of calling user whether or not 
        /// the returning date/time kind is Utc or Local
        /// </returns>
        public static DateTime ToLoginUserTime(this AirServiceIdentity identity, DateTime dateTime)
        {
            if (identity.SecondsFromGmt.HasValue)
            {
                if (dateTime.Kind != DateTimeKind.Utc)
                {
                    dateTime = dateTime.ToUniversalTime();
                }

                return dateTime.AddSeconds(identity.SecondsFromGmt.Value);
            }

            return dateTime;
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }

        public static string ToConcatenatedString<T>(this IEnumerable<T> enumerable, string delimiter = ", ")
        {
            if (enumerable == null)
            {
                return "";
            }

            var stringBuilder = new StringBuilder();
            foreach (var item in enumerable)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(delimiter);
                }

                stringBuilder.Append(item.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}