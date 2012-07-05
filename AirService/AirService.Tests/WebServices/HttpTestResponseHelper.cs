using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AirService.Tests.WebServices
{
    public static class HttpTestResponseHelper
    {
        public static T ToEntity<T>(this HttpWebResponse response) where T : class
        {
            string jsonString;
            return ToEntity<T>(response,
                               out jsonString);
        }

        public static bool ToBoolean(this HttpWebResponse response)
        {
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return bool.Parse(reader.ReadToEnd());
            }
        }

        public static T ToEntity<T>(this HttpWebResponse response, out string jsonString) where T : class
        {
            jsonString = null;
            try
            {
                Stream responseStream = response.GetResponseStream();
                Assert.IsNotNull(responseStream);
                if (responseStream.CanSeek && responseStream.Position > 0)
                {
                    responseStream.Seek(0,
                                        SeekOrigin.Begin);
                }

                using (var reader = new StreamReader(responseStream))
                {
                    jsonString = reader.ReadToEnd();
                    if (string.IsNullOrEmpty(jsonString))
                    {
                        return null;
                    }

                    var serializer = new DataContractJsonSerializer(typeof(T));
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
                    {
                        return (T)serializer.ReadObject(stream);
                    }
                }
            }
            catch
            {
                if (jsonString!= null)
                {
                    Debug.WriteLine(jsonString);
                }

                throw;
            }
            finally
            {
                response.Close();
            }
        }
    }
}