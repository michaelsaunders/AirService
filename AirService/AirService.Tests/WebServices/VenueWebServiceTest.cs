using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using AirService.Model;
using AirService.Services;
using AirService.WebServices.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AirService.Tests.WebServices
{
    [TestClass]
    public class VenueWebServiceTest : WcfWebServiceTestBase
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
        public void VenueApp_Can_Get_Information_About_Currently_Connected_Customers()
        {
            var customer1 = this.InsertCustomerForTest("8B02D508-A088-48BC-A06D-BB5261FCB4E1");
            var customer2 = this.InsertCustomerForTest("BBC5AEF9-73D9-48C6-9268-895FC1666963)");

            Assert.IsFalse(this.Post("venue/1/connect",
                                     userName: customer1.Udid).ToEntity<OperationResult>().IsError);

            Assert.IsFalse(this.Post("venue/2/connect",
                                     userName: customer2.Udid).ToEntity<OperationResult>().IsError);

            var result = this.Post("venue/4/connect",
                                   userName: customer2.Udid).ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1048, result.ErrorCode); //Resources.Err1048CannotConnectToVenueWhichIsNotFullAccountType

            var anotherCustomer1 = this.Get("venue/customer/" + customer1.Id,
                                            Venue1iPad2Udid,
                                            "1/5678").ToEntity<Customer>();
            var anotherCustomer2 = this.Get("venue/customer/" + customer2.Id,
                                            Venue2iPad2Udid,
                                            "2/5678").ToEntity<Customer>();
            Assert.IsTrue(anotherCustomer1.Id == customer1.Id);
            Assert.IsTrue(anotherCustomer2.Id == customer2.Id);

            // now try to access other venues' customers 
            Assert.IsNull(this.Get("venue/customer/" + customer1.Id,
                                   Venue2iPad2Udid,
                                   "2/5678").ToEntity<Customer>());
            Assert.IsNull(this.Get("venue/customer/" + customer2.Id,
                                   Venue1iPad2Udid,
                                   "1/5678").ToEntity<Customer>());
        }

        [TestMethod]
        public void SearchVenues()
        {
            // See /WebServices/TestApp/Global.asax to see the mock data. 
            var result = this.Get("venue/search/Test Venu").ToEntity<List<Venue>>();
            Assert.AreEqual(2,
                            result.Count);
            foreach (Venue venue in result)
            {
                Assert.IsTrue(venue.Title.Contains("Test Venu"));
            }

            Venue match = this.Get("venue/search/Venue 1").ToEntity<List<Venue>>().First();
            Assert.IsTrue(match.Title.Contains("Venue 1"));
        }

        [TestMethod]
        public void SearchVenuesByLocationTestWithin1Km_WithOrWithout_TitleCriteria()
        {
            // Test data is location of corner of Pitt & Market street 
            // See /WebServices/TestApp/Global.asax to see the mock data. 
            string url = "venue/lat/-33.871057/lng/151.208096/radius/1";
            var result = this.Get(url).ToEntity<List<Venue>>().Select(v => v.Title).ToArray();
            // expecting 5 locations from Test Venue1 which is in the Pitt St. 
            // order by cloest location. 
            var expectedVenueTitles = new[]
                                          {
                                              "Test Venue 1",
                                              "Pitt St",
                                              "York St",
                                              "Kent St",
                                              "Sussex St"
                                          };
            Assert.AreEqual(5,
                            result.Length);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(expectedVenueTitles[i],
                                result[i]);
            }

            url = "venue/lat/-33.871057/lng/151.208096/radius/1/t St";
            result = this.Get(url).ToEntity<List<Venue>>().Select(v => v.Title).ToArray();
            expectedVenueTitles = new[]
                                      {
                                          "Pitt St",
                                          "Kent St"
                                      };
            Assert.AreEqual(2,
                            result.Length);
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(expectedVenueTitles[i],
                                result[i]);
            }
        }

        [TestMethod]
        public void SearchVenuesByLocationTestWithinDefaultRadius_WithOrWithout_TitleCriteria()
        {
            // Test data is location of corner of Pitt & Market street 
            // See /WebServices/TestApp/Global.asax to see the mock data. 
            string url = "venue/lat/-33.871057/lng/151.208096";
            var result = this.Get(url).ToEntity<List<Venue>>().Select(v => v.Title).ToArray();
            // expecting 5 locations from Test Venue1 which is in the Pitt St. 

            // Default radius is 5KM defined in DefaultLocationSearchRadius of VenueWebService class. 
            // Make it configurable as needed.  

            // order by cloest location. 
            var expectedVenueTitles = new[]
                                          {
                                              "Test Venue 1",
                                              "Pitt St",
                                              "York St",
                                              "Kent St",
                                              "Sussex St",
                                              "Pymont St",
                                          };
            Assert.AreEqual(6,
                            result.Length);
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(expectedVenueTitles[i],
                                result[i]);
            }

            // Same as the above test, but this time with criteria
            url = "venue/lat/-33.871057/lng/151.208096/t St";
            result = this.Get(url).ToEntity<List<Venue>>().Select(v => v.Title).ToArray();
            expectedVenueTitles = new[]
                                      {
                                          "Pitt St",
                                          "Kent St",
                                          "Pymont St",
                                      };
            Assert.AreEqual(3,
                            result.Length);
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(expectedVenueTitles[i],
                                result[i]);
            }
        }

        [TestMethod]
        public void GetVenueById()
        {
            string url = "venue/9999";
            var result = this.Get(url).ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1011,
                            result.ErrorCode);
            Assert.AreEqual(Resources.Err1011VenueTemporarilyNotAvailable,
                            result.Message);

            // now try one that is active
            url = "venue/1";
            var venue = this.Get(url).ToEntity<Venue>();
            Assert.AreEqual(1,
                            venue.Id);
        }

        [TestMethod]
        public void CannotConnectToVenueWhenAirServiceDisabledForThatVenue()
        {
            var customer = this.InsertCustomerForTest("3EAEA8F1-BCB0-4F9D-81E3-735010F1BE95");
            // See the test case with (search for the venue with "Id = 9999") 
            const string url = "venue/9999/connect";
            var result = this.Post(url,
                                   null,
                                   customer.Udid).ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1011,
                            result.ErrorCode);
            Assert.AreEqual(Resources.Err1011VenueTemporarilyNotAvailable,
                            result.Message);
        }

        [TestMethod]
        public void Coupling_Device_With_Id_And_Pin()
        {
            // Ensure that ID 4 and Pin 0000 is coupled with iPad4's Udid
            this.Get("venue/connections",
                     Venue1iPad4Udid,
                     "1/0000").ToEntity<List<VenueConnection>>();

            // now iPad1 try to use the same id and pin.

            var result = this.Get("venue/connections",
                                  Venue1iPad1Udid,
                                  "1/0000").ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(result.ErrorCode,
                            1014);
            Assert.AreEqual(Resources.Err1014DevicePinAlreadyUsedByAnotherDevice,
                            result.Message);

            // now forcefully login.. 
            result = this.Post("venue/device/updatecredential",
                               userName: Venue1iPad1Udid,
                               password: "1/0000").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            // iPad 1 now tries to get customers with the updated credential
            // instead of error 1014, now we expect venue connection objects return as expected

            var customer = this.InsertCustomerForTest("880595F4-A3CD-47D4-AD9B-95EB13ED5559");
            result = this.Post("Venue/1/Connect",
                               userName: customer.Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
            var venueConnections = this.Get("venue/connections",
                                            Venue1iPad1Udid,
                                            "1/0000").ToEntity<List<VenueConnection>>();
            Assert.IsTrue(venueConnections.Count > 0);
        }

        /// <summary>
        ///   This method include multiple tests. 
        ///   1. Connect
        ///   2. iPad Authentication
        ///   3. Accept Connection
        ///   4. Getting new/connected customers
        ///   5. Get a connected customer
        /// </summary>
        [TestMethod]
        public void ConnectToVenue_Then_AcceptConnection_Then_GetCustomers()
        {
            var customer = this.InsertCustomerForTest("5524C58E-7B7C-4AAF-8847-94E58617C443");
            string url = "Venue/1/Connect";
            var result = this.Post(url,
                                   null,
                                   customer.Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(1013,
                            result.ErrorCode);
            Assert.AreEqual(string.Format(Resources.Err1013WaitingToConnectToVenueArgs2,
                                          "Test Venue 1",
                                          1),
                            result.Message);

            // iPad Security Test. 
            Assert.AreEqual(HttpStatusCode.Unauthorized,
                            this.Get("venue/connections").StatusCode);

            var venueConnections = this.Get("venue/connections",
                                            Venue1iPad1Udid,
                                            "1/1234").ToEntity<List<VenueConnection>>();

            // Just cache the time so we can use this to get only customers who are changed since. 
            DateTime lastRequestTime = DateTime.Now;
            Thread.Sleep(1000);
            
            // Now we should have at least one waiting customer
            VenueConnection connection = venueConnections.Find(c => c.CustomerId == customer.Id);
            Assert.IsNotNull(connection);
            Assert.AreEqual(VenueConnection.StatusWaiting, connection.ConnectionStatus);
            Assert.IsNull(connection.ConnectedSince);

            var venueConnection =
                this.Post("venue/connection/accept/" + connection.CustomerId,
                          userName: Venue1iPad1Udid,
                          password: "1/1234").ToEntity<VenueConnection>();
            Assert.AreEqual(VenueConnection.StatusActive, venueConnection.ConnectionStatus);

            // try to reject after connected
            result = this.Post("venue/connection/reject/" + connection.CustomerId,
                               userName: Venue1iPad1Udid,
                               password: "1/1234").ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1034, result.ErrorCode);

            Thread.Sleep(1000);
            url = string.Format("venue/connections/modifiedsince/{0:yyyy-MM-ddTHH.mm.ss}Z",
                                lastRequestTime.ToUniversalTime());
            string jsonString;
            venueConnections = this.Get(url,
                                        Venue1iPad1Udid,
                                        "1/1234").ToEntity<List<VenueConnection>>(out jsonString);

            Debug.WriteLine(jsonString);
            connection = venueConnections.Find(c => c.CustomerId == customer.Id);
            Assert.IsNotNull(connection);
            Assert.AreEqual(VenueConnection.StatusActive, connection.ConnectionStatus);
            Assert.IsTrue(connection.ConnectedSince.HasValue && connection.ConnectedSince.Value > lastRequestTime);

            var connection2 = this.Get("venue/connection/" + customer.Id,
                                       Venue1iPad1Udid,
                                       "1/1234").ToEntity<VenueConnection>();
            Assert.IsNotNull(connection2); 
            Assert.AreEqual(VenueConnection.StatusActive, connection2.ConnectionStatus); 
            Assert.IsTrue(connection2.ConnectedSince.HasValue);
            Assert.AreEqual(connection.CustomerId,
                            connection2.CustomerId);
            Assert.AreEqual(connection.ConnectedSince,
                            connection2.ConnectedSince);
        }

        /// <summary>
        ///   Actor: iPad
        /// </summary>
        [TestMethod]
        public void GetVenueMenu()
        {
            var menus = this.Get("venue/menus",
                                 Venue1iPad1Udid,
                                 "1/1234").ToEntity<List<Menu>>();
            Assert.AreEqual(5,
                            menus.Count);
            foreach (var menu in menus)
            {
                Assert.AreEqual(1,
                                menu.VenueId);

                //Assert.IsTrue(testVenue.Status == SimpleModel.StatusActive);
                Assert.AreEqual(4 /*3*/,
                                menu.MenuCategories.Count);
                foreach (var category in menu.MenuCategories)
                {
                    //Assert.IsTrue(category.Status == SimpleModel.StatusActive);
                    Assert.AreEqual(4 /*3*/,
                                    category.MenuItems.Count);
                    foreach (var menuItem in category.MenuItems)
                    {
                        Assert.AreEqual(4 /*3*/,
                                        menuItem.MenuItemOptions.Count);
                        //Assert.IsTrue(menuItem.Status == SimpleModel.StatusActive);
                    }
                }
            }
        }

        [TestMethod]
        public void GetVenueDeviceMenu()
        {
            var menus = this.Get("venue/device/menus",
                                 Venue1iPad1Udid,
                                 "1/1234").ToEntity<List<Menu>>();
            Assert.AreEqual(5,
                            menus.Count);
            foreach (var menu in menus)
            {
                Assert.AreEqual(1,
                                menu.VenueId);
                //Assert.IsTrue(testVenue.Status == SimpleModel.StatusActive);
                Assert.AreEqual(4 /*3*/,
                                menu.MenuCategories.Count);
                foreach (var category in menu.MenuCategories)
                {
                    //Assert.IsTrue(category.Status == SimpleModel.StatusActive);
                    Assert.AreEqual(4 /*3*/,
                                    category.MenuItems.Count);
                    foreach (var menuItem in category.MenuItems)
                    {
                        Assert.AreEqual(4 /*3*/,
                                        menuItem.MenuItemOptions.Count);
                        //Assert.IsTrue(menuItem.Status == SimpleModel.StatusActive);
                    }
                }
            }
        }

        [TestMethod]
        public void FleezeAndUnfreezeCustomer()
        {
            var customer = this.InsertCustomerForTest("26E28F85-31BC-4366-9F4C-27FBD9AF3AFD");
            this.Post("venue/1/connect",
                      userName: customer.Udid);

            var connection = this.Post("venue/connection/accept/" + customer.Id,
                                       userName: Venue1iPad1Udid,
                                       password: "1/1234").ToEntity<VenueConnection>();
            Assert.IsNull(connection.FreezeUtil);
            Assert.AreEqual(
                customer.Id,
                connection.CustomerId);

            // Once we got connection established, try to freeze the customer for 5 minutes
            var now = DateTime.Now;
            var connectionAfterFreezed = this.Post("venue/customer/" + customer.Id + "/freeze/5",
                                                   userName: Venue1iPad1Udid,
                                                   password: "1/1234").ToEntity<VenueConnection>();

            Assert.IsNotNull(connectionAfterFreezed.FreezeUtil);
            Assert.IsTrue(connectionAfterFreezed.FreezeUtil.Value > now.AddMinutes(4));
            Assert.IsTrue(connectionAfterFreezed.FreezeUtil.Value < now.AddMinutes(6));
            var connectionBeforeUnfreeze = this.Get("venue/connection/" + customer.Id,
                                                    Venue1iPad1Udid,
                                                    "1/1234").ToEntity<VenueConnection>();
            Assert.AreEqual(customer.Id,
                            connectionBeforeUnfreeze.CustomerId);
            Assert.AreEqual(connectionAfterFreezed.FreezeUtil,
                            connectionBeforeUnfreeze.FreezeUtil);

            var lastConnection = this.Post("venue/customer/" + customer.Id + "/unfreeze",
                                           userName: Venue1iPad1Udid,
                                           password: "1/1234").ToEntity<VenueConnection>();
            Assert.AreEqual(customer.Id,
                            lastConnection.CustomerId);
            Assert.IsNull(lastConnection.FreezeUtil);
        }

        [TestMethod]
        public void MenuItemAvailibilityTest()
        {
            Assert.IsTrue(this.Get("venue/1/menuitem/1/isAvailable").ToBoolean());
            Assert.IsTrue(this.Get("venue/1/menuitem/2/isAvailable").ToBoolean());
            Assert.IsFalse(this.Get("venue/1/menuitem/3/isAvailable").ToBoolean());
            Assert.IsFalse(this.Get("venue/1/menuitem/4/isAvailable").ToBoolean());
        }

        /// <summary>
        ///   Actor: iPhone
        /// </summary>
        [TestMethod]
        public void GetVenueMenuForCustomer()
        {
            string url = "venue/lat/-33.871057/lng/151.208096";
            var testVenue = this.Get(url).ToEntity<List<Venue>>().First();

            // all test menu has set up predefined menu, category and items. 
            url = "venue/" + testVenue.Id + "/menus";
            var menus = this.Get(url).ToEntity<List<Menu>>();

            // In test mock-up data 
            // each test venue has 4 menu  
            // each menu has 4 category
            // each category has 4 menu items

            // and each of this objects they have one "Frozen or Deleted" menu
            // which shouldn't be downloaded. 

            // However each of this object also do have another status which admin user can toggle on/off
            // and regardless of that status, they will return. 

            // NOTE: In unit test can't test CmsStatus for Category, Item and Option as the mock objects are all linked by references at the test server. 

            Assert.AreEqual(5,
                            menus.Count);
            foreach (var menu in menus)
            {
                Assert.AreEqual(testVenue.Id,
                                menu.VenueId);

                //Assert.IsTrue(testVenue.Status == SimpleModel.StatusActive);
                Assert.AreEqual(4 /*3*/,
                                menu.MenuCategories.Count);
                foreach (var category in menu.MenuCategories)
                {
                    //Assert.IsTrue(category.Status == SimpleModel.StatusActive);
                    Assert.AreEqual(4 /*3*/,
                                    category.MenuItems.Count);
                    foreach (var menuItem in category.MenuItems)
                    {
                        Assert.AreEqual(4 /*3*/,
                                        menuItem.MenuItemOptions.Count);
                        //Assert.IsTrue(menuItem.Status == SimpleModel.StatusActive);
                    }
                }
            }
        }

        [TestMethod]
        public void Cannot_Make_Order_For_MenuItem_Which_Menu_Is_Not_Active()
        {
            var customer = this.InsertCustomerForTest("4661A036-165E-4637-8D68-88ED1DDDD999");
            this.ConnectCustomerTo(customer,
                                   1);
            var menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            // we going to test with 2 menu items from an in-active menu and one valid menu item.
            var activeMenuItems = (from menu in menus
                                   from category in menu.MenuCategories
                                   from item in category.MenuItems
                                   where
                                       menu.MenuStatus &&
                                       category.IsLive &&
                                       item.MenuItemStatus
                                   select item).Take(1);

            var menuItemsFromInactiveMenu = (from menu in menus
                                             from category in menu.MenuCategories
                                             from item in category.MenuItems
                                             where !menu.MenuStatus
                                             select item).Take(2);
            var menuItems = menuItemsFromInactiveMenu.Concat(activeMenuItems).ToArray();
            Assert.AreEqual(3,
                            menuItems.Length);
            var orderItemJsonString = this.CreateJsonOrderItemsFromMenuItems(menuItems);
            var oderDate = DateTime.Now.AddHours(12).ToIso8061DateString().Replace(":",
                                                                                   ".");
            Debug.WriteLine(orderItemJsonString);
            var result = this.Post("venue/1/order/at/" + oderDate,
                                   orderItemJsonString,
                                   customer.Udid).ToEntity<OperationResult>();
            Assert.AreEqual(1016,
                            result.ErrorCode);
            Assert.AreEqual(Resources.Err1016FailedToPlaceOrderDueToOneOrMoreItems,
                            result.Message);

            // Everything has to fail because menu itself is disabled. 
            Assert.AreEqual(2,
                            result.Items.Count);
            foreach (var item in result.Items)
            {
                Assert.AreEqual(1007, //Resources.Err1007MenuArg0NotAvailable
                                item.ErrorCode);
            }

            var activeMenuItemIds = activeMenuItems.Select(menuItem => menuItem.Id).ToArray();
            Assert.IsNull((from item in result.Items
                           where activeMenuItemIds.Contains(item.Id.Value)
                           select item).FirstOrDefault());
        }

        [TestMethod]
        public void Cannot_Make_Order_For_MenuItem_Which_Menu_Category_Is_Not_Active()
        {
            var customer = this.InsertCustomerForTest("495EDEAC-A3AD-444A-9EB1-5708F522359D");
            this.ConnectCustomerTo(customer,
                                   1);

            var menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            var menuItemFromInactiveMenuCategory = (from menu in menus
                                                    from category in menu.MenuCategories
                                                    from menuItem in category.MenuItems
                                                    where !category.IsLive
                                                    select menuItem).Take(2);

            var activeMenuItems = (from menu in menus
                                   from category in menu.MenuCategories
                                   from item in category.MenuItems
                                   where
                                       menu.MenuStatus && category.IsLive && item.MenuItemStatus
                                   select item).Take(1);

            // we going to test with 2 items from one inactive menu category and one valid menu item.  
            var menuItems = (menuItemFromInactiveMenuCategory).Concat(activeMenuItems).ToArray();
            Assert.AreEqual(3,
                            menuItems.Length);
            var orderItemJsonString = this.CreateJsonOrderItemsFromMenuItems(menuItems);
            Debug.WriteLine(orderItemJsonString);
            var result = this.Post("venue/1/order",
                                   orderItemJsonString,
                                   customer.Udid).ToEntity<OperationResult>();

            Assert.AreEqual(1016,
                            result.ErrorCode);
            Assert.AreEqual(Resources.Err1016FailedToPlaceOrderDueToOneOrMoreItems,
                            result.Message);

            // Everything has to fail because menu itself is disabled. 
            Assert.AreEqual(2,
                            result.Items.Count);

            foreach (var item in result.Items)
            {
                Assert.AreEqual(1008,
                                item.ErrorCode);
            }

            var activeMenuItemIds = activeMenuItems.Select(menuItem => menuItem.Id).ToArray();
            Assert.IsNull((from item in result.Items
                           where activeMenuItemIds.Contains(item.Id.Value)
                           select item).FirstOrDefault());
        }

        [TestMethod]
        public void Cannot_Make_Order_For_MenuItem_That_Is_Not_Active()
        {
            var customer = this.InsertCustomerForTest("34CE17B7-769B-43C8-9EB8-45EEC630B1FC");
            this.ConnectCustomerTo(customer,
                                   1);
            var menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            // we going to test with 2 menu items from an in-active menu and one valid menu item.
            var activeMenuItems = (from menu in menus
                                   from category in menu.MenuCategories
                                   from item in category.MenuItems
                                   where
                                       menu.MenuStatus &&
                                       category.IsLive &&
                                       item.MenuItemStatus
                                   select item).Take(1);

            var inactiveMenuItems = (from menu in menus
                                     from category in menu.MenuCategories
                                     from item in category.MenuItems
                                     where
                                         menu.MenuStatus &&
                                         category.IsLive &&
                                         !item.MenuItemStatus
                                     select item).Take(2);

            var menuItems = inactiveMenuItems.Concat(activeMenuItems).ToArray();
            Assert.AreEqual(3,
                            menuItems.Length);
            var orderItemJsonString = this.CreateJsonOrderItemsFromMenuItems(menuItems);
            Debug.WriteLine(orderItemJsonString);
            var result = this.Post("venue/1/order",
                                   orderItemJsonString,
                                   customer.Udid).ToEntity<OperationResult>();

            Assert.AreEqual(1016,
                            result.ErrorCode);
            Assert.AreEqual(Resources.Err1016FailedToPlaceOrderDueToOneOrMoreItems,
                            result.Message);

            // Everything has to fail because menu itself is disabled. 
            Assert.AreEqual(2,
                            result.Items.Count);
            foreach (var item in result.Items)
            {
                Assert.AreEqual(1009,
                                item.ErrorCode);
            }

            var activeMenuItemIds = activeMenuItems.Select(menuItem => menuItem.Id).ToArray();
            Assert.IsNull((from item in result.Items
                           where activeMenuItemIds.Contains(item.Id.Value)
                           select item).FirstOrDefault());
        }

        [TestMethod]
        public void Cannot_Make_Order_If_Current_Time_Not_In_Valid_Menu_Time()
        {
            var customer = this.InsertCustomerForTest("F79CE166-794F-486C-8A84-4934D4E5B17C");
            this.ConnectCustomerTo(customer,
                                   1);
            var menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            // we going to test with 2 menu items from an in-active menu and one valid menu item.
            var menuItemsToTest = new List<MenuItem>();
            var timeDependantMenu = menus.Where(m => !m.Is24Hour && m.MenuStatus).GetEnumerator();
            timeDependantMenu.MoveNext();
            menuItemsToTest.Add((from category in timeDependantMenu.Current.MenuCategories
                                 from item in category.MenuItems
                                 where
                                     category.IsLive &&
                                     item.MenuItemStatus
                                 select item).First());

            timeDependantMenu.MoveNext();
            menuItemsToTest.Add((from category in timeDependantMenu.Current.MenuCategories
                                 from item in category.MenuItems
                                 where
                                     category.IsLive &&
                                     item.MenuItemStatus
                                 select item).First());

            var activeMenuItems = (from menu in menus
                                   from category in menu.MenuCategories
                                   from item in category.MenuItems
                                   where
                                       menu.Is24Hour &&
                                       menu.MenuStatus &&
                                       category.IsLive &&
                                       item.MenuItemStatus
                                   select item).Take(1).ToArray();

            var menuItems = menuItemsToTest.Concat(activeMenuItems).ToArray();
            Assert.AreEqual(3,
                            menuItems.Length);
            var orderItemJsonString = this.CreateJsonOrderItemsFromMenuItems(menuItems);
            Debug.WriteLine(orderItemJsonString);
            var result = this.Post("venue/1/order",
                                   orderItemJsonString,
                                   customer.Udid).ToEntity<OperationResult>();

            Assert.AreEqual(1016,
                            result.ErrorCode);
            Assert.AreEqual(Resources.Err1016FailedToPlaceOrderDueToOneOrMoreItems,
                            result.Message);

            // Everything has to fail because menu itself is disabled. 
            Assert.AreEqual(1,
                            result.Items.Count);
            Assert.AreEqual(1018,
                            result.Items[0].ErrorCode);

            var activeMenuItemIds = activeMenuItems.Select(menuItem => menuItem.Id).ToArray();
            Assert.IsNull((from item in result.Items
                           where activeMenuItemIds.Contains(item.Id.Value)
                           select item).FirstOrDefault());
        }

        [TestMethod]
        public void Order_Life_Cycle()
        {
            var customer = this.InsertCustomerForTest("B8CDD966-92FC-4C0D-87FC-BACC74C3B458");
            this.ConnectCustomerTo(customer,
                                   1);
            var order = this.MakeSuccessfulOrders(1,
                                                  customer).First();
            var orderItem = order.OrderItems.First(item => item.MenuItemId > 8);

            // another venue shouldn't see the order item id
            var result = this.Post("venue/orderitem/" + orderItem.Id + "/confirm",
                                   userName: Venue2iPad2Udid,
                                   password: "2/5678").ToEntity<OperationResult>();
            Assert.AreEqual(1020,
                            result.ErrorCode);

            // this iPad not allowed to take the order. 
            result = this.Post("venue/orderitem/" + orderItem.Id + "/confirm",
                               userName: Venue1iPad4Udid,
                               password: "1/0000").ToEntity<OperationResult>();
            Assert.AreEqual(1021,
                            result.ErrorCode);

            // iPad 2 can process the order
            result = this.Post("venue/orderitem/" + orderItem.Id + "/confirm",
                               userName: Venue1iPad2Udid,
                               password: "1/5678").ToEntity<OperationResult>();
            Assert.AreEqual(0,
                            result.ErrorCode,
                            "Order Item Menu Item Id: " + orderItem.MenuItemId);

            // iPad 1 also can process the order but already iPad 2 took it. 
            result = this.Post("venue/orderitem/" + orderItem.Id + "/confirm",
                               userName: Venue1iPad1Udid,
                               password: "1/1234").ToEntity<OperationResult>();
            Assert.AreEqual(1039,
                            result.ErrorCode);

            // iPad 2 tries to confirm it again. 
            result = this.Post("venue/orderitem/" + orderItem.Id + "/confirm",
                               userName: Venue1iPad2Udid,
                               password: "1/5678").ToEntity<OperationResult>();
            Assert.AreEqual(1022,
                            result.ErrorCode);

            var updatedOrder = this.Get("venue/order/" + order.Id,
                                        Venue1iPad4Udid,
                                        "1/0000").ToEntity<Order>();
            Assert.AreEqual(order.Id,
                            updatedOrder.Id);
            Assert.AreEqual(Order.OrderStatusConfirmed,
                            updatedOrder.OrderStatus);
            Assert.IsNotNull(
                updatedOrder.OrderItems.SingleOrDefault(item => item.OrderStatus == Order.OrderStatusConfirmed));

            // now confirm all the rest order item to see if that update order status correctly
            foreach (var item in updatedOrder.OrderItems.Where(item => item.OrderStatus == Order.OrderStatusPending))
            {
                result = this.Post("venue/orderitem/" + item.Id + "/confirm",
                                   userName: Venue1iPad1Udid,
                                   password: "1/1234").ToEntity<OperationResult>();
                Assert.IsFalse(result.IsError);
            }

            updatedOrder = this.Get("venue/order/" + order.Id,
                                    Venue1iPad4Udid,
                                    "1/0000").ToEntity<Order>();
            Assert.AreEqual(Order.OrderStatusConfirmed,
                            updatedOrder.OrderStatus);
            Assert.AreEqual(1,
                            updatedOrder.OrderItems.Count(item => item.AssignedDeviceName == "iPad 2"));
            Assert.AreEqual(3,
                            updatedOrder.OrderItems.Count(item => item.AssignedDeviceName == "iPad 1"));

            // Now iPad 4 tries to update the order item status.
            result = this.Post("venue/orderitem/" + orderItem.Id + "/processedWith/1",
                               userName: Venue1iPad4Udid,
                               password: "1/0000").ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1021,
                            result.ErrorCode);

            // iPad1 try to update order item that was assigned to iPad2
            result = this.Post("venue/orderitem/" + orderItem.Id + "/processedWith/2",
                               userName: Venue1iPad1Udid,
                               password: "1/1234").ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1039,
                            result.ErrorCode);

            // iPad2 now update it's own order item
            result = this.Post("venue/orderitem/" + orderItem.Id + "/processedWith/1",
                               userName: Venue1iPad2Udid,
                               password: "1/5678").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            // Order item status should be still Confirmed. 
            updatedOrder = this.Get("venue/order/" + order.Id,
                                    Venue1iPad4Udid,
                                    "1/0000").ToEntity<Order>();
            Assert.AreEqual(Order.OrderStatusConfirmed,
                            updatedOrder.OrderStatus);

            // iPad1 update all of its order items.
            foreach (var item in updatedOrder.OrderItems.Where(item => item.Id != orderItem.Id))
            {
                result = this.Post("venue/orderitem/" + item.Id + "/processedWith/1/withMessage/dummy Message here",
                                   userName: Venue1iPad1Udid,
                                   password: "1/1234").ToEntity<OperationResult>();
                Assert.IsFalse(result.IsError);
            }

            updatedOrder = this.Get("venue/order/" + order.Id,
                                    Venue1iPad4Udid,
                                    "1/0000").ToEntity<Order>();

            // after all items are updated, the order must be automatically set to processed.
            Assert.AreEqual(Order.OrderStatusProcessed,
                            updatedOrder.OrderStatus);
        }


        [TestMethod]
        public void Order_Total_Must_Be_Recalculated_When_Item_Changes()
        {
            var customer = this.InsertCustomerForTest("5CE9092B-AAE8-43BB-92C1-20664661C6D3");
            this.ConnectCustomerTo(customer,
                                   1);
            var order = this.MakeSuccessfulOrders(1,
                                                  customer).First();
            Debug.WriteLine("Order ID: " + order.Id);
            var total = order.TotalAmount;
            var itemToCancel = order.OrderItems.First();
            var reduce = itemToCancel.Price;
            var result = this.Post("venue/orderitem/" + itemToCancel.Id + "/cancel",
                                       userName: Venue1iPad1Udid,
                                       password: "1/1234").ToEntity<OperationResult>();
            Assert.AreEqual(0,
                            result.ErrorCode);
            var updatedOrder = this.Get("venue/order/" + order.Id,
                                        Venue1iPad1Udid,
                                        "1/1234").ToEntity<Order>();
            Assert.AreEqual(total - reduce, updatedOrder.TotalAmount);
        }

        [TestMethod]
        public void CancelOrder()
        {
            Thread.Sleep(1000);
            var customer = this.InsertCustomerForTest("BDB5E8E0-20EF-435D-8393-58606172F7BE");
            this.ConnectCustomerTo(customer,
                                   1);
            var order = this.MakeSuccessfulOrders(1,
                                                  customer).First();

            int count = order.OrderItems.Count;
            Assert.IsTrue(count > 2);
            for (int i = 0; i < count; i++)
            {
                var orderItem = order.OrderItems[i];
                var result = this.Post("venue/orderitem/" + orderItem.Id + "/cancel",
                                       userName: Venue1iPad1Udid,
                                       password: "1/1234").ToEntity<OperationResult>();
                Assert.AreEqual(0,
                                result.ErrorCode);
                var updatedOrder = this.Get("venue/order/" + order.Id,
                                            Venue1iPad1Udid,
                                            "1/1234").ToEntity<Order>();
                if (i == count - 1)
                {
                    Assert.AreEqual(Order.OrderStatusCancelled,
                                    updatedOrder.OrderStatus);
                }
                else
                {
                    Assert.AreEqual(Order.OrderStatusPending,
                                    updatedOrder.OrderStatus);
                }
            }

            order = this.MakeSuccessfulOrders(1,
                                              customer).First();
            var cancelledTime = DateTime.Now;
            Thread.Sleep(1000);
            Assert.IsFalse(this.Post("venue/order/" + order.Id + "/cancel",
                                     userName: Venue1iPad1Udid,
                                     password: "1/1234").ToEntity<OperationResult>().IsError);

            var orders = (from o in this.Get("venue/orders",
                                             Venue1iPad1Udid,
                                             "1/1234").ToEntity<List<Order>>()
                          where o.CustomerId == customer.Id
                          select o).ToList();
            Assert.AreEqual(2,
                            orders.Count);
            Assert.AreEqual(Order.OrderStatusCancelled,
                            orders[0].OrderStatus);

            Assert.AreEqual(Order.OrderStatusCancelled,
                            orders[1].OrderStatus);

            Thread.Sleep(1000);
            // modified since must return cancelled order too
            var url = string.Format("venue/orders/modifiedsince/{0}", cancelledTime.ToIso8061DateString().Replace(":", "%2E"));
            orders = (from o in this.Get(url,
                                         Venue1iPad1Udid,
                                         "1/1234").ToEntity<List<Order>>()
                      where o.CustomerId == customer.Id
                      select o).ToList();
            Assert.AreEqual(2,
                            orders.Count);
            Assert.AreEqual(Order.OrderStatusCancelled,
                            orders[0].OrderStatus);

            Assert.AreEqual(Order.OrderStatusCancelled,
                            orders[1].OrderStatus);


            // modified since must return cancelled order item too
            url = string.Format("venue/orderitems/modifiedsince/{0}", cancelledTime.ToIso8061DateString().Replace(":", "%2E"));
            var orderItems = (from item in this.Get(url,
                                                    Venue1iPad1Udid,
                                                    "1/1234").ToEntity<List<OrderItem>>()
                              where item.OrderStatus == Order.OrderStatusCancelled
                              select item).ToList();
            Assert.IsTrue(
                (from o in orders
                 from item in o.OrderItems
                 orderby item.Id
                 select item.Id).SequenceEqual(orderItems.Select(item => item.Id).OrderBy(id => id))
                );
        }

        [TestMethod]
        public void CustomerCanRetrieveOrderHistory()
        {
            var customer = this.InsertCustomerForTest("89B43B7E-2015-44D2-971B-07377D678A6C");
            this.ConnectCustomerTo(customer,
                                   1);
            var order1 = this.MakeSuccessfulOrders(1,
                                                   customer).First();
            // Another customer can't see the order. 
            var customer2 = this.InsertCustomerForTest("14C53B61-3F05-49B5-B41D-0C67942EA4BE");
            var error = this.Get("venue/1/order/" + order1.Id,
                                 customer2.Udid).ToEntity<OperationResult>();
            Assert.IsTrue(error.IsError);
            Assert.AreEqual(1019,
                            error.ErrorCode);

            // Now retrieve the order with the first customer's credential
            var order2 = this.Get("venue/1/order/" + order1.Id,
                                  customer.Udid).ToEntity<Order>();
            Assert.AreEqual(order1.Id,
                            order2.Id);
            Assert.IsTrue(order1.OrderItems.Count > 0);
            Assert.AreEqual(order1.OrderItems.Count,
                            order2.OrderItems.Count);
        }

        [TestMethod]
        public void CustomerCanRetrieveAllOrdersInSession()
        {
            var customer = this.InsertCustomerForTest("F2A79DFB-08E4-4D54-B19D-79F16208E1C1");
            DateTime processedTime;
            DateTime finalizedTime;
            this.FinializingOrder(out processedTime,
                                  out finalizedTime,
                                  customer,
                                  3);
            // made 3 orders and all finalized
            //now connect again
            this.ConnectCustomerTo(customer,
                                   1);
            var orders1 = this.MakeSuccessfulOrders(1,
                                                    customer,
                                                    2).ToList();
            Debug.Write(string.Format("Order {0}, {1}",
                                      orders1[0].Id,
                                      orders1[1].Id));
            var orders2 = this.Get("venue/1/orders",
                                   customer.Udid).ToEntity<List<Order>>();
            Debug.Write(string.Format("Order {0}, {1}",
                                      orders2[0].Id,
                                      orders2[1].Id));
            // should only have returned all orders in the current session.
            Assert.AreEqual(2,
                            orders2.Count);

            // ensure the function returns in descending order of order create date time.
            Assert.IsTrue(
                orders2.OrderBy(order => order.Id).Select(order => order.Id).SequenceEqual(
                    orders1.OrderBy(order => order.Id).Select(order => order.Id)));
        }

        [TestMethod]
        public void VenueAppCanRetrieveCustomersOrders()
        {
            var customer = this.InsertCustomerForTest("ACAE76F4-30E9-4831-A354-B045C8566776");
            this.ConnectCustomerTo(customer,
                                   1);
            var orderIds = this.MakeSuccessfulOrders(1,
                                                     customer,
                                                     10).Select(order => order.Id).ToArray();
            Assert.AreEqual(10,
                            orderIds.Length);

            // another venue can't retrieve orders
            var orders = this.Get("venue/orders",
                                  Venue2iPad2Udid,
                                  "2/5678").ToEntity<List<Order>>();
            Assert.IsNull((from order in orders
                           where orderIds.Contains(order.Id)
                           select order).FirstOrDefault());

            // now continue test with venue 1 devices
            orders = this.Get("venue/orders",
                              Venue1iPad1Udid,
                              "1/1234").ToEntity<List<Order>>();
            Assert.AreEqual(10,
                            (from order in orders
                             where orderIds.Contains(order.Id)
                             select order).Count());
            orders = this.Get("venue/orders",
                              Venue1iPad2Udid,
                              "1/5678").ToEntity<List<Order>>();
            Assert.AreEqual(10,
                            (from order in orders
                             where orderIds.Contains(order.Id)
                             select order).Count());
            var now = DateTime.Now;
            Thread.Sleep(1000);
            // Now Try to get only items are modified since a certain date. 
            orderIds = this.MakeSuccessfulOrders(1,
                                                 customer,
                                                 10).Select(order => order.Id).ToArray();
            orders =
                this.Get(
                    string.Format("venue/orders/modifiedsince/{0}",
                                  now.ToUniversalTime().ToString("yyyy-MM-ddTHH.mm.ssZ")),
                    Venue1iPad2Udid,
                    "1/5678").ToEntity<List<Order>>();
            Assert.AreEqual(10,
                            (from order in orders
                             where orderIds.Contains(order.Id)
                             select order).Count());
        }

        [TestMethod]
        public void VenueAppCanRetrieveCustomersOrderById()
        {
            var customer = this.InsertCustomerForTest("D97C21C5-03C3-46A5-82D1-935A7391A370");
            this.ConnectCustomerTo(customer,
                                   1);
            var order1 = this.MakeSuccessfulOrders(1,
                                                   customer).First();
            // another venue can't retrieve orders
            var result = this.Get("venue/order/" + order1.Id,
                                  Venue2iPad2Udid,
                                  "2/5678").ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1019,
                            result.ErrorCode);

            // now continue test with venue 1 devices
            var order2 = this.Get("venue/order/" + order1.Id,
                                  Venue1iPad1Udid,
                                  "1/1234").ToEntity<Order>();
            Assert.AreEqual(order1.Id,
                            order2.Id);
        }

        [TestMethod]
        public void When_Retrieve_Orders_OrderItems_CanAssign_Flag_Must_Be_Updated_For_The_Requesting_Device()
        {
            var customer = this.InsertCustomerForTest("7697A90C-4831-465E-8531-A4FC79FCA37A");
            this.ConnectCustomerTo(customer,
                                   1);
            var order1 = this.MakeSuccessfulOrders(1,
                                                   customer).First();

            var order2 = this.Get("venue/order/" + order1.Id,
                                  Venue1iPad1Udid,
                                  "1/1234").ToEntity<Order>();

            // in the test setup, iPad 1 have association with all menu
            foreach (var orderItem in order2.OrderItems)
            {
                Assert.IsTrue(orderItem.CanAssign);
            }

            #region Test Function

            Action<Order> orderTestFunction = testOrder =>
                                                  {
                                                      // This second iPad is only associated with menu item 1 ~ 16
                                                      var iPad2MenuItemIds = Enumerable.Range(9, 16);
                                                      foreach (var orderItem in testOrder.OrderItems)
                                                      {
                                                          if (iPad2MenuItemIds.Contains(orderItem.MenuItemId))
                                                          {
                                                              Assert.IsTrue(orderItem.CanAssign);
                                                          }
                                                          else
                                                          {
                                                              Assert.IsFalse(orderItem.CanAssign);
                                                          }
                                                      }
                                                  };

            #endregion

            // Get the same order but requesting device is now iPad 2 
            // previous call modified CanAssign flags in our in-memory test objects
            // so make another order to test. 
            order1 = this.MakeSuccessfulOrders(1,
                                               customer).First();
            order2 = this.Get("venue/order/" + order1.Id,
                              Venue1iPad2Udid,
                              "1/5678").ToEntity<Order>();
            orderTestFunction(order2);

            // now ensure other methods works the same. 
            order1 = this.MakeSuccessfulOrders(1,
                                               customer).First();
            var orders = this.Get("venue/orders",
                                  Venue1iPad2Udid,
                                  "1/5678").ToEntity<List<Order>>();
            orderTestFunction(orders.First(o => o.Id == order1.Id));

            order1 = this.MakeSuccessfulOrders(1,
                                               customer).First();
            orders =
                this.Get(
                    string.Format("venue/orders/modifiedsince/{0:yyyy-MM-ddTHH.mm.ss}Z",
                                  DateTime.Now.AddMinutes(-1).ToUniversalTime()),
                    Venue1iPad2Udid,
                    "1/5678").ToEntity<List<Order>>();
            orderTestFunction(orders.First(o => o.Id == order1.Id));
        }

        [TestMethod]
        public void Order_Item_Price_Must_Be_Validated()
        {
            var menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            // we going to test with 2 menu items from an in-active menu and one valid menu item.
            var activeMenu = from menu in menus
                             where menu.MenuStatus
                             select menu;
            var activeMenuItems = (from category in activeMenu.First().MenuCategories
                                   from item in category.MenuItems
                                   where
                                       category.IsLive &&
                                       item.MenuItemStatus
                                   select item).Take(2);

            activeMenuItems = activeMenuItems.Concat(
                (from category in activeMenu.Skip(1).First().MenuCategories
                 from item in category.MenuItems
                 where
                     category.IsLive &&
                     item.MenuItemStatus
                 select item
                ).Take(2));

            Assert.AreEqual(4,
                            activeMenuItems.Count());
            var orderItemJsonString = "[" + string.Join(",",
                                                        (from menuItem in activeMenuItems
                                                         select
                                                             string.Format(
                                                                 @"
{{
    ""menuItemId"": {0}, 
    ""optionId"": {1},
    ""quantity"": 1, 
    ""price"": 92843.23
}}
",
                                                                 menuItem.Id,
                                                                 (menuItem.Id%2 == 1
                                                                      ? menuItem.MenuItemOptions.First().Id.ToString()
                                                                      : "null")
                                                             ))
                                                            .ToArray()) + "]";
            var customer = this.InsertCustomerForTest("7F60B7B9-4213-4B4F-8FE6-9583C3CD53AC");
            this.ConnectCustomerTo(customer,
                                   1);
            var result = this.Post("venue/1/order",
                                   orderItemJsonString,
                                   customer.Udid).ToEntity<OperationResult>();
            Assert.AreEqual(4,
                            result.Items.Count);
            foreach (var item in result.Items)
            {
                Assert.AreEqual(item.ErrorCode,
                                1023);
            }
        }

        [TestMethod]
        public void Undo_OrderItem_Status_Update()
        {
            var customer = this.InsertCustomerForTest("F4C38A6C-BC51-4B50-8473-612DBF88240E");
            this.ConnectCustomerTo(customer,
                                   1);
            var connection = this.Get("venue/connection/" + customer.Id,
                                      Venue1iPad1Udid,
                                      "1/1234").ToEntity<VenueConnection>();
            Assert.AreEqual(customer.Id,
                            connection.CustomerId);
            Assert.AreEqual(1,
                            connection.VenueId);

            // make some orders. 
            var order = this.MakeSuccessfulOrders(1,
                                                  customer).First();
            var orderItem = order.OrderItems.First();
            var result = this.Post("venue/orderitem/" + orderItem.Id + "/confirm",
                                   userName: Venue1iPad1Udid,
                                   password: "1/1234").ToEntity<OperationResult>();

            Assert.IsFalse(result.IsError); 
            var updatedOrder = this.Get("venue/order/" + order.Id,
                                        Venue1iPad1Udid,
                                        "1/1234").ToEntity<Order>();
            Assert.AreEqual(Order.OrderStatusConfirmed,
                            updatedOrder.OrderStatus);

            // now cancel the status update
            result = this.Post("venue/orderitem/" + orderItem.Id + "/undostatus",
                               userName: Venue1iPad1Udid,
                               password: "1/1234").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
            updatedOrder = this.Get("venue/order/" + order.Id,
                                    Venue1iPad1Udid,
                                    "1/1234").ToEntity<Order>();
            Assert.AreEqual(Order.OrderStatusPending,
                            updatedOrder.OrderStatus);
            Assert.AreEqual(Order.OrderStatusPending,
                            updatedOrder.OrderItems.First(item => item.Id == orderItem.Id).OrderStatus);

            // confirm and process all items
            foreach (var item in order.OrderItems)
            {
                result = this.Post("venue/orderitem/" + item.Id + "/confirm",
                                   userName: Venue1iPad1Udid,
                                   password: "1/1234").ToEntity<OperationResult>();
                Assert.IsFalse(result.IsError);

                result = this.Post("venue/orderitem/" + item.Id + "/processedWith/1/withMessage/hello there",
                                   userName: Venue1iPad1Udid,
                                   password: "1/1234").ToEntity<OperationResult>();
                Assert.IsFalse(result.IsError);
            }

            // now cancel the status update
            result = this.Post("venue/orderitem/" + orderItem.Id + "/undostatus",
                               userName: Venue1iPad1Udid,
                               password: "1/1234").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
            updatedOrder = this.Get("venue/order/" + order.Id,
                                    Venue1iPad1Udid,
                                    "1/1234").ToEntity<Order>();
            Assert.AreEqual(Order.OrderStatusConfirmed,
                            updatedOrder.OrderStatus);
            Assert.AreEqual(Order.OrderStatusConfirmed,
                            updatedOrder.OrderItems.First(item => item.Id == orderItem.Id).OrderStatus);
        }

        [TestMethod]
        public void Disconnected_Customers_Shouldbe_Included_in_Modified_Connections_Result()
        {
            DateTime processedTime, finializedTime;
            var customer = this.InsertCustomerForTest("6D9ABCCF-26D4-4BD9-B80A-E70DC5A8F8F3");
            this.FinializingOrder(out processedTime, out finializedTime, customer); 
            string url = string.Format("venue/connections/modifiedsince/{0:yyyy-MM-ddTHH.mm.ss}Z",
                                       processedTime.ToUniversalTime());
            
            url = url.Replace(".", "%2E");
            string jsonString;
            var venueConnections = this.Get(url,
                                            Venue1iPad1Udid,
                                            "1/1234").ToEntity<List<VenueConnection>>(out jsonString);
            var disconnectedOnes = from connection in venueConnections
                                   where
                                       connection.ConnectionStatus == VenueConnection.Disconnected
                                   select connection;
            // ensure the disconnected customer is included in the "modifiedSince" method
            Assert.AreEqual(1, disconnectedOnes.Count());
            Assert.AreEqual(disconnectedOnes.First().CustomerId, customer.Id); 
            // but when get all customers, none of disconnected customers should return
            venueConnections = this.Get("venue/connections",
                                            Venue1iPad1Udid,
                                            "1/1234").ToEntity<List<VenueConnection>>(out jsonString);

            Assert.IsNull((from c in venueConnections
                           where c.Status == VenueConnection.Disconnected
                           select c).FirstOrDefault()); 
        }

        [TestMethod]
        public void FinalizingOrder()
        {
            DateTime processedTime, finializedTime;
            this.FinializingOrder(out processedTime, out finializedTime);
            var url = string.Format("venue/orders/modifiedsince/{0:yyyy-MM-ddTHH.mm.ss}Z",
                                    processedTime.ToUniversalTime());
            var orders = this.Get(url,
                                  Venue1iPad1Udid,
                                  "1/1234").ToEntity<List<Order>>();
            
            //finalized order should be included in the modifiedsince call
            Assert.IsNotNull((from order in orders
                              where order.OrderStatus == Order.OrderStatusFinalized
                              select order).FirstOrDefault());
        }

        private void FinializingOrder(out DateTime processedTime, out DateTime finializedTime, Customer customerToUse = null, int numOrders = 2)
        {
            Customer customer = customerToUse ?? this.InsertCustomerForTest("8C4B22DC-9D5D-4FA9-9522-B4E8F43F3CDC");
            this.ConnectCustomerTo(customer,
                                   1);
            var connection = this.Get("venue/connection/" + customer.Id,
                                      Venue1iPad1Udid,
                                      "1/1234").ToEntity<VenueConnection>();
            Assert.AreEqual(customer.Id,
                            connection.CustomerId);
            Assert.AreEqual(1,
                            connection.VenueId);

            // make some orders. 
            var orders = this.MakeSuccessfulOrders(1,
                                                   customer,
                                                   numOrders).ToList();

            // try to finialize 
            var result = this.Post("venue/customer/" + customer.Id + "/finalizeOrders",
                                   userName: Venue1iPad1Udid,
                                   password: "1/1234").ToEntity<OperationResult>();
            Assert.AreEqual(1028,
                            result.ErrorCode);

            // let customer to request to close 
            result = this.Post("venue/1/close",
                               userName: customer.Udid).ToEntity<OperationResult>();
            Assert.AreEqual(1037, //Resources.Err1037CannotCloseConnectionWhenActiveOrdersExist
                            result.ErrorCode);

            // process all orders

            int i = 0;
            foreach (var order in orders)
            {
                foreach (var orderItem in order.OrderItems)
                {
                    result = this.Post("venue/orderitem/" + orderItem.Id + "/confirm",
                                       userName: Venue1iPad1Udid,
                                       password: "1/1234").ToEntity<OperationResult>();
                    Assert.IsFalse(result.IsError);

                    var url = "venue/orderitem/" + orderItem.Id + "/processedWith/1";
                    if (i++ % 2 == 0)
                    {
                        url += "/WithMessage/hello there";
                    }

                    result = this.Post(url,
                                       userName: Venue1iPad1Udid,
                                       password: "1/1234").ToEntity<OperationResult>();
                    Assert.IsFalse(result.IsError);
                }
            }

            processedTime = DateTime.Now;
            Thread.Sleep(1000);
            // let customer to request to close 
            result = this.Post("venue/1/close",
                               userName: customer.Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            // now finalize it 
            DateTime beforeCall = DateTime.Now;
            var finalResult = this.Post("venue/customer/" + customer.Id + "/finalizeOrders",
                                        userName: Venue1iPad1Udid,
                                        password: "1/1234",
                                        secondsFromGmt: 18000).ToEntity<OperationResult<string>>();
            DateTime afterCall = DateTime.Now;
            Assert.IsFalse(finalResult.IsError);
            Debug.Write(finalResult.Data);
            var duration = afterCall - beforeCall;
            var wasLocalTime = false;
            for(int minute = 0; minute < Math.Ceiling(duration.TotalSeconds); minute++)
            {
                var receiptTime = beforeCall.ToUniversalTime().AddSeconds(18000).AddMinutes(minute);
                wasLocalTime = finalResult.Data.Contains(receiptTime.ToString("h:mmtt"));
                if (wasLocalTime)
                {
                    break;
                }
            }

            Assert.IsTrue(wasLocalTime);
            finializedTime = DateTime.Now;
            // the customer is now disconnected.
            result = this.Get("venue/connection/" + customer.Id,
                              Venue1iPad1Udid,
                              "1/1234").ToEntity<OperationResult>();
            Assert.AreEqual(1026,
                            result.ErrorCode);
        }

        [TestMethod]
        public void EnableDisableVenue()
        { 
            // ensure unknown user can't reach the method
            Assert.AreEqual(HttpStatusCode.Unauthorized, this.Post("venue/enable/true",
                                                                   userName: "asdfasdf",
                                                                   password: "qwerqwer").StatusCode);

            // ensure venue data is not visible to another venue data
            var result = this.Post("venue/enable/false",
                                   userName: "venue2Admin",
                                   password: "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            // we disabled venue 2. thus nothing to do with venue 1 test below 
            var customer = this.InsertCustomerForTest("AB9AE451-1DCD-462B-9A1D-114C7E79A877");
            result = this.ConnectCustomerTo(customer,
                                            1);
            Assert.IsFalse(result.IsError);

            // now that we know the URI works independantly per venue, re-enable the venue 2 for other tests. 
            result = this.Post("venue/enable/true",
                               userName: "venue2Admin",
                               password: "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            // now going to disable venue 1
            result = this.Post("venue/enable/false",
                               userName: "venue1Admin",
                               password: "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            customer = this.InsertCustomerForTest("3128AB03-A75D-419E-80C2-22B3245B667D");
            result = this.ConnectCustomerTo(customer,
                                            1,
                                            false);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1011,
                            result.ErrorCode); // Resources.Err1011VenueTemporarilyNotAvailable

            // enable back the venue 
            result = this.Post("venue/enable/true",
                               userName: "venue1Admin",
                               password: "password").ToEntity<OperationResult>();

            Assert.IsFalse(result.IsError);
            result = this.ConnectCustomerTo(customer,
                                            1);
            Assert.IsFalse(result.IsError);

            // also device admin can do the same
            result = this.Post("venue/enable/false",
                               userName: "venue1DeviceAdmin@airservice.com",
                               password: "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
            result = this.Post("venue/enable/true",
                               userName: "venue1DeviceAdmin@airservice.com",
                               password: "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError); 
        }

        [TestMethod]
        public void EnableDisableMenu()
        {
            Assert.AreEqual(1030,
                            this.Post("venue/menu/1/enable/true",
                                      userName: "venue2Admin",
                                      password: "password").ToEntity<OperationResult>().ErrorCode);

            var result = this.Post("venue/menu/2/enable/false",
                                   userName: "venue1Admin",
                                   password: "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            var menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            Assert.IsFalse(menus.First(m => m.Id == 2).MenuStatus);

            var lastMenu = menus.Last();
            // device admin in this test is setup to access two menus from the second menu
            result = this.Post("venue/menu/" + lastMenu.Id + "/enable/true",
                               userName: "venue1DeviceAdmin@airservice.com",
                               password: "password").ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1033, result.ErrorCode);

            // re-enable it by device admin  
            result = this.Post("venue/menu/2/enable/true",
                               userName: "venue1DeviceAdmin@airservice.com",
                               password: "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            var menu = this.Get("venue/1/menus").ToEntity<List<Menu>>().First(m => m.Id == 2);
            Assert.IsTrue(menu.MenuStatus);
        }

        [TestMethod]
        public void EnableDisableMenuPrinting()
        {
            var result = this.Post("venue/device/updatecredential",
                                  userName: Venue1iPad1Udid,
                                  password: "1/0000").ToEntity<OperationResult>();
            
            Assert.AreEqual(1033,
                            this.Post("venue/menu/2/printing/true",
                                      userName: "venue2Admin",
                                      password: "password",
                                      deviceId: Venue1iPad1Udid).ToEntity<OperationResult>().ErrorCode);

            result = this.Post("venue/menu/2/printing/true",
                               userName: "venue1Admin",
                               password: "password",
                               deviceId: Venue1iPad1Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError, result.Message);

            var menus = this.Get("venue/1/menus", deviceId: Venue1iPad1Udid).ToEntity<List<Menu>>();
            Assert.IsTrue(menus.First(m => m.Id == 1).Print);

            // disable it by device admin  
            result = this.Post("venue/menu/2/printing/false",
                               userName: "venue1DeviceAdmin@airservice.com",
                               password: "password",
                               deviceId: Venue1iPad1Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError, result.Message);

            var menu = this.Get("venue/1/menus", deviceId: Venue1iPad1Udid).ToEntity<List<Menu>>().First(m => m.Id == 2);
            Assert.IsFalse(menu.Print);
        }

        [TestMethod]
        public void EnableDisableMenuCategory()
        {
            Assert.AreEqual(1031,
                            this.Post("venue/menucategory/1/enable/true",
                                      userName: "venue2Admin",
                                      password: "password").ToEntity<OperationResult>().ErrorCode);

            var menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            // get second menu's first category
            var categoryId = menus.Skip(1).First().MenuCategories.First().Id;

            var result = this.Post("venue/menucategory/" + categoryId + "/enable/false",
                                   userName: "venue1Admin",
                                   password: "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            var menuCategory = (from menu in menus
                                from category in menu.MenuCategories
                                where category.Id == categoryId
                                select category).First();
            Assert.IsFalse(menuCategory.IsLive);

            // device admin in this test is setup to access two menus from the second menu
            var lastMenuCategoryId = menus.Last().MenuCategories.First().Id;
            result = this.Post("venue/menucategory/" + lastMenuCategoryId + "/enable/false",
                               userName: "venue1DeviceAdmin@airservice.com",
                               password: "password").ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1033, result.ErrorCode);

            // re-enable it by the device admin
            result = this.Post("venue/menucategory/" +categoryId + "/enable/true",
                               userName: "venue1DeviceAdmin@airservice.com",
                               password: "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            menuCategory = (from menu in menus
                            from category in menu.MenuCategories
                            where category.Id == 1
                            select category).First();
            Assert.IsTrue(menuCategory.IsLive);
        }

        [TestMethod]
        public void EnableDisableMenuItem()
        {
            Assert.IsTrue(this.Post("venue/menuitem/1/enable/true",
                                    userName: Venue1iPad1Udid,
                                    password: "1/1234").StatusCode == HttpStatusCode.Unauthorized);

            Assert.AreEqual(1032,
                            this.Post("venue/menuitem/1/enable/true",
                                      userName: "venue2Admin",
                                      password: "password").ToEntity<OperationResult>().ErrorCode);
            var menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            var menuItemId = (from category in menus.Skip(1).First().MenuCategories
                              from item in category.MenuItems
                              select item).First().Id; 

            var result = this.Post("venue/menuitem/" + menuItemId + "/enable/false",
                                   userName: "venue1Admin",
                                   password: "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            menus = this.Get("venue/1/menus").ToEntity<List<Menu>>(); 
            // get the second menu's first menu item
            var menuItem = (from menu in menus
                            from category in menu.MenuCategories
                            from item in category.MenuItems
                            where item.Id == menuItemId
                            select item).First(); 

            Assert.IsFalse(menuItem.MenuItemStatus);

            // device admin in this test is setup to access two menus from the second menu
            var inaccessibleItemId = menus.Last().MenuCategories.First().MenuItems.First().Id;
            result = this.Post("venue/menuitem/" + inaccessibleItemId + "/enable/false",
                               userName: "venue1DeviceAdmin@airservice.com",
                               password: "password").ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1033, result.ErrorCode);

            // re-enable it by the device admin 

            result = this.Post("venue/menuitem/" + menuItemId + "/enable/true",
                               userName: "venue1DeviceAdmin@airservice.com",
                               password: "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
                      
            menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            menuItem = (from menu in menus
                        from category in menu.MenuCategories
                        from item in category.MenuItems
                        where item.Id == 1
                        select item).First();
            Assert.IsTrue(menuItem.MenuItemStatus);
        }

        [TestMethod]
        public void EnableDisableMenuItemPrinting()
        {
            var result = this.Post("venue/device/updatecredential",
                               userName: Venue1iPad1Udid,
                               password: "1/0000").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            Assert.IsTrue(this.Post("venue/menuitem/1/printing/true",
                                    userName: Venue1iPad1Udid,
                                    password: "1/1234", 
                                    deviceId: Venue1iPad1Udid).StatusCode == HttpStatusCode.Unauthorized);

            Assert.AreEqual(1033,
                            this.Post("venue/menuitem/1/printing/true",
                                      userName: "venue2Admin",
                                      password: "password",
                                      deviceId: Venue1iPad1Udid).ToEntity<OperationResult>().ErrorCode);
            
            var menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            var menuItemId = (from category in menus.Skip(1).First().MenuCategories
                              from item in category.MenuItems
                              select item).First().Id;

            result = this.Post("venue/menuitem/" + menuItemId + "/printing/true",
                               userName: "venue1Admin",
                               password: "password",
                               deviceId: Venue1iPad1Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            menus = this.Get("venue/1/menus",
                             deviceId: Venue1iPad1Udid).ToEntity<List<Menu>>();
            // get the second menu's first menu item
            var menuItem = (from menu in menus
                            from category in menu.MenuCategories
                            from item in category.MenuItems
                            where item.Id == menuItemId
                            select item).First();

            Assert.IsTrue(menuItem.Print);

            // re-enable it by the device admin 
            result = this.Post("venue/menuitem/" + menuItemId + "/printing/false",
                               userName: "venue1DeviceAdmin@airservice.com",
                               password: "password",
                               deviceId: Venue1iPad1Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError, result.Message);

            menus = this.Get("venue/1/menus", deviceId: Venue1iPad1Udid).ToEntity<List<Menu>>();
            menuItem = (from menu in menus
                        from category in menu.MenuCategories
                        from item in category.MenuItems
                        where item.Id == menuItemId
                        select item).First();
            Assert.IsFalse(menuItem.Print);
        }

        [TestMethod]
        public void Credential_Validations()
        {
            Assert.AreEqual(HttpStatusCode.Unauthorized,
                            this.Post("venue/device/validatecredential",
                                      userName: Venue1iPad1Udid,
                                      password: "4/158359").StatusCode);

            Assert.AreEqual(HttpStatusCode.Unauthorized,
                            this.Post("venue/deviceadmin/validatecredential").StatusCode);
            var result = this.Post("venue/deviceadmin/validatecredential",
                                   userName: "venue1DeviceAdmin@airservice.com",
                                   password: "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            Assert.AreEqual(HttpStatusCode.Unauthorized,
                            this.Post("venue/device/validatecredential").StatusCode);
            result = this.Post("venue/device/validatecredential",
                               userName: Venue1iPad2Udid,
                               password: "1/5678").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        public void Reject_Customer_Connection()
        {
            var customer = this.InsertCustomerForTest("4BCAC8A1-F587-4423-91FA-C47F71F3B739");
            string url = "Venue/1/Connect";
            var result = this.Post(url,
                                   null,
                                   customer.Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(1013,
                            result.ErrorCode);

            var connection = this.Get("venue/connection/" + customer.Id,
                                      Venue1iPad1Udid,
                                      "1/1234").ToEntity<VenueConnection>();

            result = this.Post("venue/connection/reject/" + connection.CustomerId,
                               userName: Venue1iPad1Udid,
                               password: "1/1234").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            result = this.Get("venue/connection/" + customer.Id,
                              Venue1iPad1Udid,
                              "1/1234").ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(1026, result.ErrorCode);
        }

        [TestMethod]
        public void Update_Customer_Area()
        {
            var customer = this.InsertCustomerForTest("0267AD43-E6C4-4D87-8F91-03185A9981E8");
            this.ConnectCustomerTo(customer,
                                   1);
            var venueAreas = this.Get("venue/VenueAreas",
                                      Venue1iPad1Udid,
                                      "1/1234").ToEntity<List<VenueArea>>();

            VenueArea venueArea = venueAreas.First();
            var result = this.Post("venue/customer/" + customer.Id + "/locatedAt/" + venueArea.Id,
                                   userName: Venue1iPad1Udid,
                                   password: "1/1234").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            string jsonString;
            var connection = this.Get("venue/connection/" + customer.Id,
                                      Venue1iPad1Udid,
                                      "1/1234").ToEntity<VenueConnection>(out jsonString);
            Assert.AreEqual(customer.Id, connection.CustomerId);
            Assert.IsTrue(jsonString.Contains("\"customerLocation\":\"" + venueArea.Title + "\""));
        }
         
        [TestMethod]
        public void Retrieve_Customer_Orders_With_Profile()
        {
            var customer = this.InsertCustomerForTest("81DB1F35-0C58-414E-BAC4-E793D582D1D0");
            this.ConnectCustomerTo(customer,
                                   1);
            var orders = this.MakeSuccessfulOrders(1,
                                                   customer,
                                                   2).ToList();
            string jsonString;
            var customerOrders = this.Get("venue/customer/" + customer.Id + "/withOrders",
                                          Venue1iPad1Udid,
                                          "1/1234").ToEntity<CustomerOrders>(out jsonString);
            Debug.WriteLine(jsonString);
            Assert.AreEqual(customer.Id,
                            customerOrders.Customer.Id);
            var orders2 = customerOrders.Orders;
            var set1 = orders.OrderBy(o => o.Id).Select(o => o.Id);
            var set2 = orders2.OrderBy(o => o.Id).Select(o => o.Id);
            Assert.IsTrue(set1.SequenceEqual(set2));
        }

        [TestMethod]
        public void RequestPassword()
        {
            var result =
                this.Post("venue/deviceAdmin/" +
                          HttpUtility.UrlEncode("venue1DeviceAdmin@airservice.com") + "/forgotpassword").ToEntity
                    <OperationResult>();
            Assert.IsTrue(!result.IsError || result.ErrorCode == 1044);
        }
         
        [TestMethod]
        public void Retrieve_and_Update_Device_Options()
        {
            var result = this.Post("venue/device/updateOptions",
                                   @"
{
    ""pin"": ""5678"", 
    ""isDeliveryEnabled"": false,
    ""isPickupEnabled"": false
}",
                                   "venue1DeviceAdmin@airservice.com",
                                   "password").ToEntity<OperationResult>();
            Assert.AreEqual(1041,
                            result.ErrorCode);

            result = this.Post("venue/device/updateOptions",
                               @"
{
    ""pin"": ""5678"", 
    ""isDeliveryEnabled"": true,
    ""isPickupEnabled"": false
}",
                               "venue1DeviceAdmin@airservice.com",
                               "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            var option = this.Get("venue/device/options",
                                   Venue1iPad2Udid,
                                   "1/5678").ToEntity<DeviceOptions>();
            Assert.IsTrue(option.IsDeliveryEnabled);
            Assert.IsFalse(option.IsPickupEnabled);

            result = this.Post("venue/device/updateOptions",
                               @"
{
    ""pin"": ""5678"", 
    ""isDeliveryEnabled"": false,
    ""isPickupEnabled"": true
}",
                              "venue1Admin",
                              "password").ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            option = this.Get("venue/device/options",
                                  Venue1iPad2Udid,
                                   "1/5678").ToEntity<DeviceOptions>();
            Assert.IsFalse(option.IsDeliveryEnabled);
            Assert.IsTrue(option.IsPickupEnabled);
        }

        [TestMethod]
        public void RegisterForNotificationService()
        {
            var result = this.Post("/venue/registerForNotification",
                                   "{ \"udid\": \"941FDF8E-2823-4AA5-A9B3-FA6EE2990D4A\", \"token\": \"11DFF2CC-BD35-4244-AB9C-0119DB6D64EA\"}")
                .ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        public void BroadcastMessageToActiveCustomers()
        { 
            Customer customer = this.InsertCustomerForTest("584B8DD5-F387-44FA-8D6F-1935A8AACE29",
                                                           receiveSpecialOffers: true);
            Assert.IsFalse(this.Post("/customer/registerForNotification",
                                     "{ \"udid\": \"" + customer.Udid +
                                     "\", \"token\": \"11DFF2CC-BD35-4244-AB9C-0119DB6D64EA\"}")
                               .ToEntity<OperationResult>().IsError);
            this.ConnectCustomerTo(customer,
                                   1);
            Assert.AreEqual(HttpStatusCode.Unauthorized,
                            this.Post("venue/broadcastMessage",
                                      "{\"message\": \"Hello there. How are you?\"}",
                                      Venue1iPad2Udid,
                                      "1/5678").StatusCode);

            var result = this.Post("venue/broadcastMessage",
                                   "{\"message\": \"Hello there. How are you?\"}",
                                   "venue1DeviceAdmin@airservice.com",
                                   "password").ToEntity<OperationResult>();
            Assert.IsTrue(!result.IsError);

            result = this.Post("venue/broadcastMessage",
                               "{\"message\": \"Hello there. How are you?\"}",
                               "venue1Admin",
                               "password").ToEntity<OperationResult>();
            Assert.IsTrue(!result.IsError);

            result = this.Post("venue/broadcastMessage",
                               "{\"message\": \"Hello there. How are you?\"}",
                               "venue1Admin",
                               "password").ToEntity<OperationResult>();
            Assert.IsTrue(result.IsError);
            // currently 2 messages per an hour. See web.config
            // Resources.Err1046BroadcastMessageLimitArg0PerArg1Reached
            Assert.AreEqual(1046, result.ErrorCode);
        }

        /// <summary>
        /// This is just a simple test to ensure all DI setup was corrected registered.
        /// </summary>
        [TestMethod]
        public void MobileSettings()
        {
            var settings = this.Get("venue/1/settings").ToEntity<MobileApplicationSettings>();
            Assert.IsNotNull(settings);
        }

        [TestMethod]
        public void OrderDocketPrintingTest()
        {
            var menus = this.Get("venue/1/menus").ToEntity<List<Menu>>();
            var customer = this.InsertCustomerForTest("1784E604-B98E-448C-96B6-890C8954B44F");
            this.ConnectCustomerTo(customer,
                                   1);
            var order = this.MakeSuccessfulOrders(1,
                                                  customer).First();

            var orderItem = order.OrderItems.First();
            var targetMenu = (from menu in menus
                              from category in menu.MenuCategories
                              from item in category.MenuItems
                              where item.Id == orderItem.MenuItemId
                              select item).First(); 
            var result = this.Post("venue/menu/" + targetMenu.Id + "/printing/false",
                                   userName: "venue1Admin",
                                   password: "password",
                                   deviceId: Venue1iPad1Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
            result = this.Post("venue/menuItem/" + orderItem.MenuItemId + "/printing/false",
                               userName: "venue1Admin",
                               password: "password",
                               deviceId: Venue1iPad1Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
            var finalResult = this.Get("venue/docket/" + orderItem.Id,
                                       Venue1iPad1Udid,
                                       "1/1234",
                                       secondsFromGmt:18000).ToEntity<OperationResult<string>>();
            Assert.IsFalse(finalResult.IsError);
            Assert.IsNull(finalResult.Data);

            result = this.Post("venue/menuItem/" + orderItem.MenuItemId + "/printing/true",
                               userName: "venue1Admin",
                               password: "password",
                               deviceId: Venue1iPad1Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
            
            //still nothing returns as the menu isn't enabled for printing
            finalResult = this.Get("venue/docket/" + orderItem.Id,
                                   Venue1iPad1Udid,
                                   "1/1234",
                                   secondsFromGmt:18000).ToEntity<OperationResult<string>>();
            Assert.IsFalse(finalResult.IsError);
            Assert.IsNull(finalResult.Data);

            // now enable menu printing 
            result = this.Post("venue/menu/" + targetMenu.Id + "/printing/true",
                               userName: "venue1Admin",
                               password: "password",
                               deviceId: Venue1iPad1Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);

            finalResult = this.Get("venue/docket/" + orderItem.Id,
                                   Venue1iPad1Udid,
                                   "1/1234",
                                   secondsFromGmt: 18000).ToEntity<OperationResult<string>>();
            Assert.IsFalse(finalResult.IsError);
            Assert.IsNotNull(finalResult.Data); 
            Debug.WriteLine(finalResult.Data);

            // whether or not item is processed/confirmed they can be printed. 
            result = this.Post("venue/orderitem/" + orderItem.Id + "/confirm",
                               userName: Venue1iPad1Udid,
                               password: "1/1234").ToEntity<OperationResult>();
            Assert.AreEqual(0,
                            result.ErrorCode);

            finalResult = this.Get("venue/docket/" + orderItem.Id,
                                       Venue1iPad1Udid,
                                       "1/1234",
                                       secondsFromGmt: 18000).ToEntity<OperationResult<string>>();
            Assert.IsFalse(finalResult.IsError);
            Assert.IsNotNull(finalResult.Data);
            Debug.WriteLine(finalResult.Data);

            result = this.Post("venue/menuItem/" + orderItem.MenuItemId + "/printing/false",
                               userName: "venue1Admin",
                               password: "password",
                               deviceId: Venue1iPad1Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
            
            // menu is enabled for printing but not menu item
            finalResult = this.Get("venue/docket/" + orderItem.Id,
                                       Venue1iPad1Udid,
                                       password: "1/1234",
                                       secondsFromGmt: 18000).ToEntity<OperationResult<string>>();
            Assert.IsFalse(finalResult.IsError);
            Assert.IsNull(finalResult.Data);

            result = this.Post("venue/menu/" + targetMenu.Id + "/printing/false",
                               userName: "venue1Admin",
                               password: "password",
                               deviceId: Venue1iPad1Udid).ToEntity<OperationResult>();
            Assert.IsFalse(result.IsError);
        } 
    }
}