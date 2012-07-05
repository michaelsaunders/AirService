using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Services.Notifications
{
    ///<summary>
    ///  This class is to ensure we keep minimal number of concurrent connection to APN
    ///  as to avoid too many connection requests that can be seen as denial of service attack (See Apple's APN Doc)
    ///</summary>
    public class ApnConnection : IApnConnection
    {  
        private static readonly int NotificationExpiryTimeInSeconds;
        private readonly Queue<NotificationData> _dataQueue;
        private readonly bool _messageToCustomer;
        private volatile bool _busy; 
        private static readonly DateTime UnixEpochTime;

        static ApnConnection()
        {
            UnixEpochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);  
            int numberValue;
            if (!int.TryParse(ConfigurationManager.AppSettings["APNExpiryTimeInSeconds"],
                              out numberValue))
            {
                numberValue = 3600; // default to an hour
            }

            NotificationExpiryTimeInSeconds = numberValue;
        }

        private ApnConnection(bool messageToCustomer)
        {
            this._messageToCustomer = messageToCustomer;
            this._dataQueue = new Queue<NotificationData>();
        }

        internal static ApnConnection GetApnClient(bool messageToCustomer, IAirServiceContext dataContext)
        {
            return new ApnConnection(messageToCustomer);
        }

        public void Add(NotificationToken token, byte[] payload)
        {
            this._dataQueue.Enqueue(new NotificationData(token,
                                                         payload));
        }  

        /// <summary>
        ///   Send all notifications. Do not use this object once notifications are sent.
        /// </summary>
        public void SendAsync()
        {
            // Precondition.
            if (this._busy)
            {
                throw new InvalidOperationException("Send operation already started.");
            }

            this._busy = true;
            ThreadPool.QueueUserWorkItem(this.SendQueuedNotifications);
        }

        /// <summary>
        /// Blocking Send
        /// </summary>
        public void Send()
        {
            this._busy = true; 
            this.SendQueuedNotifications(null);
        }

        private void SendQueuedNotifications(object state)
        {
            SslStream sslStream = null;
            while (this._dataQueue.Count > 0)
            {
                NotificationData data = this._dataQueue.Dequeue();
                bool messageSent = false;
                int attempt = 0;
                while (!messageSent)
                {
                    try
                    {
                        attempt++;
                        sslStream = this.EnsureStreamIsReadyForWrite();
                        if (sslStream == null)
                        {
                            // failed to connect to APN server
                            // try agian after quick 1 second sleep
                            if (attempt > 2)
                            {
                                Logger.Trace("Given up trying to establish secure connection to APN server..");
                                return;
                            }

                            // failed to connect to APN server
                            // try agian after quick 1 second sleep
                            Thread.Sleep(1000);
                            continue;
                        }

                        // Following piece of code originated from below URL, but modified for enhanced notification format
                        // to support error response (simple format won't have any response at all)
                        // http://stackoverflow.com/questions/1020762/does-anyone-know-how-to-write-an-apple-push-notification-provider-in-c   
                        // Enhanced notification format
                        //    Command Identifier Expiry Token Length(Big endian) Device Token Payload length(big endian) payload
                        // Bytes: 1     4          4     32          2                32          2                      max length = 256 - payload length
                        var memoryStream = new MemoryStream();
                        var writer = new BinaryWriter(memoryStream);
                        writer.Write((byte) 1); //The command 1 indicates enhanced format. 
                        writer.Write(new byte[] {0, 0, 0, 0}); //identifier 

                        // utc expiry date time in unix epoch date time  - big endian
                        DateTime currentTime = (DateTime.UtcNow.AddSeconds(NotificationExpiryTimeInSeconds)).ToUniversalTime();
                        var expiry = (int)(currentTime - UnixEpochTime).TotalSeconds;
                        writer.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(expiry))); 

                        writer.Write((byte) 0); //The first byte of the deviceId length (big-endian first byte)
                        writer.Write((byte) 32); //The deviceId length (big-endian second byte)

                        writer.Write(data.Token.GetDeviceTokenBytes());
                        byte[] payloadBytes = data.Payload;
                        writer.Write((byte) 0); //First byte of payload length; (big-endian first byte)
                        writer.Write((byte) payloadBytes.Length); //payload length (big-endian second byte)
                        writer.Write(payloadBytes);
                        writer.Flush();

                        byte[] array = memoryStream.ToArray();
                        sslStream.Write(array);
                        sslStream.Flush();
                        messageSent = true;
                        data.Processed = true;

                        try
                        {
                            // try to get response.
                            // first byte is for command number 8
                            // second byte is status code
                            // last 4 bytes for custom identifier.
                            var response = new byte[6];
                            Logger.Trace(string.Format("Sending Notification to {0}", data.Token.Udid));
                            var read = sslStream.Read(response,
                                                   0,
                                                   6);
                            Logger.Trace(string.Format("APN response code {0}, {1}",
                                                       response[0],
                                                       response[1]));
                            if (read == 6)
                            {
                                /*
                                No errors encountered
                                1 Processing error - nothing can do
                                2 Missing device token - shouldn't happen as we will never call without device token
                                3 Missing topic - not relevant 
                                4 Missing payload - not relevant
                                5 Invalid token size - not relevant
                                6 Invalid topic size - not relevant
                                7 Invalid payload size - not relevant
                                8 Invalid token - this device may be uninstalled or not available anymore
                                255 None (unknown) not relevant
                                */

                                // response command must be 8 and ignore all status except 8
                                if (response[0] == 8 && response[1] == 8)
                                {
                                    Logger.Trace("8 Invalid token - this device may be uninstalled or not available anymore");
                                }

                                Logger.Trace("Response code " + response[0] + ", " + response[1]);
                            }
                        }
                        catch(Exception e)
                        {
                            Logger.Log("No Panic. Exception expected due to APN design..", e);
                            // apple decided to keep quiet if succeeded....
                            // unless we decided to wait forever, we will never know if really 
                            // succeeded or not..
                        }
                    }
                    catch(Exception e)
                    {
                        if (attempt > 2)
                        {
                            Logger.Log("Gave up sending notification after 3rd attempts..", e); 
                            // APN never should throw error unless we are experiencing a connection problem.
                            // after 3rd attempt skip this payload.  
                            // can't do anymore. Server may be down APN never meant to reliable.
                            break;
                        }

                        Logger.Log("Cannot send message to APN. Attempting again", e); 
                    }
                }
            }

            if (sslStream != null)
            {
                sslStream.Close();
            }
        }

        private SslStream EnsureStreamIsReadyForWrite()
        {
            string certificatePathKey = this._messageToCustomer ? "iPhoneAPNSSLCertPath" : "iPadAPNSSLCertPath";
            string passwordKey = this._messageToCustomer ? "iPhoneAPNSSLCertPassword" : "iPadAPNSSLCertPathPassword";
            string certificatePath = ConfigurationManager.AppSettings[certificatePathKey];
            string password = ConfigurationManager.AppSettings[passwordKey];
            X509Certificate2 clientCertificate;
            using (var stream = File.OpenRead(certificatePath))
            {
                using (var reader = new BinaryReader(stream))
                {
                    clientCertificate = new X509Certificate2(reader.ReadBytes((int) stream.Length),
                                                             password);
                }
            }

            var certificatesCollection = new X509Certificate2Collection(clientCertificate);
            int port = Int32.Parse(ConfigurationManager.AppSettings["APNServicePort"]);
            string hostname = ConfigurationManager.AppSettings["APNServiceHost"];
            var tcpClient = new TcpClient(hostname,
                                          port);
            var sslStream = new SslStream(tcpClient.GetStream(),
                                          false,
                                          ValidateServerCertificate,
                                          null);

            try
            {
                sslStream.AuthenticateAsClient(hostname,
                                               certificatesCollection,
                                               SslProtocols.Default,
                                               false);
                // if success there won't be any response at all
                // since we can't wait forever, if response is not immidately available
                // we take it as success. whether it is async or syn read doesn't matter. 
                sslStream.ReadTimeout = 500;
                return sslStream;
            }
            catch (AuthenticationException)
            {
                tcpClient.Close();
                return null;
            }
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
                                                      SslPolicyErrors sslpolicyerrors)
        {
            // just return true as we don't care if Apple's certificate is expired. 
            return true;
        }
    }
}