using JsonRpc.ServiceModel.Channels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;

namespace JsonRpc.ServiceModel.Dispatcher
{
    class JsonRpcClientFormatter : JsonRpcFormatterBase, IClientMessageFormatter
    {
        private readonly ServiceEndpoint _endpoint;

        public JsonRpcClientFormatter(OperationDescription operation, ServiceEndpoint endpoint)
            : base(operation)
        {
            _endpoint = endpoint;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            var property = (HttpResponseMessageProperty)message.Properties[HttpResponseMessageProperty.Name];
            bool isError = ((int)property.StatusCode >= 400);
            byte[] rawBody = DispatcherUtils.DeserializeBody(message);

            if (isError) {
                // TODO
                throw null;
            } else {
                
                IJsonRpcResponseResult body;
                Type destType = _responseMessage.Body.ReturnValue.Type;
                if (typeof(void) == destType)
                    return null;

                var responseType = typeof(JsonRpcResponse<>).MakeGenericType(destType);
                using (var bodyReader = new StreamReader(new MemoryStream(rawBody))) {
                    var serializer = new JsonSerializer();
                    body = (IJsonRpcResponseResult)serializer.Deserialize(bodyReader, responseType);
                }

                // TODO: check id

                return body.Result;
            }
        }

        private JsonRpcRequest CreateRequest(object[] parameters)
        {
            JsonRpcRequest jsonRequest = new JsonRpcRequest();
            jsonRequest.Id = Guid.NewGuid().ToString(); // TODO: mb int?
            jsonRequest.Method = _operation.Name;
            var args = new JObject();

            int paramIndex = 0;
            foreach (var parameter in _requestMessage.Body.Parts) {
                var token = JToken.FromObject(parameters[paramIndex]);
                args.Add(new JProperty(parameter.Name, token));
                paramIndex++;
            }

            jsonRequest.Params = args;

            return jsonRequest;
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            JsonRpcRequest jsonRequest = CreateRequest(parameters);
            byte[] rawBody = DispatcherUtils.SerializeBody(jsonRequest, Encoding.UTF8);

            Message requestMessage = DispatcherUtils.CreateMessage(
                messageVersion, _requestMessage.Action, rawBody, Encoding.UTF8);

            requestMessage.Headers.To = _endpoint.Address.Uri;
            requestMessage.Properties.Add("HttpOperationName", _operation.Name);

            return requestMessage;
        }
    }
}
