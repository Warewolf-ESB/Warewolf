using System;

namespace Dev2
{
    public sealed class Isolated<T> : IDisposable where T : MarshalByRefObject
    {
        private AppDomain _domain;
        private readonly T _value;

        public Isolated()
        {
            _domain = AppDomain.CreateDomain("Isolated:" + Guid.NewGuid(),
                null, AppDomain.CurrentDomain.SetupInformation);

            Type type = typeof(T);

            _value = (T)_domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }

        public T Value
        {
            get
            {
                return _value;
            }
        }

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