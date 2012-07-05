using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

/*
 * To avoid type details serialized as __type (which will use up apn body length)
 * Each type is specifically created. 
 * No polymorphism in custom APN properties.
 */

namespace AirService.Services.Notifications
{
    [DataContract]
    public class SimpleApnPayload : ApnPayloadBase
    {
        [DataMember(Name = "data")]
        public DataDictionary Data
        {
            get;
            set;
        }
    }

    [DataContract]
    public class OrderApnPayload : ApnPayloadBase
    {
        [DataMember(Name = "data")]
        public OrderDictionary Data
        {
            get;
            set;
        }
    }

    [DataContract]
    public class OrderItemApnPayload : ApnPayloadBase
    {
        [DataMember(Name = "data")]
        public OrderItemDictionary Data
        {
            get;
            set;
        }
    }

    [DataContract]
    public class ApnPayloadBase
    {
        private byte[] _bytes;

        public ApnPayloadBase()
        {
            this.Aps = new ApsDictionary();
        }

        [DataMember(Name = "aps")]
        private ApsDictionary Aps
        {
            get;
            set;
        }

        public string Message
        {
            get
            {
                return this.Aps.Alert;
            }
            set
            {
                this.Aps.Alert = value;
            }
        }

        public byte[] ToBytes()
        {
            if (this._bytes == null)
            {
                var serializer = new DataContractJsonSerializer(this.GetType());
                MemoryStream stream = new MemoryStream();
                serializer.WriteObject(stream,
                                       this);
                this._bytes = stream.ToArray();
            }

            return this._bytes;
        }

        public string Sound { get; set; }

        public void EnableDefaultSound()
        {
            this.Aps.Sound = "default"; 
        }

        #region Nested type: ApsDictionary

        [DataContract]
        private class ApsDictionary
        {
            private string _alert;

            [DataMember(Name = "alert")]
            public string Alert
            {
                get
                {
                    return this._alert;
                }
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        this._alert = "";
                    }
                    else
                    {
                        if (value.Length > 128)
                        {
                            this._alert = value.Substring(0,
                                                          128);
                        }
                        else
                        {
                            this._alert = value;
                        }
                    }
                }
            }

            [DataMember(Name = "badge")]
            public int Badge
            {
                get;
                set;
            }

            [DataMember(Name = "sound", EmitDefaultValue = false)]
            public string Sound { get; set; }
        }

        #endregion
    }

    #region Nested type: DataDictionary

    [DataContract]
    public class DataDictionary
    {
        [DataMember(Name = "code")]
        public int Code
        {
            get;
            set;
        }

        [DataMember(Name = "sender")]
        public int Sender
        {
            get;
            set;
        }
    }

    #endregion

    #region Nested type: OrderItemDictionary

    [DataContract]
    public class OrderItemDictionary : DataDictionary

    {
        [DataMember(Name = "orderId")]
        public int OrderId
        {
            get;
            set;
        }

        [DataMember(Name = "orderItemId")]
        public int OrderItemId
        {
            get;
            set;
        }

        [DataMember(Name = "oldStatus")]
        public int OldStatus
        {
            get;
            set;
        }

        [DataMember(Name = "newStatus")]
        public int NewStatus
        {
            get;
            set;
        }
    }

    [DataContract]
    public class OrderDictionary : DataDictionary
    {
        [DataMember(Name = "orderId")]
        public int OrderId
        {
            get;
            set;
        }

        [DataMember(Name = "oldStatus")]
        public int OldStatus
        {
            get;
            set;
        }

        [DataMember(Name = "newStatus")]
        public int NewStatus
        {
            get;
            set;
        }
    }

    #endregion
}