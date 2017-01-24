using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Common.Annotations;
using Dev2.Common.Interfaces.DB;

// ReSharper disable once CheckNamespace
namespace Dev2.Common.Interfaces
{
    public class PluginAction : IPluginAction, INotifyPropertyChanged
    {
        string _fullName;
        string _method;
        IList<IServiceInput> _inputs;
        Type _returnType;
        IList<INameValue> _variables;
        
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
        public IList<INameValue> Variables
        {
            get
            {
                return _variables;
            }
            set
            {
                _variables = value;
            }
        }
        public string Dev2ReturnType { get; set; }

        public string GetIdentifier()
        {
            //Rather add SourceId instead of full name
            return FullName + Method;
        }

        public string MethodResult { get; set; }
        public string OutputVariable { get; set; }
        public bool IsObject { get; set; }
        public bool IsVoid { get; set; }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Method;
        }
    }
}