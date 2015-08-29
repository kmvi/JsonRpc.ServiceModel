using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonRpc.ServiceModel
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcException : ApplicationException
    {
        [JsonProperty]
        public int code { get; set; }

        [JsonProperty]
        public string message { get; set; }

        [JsonProperty]
        public object data { get; set; }

        public JsonRpcException(int code, string message, object data)
        {
            this.code = code;
            this.message = message;
            this.data = data;
        }
    }
}
