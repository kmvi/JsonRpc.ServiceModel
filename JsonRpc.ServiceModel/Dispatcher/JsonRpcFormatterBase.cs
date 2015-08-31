using JsonRpc.ServiceModel.Channels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;

namespace JsonRpc.ServiceModel.Dispatcher
{
    abstract class JsonRpcFormatterBase
    {
        protected readonly OperationDescription _operation;
        protected readonly MessageDescription _requestMessage;
        protected readonly MessageDescription _responseMessage;

        protected const string MessageIdKey = "MessageId";

        protected JsonRpcFormatterBase(OperationDescription operation)
        {
            _operation = operation;
            _requestMessage = _operation.Messages.First(x => x.Direction == MessageDirection.Input);
            _responseMessage = _operation.Messages.First(x => x.Direction == MessageDirection.Output);
        }

        protected static byte[] DeserializeBody(Message message)
        {
            using (XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents()) {
                bodyReader.ReadStartElement("Binary");
                return bodyReader.ReadContentAsBase64();
            }
        }

        protected static byte[] SerializeBody(object content, Encoding encoding)
        {
            var serializer = new JsonSerializer();
            using (var memStream = new MemoryStream()) {
                using (var writer = new JsonTextWriter(new StreamWriter(memStream, encoding))) {
                    writer.Formatting = Newtonsoft.Json.Formatting.None;

                    serializer.Serialize(writer, content);
                    writer.Flush();

                    return memStream.ToArray();
                }
            }
        }

        protected Message CreateMessage(MessageVersion messageVersion, string action, byte[] rawBody, Encoding encoding)
        {
            Message message = Message.CreateMessage(messageVersion,
                action, new RawBodyWriter(rawBody));

            message.Properties.Add(WebBodyFormatMessageProperty.Name,
                new WebBodyFormatMessageProperty(WebContentFormat.Raw));

            var respProp = new HttpResponseMessageProperty();
            respProp.Headers[HttpResponseHeader.ContentType] =
                String.Format("application/json; charset={0}", encoding.WebName);
            message.Properties.Add(HttpResponseMessageProperty.Name, respProp);

            return message;
        }
    }
}
