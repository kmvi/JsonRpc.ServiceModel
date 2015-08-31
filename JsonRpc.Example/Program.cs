using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonRpc.ServiceModel;
using JsonRpc.ServiceModel.Description;
using System.ServiceModel;
using System.Net;
using System.Threading;
using System.ServiceModel.Description;
using System.IO;

namespace JsonRpc.Example
{
    class Program
    {
        static void WebClientExample(string baseUri)
        {
            string uri = baseUri + "/json-rpc";
            var client = new WebClient();
            client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";

            // call SimpleMethod
            string simpleMethodCall = @"{""jsonrpc"": ""2.0"", ""method"": ""SimpleMethod"", ""params"": {""str"": ""World""}, ""id"": 1}";
            string response = client.UploadString(uri, simpleMethodCall);
            Console.WriteLine("SimpleMethod(\"World\"): " + response);

            string addCall = @"{""jsonrpc"": ""2.0"", ""method"": ""Add"", ""params"": {""a"": 42, ""b"": 24}, ""id"": 2}";
            response = client.UploadString(uri, addCall);
            Console.WriteLine("Add(42, 24): " + response);

            string getComplexTypeCall = @"{""jsonrpc"": ""2.0"", ""method"": ""GetComplexType"", ""params"": {""arg"": 3.14}, ""id"": 3}";
            response = client.UploadString(uri, getComplexTypeCall);
            Console.WriteLine("GetComplexType(3.14): " + response);

            string voidMethodCall = @"{""jsonrpc"": ""2.0"", ""method"": ""VoidMethod"", ""params"": null, ""id"": 3}";
            response = client.UploadString(uri, voidMethodCall);
            Console.WriteLine("VoidMethod(): " + response);

            string complexArgCall = @"{""jsonrpc"": ""2.0"", ""method"": ""ComplexArg"", ""params"": {""arg"": {""Name"": ""1234"", ""BirthDate"": ""1980-01-01""}}, ""id"": 4}";
            response = client.UploadString(uri, complexArgCall);
            Console.WriteLine("ComplexArg(arg): " + response);

            string gotExceptionCall = @"{""jsonrpc"": ""2.0"", ""method"": ""GotException"", ""id"": 5}";
            try {
                client.UploadString(uri, gotExceptionCall);
            } catch (WebException e) {
                Console.WriteLine("GotException(): " + e.Message);
                using (var reader = new StreamReader(e.Response.GetResponseStream())) {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
        }

        static void ChannelFactoryExample(string baseUri)
        {
            var factory = new ChannelFactory<ISimpleService>(
                new JsonRpcHttpBinding(),
                new EndpointAddress(baseUri + "/json-rpc"));

            factory.Endpoint.Behaviors.Add(new JsonRpcBehavior());

            var client = factory.CreateChannel();

            /*Console.WriteLine("SimpleMethod(\"World\"): " + client.SimpleMethod("World"));
            Console.WriteLine("Add(42, 24): " + client.Add(42, 24).ToString());
            Console.WriteLine("GetComplexType(3.14): " + client.GetComplexType(3.14).ToString());

            Console.Write("Call VoidMethod()... ");
            client.VoidMethod();
            Console.WriteLine("success");

            Console.WriteLine("ComplexArg(arg): " +
                client.ComplexArg(new ComplexType { Name = "1234", BirthDate = DateTime.Now }));*/

            try {
                var result = client.GotException();
            } catch (WebException e) {
                Console.WriteLine("GotException(): " + e.Message);
            }
        }

        static void Main(string[] args)
        {
            var baseUri = new Uri("http://" + Environment.MachineName + ":8085/simplesvc");
            var host = new ServiceHost(typeof(SimpleService), baseUri);

            host.AddServiceEndpoint(typeof(ISimpleService), new JsonRpcHttpBinding(), "json-rpc")
                .Behaviors.Add(new JsonRpcBehavior());

            host.Open();

            //Thread.Sleep(-1);

            Console.WriteLine("Using WebClient to make requests...");
            //WebClientExample(baseUri.ToString());

            Console.WriteLine();
            Console.WriteLine("Using ChannelFactory to make requests...");
            ChannelFactoryExample(baseUri.ToString());

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
