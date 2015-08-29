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
    class Program
    {
        static void Main(string[] args)
        {
            var baseUri = new Uri("http://" + Environment.MachineName + ":8085/simplesvc");
            var host = new ServiceHost(typeof(SimpleService), baseUri);

            host.AddServiceEndpoint(typeof(ISimpleService), new JsonRpcHttpBinding(), "json-rpc")
                .Behaviors.Add(new JsonRpcBehavior());

            host.Open();

            //Thread.Sleep(-1);
            
            var client = new WebClient();
            client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";

            // call SimpleMethod
            string simpleMethodCall = @"{""jsonrpc"": ""2.0"", ""method"": ""SimpleMethod"", ""params"": {""str"": ""World""}, ""id"": 1}";
            string response = client.UploadString(baseUri.ToString() + "/json-rpc", simpleMethodCall);
            Console.WriteLine("SimpleMethod(\"World\"): " + response);

            string addCall = @"{""jsonrpc"": ""2.0"", ""method"": ""Add"", ""params"": {""a"": 42, ""b"": 24}, ""id"": 2}";
            response = client.UploadString(baseUri.ToString() + "/json-rpc", addCall);
            Console.WriteLine("Add(42, 24): " + response);

            string getComplexTypeCall = @"{""jsonrpc"": ""2.0"", ""method"": ""GetComplexType"", ""params"": {""arg"": 3.14}, ""id"": 3}";
            response = client.UploadString(baseUri.ToString() + "/json-rpc", getComplexTypeCall);
            Console.WriteLine("GetComplexType(3.14): " + response);

            string voidMethodCall = @"{""jsonrpc"": ""2.0"", ""method"": ""VoidMethod"", ""params"": null, ""id"": 3}";
            response = client.UploadString(baseUri.ToString() + "/json-rpc", voidMethodCall);
            Console.WriteLine("VoidMethod(): " + response);
        }
    }
}
