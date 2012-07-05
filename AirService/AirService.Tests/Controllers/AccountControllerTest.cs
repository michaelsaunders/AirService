using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using AirService.Model;
using AirService.Services.Contracts;
using AirService.Services.Security;
using AirService.Web.Controllers;
using AirService.Web.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AirService;
using Moq;

namespace AirService.Tests.Controllers
{

    [TestClass]
    public class AccountControllerTest
    {

        [TestMethod]
        public void ChangePassword_Get_ReturnsView()
        {
            // Arrange
            AccountController controller = GetAccountController();

            // Act
            ActionResult result = controller.ChangePassword();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual(10, ((ViewResult)result).ViewData["PasswordLength"]);
        }

        [TestMethod]
        public void ChangePassword_Post_ReturnsRedirectOnSuccess()
        {
            // Arrange
            AccountController controller = GetAccountController();
            ChangePasswordModel model = new ChangePasswordModel()
            {
                OldPassword = "goodOldPassword",
                NewPassword = "goodNewPassword",
                ConfirmPassword = "goodNewPassword"
            };

            // Act
            ActionResult result = controller.ChangePassword(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            RedirectToRouteResult redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("ChangePasswordSuccess", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void ChangePassword_Post_ReturnsViewIfChangePasswordFails()
        {
            // Arrange
            AccountController controller = GetAccountController();
            ChangePasswordModel model = new ChangePasswordModel()
            {
                OldPassword = "goodOldPassword",
                NewPassword = "badNewPassword",
                ConfirmPassword = "badNewPassword"
            };

            // Act
            ActionResult result = controller.ChangePassword(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
            Assert.AreEqual("The current password is incorrect or the new password is invalid.", controller.ModelState[""].Errors[0].ErrorMessage);
            Assert.AreEqual(10, viewResult.ViewData["PasswordLength"]);
        }

        [TestMethod]
        public void ChangePassword_Post_ReturnsViewIfModelStateIsInvalid()
        {
            // Arrange
            AccountController controller = GetAccountController();
            ChangePasswordModel model = new ChangePasswordModel()
            {
                OldPassword = "goodOldPassword",
                NewPassword = "goodNewPassword",
                ConfirmPassword = "goodNewPassword"
            };
            controller.ModelState.AddModelError("", "Dummy error message.");

            // Act
            ActionResult result = controller.ChangePassword(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
            Assert.AreEqual(10, viewResult.ViewData["PasswordLength"]);
        }

        [TestMethod]
        public void ChangePasswordSuccess_ReturnsView()
        {
            // Arrange
            AccountController controller = GetAccountController();

            // Act
            ActionResult result = controller.ChangePasswordSuccess();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        } 

        [TestMethod]
        public void LogOn_Get_ReturnsView()
        {
            // Arrange
            AccountController controller = GetAccountController();

            // Act
            ActionResult result = controller.LogOn();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }
         

        [TestMethod]
        public void LogOn_Post_ReturnsViewIfModelStateIsInvalid()
        {
            // Arrange
            AccountController controller = GetAccountController();
            LogOnModel model = new LogOnModel()
            {
                Email = "someUser@mail.com",
                Password = "goodPassword",
                RememberMe = false
            };
            controller.ModelState.AddModelError("", "Dummy error message.");

            // Act
            ActionResult result = controller.LogOn(model, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
        }

        [TestMethod]
        public void LogOn_Post_ReturnsViewIfValidateUserFails()
        {
            // Arrange
            AccountController controller = GetAccountController();
            LogOnModel model = new LogOnModel()
            {
                Email = "someUser@mail.com",
                Password = "badPassword",
                RememberMe = false
            };

            // Act
            ActionResult result = controller.LogOn(model, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
            Assert.AreEqual("The user name or password provided is incorrect.", controller.ModelState[""].Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void Register_Get_ReturnsView()
        {
            // Arrange
            AccountController controller = GetAccountController();

            // Act
            ActionResult result = controller.Register();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual(10, ((ViewResult)result).ViewData["PasswordLength"]);
        }

        [TestMethod]
        public void Register_Post_ReturnsRedirectOnSuccess()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new AccountModel
            {
                Email = "goodEmail",
                ConfirmEmail = "goodEmail",
                Password = "goodPassword",
                ConfirmPassword = "goodPassword"
            };

            // Act
            ActionResult result = controller.Register(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            RedirectToRouteResult redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Home", redirectResult.RouteValues["controller"]);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void Register_Post_ReturnsViewIfRegistrationFails()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new AccountModel()
            {
                Email = "goodEmail",
                ConfirmEmail = "goodEmail",
                Password = "goodPassword",
                ConfirmPassword = "goodPassword"
            };

            // Act
            ActionResult result = controller.Register(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
            Assert.AreEqual("Username already exists. Please enter a different user name.", controller.ModelState[""].Errors[0].ErrorMessage);
            Assert.AreEqual(10, viewResult.ViewData["PasswordLength"]);
        }

        [TestMethod]
        public void Register_Post_ReturnsViewIfModelStateIsInvalid()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new AccountModel()
            {
                Email = "goodEmail",
                ConfirmEmail = "goodEmail",
                Password = "goodPassword",
                ConfirmPassword = "goodPassword"
            };
            controller.ModelState.AddModelError("", "Dummy error message.");

            // Act
            ActionResult result = controller.Register(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
            Assert.AreEqual(10, viewResult.ViewData["PasswordLength"]);
        }

        private static AccountController GetAccountController()
        {
            var mockServiceproviderService = new Mock<IServiceProviderService>();
            var mockVenueService = new Mock<IVenueService>();
            var mockVenueTypeServicde = new Mock<IService<VenueType>>();
            var countryService = new Mock<IService<Country>>();
            var stateService = new Mock<IStateService>();
            var membershipService = new Mock<IMembershipService>();
            RequestContext requestContext = new RequestContext(new MockHttpContext(), new RouteData());
            var controller = new AccountController(mockServiceproviderService.Object,
                                                   mockVenueService.Object,
                                                   mockVenueTypeServicde.Object,
                                                   countryService.Object, 
                                                   stateService.Object,
                                                   membershipService.Object)
                                 {
                                     Url = new UrlHelper(requestContext)
                                 };

            controller.ControllerContext = new ControllerContext
                                               {
                                                   Controller = controller,
                                                   RequestContext = requestContext
                                               };

            return controller;
        } 

        private class MockHttpContext : HttpContextBase
        {
            private readonly IPrincipal _user = new GenericPrincipal(new GenericIdentity("someUser"), null /* roles */);
            private readonly HttpRequestBase _request = new MockHttpRequest();

            public override IPrincipal User
            {
                get
                {
                    return _user;
                }
                set
                {
                    base.User = value;
                }
            }

            public override HttpRequestBase Request
            {
                get
                {
                    return _request;
                }
            }
        }

        private class MockHttpRequest : HttpRequestBase
        {
            private readonly Uri _url = new Uri("http://mysite.example.com/");

            public override Uri Url
            {
                get
                {
                    return _url;
                }
            }
        }  
    }
}
