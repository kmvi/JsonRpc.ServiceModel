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
    class JsonRpcDispatchFormatter : JsonRpcFormatterBase, IDispatchMessageFormatter
    {
        public JsonRpcDispatchFormatter(OperationDescription operation)
            : base(operation)
        {
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            // TODO: check message format (raw)

            byte[] rawBody = DispatcherUtils.DeserializeBody(message);

            JsonRpcRequest body;
            using (var bodyReader = new StreamReader(new MemoryStream(rawBody))) {
                var serializer = new JsonSerializer();
                body = (JsonRpcRequest)serializer.Deserialize(bodyReader, typeof(JsonRpcRequest));
            }

            var paramValues = body.Params as JObject;
            if (paramValues != null) {
                int paramIndex = 0;
                foreach (var parameter in _requestMessage.Body.Parts) {
                    JToken value;
                    if (paramValues.TryGetValue(parameter.Name, out value)) {
                        parameters[paramIndex] = value.ToObject(parameter.Type);
                    }

                    ++paramIndex;
                }
            }

            message.Properties[DispatcherUtils.MessageIdKey] = body.Id;
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            JsonRpcResponse<object> jsonResponse = CreateResponse(result);
            Encoding encoding = GetResponseMessageEncoding();

            byte[] rawBody = DispatcherUtils.SerializeBody(jsonResponse, encoding);

            return DispatcherUtils.CreateMessage(messageVersion, _responseMessage.Action, rawBody, encoding);
        }

        private JsonRpcResponse<object> CreateResponse(object result)
        {
            var jsonResponse = new JsonRpcResponse<object>();
            jsonResponse.Result = result;

            object messageId;
            MessageProperties inMsgProperties = OperationContext.Current.IncomingMessageProperties;
            if (inMsgProperties.TryGetValue(DispatcherUtils.MessageIdKey, out messageId))
                jsonResponse.Id = messageId;

            return jsonResponse;
        }

        private Encoding GetResponseMessageEncoding()
        {
            HttpRequestMessageProperty httpRequest = null;
            MessageProperties inMsgProperties = OperationContext.Current.IncomingMessageProperties;
            if (inMsgProperties.ContainsKey(HttpRequestMessageProperty.Name)) {
                httpRequest = (HttpRequestMessageProperty)inMsgProperties[HttpRequestMessageProperty.Name];
            }

            Encoding encoding = Encoding.UTF8;
            string charset = null;
            if (httpRequest != null) {
                charset = httpRequest.Headers[HttpRequestHeader.AcceptCharset];
            }

            if (charset != null)
                encoding = Encoding.GetEncoding(charset);

            return encoding;
        }
    }
}
