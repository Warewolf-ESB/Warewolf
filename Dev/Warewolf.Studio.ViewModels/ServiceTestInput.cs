using System;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestInput : BindableBase, IServiceTestInput
    {
        private string _variable;
        private string _value;
        private bool _emptyIsNull;

        // ReSharper disable once UnusedMember.Global
        public ServiceTestInput()
        {
            
        }

        public ServiceTestInput(string variableName, string value)
        {
            if (variableName == null)
                throw new ArgumentNullException(nameof(variableName));
            EmptyIsNull = false;
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
                OnPropertyChanged(() => Variable);
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
                if (!string.IsNullOrEmpty(_value))
                {
                    AddNewAction?.Invoke();
                }
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
                AddNewAction?.Invoke();
                OnPropertyChanged(() => EmptyIsNull);
            }
        }
        [JsonIgnore]
        public Action AddNewAction { get; set; }

        #endregion
       
    }
}