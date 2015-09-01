using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonRpc.ServiceModel
{
    [JsonObject(MemberSerialization.OptIn)]
    class JsonRpcRequest
    {
        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string JsonRpcVersion { get { return "2.0"; } }

        [JsonProperty("method", Required = Required.Always)]
        public string Method { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public object Params { get; set; }

        [JsonProperty("id", Required = Required.AllowNull)]
        public object Id { get; set; }
    }
}
