using System;

namespace Dev2.Runtime
{
    public sealed class Isolated<T> : IDisposable where T : MarshalByRefObject
    {
        AppDomain _domain;
        readonly T _value;

        public Isolated()
        {
           _domain = AppDomain.CreateDomain("Isolated:" + Guid.NewGuid(),
                null, AppDomain.CurrentDomain.SetupInformation);

            var type = typeof(T);

            _value = (T)_domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }
        
        public T Value => _value;

        public void Dispose()
        {
            if (_domain != null)
            {
                AppDomain.Unload(_domain);

                _domain = null;
            }
        }
    }
}