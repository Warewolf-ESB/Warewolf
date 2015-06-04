using System;
using System.Collections;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IPluginAction  
    {
        string FullName { get; set; }
        string Method { get; set; }
        IList<IServiceInput> Inputs { get; set; }
        Type ReturnType { get; set; }

    }

    public class PluginAction : IPluginAction
    {
        string _fullName;
        string _method;
        IList<IServiceInput> _inputs;
        Type _returnType;

        #region Implementation of IPluginAction

        public string FullName
        {
            get
            {
                return _fullName;
            }
            set
            {
                _fullName = value;
            }
        }
        public string Method
        {
            get
            {
                return _method;
            }
            set
            {
                _method = value;
            }
        }
        public IList<IServiceInput> Inputs
        {
            get
            {
                return _inputs;
            }
            set
            {
                _inputs = value;
            }
        }
        public Type ReturnType
        {
            get
            {
                return _returnType;
            }
            set
            {
                _returnType = value;
            }
        }

        #endregion
    }
}