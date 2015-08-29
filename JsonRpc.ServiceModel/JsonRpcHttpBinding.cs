using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Text;

namespace JsonRpc.ServiceModel
{
    class JsonRpcRawMapper : WebContentTypeMapper
    {
        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            return WebContentFormat.Raw;
        }
    }

    public class JsonRpcHttpBinding : Binding
    {
        private HttpTransportBindingElement transport;
        private WebMessageEncodingBindingElement encoding;

        public override string Scheme { get { return transport.Scheme; } }

        public JsonRpcHttpBinding()
            : base()
        {
            transport = new HttpTransportBindingElement();
            encoding = new WebMessageEncodingBindingElement();
            encoding.ContentTypeMapper = new JsonRpcRawMapper();
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = new BindingElementCollection();
            elements.Add(encoding);
            elements.Add(transport);
            return elements;
        }
    }
}
