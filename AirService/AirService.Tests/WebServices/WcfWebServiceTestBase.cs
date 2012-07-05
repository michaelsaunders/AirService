using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Sockets;
using System.Text;
using AirService.Model;
using AirService.Services;
using CassiniDev;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AirService.Tests.WebServices
{
    /// <summary>
    ///   http://cassinidev.codeplex.com/wikipage?title=Using%20CassiniDev%20with%20unit%20testing%20frameworks%20and%20continuous%20integration&referringTitle=Documentation
    /// </summary>
    public class WcfWebServiceTestBase
    {
        public const string Venue1iPad1Udid = "A09743D6-5A32-424A-A661-EF1081C9FACE0000";
        public const string Venue1iPad4Udid = "DCA19909-E07C-4B45-A68F-7D1195F35F1C0000";
        public const string Venue1iPad2Udid = "B9CCE69B-858D-459D-843C-BC36BAEC260E0000";
        public const string Venue2iPad2Udid = "669CC57E-C698-4499-9C49-BFB16BC8BDD90000";

        
        private static int _numTestClasses;
        private static CassiniDevServer _devServer;
        private static readonly object SyncLock = new object();
        private static Configuration _config;

        public static Configuration Config
        {
            get
            {
                return _config;
            }
        }

        public static void InitTestServer()
        {
            lock (SyncLock)
            {
                if (_numTestClasses++ == 0)
                {
                    DirectoryInfo parentDirectory = Directory.GetParent(Directory.GetCurrentDirectory());
                    parentDirectory = Directory.GetParent(parentDirectory.FullName);
                    DirectoryInfo solutionDirectory = Directory.GetParent(parentDirectory.FullName);
                    string destFolder = Path.Combine(solutionDirectory.FullName,
                                                     "AirService.Tests\\WebServices\\TestApp\\Bin");
                    if (!Directory.Exists(destFolder))
                    {
                        Directory.CreateDirectory(destFolder);
                    }

                    foreach (string file in Directory.GetFiles(Path.Combine(solutionDirectory.FullName,
                                                                            "AirService.WebServices\\Bin")))
                    {
                        string destFile = Path.Combine(destFolder,
                                                       Path.GetFileName(file));
                        File.Copy(file,
                                  destFile,
                                  true);
                    }

                    File.Copy(Path.Combine(solutionDirectory.FullName,
                                           "AirService.Tests\\Bin\\Debug\\Moq.dll"),
                              Path.Combine(destFolder,
                                           "Moq.dll"),
                              true);

                    _devServer = new CassiniDevServer();
                    //_devServer.StartServer(@"..\..\..\AirService.Tests\WebServices\TestApp");
                    IPAddress address =
                        Dns.GetHostAddresses(Environment.MachineName).First(
                            ip => ip.AddressFamily == AddressFamily.InterNetwork);
                    _devServer.StartServer(@"..\..\..\AirService.Tests\WebServices\TestApp",
                                           address,
                                           new Random().Next(7000,
                                                             8000),
                                           "/",
                                           Environment.MachineName);
                    var fileMap = new ExeConfigurationFileMap();
                    fileMap.ExeConfigFilename = @"..\..\..\AirService.Tests\WebServices\TestApp\web.config";
                    _config = ConfigurationManager.OpenMappedExeConfiguration(fileMap,
                                                                              ConfigurationUserLevel.None);
                }
            }
        }

        public static void EndTestServer()
        {
            lock (SyncLock)
            {
                if (--_numTestClasses == 0)
                {
                    _devServer.StopServer();
                }
            }
        }

        protected HttpWebResponse Post(string uri,
                                       string requestBody = null,
                                       string userName = null,
                                       string password = null,
                                       int? secondsFromGmt = null,
                                       string deviceId = null)
        {
            return this.InvokeService(uri,
                                      "POST",
                                      requestBody,
                                      userName,
                                      password,
                                      secondsFromGmt, 
                                      deviceId);
        }

        private HttpWebResponse InvokeService(string uri,
                                              string method,
                                              string requestBody,
                                              string userName = null,
                                              string password = null,
                                              int? secondsFromGmt = null,
                                              string deviceId = null)
        {
            ServicePointManager.Expect100Continue = false;
            var request = (HttpWebRequest) WebRequest.Create(_devServer.NormalizeUrl(uri));
            request.Method = method.ToUpper();
            request.UserAgent = "Unit Test";
            if (userName != null)
            {
                //request.Credentials = new NetworkCredential(userName, password);
                string credential = String.Format("{0}:{1}",
                                                  userName,
                                                  password);

                byte[] bytes = Encoding.UTF8.GetBytes(credential);
                string base64 = Convert.ToBase64String(bytes);
                request.Headers.Add("Authorization",
                                    "Basic " + base64);
            }

            if (secondsFromGmt.HasValue)
            {
                request.Headers.Add("secondsFromGMT",
                                    secondsFromGmt.ToString());
            }

            if (deviceId != null)
            {
                request.Headers.Add("Device-Id",
                                    deviceId);
            }

            if (method == "POST")
            {
                request.ContentType = "application/json; charset=utf-8";
                if (!string.IsNullOrWhiteSpace(requestBody))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(requestBody);
                    request.ContentLength = bytes.Length;
                    Stream stream = request.GetRequestStream();
                    stream.Write(bytes,
                                 0,
                                 bytes.Length);
                    stream.Close();
                }
            }

            try
            {
                return (HttpWebResponse) request.GetResponse();
            }
            catch (WebException webException)
            {
                try
                {
                    Stream stream = webException.Response.GetResponseStream();
                    if (stream != null)
                    {
                        Debug.Write(new StreamReader(stream).ReadToEnd());
                    }
                }
                catch
                {
                }

                return (HttpWebResponse) webException.Response;
            }
        }

        protected HttpWebResponse PostStream(string uri, Stream inputStream, string userName = null,
                                             string password = null,
                                             string baseUri = null)
        {
            ServicePointManager.Expect100Continue = false;
            var serviceMethodUri = baseUri == null
                                       ? _devServer.NormalizeUrl(uri)
                                       : baseUri + uri;
            var request = (HttpWebRequest) WebRequest.Create(serviceMethodUri);
            request.Method = "POST";
            request.UserAgent = "Unit Test";
            if (userName != null)
            {
                // NetworkCredential will only set the header if challenged. 
                // we need to set every time. 
                //request.Credentials = new NetworkCredential(userName,
                //                                            password);

                string credential = String.Format("{0}:{1}",
                                                  userName,
                                                  password);

                byte[] bytes = Encoding.UTF8.GetBytes(credential);
                string base64 = Convert.ToBase64String(bytes);
                request.Headers.Add("Authorization",
                                    "Basic " + base64);
            }

            request.ContentType = "application/octet-stream";
            Stream requestStream = request.GetRequestStream();
            inputStream.CopyTo(requestStream);
            requestStream.Close();

            //const string boundary = "boundary=---------------------------";
            //request.ContentType = "multipart/form-data; boundar=" + boundary;

            //var requestStream = request.GetRequestStream();
            //using(var writer = new StreamWriter(requestStream))
            //{
            //    writer.WriteLine(boundary);
            //    writer.WriteLine(string.Format("Content-Disposition: form-data; name=\"photo\" filename=\"{0}\"",
            //                                   fileName));
            //    writer.WriteLine();

            //    inputStream.CopyTo(requestStream);
            //    writer.WriteLine(boundary + "--");
            //});

            try
            {
                return (HttpWebResponse) request.GetResponse();
            }
            catch (WebException webException)
            {
                try
                {
                    Stream stream = webException.Response.GetResponseStream();
                    if (stream != null)
                    {
                        Debug.Write(new StreamReader(stream).ReadToEnd());
                    }
                }
                catch
                {
                }

                return (HttpWebResponse) webException.Response;
            }
        }

        protected HttpWebResponse Get(string url, string userName = null, string password = null, bool cache = false, int? secondsFromGmt = null, string deviceId = null)
        {
            var request = (HttpWebRequest) WebRequest.Create(_devServer.NormalizeUrl(url));
            request.Method = "GET";
            request.UserAgent = "Unit Test";
            if (userName != null)
            {
                // NetworkCredential will only set the header if challenged. 
                // we need to set every time. 
                //request.Credentials = new NetworkCredential(userName,
                //                                            password);

                string credential = String.Format("{0}:{1}",
                                                  userName,
                                                  password);

                byte[] bytes = Encoding.UTF8.GetBytes(credential);
                string base64 = Convert.ToBase64String(bytes);
                request.Headers.Add("Authorization",
                                    "Basic " + base64);
            }

            if (secondsFromGmt.HasValue)
            {
                request.Headers.Add("SecondsFromGMT",
                                    secondsFromGmt.ToString());
            }

            if (deviceId != null)
            {
                request.Headers.Add("Device-Id",
                                    deviceId);
            }

            if (cache)
            {
                request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
            }

            try
            {
                return (HttpWebResponse) request.GetResponse();
            }
            catch (WebException webException)
            {
                return (HttpWebResponse) webException.Response;
            }
        }

        public Customer InsertCustomerForTest(string udid, string firstName = "First Name", string lastName = "Last Name",
                                              double facebookId = 1234, bool receiveSpecialOffers = false, bool receiveEmailNotifications = false)
        {
            if (udid.Length < 40)
            {
                udid = udid.PadRight(40,
                                     '0');
            }

            const string requestBody =
                @"{{
    ""udid"": ""{0}"",
    ""firstName"": ""{1}"", 
    ""lastName"": ""{2}"",
    ""facebookId"": {3}, 
    ""receiveSpecialOffers"": {4}, 
    ""receiveEmailNotification"": {5}    
}}";
            string jsonString;
            return this.Post("customer",
                             string.Format(requestBody,
                                           udid,
                                           firstName,
                                           lastName,
                                           facebookId,
                                           receiveSpecialOffers ? "true" : "false",
                                           receiveEmailNotifications ? "true" : "false"))
                .ToEntity<Customer>(out jsonString);
        }

        protected OperationResult ConnectCustomerTo(Customer customer, int venueId, bool mustConnect = true)
        {
            var connectionResult = this.Post("venue/" + venueId + "/connect",
                                             userName: customer.Udid).ToEntity<OperationResult>();
            if (mustConnect)
            {
                Assert.IsFalse(connectionResult.IsError);
                connectionResult = this.Post("venue/connection/accept/" + customer.Id,
                                             userName: Venue1iPad1Udid,
                                             password: "1/1234").ToEntity<OperationResult>();

                Assert.IsFalse(connectionResult.IsError);
            }

            return connectionResult;
        }



        /// <summary>
        ///   Make an successful order two items from two different menus
        /// </summary>
        protected IEnumerable<Order> MakeSuccessfulOrders(int venueId, Customer customer, int howMany = 1)
        {
            var menus = this.Get(string.Format("venue/{0}/menus",
                                               venueId)).ToEntity<List<Menu>>();
            // we going to test with 2 menu items from an in-active menu and one valid menu item.
            var activeMenus = from menu in menus
                              where menu.MenuStatus
                              select menu;
            var activeMenuItems = (from category in activeMenus.First().MenuCategories
                                   from item in category.MenuItems
                                   where
                                       category.IsLive &&
                                       item.MenuItemStatus
                                   select item).Take(2);

            activeMenuItems = activeMenuItems.Concat(
                (from category in activeMenus.Skip(1).First().MenuCategories
                 from item in category.MenuItems
                 where
                     category.IsLive &&
                     item.MenuItemStatus
                 select item
                ).Take(2));

            Assert.AreEqual(4,
                            activeMenuItems.Count());
            var orderItemJsonString = this.CreateJsonOrderItemsFromMenuItems(activeMenuItems);

            Debug.WriteLine(orderItemJsonString);

            for (int i = 0; i < howMany; i++)
            {
                var result = this.Post(string.Format("venue/{0}/order",
                                                     venueId),
                                       orderItemJsonString,
                                       customer.Udid).ToEntity<Order>();
                Assert.IsNotNull(result);
                yield return result;
            }
        }

        protected string CreateJsonOrderItemsFromMenuItems(IEnumerable<MenuItem> menuItems)
        {
            return "[" + string.Join(",",
                                     (from menuItem in menuItems
                                      select
                                          string.Format(
                                              @"
{{
    ""menuItemId"": {0}, 
    ""optionId"": {1},
    ""quantity"": 1, 
    ""price"": {2}
}}
",
                                              menuItem.Id,
                                              (menuItem.Id%2 == 1
                                                   ? menuItem.MenuItemOptions.First().Id.ToString()
                                                   : "null"),
                                              menuItem.Id%2 == 1
                                                  ? menuItem.MenuItemOptions.First().Price
                                                  : menuItem.Price
                                          ))
                                         .ToArray()) + "]";
        }
    }
}