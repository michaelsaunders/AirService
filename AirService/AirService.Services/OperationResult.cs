using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AirService.Services
{
    [DataContract]
    public class OperationResult : SimpleMessage
    {
        [DataMember(Name = "id")]
        public int? Id
        {
            get;
            set;
        }

        [DataMember(Name = "errorCode")]
        public int ErrorCode
        {
            get;
            set;
        }

        [DataMember(Name = "error")]
        public bool IsError
        {
            get;
            set;
        }

        [DataMember(Name = "items")]
        public List<OperationResult> Items
        {
            get;
            set;
        }
    }

    [DataContract]
    public class SimpleMessage
    {
        [DataMember(Name = "message")]
        public string Message
        {
            get;
            set;
        }
    }
     
    [DataContract]
    public class OperationResult<T> : OperationResult
    {
        [DataMember(Name = "data")]
        public T Data { get; set; }
    }
}