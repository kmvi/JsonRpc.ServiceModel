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
    class JsonRpcDispatchFormatter : IDispatchMessageFormatter
    {
        public void DeserializeRequest(Message message, object[] parameters)
        {
            throw new NotImplementedException();
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            throw new NotImplementedException();
        }
    }
}
