﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonRpc.ServiceModel
{
    public interface IJsonRpcResponseResult
    {
        string JsonRpcVersion { get; }
        object Result { get; }
        object Id { get; set; }
        JsonRpcException Error { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcResponse<T> : IJsonRpcResponseResult
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "jsonrpc")]
        public string JsonRpcVersion { get { return "2.0"; } }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "result")]
        public T Result { get; set; }

        object IJsonRpcResponseResult.Result { get { return Result; } }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "error")]
        public JsonRpcException Error { get; set; }

        [JsonProperty(PropertyName = "id")]
        public object Id { get; set; }
    }
}
