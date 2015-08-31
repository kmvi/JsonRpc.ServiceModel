using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;

namespace JsonRpc.ServiceModel.Dispatcher
{
    class JsonRpcOperationSelector : IDispatchOperationSelector
    {
        private string SelectOperationInternal(Message message)
        {
            byte[] rawBody;
            using (XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents()) {
                bodyReader.ReadStartElement("Binary");
                rawBody = bodyReader.ReadContentAsBase64();
            }

            string operation = null;
            using (var bodyReader = new JsonTextReader(new StreamReader(new MemoryStream(rawBody)))) {
                while (bodyReader.Read()) {
                    if (bodyReader.TokenType == JsonToken.PropertyName) {
                        string propertyName = (string)bodyReader.Value;
                        if (propertyName.Equals("method", StringComparison.Ordinal)) {
                            if (!bodyReader.Read() || bodyReader.TokenType != JsonToken.String)
                                throw new InvalidOperationException("Invalid message format.");

                            operation = (string)bodyReader.Value;
                            break;
                        }
                    }
                }
            }

            return operation;
        }

        public string SelectOperation(ref Message message)
        {
            // TODO: check message format (raw)

            // Ignore non-POST requests
            if (!EnsurePostRequest(message))
                return null;

            if (message.Properties.ContainsKey("HttpOperationName"))
                return (string)message.Properties["HttpOperationName"];

            Message messageCopy;
            using (MessageBuffer buffer = message.CreateBufferedCopy(Int32.MaxValue)) {                
                message = buffer.CreateMessage();
                messageCopy = buffer.CreateMessage();                
            }

            string operation = SelectOperationInternal(messageCopy);
            if (operation == null)
                throw new InvalidOperationException("Invalid message format.");

            message.Properties["HttpOperationName"] = operation;

            return operation;
        }

        private bool EnsurePostRequest(Message message)
        {
            if (message.Properties.ContainsKey(HttpRequestMessageProperty.Name)) {
                var property = (HttpRequestMessageProperty)message.Properties[HttpRequestMessageProperty.Name];
                if (property.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
