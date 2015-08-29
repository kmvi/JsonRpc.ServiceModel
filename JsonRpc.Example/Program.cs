﻿using System;
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

        [OperationContract]
        int Add(int a, int b);
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    class SimpleService : ISimpleService
    {
        public string SimpleMethod(string str)
        {
            return "Hello " + str;
        }

        public int Add(int a, int b)
        {
            return a + b;
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

            //Thread.Sleep(-1);
            
            var client = new WebClient();
            client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";

            // call SimpleMethod
            var simpleMethodCall = @"{""jsonrpc"": ""2.0"", ""method"": ""SimpleMethod"", ""params"": {""str"": ""World""}, ""id"": 1}";
            string response = client.UploadString(baseUri.ToString() + "/json-rpc", simpleMethodCall);
            Console.WriteLine("SimpleMethod(\"World\"): " + response);

            var addCall = @"{""jsonrpc"": ""2.0"", ""method"": ""Add"", ""params"": {""a"": 42, ""b"": 24}, ""id"": 2}";
            response = client.UploadString(baseUri.ToString() + "/json-rpc", addCall);
            Console.WriteLine("Add(42, 24): " + response);
        }
    }
}
