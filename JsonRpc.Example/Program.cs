using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonRpc.ServiceModel;
using JsonRpc.ServiceModel.Description;
using System.ServiceModel;
using System.Net;
using System.Threading;

namespace JsonRpc.Example
{
    [ServiceContract]
    interface ISimpleService
    {
        [OperationContract]
        string SimpleMethod(string str);
    }

    class SimpleService : ISimpleService
    {
        public string SimpleMethod(string str)
        {
            return "Hello " + str;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var baseUri = new Uri("http://" + Environment.MachineName + ":8085/simplesvc");
            var host = new ServiceHost(typeof(SimpleService), baseUri);

            host.AddServiceEndpoint(typeof(ISimpleService), new JsonRpcHttpBinding(), "json-rpc")
                .Behaviors.Add(new JsonRpcBehavior());

            host.Open();
            
            var client = new WebClient();
            client.Headers[HttpRequestHeader.ContentType] = "application/json";

            // call SimpleMethod
            var simpleMethodCall = @"{""jsonrpc"": ""2.0"", ""method"": ""SimpleMethod"", ""params"": {""str"": ""World""}, ""id"": 1}";
            var response = client.UploadString(baseUri.ToString() + "/json-rpc", simpleMethodCall);
        }
    }
}
