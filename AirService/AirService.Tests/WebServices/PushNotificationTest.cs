using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services;
using AirService.Services.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AirService.Tests.WebServices
{
    ///<summary>
    ///  This is a test class for NotificationServiceTest and is intended
    ///  to contain all NotificationServiceTest Unit Tests
    ///</summary>
    [TestClass]
    public class PushNotificationTest
    {
        ///<summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get;
            set;
        }

        #region Additional test attributes

        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //

        #endregion

        ///<summary>
        ///  A test for Sending Notification. 
        ///  This *really* hit APN server.
        ///</summary>
        [TestMethod]
        [Conditional("DEBUG")]
        [DeploymentItem("AirService.Services.dll")]
        public void SendNotificationTest()
        {
            var mockContext = new Mock<IAirServiceContext>();
            var notificationTokens = new Mock<IDbSet<NotificationToken>>();
            NotificationToken invalidToken = null;
            notificationTokens.Setup(m => m.Remove(It.IsAny<NotificationToken>())).Callback(
                (NotificationToken tokenToRemove) => { invalidToken = tokenToRemove; });
            mockContext.SetupGet(m => m.NotificationTokens).Returns(notificationTokens.Object);
            IApnConnectionFactory connectionFactory = new ApnConnectionFactory();
            IApnConnection connection = connectionFactory.GetApnClientForCustomer(mockContext.Object);
            Assert.IsNotNull(connection);
            var token = new NotificationToken
                            {
                                Token = ConfigurationManager.AppSettings["TestAPNToken"],
                                Udid = ConfigurationManager.AppSettings["TestAPNUdid"]
                            };
            if (token.Udid == null || token.Token == null)
            {
                Assert.Inconclusive("TestAPNToken & TestAPNUdid not configured in your Test project app.config");
            }

            connection.Add(token,
                           new SimpleApnPayload
                               {
                                   Message = "hello?"
                               }.ToBytes());
            connection.Add(token,
                           new SimpleApnPayload
                               {
                                   Message = "hello again?"
                               }.ToBytes());
            connection.Send();
            notificationTokens.Verify(m => m.Remove(It.IsAny<NotificationToken>()),
                                      Times.Never());

            token.Token = token.Token.Substring(4) + "0000"; //make invalid token
            connection.Add(token,
                           new SimpleApnPayload
                               {
                                   Message = "hello!"
                               }.ToBytes());

            var nextConnection = connectionFactory.GetApnClientForCustomer(mockContext.Object);
            Assert.AreEqual(nextConnection, connection);
            // invalid token must be removed from the database.
            connection.Send();
            notificationTokens.Verify(m => m.Remove(It.IsAny<NotificationToken>()),
                                      Times.Once());
            Assert.IsNotNull(invalidToken);
        }
    }
}