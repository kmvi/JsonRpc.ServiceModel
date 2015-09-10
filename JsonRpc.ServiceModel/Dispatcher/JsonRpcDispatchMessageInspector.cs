using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace JsonRpc.ServiceModel.Dispatcher
{
    class JsonRpcDispatchMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (reply.IsFault) {
                MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue);
                var fault = MessageFault.CreateFault(buffer.CreateMessage(), Int32.MaxValue);
                if (fault.Code.SubCode.Name == "ActionNotSupported") {
                    // TODO message id
                    reply = DispatcherUtils.CreateErrorMessage(reply.Version, null,
                        (int)JsonRpcErrorCodes.MethodNotFound, "Method not found.",
                        fault.Reason.GetMatchingTranslation().Text);
                }
                // TODO process other fault codes
            }
        }
    }
}
