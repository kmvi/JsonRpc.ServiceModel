using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonRpc.ServiceModel
{
    public enum JsonRpcErrorCodes
    {
        ParseError = -23700,
        InvalidRequest = -32600,
        MethodNotFound = -32601,
        InvalidParams = -32602,
        InternalError = -32603,
        ServerError = -32000,
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcException : Exception
    {
        [JsonProperty(PropertyName = "code", Required = Required.Always)]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "message", Required = Required.Always)]
        public override string Message { get { return base.Message; } }

        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public object AdditionalData { get; set; }

        public JsonRpcException(int code, string message, object data)
            : base(message)
        {
            Code = code;
            AdditionalData = data;
        }
    }
}
