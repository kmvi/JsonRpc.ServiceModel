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
        public JsonRpcRequest()
        {
        }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public object Params { get; set; }

        [JsonProperty("id")]
        public object Id { get; set; }
    }
}
