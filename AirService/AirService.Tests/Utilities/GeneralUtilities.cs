using System;
using System.Collections.Generic;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Web.Content.EmailTemplates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AirService.Tests.Utilities
{
    [TestClass]
    public class GeneralUtilities
    {
        [TestMethod]
        public void Iso8061Date_String_Serialization()
        {
            // currently not supporting time offset
            var testDateValue = new DateTime(2011, 6, 20, 12, 41, 2);
            string utcFormatted = testDateValue.ToIso8061DateString();
            Assert.AreEqual("2011-06-20T02:41:02Z", utcFormatted);
            DateTime localTime1 = DateUtility.FromIso8061FormattedDateString(utcFormatted);
            DateTime localTime2 = DateUtility.FromIso8061FormattedDateString("2011-06-20T2:41:2Z");
            DateTime localTime3 = DateUtility.FromIso8061FormattedDateString("2011-6-20T02:41:02Z");
            Assert.AreEqual(testDateValue, localTime1);
            Assert.AreEqual(testDateValue, localTime2);
            Assert.AreEqual(testDateValue, localTime3);

            testDateValue = new DateTime(2011, 6, 20, 12, 41, 0);
            localTime1 = DateUtility.FromIso8061FormattedDateString("2011-06-20T2:41Z");
            localTime2 = DateUtility.FromIso8061FormattedDateString("2011-6-20T02:41Z");
            Assert.AreEqual(testDateValue, localTime1);
            Assert.AreEqual(testDateValue, localTime2);
         }

        [TestMethod]
        public void Temp()
        {
            var context = new AirServiceEntityFrameworkContext();
            if (context.Database.Exists())
            {
                context.Database.Delete();

            }

            context.Database.Create(); 
            new AirServiceInitializer().InitializeDatabase(context); 
        }

        [TestMethod]
        public void SubscriptionConfirmEmailContent()
        {
            var subscriptionEmail = new SubscriptionEmail();
            subscriptionEmail.Session = new Dictionary<string, object>();
            subscriptionEmail.Session.Add("VenueName", "Venue Name");
            subscriptionEmail.Session.Add("Address", "My Address");
            subscriptionEmail.Session.Add("PhoneNumber", "Telephone");
            subscriptionEmail.Session.Add("Email", "Email");
            subscriptionEmail.Session.Add("TypeOfVenue", "A Venue");
            subscriptionEmail.Session.Add("SubscriptionLevel", "AirService Premium");
            subscriptionEmail.Session.Add("ReferralCode", "Referral Code");
            subscriptionEmail.Initialize();
            var result = subscriptionEmail.TransformText();
            Assert.IsTrue(result.Contains("Venue Name"));
            Assert.IsTrue(result.Contains("My Address"));
            Assert.IsTrue(result.Contains("Telephone"));
            Assert.IsTrue(result.Contains("Email"));
            Assert.IsTrue(result.Contains("A Venue"));
            Assert.IsTrue(result.Contains("Referral Code"));  
        }
    }
}