using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.ServiceModel.Channels;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using JsonRpc.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace JsonRpc.ServiceModel.Dispatcher
{
    class JsonRpcErrorHandler : IErrorHandler
    {
        public bool HandleError(Exception error)
        {
            return false;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            bool includeDetails = IncludeExceptionDetails();

            // TODO: check error type and set appropriate error code

            object msgId = null;
            if (OperationContext.Current.IncomingMessageProperties.ContainsKey(DispatcherUtils.MessageIdKey))
                msgId = OperationContext.Current.IncomingMessageProperties[DispatcherUtils.MessageIdKey];

            // TODO: extract exception details from FaultException
            object additionalData;
            var faultException = error as FaultException;
            if (faultException != null && faultException.GetType().IsGenericType) {
                additionalData = faultException.GetType().GetProperty("Detail").GetValue(faultException, null);
            } else {
                additionalData = error;
            }

            var exception = new JsonRpcException(123, error.Message, additionalData);
            var errMessage = new JsonRpcResponse<object>()
            {
                Error = exception,
                Result = null,
                Id = msgId
            };

            byte[] rawBody = DispatcherUtils.SerializeBody(errMessage, Encoding.UTF8);
            Message msg = DispatcherUtils.CreateMessage(version, "", rawBody, Encoding.UTF8);

            var property = (HttpResponseMessageProperty)msg.Properties[HttpResponseMessageProperty.Name];
            property.StatusCode = HttpStatusCode.InternalServerError;
            property.StatusDescription = "Internal Server Error";

            fault = msg;
        }

        private bool IncludeExceptionDetails()
        {
            var behaviors = OperationContext.Current.Host.Description.Behaviors;
            var debugBehavior = behaviors.Find<ServiceDebugBehavior>();
            var serviceBehavior = behaviors.Find<ServiceBehaviorAttribute>();

            return (debugBehavior != null && debugBehavior.IncludeExceptionDetailInFaults) ||
                (serviceBehavior != null && serviceBehavior.IncludeExceptionDetailInFaults);
        }
    }
}
