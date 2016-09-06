using System;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceServiceTestInput: BindableBase,IServiceTestInput
    {
        private string _variable;
        private string _value;
        private bool _emptyIsNull;

        public ServiceServiceTestInput(string variableName, string value)
        {
            if(variableName == null)
                throw new ArgumentNullException(nameof(variableName));
            EmptyIsNull = true;
            Variable = variableName;
            Value = value;
        }

        #region Implementation of IServiceTestInput

        public string Variable
        {
            get
            {
                return _variable;
            }
            set
            {
                _variable = value;
                OnPropertyChanged(()=>Variable);
            }
        }
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged(() => Value);
            }
        }
        public bool EmptyIsNull
        {
            get
            {
                return _emptyIsNull;
            }
            set
            {
                _emptyIsNull = value;               
                OnPropertyChanged(()=>EmptyIsNull);
            }
        }

        #endregion
    }
}