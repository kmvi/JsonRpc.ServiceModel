using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace JsonRpc.ServiceModel.Dispatcher
{
    class JsonRpcClientFormatter : IClientMessageFormatter
    {
        public object DeserializeReply(Message message, object[] parameters)
        {
            throw new NotImplementedException();
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
