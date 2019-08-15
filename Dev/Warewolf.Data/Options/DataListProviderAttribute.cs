using System;

namespace Warewolf.Options
{
    public class DataProviderAttribute : Attribute
    {
        private readonly Type _providerType;

        public DataProviderAttribute(Type dataProvider)
        {
            _providerType = dataProvider;
        }

        public object Get()
        {
            return Activator.CreateInstance(_providerType);
        }
    }
}