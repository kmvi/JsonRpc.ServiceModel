using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using JsonRpc.ServiceModel.Dispatcher;

namespace JsonRpc.ServiceModel.Description
{
    public class JsonRpcBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            foreach (var op in endpoint.Contract.Operations) {
                if (op.Behaviors.Find<JsonRpcOperationBehavior>() == null)
                    op.Behaviors.Add(new JsonRpcOperationBehavior(endpoint));
            }
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.ContractFilter = new MatchAllMessageFilter();
            endpointDispatcher.DispatchRuntime.OperationSelector = new JsonRpcOperationSelector();

            var errHandlers = endpointDispatcher.DispatchRuntime.ChannelDispatcher.ErrorHandlers;
            if (!errHandlers.OfType<JsonRpcErrorHandler>().Any())
                errHandlers.Add(new JsonRpcErrorHandler());
            
            foreach (var op in endpoint.Contract.Operations) {
                if (op.Behaviors.Find<JsonRpcOperationBehavior>() == null)
                    op.Behaviors.Add(new JsonRpcOperationBehavior(endpoint));
            }
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // TODO: check WebMessageEncodingBindingElement presence
            // TODO: check http method (POST only)
        }
    }
}
