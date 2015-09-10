using JsonRpc.ServiceModel.Channels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace JsonRpc.ServiceModel.Dispatcher
{
    class DispatcherUtils
    {
        public const string MessageIdKey = "MessageId";
        public const string OperationNameKey = "JsonRpcOperationName";

        public static byte[] SerializeBody(object content, Encoding encoding)
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

        public static Message CreateMessage(MessageVersion messageVersion, string action, byte[] rawBody, Encoding encoding)
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

        public static Message CreateErrorMessage<T>(MessageVersion messageVersion, JsonRpcResponse<T> response)
        {
            byte[] rawBody = SerializeBody(response, Encoding.UTF8);
            Message msg = CreateMessage(messageVersion, "", rawBody, Encoding.UTF8);

            var property = (HttpResponseMessageProperty)msg.Properties[HttpResponseMessageProperty.Name];
            if (property == null) {
                property = new HttpResponseMessageProperty();
                msg.Properties.Add(HttpResponseMessageProperty.Name, property);
            }

            SetStatusCode(response.Error.Code, property);

            return msg;
        }

        public static Message CreateErrorMessage(MessageVersion messageVersion, object messageId, JsonRpcException error)
        {
            var response = new JsonRpcResponse<object>()
            {
                Error = error,
                Result = null,
                Id = messageId
            };

            return CreateErrorMessage(messageVersion, response);
        }

        public static Message CreateErrorMessage(MessageVersion messageVersion, object messageId,
            int errorCode, string errorMessage, object details)
        {
            var exception = new JsonRpcException(errorCode, errorMessage, details);
            var response = new JsonRpcResponse<object>()
            {
                Error = exception,
                Result = null,
                Id = messageId
            };

            return CreateErrorMessage(messageVersion, response);
        }

        private static void SetStatusCode(int errorCode, HttpResponseMessageProperty property)
        {
            switch ((JsonRpcErrorCodes)errorCode) {
                case JsonRpcErrorCodes.ParseError:
                case JsonRpcErrorCodes.InvalidRequest:
                case JsonRpcErrorCodes.InvalidParams:
                    property.StatusCode = HttpStatusCode.BadRequest;
                    property.StatusDescription = "Bad Request";
                    break;
                case JsonRpcErrorCodes.MethodNotFound:
                    property.StatusCode = HttpStatusCode.NotFound;
                    property.StatusDescription = "Method Not Found";
                    break;
                case JsonRpcErrorCodes.InternalError:
                case JsonRpcErrorCodes.ServerError:
                default:
                    property.StatusCode = HttpStatusCode.InternalServerError;
                    property.StatusDescription = "Internal Server Error";
                    break;
            }
        }

        public static byte[] DeserializeBody(Message message)
        {
            using (XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents()) {
                bodyReader.ReadStartElement("Binary");
                return bodyReader.ReadContentAsBase64();
            }
        }
    }
}
