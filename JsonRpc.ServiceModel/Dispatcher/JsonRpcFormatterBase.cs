using JsonRpc.ServiceModel.Channels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;

namespace JsonRpc.ServiceModel.Dispatcher
{
    abstract class JsonRpcFormatterBase
    {
        protected readonly OperationDescription _operation;
        protected readonly MessageDescription _requestMessage;
        protected readonly MessageDescription _responseMessage;

        protected const string MessageIdKey = "MessageId";

        protected JsonRpcFormatterBase(OperationDescription operation)
        {
            _operation = operation;
            _requestMessage = _operation.Messages.First(x => x.Direction == MessageDirection.Input);
            _responseMessage = _operation.Messages.First(x => x.Direction == MessageDirection.Output);
        }
    }
}
