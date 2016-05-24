using System;
using System.Reflection;

// ReSharper disable UseNameofExpression

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
                throw new ArgumentNullException("obj");

            _obj = obj;
            _objType = obj.GetType();
        }

        public DynamicObject(Type objType)
        {
            if (objType == null)
                throw new ArgumentNullException("objType");

            _objType = objType;
        }

        public void CallConstructor()
        {
            CallConstructor(new Type[0], new object[0]);
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

        public object GetProperty(string property)
        {
            object retval = _objType.InvokeMember(
                property,
                BindingFlags.GetProperty | _commonBindingFlags,
                null /* Binder */,
                _obj,
                null /* args */);

            return retval;
        }

        public object SetProperty(string property, object value)
        {
            object retval = _objType.InvokeMember(
                property,
                BindingFlags.SetProperty | _commonBindingFlags,
                null /* Binder */,
                _obj,
                new[] { value });

            return retval;
        }

        public object GetField(string field)
        {
            object retval = _objType.InvokeMember(
                field,
                BindingFlags.GetField | _commonBindingFlags,
                null /* Binder */,
                _obj,
                null /* args */);

            return retval;
        }

        public object SetField(string field, object value)
        {
            object retval = _objType.InvokeMember(
                field,
                BindingFlags.SetField | _commonBindingFlags,
                null /* Binder */,
                _obj,
                new[] { value });

            return retval;
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

        public object CallMethod(string method, Type[] types, 
            object[] parameters)
        {
            if (types.Length != parameters.Length)
                throw new ArgumentException(
                    Constants.ErrorMessages.ParameterValueMistmatch);

            MethodInfo mi = _objType.GetMethod(method, types);
            if (mi == null) 
                throw new ApplicationException(string.Format(
                    Constants.ErrorMessages.MethodNotFound, method));

            object retval = mi.Invoke(_obj, _commonBindingFlags, null, 
                parameters, null);

            return retval;
        }

        public Type ObjectType
        {
            get
            {
                return _objType;
            }
        }

        public object ObjectInstance
        {
            get
            {
                return _obj;
            }
        }

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
