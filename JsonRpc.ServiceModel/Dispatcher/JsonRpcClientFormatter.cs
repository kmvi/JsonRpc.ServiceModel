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
    class JsonRpcClientFormatter : IClientMessageFormatter
    {
        private readonly OperationDescription _operation;
        private readonly MessageDescription _requestMessage;
        private readonly MessageDescription _responseMessage;
        private readonly ServiceEndpoint _endpoint;

        private const string MessageIdKey = "MessageId";

        public JsonRpcClientFormatter(OperationDescription operation, ServiceEndpoint endpoint)
        {
            _operation = operation;
            _endpoint = endpoint;
            _requestMessage = _operation.Messages.First(x => x.Direction == MessageDirection.Input);
            _responseMessage = _operation.Messages.First(x => x.Direction == MessageDirection.Output);
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            Type destType = _responseMessage.Body.ReturnValue.Type;
            if (typeof(void) == destType)
                return null;

            byte[] rawBody;
            using (XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents()) {
                bodyReader.ReadStartElement("Binary");
                rawBody = bodyReader.ReadContentAsBase64();
            }

            IJsonRpcResponseResult body;
            var responseType = typeof(JsonRpcResponse<>).MakeGenericType(destType);            
            using (var bodyReader = new StreamReader(new MemoryStream(rawBody))) {
                var serializer = new JsonSerializer();
                body = (IJsonRpcResponseResult)serializer.Deserialize(bodyReader, responseType);
            }

            // TODO: check id

            return body.Result;
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            byte[] rawBody;
            var serializer = new JsonSerializer();
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

            using (var memStream = new MemoryStream()) {
                using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(memStream, Encoding.UTF8))) {
                    writer.Formatting = Newtonsoft.Json.Formatting.None;

                    serializer.Serialize(writer, jsonRequest);
                    writer.Flush();

                    rawBody = memStream.ToArray();
                }
            }

            Message requestMessage = Message.CreateMessage(messageVersion,
                _requestMessage.Action, new RawBodyWriter(rawBody));

            requestMessage.Headers.To = _endpoint.Address.Uri;

            requestMessage.Properties.Add(WebBodyFormatMessageProperty.Name,
                new WebBodyFormatMessageProperty(WebContentFormat.Raw));
            HttpRequestMessageProperty reqProp = new HttpRequestMessageProperty();
            reqProp.Headers[HttpRequestHeader.ContentType] = String.Format("application/json; charset=utf-8");
            requestMessage.Properties.Add(HttpRequestMessageProperty.Name, reqProp);
            requestMessage.Properties.Add("HttpOperationName", _operation.Name);

            return requestMessage;
        }
    }
}
