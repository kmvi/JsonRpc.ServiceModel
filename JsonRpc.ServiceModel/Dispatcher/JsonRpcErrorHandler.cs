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
            var debugBehavior = OperationContext.Current.Host.Description.Behaviors.Find<ServiceDebugBehavior>();
            bool includeDetails = (debugBehavior != null && debugBehavior.IncludeExceptionDetailInFaults);

            // TODO: check error type and set appropriate error code

            var errMessage = new JsonRpcException(123, includeDetails ? error.ToString() : error.Message, null);
            byte[] rawBody = DispatcherUtils.SerializeBody(errMessage, Encoding.UTF8);
            Message msg = DispatcherUtils.CreateMessage(version, "", rawBody, Encoding.UTF8);

            var property = (HttpResponseMessageProperty)msg.Properties[HttpResponseMessageProperty.Name];
            property.StatusCode = HttpStatusCode.InternalServerError;
            property.StatusDescription = "Internal Server Error";
        }
    }
}
