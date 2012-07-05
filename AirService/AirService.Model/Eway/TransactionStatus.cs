using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace AirService.Model.Eway
{
    /// <remarks/>
    [GeneratedCode("svcutil", "3.0.4506.2152")]
    [Serializable()]
    [XmlType(Namespace = "http://www.eway.com.au/gateway/rebill/manageRebill")]
    public enum TransactionStatus
    {
        /// <remarks/>
        Future,

        /// <remarks/>
        Successful,

        /// <remarks/>
        Failed,

        /// <remarks/>
        Pending,
    }
}