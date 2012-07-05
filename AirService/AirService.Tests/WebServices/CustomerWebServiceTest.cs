using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using AirService.Model;
using AirService.Services;
using AirService.WebServices.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AirService.Tests.WebServices
{
    [TestClass]
    public class CustomerWebServiceTest : WcfWebServiceTestBase
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            InitTestServer();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            EndTestServer();
        }

        [TestMethod]
        public void RejectInsertingCustomerWithoutUuid()
        {
            const string requestBody =
                @"{
    ""uuid"":       ""248C1A32-655F-4EF6-A853-6621E80356BC"",
    ""firstName"":  ""test first name"", 
    ""lastName"":   ""test last name"",
    ""facebookId"": 1234567
}";
            var result = this.Post("customer",
                                   requestBody)
                .ToEntity<OperationResult>();

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1001,
                            result.ErrorCode);
            Assert.AreEqual(Resources.Err1001UdidRequired,
                            result.Message);
        }

        [TestMethod]
        public void InsertCustomer()
        {
            const string requestBody =
                @"{
    ""udid"":       ""A385A46EDEAD48129E5B25725B35211D12345678"",
    ""firstName"":  ""test first name"", 
    ""lastName"":   ""test last name"",
    ""facebookId"": 1234567
}";
            HttpWebResponse result = this.Post("customer",
                                               requestBody);

            string jsonString;
            var customer = result.ToEntity<Customer>(out jsonString);
            Debug.WriteLine(jsonString);
            Assert.AreEqual(
                "test first name",
                customer.FirstName);
            Assert.AreEqual("test last name",
                            customer.LastName);
            Assert.AreEqual("1234567",
                            customer.FacebookId
                );
        }

        [TestMethod]
        public void GetCustomer()
        {
            var customer1 = this.InsertCustomerForTest("1101C796-FEB0-4D16-8918-300E0159D8210000");
            var customer2 = this.Get("customer/",
                                     userName: "1101C796-FEB0-4D16-8918-300E0159D8210000").ToEntity<Customer>();
            Assert.AreEqual(customer1.Id,
                            customer2.Id);
        }

        [TestMethod]
        public void UpdateCustomerRequiresAuthentication()
        {
            const string requestBody =
                @"{
    ""firstName"": ""updated first name"", 
    ""lastName"": ""updated last name"",
    ""facebookId"": 98765
}";
            HttpWebResponse response = this.Post("customer/update",
                                                 requestBody);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public void UpdateCustomer()
        {
            string jsonString;
            string requestBody =
                @"{
    ""udid"": ""B385A46EDEAD48129E5B25725B35211D12345678"",
    ""firstName"": ""test first name"", 
    ""lastName"": ""test last name"",
    ""facebookId"": 1234567
}";
            var customer = this.Post("customer",
                                     requestBody)
                .ToEntity<Customer>(out jsonString);

            Debug.WriteLine(jsonString);
            Assert.IsTrue(customer.Id > 0);

            Thread.Sleep(1000);
            requestBody =
                @"{
    ""firstName"": ""updated first name"", 
    ""lastName"": ""updated last name"",
    ""facebookId"": 98765
}";
            var updatedCustomer = this.Post("customer/update", 
                                            requestBody,
                                            "B385A46EDEAD48129E5B25725B35211D12345678")
                .ToEntity<Customer>(out jsonString);

            Debug.WriteLine(jsonString);
            Assert.AreEqual(updatedCustomer.Id,
                            customer.Id);
            Assert.AreEqual(updatedCustomer.FirstName,
                            "updated first name");
            Assert.AreEqual(updatedCustomer.LastName,
                            "updated last name");
            Assert.AreEqual(updatedCustomer.FacebookId,
                            "98765");
            Assert.AreEqual(updatedCustomer.CreateDate,
                            customer.CreateDate);
            Assert.IsTrue(updatedCustomer.UpdateDate >
                          customer.UpdateDate);
        }

        [TestMethod]
        public void UpdateCustomerPhoto()
        {
            const string requestBody =
                @"{
    ""udid"": ""C385A46EDEAD48129E5B25725B35211D12345678"",
    ""firstName"": ""test first name"", 
    ""lastName"": ""test last name"",
    ""facebookId"": 545454
}";
            var customer = this.Post("customer/",
                                     requestBody).ToEntity<Customer>();

            Assert.IsNotNull(customer);
            Assembly assembly = this.GetType().Assembly;
            const string embeddedSamplePhoto = "AirService.Tests.WebServices.SamplePhoto.jpg";
            Stream stream = assembly.GetManifestResourceStream(embeddedSamplePhoto);
            var result = this.PostStream("customer/updatephoto/SamplePhoto.jpg",
                                         stream,
                                         "C385A46EDEAD48129E5B25725B35211D12345678").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(0,
                            result.ErrorCode);
            Assert.IsTrue(result.Message.StartsWith("/ProfileImages/", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(result.Message.EndsWith(".jpg"));
            Stream responseStream = this.Get(result.Message).GetResponseStream();
            MD5 md5 = MD5.Create();
            byte[] hash1 = md5.ComputeHash(assembly.GetManifestResourceStream(embeddedSamplePhoto));
            byte[] hash2 = md5.ComputeHash(responseStream);
            md5.Clear();
            for (int i = 0; i < hash1.Length; i++)
            {
                Assert.AreEqual(hash1[i],
                                hash2[i]);
            }
        }

        [TestMethod]
        public void Inserting_New_Customer_With_Existing_UDID_Updates_Data_Instead_Of_Create_New()
        {
            var customer1 = this.InsertCustomerForTest("66F36346-517C-48F1-81C7-5647A8145979");
            var customer2 = this.InsertCustomerForTest("66F36346-517C-48F1-81C7-5647A8145979");
            Assert.AreEqual(customer1.Id,
                            customer2.Id);
        }

        //[TestMethod]
        //public void Temp()
        //{
        //    Assembly assembly = this.GetType().Assembly;
        //    const string embeddedSamplePhoto = "AirService.Tests.WebServices.SamplePhoto.jpg";
        //    Stream stream = assembly.GetManifestResourceStream(embeddedSamplePhoto);
        //    var result = this.PostStream("customer/updatephoto/SamplePhoto%2Ejpg",
        //                                 stream,
        //                                 userName: "00000000762fb9d277be4bd383107804ec882590",
        //                                 baseUri: "http://airservice.clients.webling.com.au/api-1/"
        //        ).ToEntity<OperationResult>();
        //}

        [TestMethod]
        public void RegisterForNotificationService()
        {
            var result = this.Post("/customer/registerForNotification",
                                   "{ \"udid\": \"941FDF8E-2823-4AA5-A9B3-FA6EE2990D4A\", \"token\": \"11DFF2CC-BD35-4244-AB9C-0119DB6D64EA\"}")
                .ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        public void RetrieveCustomerConnectedVenues()
        {
            var customer = this.InsertCustomerForTest("6CDFE35D-8F3B-491C-A454-764734065692");
            this.ConnectCustomerTo(customer,
                                   1);
            var orders = this.MakeSuccessfulOrders(1, customer, 2);
            var total = orders.Sum(o => o.TotalAmount);
            var result = this.Get("customer/venues", customer.Udid).ToEntity<List<CustomerVenueConnection>>();
            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(1, result[0].VenueId);
            Assert.AreEqual(total, result[0].SessionTotalAmount);
        }  
    }
}