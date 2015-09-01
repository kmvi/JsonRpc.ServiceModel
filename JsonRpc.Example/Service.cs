using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace JsonRpc.Example
{
    [DataContract]
    struct ComplexType
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime BirthDate { get; set; }

        [DataMember]
        public double? NullableProperty { get; set; }

        public override string ToString()
        {
            return String.Format("Name: {0}, BirthDate: {1}, NullableProperty: {2}",
                Name, BirthDate, NullableProperty);
        }
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

        [OperationContract]
        string ComplexArg(ComplexType arg);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        string GotException();
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = false)]
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

        public string ComplexArg(ComplexType arg)
        {
            return arg.ToString();
        }

        public string GotException()
        {
            throw new FaultException<ArgumentException>(
                new ArgumentException("exception"),
                new FaultReason("reason"));
            //throw new ArgumentException("exception");
        }
    }
}
