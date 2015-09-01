JSON-PRC 2.0 specification: [http://www.jsonrpc.org/specification](http://www.jsonrpc.org/specification)

### Usage

See JsonRpc.Example project.

```csharp
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

// ...

var baseUri = new Uri("http://" + Environment.MachineName + ":8085/simplesvc");
var host = new ServiceHost(typeof(SimpleService), baseUri);

ServiceEndpoint ep = host.AddServiceEndpoint(typeof(ISimpleService), new JsonRpcHttpBinding(), "json-rpc");
ep.Behaviors.Add(new JsonRpcBehavior());

host.Open();
```

### TODO
- TCP binding
- HTTPS binding
- Metadata
- Set correct error codes
- Unwrap `FaultException<T>`
- Tests
- Batches (???)
- Configuration elements
