using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AirService.Data.Contracts;
using AirService.Services.Eway;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AirService.Tests.Services
{
    [TestClass]
    public class EwayTests
    {
        [TestMethod]
        public void CreateCustomerSuccess()
        {
            var target = new EwayWrapper();

            var request = new CreateRebillCustomerRequest();
            request.customerFirstName = "John";
            request.customerLastName = "Smith";

            var response = target.CreateCustomer(request);
            Assert.AreEqual("Success", response.CreateRebillCustomerResult.Result);
        }

        [TestMethod]
        public void CreateRebillEventSuccess()
        {
            var target = new EwayWrapper();

            var request = new CreateRebillEventRequest();
            request.RebillCustomerID = "6000154";

            request.RebillCCExpMonth = "1";
            request.RebillCCExpYear = (DateTime.Now.Year + 1).ToString().Substring(2, 2);
            request.RebillCCName = "John Smith";
            request.RebillCCNumber = "4444333322221111";

            request.RebillInitAmt = "500";
            request.RebillInitDate = DateTime.Now.ToString("dd/MM/yyyy");

            request.RebillStartDate = DateTime.Now.ToString("dd/MM/yyyy");
            request.RebillRecurAmt = "1";
            request.RebillEndDate = DateTime.Now.Add(new TimeSpan(3650, 0, 0)).ToString("dd/MM/yyyy");
            request.RebillInterval = "1";
            request.RebillIntervalType = "4";

            var response = target.CreateRebillEvent(request);

            Assert.AreEqual("Success", response.CreateRebillEventResult.Result);
        }

        [TestMethod]
        public void TestFullRum()
        {
            var target = new EwayWrapper();

            var request1 = new CreateRebillCustomerRequest();
            request1.customerFirstName = "John";
            request1.customerLastName = "Smith";

            var response1 = target.CreateCustomer(request1);
            Assert.AreEqual("Success", response1.CreateRebillCustomerResult.Result);

            var request = new CreateRebillEventRequest();
            request.RebillCustomerID = response1.CreateRebillCustomerResult.RebillCustomerID;

            request.RebillCCExpMonth = "1";
            request.RebillCCExpYear = (DateTime.Now.Year + 1).ToString().Substring(2, 2);
            request.RebillCCName = "John Smith";
            request.RebillCCNumber = "4444333322221111";

            request.RebillInitAmt = "500";
            request.RebillInitDate = DateTime.Now.ToString("dd/MM/yyyy");

            request.RebillStartDate = DateTime.Now.ToString("dd/MM/yyyy");
            request.RebillRecurAmt = "1";
            request.RebillEndDate = DateTime.Now.Add(new TimeSpan(3650, 0, 0)).ToString("dd/MM/yyyy");
            request.RebillInterval = "1";
            request.RebillIntervalType = "4";

            var response = target.CreateRebillEvent(request);

            Assert.AreEqual("Success", response.CreateRebillEventResult.Result);
        }

        [TestMethod]
        public void Temp()
        {
            using(var context = new AirServiceEntityFrameworkContext())
            {
                context.Database.Create();
            }
        }
    }
}
