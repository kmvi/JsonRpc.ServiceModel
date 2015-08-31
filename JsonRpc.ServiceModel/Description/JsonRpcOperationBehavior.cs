using JsonRpc.ServiceModel.Dispatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace JsonRpc.ServiceModel.Description
{
    class JsonRpcOperationBehavior : IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            clientOperation.DeserializeReply = true;
            clientOperation.SerializeRequest = true;
            clientOperation.Formatter = new JsonRpcClientFormatter(operationDescription);
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            dispatchOperation.Formatter = new JsonRpcDispatchFormatter(operationDescription);
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    }
}
