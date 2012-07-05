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
    public partial class RegistrationNotificationEmail
    {
        public static void Send(Venue venue, string activationUrl, string customerEmail)
        {
            try
            {
                var registrationNotificationEmail = new RegistrationNotificationEmail();
                registrationNotificationEmail.Session = new Dictionary<string, object>();
                registrationNotificationEmail.Session.Add("VenueName", venue.Title);
                var address = venue.Address1 + " " + venue.Address2 + " " + venue.Suburb + " " +
                              venue.StateName + " " + venue.CountryName;
                registrationNotificationEmail.Session.Add("Address", address);
                registrationNotificationEmail.Session.Add("PhoneNumber", venue.Telephone ?? "");
                registrationNotificationEmail.Session.Add("Email", customerEmail);
                var typeOfVenus = venue.VenueTypes == null ? "" : string.Join(",", venue.VenueTypes.DefaultIfEmpty().Select(v => v.Title).ToArray());
                registrationNotificationEmail.Session.Add("TypeOfVenue", typeOfVenus);
                registrationNotificationEmail.Session.Add("ReferralCode", venue.PromoCode ?? "");
                registrationNotificationEmail.Session.Add("ActivationUrl", activationUrl);
                registrationNotificationEmail.Initialize();
                using (var smtp = new SmtpClient())
                {
                    var message = new MailMessage(ConfigurationManager.AppSettings["DefaultEmailFromAddress"] ??
                                                  "noreply@airservicehq.com",
                                                  ConfigurationManager.AppSettings["RegistrationEmailTo"] ?? "newsubscription.as@luxedigital.net");

                    var registrationEmailBcc = ConfigurationManager.AppSettings["RegistrationEmailBcc"];
                    if (registrationEmailBcc != null)
                    {
                        message.Bcc.Add(registrationEmailBcc);
                    }

                    message.Subject = "New Customer";
                    message.Body = registrationNotificationEmail.TransformText();
                    message.IsBodyHtml = true;
                    smtp.Send(message);
                }
            }
            catch (Exception exception)
            {
                Logger.Log("Failed to send registration notification email", exception);
            }
        }
    }
}
