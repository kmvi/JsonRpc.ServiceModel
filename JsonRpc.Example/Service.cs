using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace JsonRpc.Example
{
    [DataContract]
    class ComplexType
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime BirthDate { get; set; }

        [DataMember]
        public double? NullableProperty { get; set; }
    }

    [ServiceContract]
    interface ISimpleService
    {
        [OperationContract]
        string SimpleMethod(string str);

        [OperationContract]
        int Add(int a, int b);

        [OperationContract]
        ComplexType GetComplexType(double arg);

        [OperationContract]
        void VoidMethod();
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

        public ComplexType GetComplexType(double arg)
        {
            return new ComplexType {
                Name = "test",
                BirthDate = DateTime.Now,
                NullableProperty = arg
            };
        }

        public void VoidMethod()
        {

        }
    }
}
