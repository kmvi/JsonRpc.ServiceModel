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

            object msgId = null;
            if (OperationContext.Current.IncomingMessageProperties.ContainsKey(DispatcherUtils.MessageIdKey))
                msgId = OperationContext.Current.IncomingMessageProperties[DispatcherUtils.MessageIdKey];

            var jsonRpcError = error as JsonRpcException;
            if (jsonRpcError != null)
                fault = DispatcherUtils.CreateErrorMessage(version, msgId, jsonRpcError);
            else {
                // TODO: extract exception details from FaultException
                object additionalData;
                var faultException = error as FaultException;
                if (faultException != null && faultException.GetType().IsGenericType) {
                    additionalData = faultException.GetType().GetProperty("Detail").GetValue(faultException, null);
                } else {
                    additionalData = error;
                }

                // TODO: check error type and set appropriate error code
                fault = DispatcherUtils.CreateErrorMessage(version, msgId,
                    (int)JsonRpcErrorCodes.ServerError, error.Message, additionalData);
            }
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
