using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AirService.Model;
using AirService.Services.Contracts;
using AirService.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AirService.Tests.Controllers
{
    /// <summary>
    /// Summary description for VendorControllerTest
    /// </summary>
    [TestClass]
    public class VendorControllerTest
    {
        public VendorControllerTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ListActionReturnsListOfVendors()
        {
            //var mockService = new Mock<IService<Vendor>>();
            //var vendors = new List<Vendor> { new Vendor { Id = 1, Title = "test" } };
            //mockService.Setup(m => m.FindAll()).Returns(vendors.AsQueryable<Vendor>);
            //var controller = new VendorController(mockService.Object);
            //var result = (ViewResult)controller.Index();
            //var model = result.ViewData.Model as IEnumerable<Vendor>;

            //Assert.AreEqual(1, model.Count());
        }

    }
}
