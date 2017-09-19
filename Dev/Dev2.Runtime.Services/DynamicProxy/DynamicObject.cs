using System;
using System.Reflection;



namespace Dev2.Runtime.DynamicProxy
{
    public class DynamicObject
    {
        private readonly Type _objType;
        private object _obj;

        private BindingFlags _commonBindingFlags =
            BindingFlags.Instance |
            BindingFlags.Public;
         
        public DynamicObject(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            _obj = obj;
            _objType = obj.GetType();
        }

        public DynamicObject(Type objType)
        {
            if (objType == null)
            {
                throw new ArgumentNullException("objType");
            }

            _objType = objType;
        }

        public void CallConstructor(Type[] paramTypes, object[] paramValues)
        {
            ConstructorInfo ctor = _objType.GetConstructor(paramTypes);
            if (ctor == null)
            {
                throw new DynamicProxyException(
                        Constants.ErrorMessages.ProxyCtorNotFound);            
            }

            _obj = ctor.Invoke(paramValues);
        }

        public object CallMethod(string method, params object[] parameters)
        {
            object retval = _objType.InvokeMember(
                method,
                BindingFlags.InvokeMethod | _commonBindingFlags,
                null /* Binder */,
                _obj,
                parameters /* args */);

            return retval;
        }

        public Type ObjectType => _objType;

        public object ObjectInstance => _obj;

        public BindingFlags BindingFlags
        {
            get
            {
                return _commonBindingFlags;
            }

            set
            {
                _commonBindingFlags = value;
            }
        }
    }
}
