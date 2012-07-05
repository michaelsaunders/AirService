using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AirService.Services
{
    [Serializable]
    public class ServiceFaultException : Exception
    {
        private readonly List<OperationResult> _operationResults;

        public ServiceFaultException(int errorNumber, string message, List<OperationResult> operationResults = null)
            : base(message)
        {
            this._operationResults = operationResults;
            this.ErrorNumber = errorNumber;
        }

        protected ServiceFaultException(SerializationInfo info, StreamingContext context)
            : base(info,
                   context)
        {
        }

        public List<OperationResult> OperationResults
        {
            get
            {
                return this._operationResults;
            }
        }

        public int ErrorNumber
        {
            get;
            set;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("errorNumber",
                          this.ErrorNumber);
            base.GetObjectData(info,
                               context);
        }
    }
}