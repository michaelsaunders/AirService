using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web.Security;
using AirService.Model;
using AirService.Services;

namespace AirService.Web.Content.EmailTemplates
{
    public partial class SubscriptionCancelEmail
    {
        public static void Send(string customerEmail, Venue venue)
        {
            try
            {
                var subscriptionEmail = new SubscriptionCancelEmail();
                subscriptionEmail.Session = new Dictionary<string, object>();
                subscriptionEmail.Session.Add("VenueName", venue.Title);
                var address = venue.Address1 + " " + venue.Address2 + " " + venue.Suburb + " " +
                              venue.StateName + " " + venue.CountryName;
                subscriptionEmail.Session.Add("Address", address);
                subscriptionEmail.Session.Add("PhoneNumber", venue.Telephone ?? "");
                subscriptionEmail.Session.Add("Email", customerEmail);
                subscriptionEmail.Session.Add("TypeOfVenue", string.Join(",", venue.VenueTypes.Select(v => v.Title).ToArray()));
                subscriptionEmail.Session.Add("ReferralCode", venue.PromoCode ?? "");
                subscriptionEmail.Initialize();
                using (var smtp = new SmtpClient())
                {
                    var message = new MailMessage(ConfigurationManager.AppSettings["DefaultEmailFromAddress"] ??
                                                  "noreply@airservicehq.com",
                                                  customerEmail);

                    var registrationEmailBcc = ConfigurationManager.AppSettings["RegistrationEmailBcc"];
                    if (registrationEmailBcc != null)
                    {
                        message.Bcc.Add(registrationEmailBcc);
                    }

                    message.Subject = "Cancellation of Subscription";
                    message.Body = subscriptionEmail.TransformText();
                    message.IsBodyHtml = true;
                    smtp.Send(message);
                }
            }
            catch (Exception exception)
            {
                Logger.Log("Failed to send subscription confirmation email", exception);
            }
        }
    }
}
