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

        public JsonRpcDispatchFormatter(OperationDescription operation)
        {
            _operation = operation;
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

            var requestMessage = _operation.Messages.First(x => x.Direction == MessageDirection.Input);
            var paramValues = body.Params as JObject;
            if (paramValues != null) {
                int paramIndex = 0;
                foreach (var parameter in requestMessage.Body.Parts) {
                    JToken value;
                    if (paramValues.TryGetValue(parameter.Name, out value)) {
                        parameters[paramIndex] = value.ToObject(parameter.Type);
                    }

                    ++paramIndex;
                }
            }

            OperationContext.Current.IncomingMessageProperties["MessageId"] = body.Id;
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            byte[] rawBody;
            
            using (var memStream = new MemoryStream()) {
                using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(memStream, Encoding.UTF8))) {
                    writer.Formatting = Newtonsoft.Json.Formatting.None;
                    var jsonResponse = new JsonRpcResponse();
                    jsonResponse.Result = result;
                    jsonResponse.Id = OperationContext.Current.IncomingMessageProperties["MessageId"];
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, jsonResponse);
                    writer.Flush();
                    rawBody = memStream.ToArray();
                }
            }

            var responseMessage = _operation.Messages.First(x => x.Direction == MessageDirection.Output);
            Message replyMessage = Message.CreateMessage(messageVersion, responseMessage.Action, new RawBodyWriter(rawBody));

            replyMessage.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Raw));

            HttpResponseMessageProperty respProp = new HttpResponseMessageProperty();
            respProp.Headers[HttpResponseHeader.ContentType] = "application/json";
            replyMessage.Properties.Add(HttpResponseMessageProperty.Name, respProp);

            return replyMessage;
        }
    }
}
