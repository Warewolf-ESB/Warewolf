using System;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestInput : BindableBase, IServiceTestInput
    {
        string _variable;
        string _value;
        bool _emptyIsNull;

        public ServiceTestInput()
        {
            
        }

        public ServiceTestInput(string variableName, string value)
        {
            EmptyIsNull = false;
            Variable = variableName ?? throw new ArgumentNullException(nameof(variableName));
            Value = value;
        }

        public string Variable
        {
            get => _variable;
            set
            {
                _variable = value;
                OnPropertyChanged(() => Variable);
            }
        }
        public string Value
        {
            get => _value;
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
            get => _emptyIsNull;
            set
            {
                _emptyIsNull = value;
                AddNewAction?.Invoke();
                OnPropertyChanged(() => EmptyIsNull);
            }
        }
        [JsonIgnore]
        public Action AddNewAction { get; set; }
    }
}