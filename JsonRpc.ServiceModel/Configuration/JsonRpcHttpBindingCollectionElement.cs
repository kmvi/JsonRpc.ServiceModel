using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Text;

namespace JsonRpc.ServiceModel.Configuration
{
    public class JsonRpcHttpBindingCollectionElement : BindingCollectionElement
    {
        public override Type BindingType
        {
            get { return typeof(JsonRpcHttpBinding); }
        }

        public override ReadOnlyCollection<IBindingConfigurationElement> ConfiguredBindings
        {
            get
            {
                // TODO
                return new ReadOnlyCollection<IBindingConfigurationElement>(new List<IBindingConfigurationElement>());
            }
        }

        public override bool ContainsKey(string name)
        {
            // TODO
            return false;
        }

        protected override Binding GetDefault()
        {
            return new JsonRpcHttpBinding();
        }

        protected override bool TryAdd(string name, Binding binding, System.Configuration.Configuration config)
        {
            // TODO
            return false;
        }
    }
}
