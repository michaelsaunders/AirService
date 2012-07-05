using System.Collections.Generic;
using System.Linq;
using AirService.Data;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AirService.Tests.Services
{
    /// <summary>
    /// Summary description for VendorServiceTest
    /// </summary>
    [TestClass]
    public class VendorServiceTest
    {
        public VendorServiceTest()
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
        public void FindReturnsVendor()
        {
            //const int testId = 5;
            //var mock = new Mock<IRepository<Vendor>>();
            //mock.Setup(m => m.Find(testId)).Returns(new Vendor { Id = testId });
            //var vendorRepository = mock.Object;
            //var vendorService = new VendorService(vendorRepository);
            //var vendor = vendorService.Find(0);
            //Assert.IsNull(vendor);
            //vendor = vendorService.Find(testId);
            //Assert.IsInstanceOfType(vendor, typeof(Vendor));
            //Assert.IsTrue(vendor.Id == testId);
        }

        [TestMethod]
        public void FindAllReturnsVendorList()
        {
            //var fakeVendors = new List<Vendor>
            //                      {
            //                          new Vendor { Id = 1, Title = "alpha" },
            //                          new Vendor { Id = 2, Title = "beta" }
            //                      };
            //var mock = new Mock<IRepository<Vendor>>();
            //mock.Setup(m => m.FindAll()).Returns(fakeVendors.AsQueryable<Vendor>);
            //var vendorRepository = mock.Object;
            //var vendorService = new VendorService(vendorRepository);
            //var vendors = vendorService.FindAll();
            //Assert.IsInstanceOfType(vendors, typeof(IQueryable<Vendor>));
            //Assert.AreEqual(vendors.Count(), 2);
        }

        [TestMethod]
        public void InsertOrUpdateInsertsOnNew()
        {
            //const string testTitle = "test";
            //var mock = new Mock<IRepository<Vendor>>();
            //mock.Setup(m => m.Insert(It.IsAny<Vendor>())).Callback((Vendor v) => Assert.AreEqual(testTitle, v.Title));
            //var vendor = new Vendor { Title = testTitle };
            //var vendorService = new VendorService(mock.Object);
            //vendorService.InsertOrUpdate(vendor);
            //mock.VerifyAll();
        }

        [TestMethod]
        public void InsertOrUpdateUpdatesExisting()
        {
            //const int testId = 5;
            //var mock = new Mock<IRepository<Vendor>>();
            //mock.Setup(m => m.Update(It.IsAny<Vendor>())).Callback((Vendor v) => Assert.AreEqual(testId, v.Id));
            //var vendor = new Vendor { Id = testId, Title = "test" };
            //var vendorService = new VendorService(mock.Object);
            //vendorService.InsertOrUpdate(vendor);
            //mock.VerifyAll();
        }

        [TestMethod]
        public void DeleteDeletesExisting()
        {
            //const int testId = 5;
            //var mock = new Mock<IRepository<Vendor>>();
            //mock.Setup(m => m.Delete(testId)).Callback((int i) => Assert.AreEqual(testId, i));
            //var vendorService = new VendorService(mock.Object);
            //vendorService.Delete(testId);
            //mock.VerifyAll();
        }

    }
}
