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
    class JsonRpcDispatchFormatter : IDispatchMessageFormatter
    {
        private readonly OperationDescription _operation;        
        private readonly MessageDescription _requestMessage;
        private readonly MessageDescription _responseMessage;

        private const string MessageIdKey = "MessageId";

        public JsonRpcDispatchFormatter(OperationDescription operation)
        {
            _operation = operation;
            _requestMessage = _operation.Messages.First(x => x.Direction == MessageDirection.Input);
            _responseMessage = _operation.Messages.First(x => x.Direction == MessageDirection.Output);
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            // TODO: check message format (raw)

            byte[] rawBody;
            using (XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents()) {
                bodyReader.ReadStartElement("Binary");
                rawBody = bodyReader.ReadContentAsBase64();
            }

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

            message.Properties[MessageIdKey] = body.Id;
        }

        private JsonRpcResponse<object> CreateResponse(object result)
        {
            var jsonResponse = new JsonRpcResponse<object>();
            jsonResponse.Result = result;

            object messageId;
            MessageProperties inMsgProperties = OperationContext.Current.IncomingMessageProperties;
            if (inMsgProperties.TryGetValue(MessageIdKey, out messageId))
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

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            byte[] rawBody;
            var serializer = new JsonSerializer();
            JsonRpcResponse<object> jsonResponse = CreateResponse(result);
            Encoding encoding = GetResponseMessageEncoding();

            using (var memStream = new MemoryStream()) {
                using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(memStream, encoding))) {
                    writer.Formatting = Newtonsoft.Json.Formatting.None;

                    serializer.Serialize(writer, jsonResponse);
                    writer.Flush();

                    rawBody = memStream.ToArray();
                }
            }

            Message replyMessage = Message.CreateMessage(messageVersion,
                _responseMessage.Action, new RawBodyWriter(rawBody));

            replyMessage.Properties.Add(WebBodyFormatMessageProperty.Name,
                new WebBodyFormatMessageProperty(WebContentFormat.Raw));

            var respProp = new HttpResponseMessageProperty();
            respProp.Headers[HttpResponseHeader.ContentType] =
                String.Format("application/json; charset={0}", encoding.WebName);
            replyMessage.Properties.Add(HttpResponseMessageProperty.Name, respProp);

            return replyMessage;
        }
    }
}
