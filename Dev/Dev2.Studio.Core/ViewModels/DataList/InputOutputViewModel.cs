using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.ViewModels.Base;
using System;

// ReSharper disable once CheckNamespace
// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.DataList
// ReSharper restore CheckNamespace
{
    public class InputOutputViewModel : SimpleBaseViewModel, IInputOutputViewModel, ICloneable
    {
        private string _value;
        private string _mapsTo;
        private bool _required;
        bool _isNew;
        bool _requiredMissing;
        string _typeName;
        bool _isMapsToFocused;
        bool _isValueFocused;

        #region Properties

        public string TypeName
        {
            get
            {
                return _typeName;
            }
            set
            {
                _typeName = value;

                base.OnPropertyChanged("TypeName");
            }
        }
        public bool RequiredMissing
        {
            get
            {
                return _requiredMissing;
            }
            set
            {
                if(value.Equals(_requiredMissing))
                {
                    return;
                }
                _requiredMissing = value;
                NotifyOfPropertyChange(() => RequiredMissing);
            }
        }

        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                if(value.Equals(_isNew))
                {
                    return;
                }
                _isNew = value;
                NotifyOfPropertyChange(() => IsNew);
            }
        }

        public bool IsSelected { get; set; }

        public string Name { get; set; }

        public string DisplayDefaultValue
        {
            get
            {
                string result = string.Empty;

                if(DefaultValue == string.Empty)
                {
                    if(EmptyToNull)
                    {
                        result = "Empty to NULL";
                    }
                }
                else
                {
                    result = "Default: " + DefaultValue;
                }

                return result;

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

                base.OnPropertyChanged("Value");
            }
        }

        public string MapsTo
        {
            get
            {
                return _mapsTo;
            }
            set
            {
                if(!value.Equals(_mapsTo))
                {
                    _mapsTo = value;
                    base.OnPropertyChanged("MapsTo");

                    if(Required)
                    {
                        RequiredMissing = string.IsNullOrEmpty(_mapsTo);
                    }
                }
            }
        }

        //public ObservableCollection<IDataListItemModel> DataList
        //{
        //    get
        //    {
        //        return _dataList;
        //    }
        //}

        public string DefaultValue { get; set; }

        public bool Required
        {
            get
            {
                return _required;
            }
            set
            {
                if(!value.Equals(_required))
                {
                    _required = value;
                    NotifyOfPropertyChange(() => Required);
                }
            }
        }

        public string RecordSetName { get; set; }
        public bool EmptyToNull { get; private set; }

        public bool IsMapsToFocused
        {
            get { return _isMapsToFocused; }
            set
            {
                if(value.Equals(_isMapsToFocused))
                {
                    return;
                }
                _isMapsToFocused = value;
                NotifyOfPropertyChange(() => IsMapsToFocused);
            }
        }

        public bool IsValueFocused
        {
            get { return _isValueFocused; }
            set
            {
                if(value.Equals(_isValueFocused))
                {
                    return;
                }
                _isValueFocused = value;
                NotifyOfPropertyChange(() => IsValueFocused);
            }
        }

        #endregion

        public InputOutputViewModel(string name, string value, string mapsTo, string defaultValue, bool required, string recordSetName)
            : this(name, value, mapsTo, defaultValue, required, recordSetName, false)
        {
        }

        public InputOutputViewModel(string name, string value, string mapsTo, string defaultValue, bool required, string recordSetName, bool emptyToNull)
        {
            Name = name;
            RecordSetName = recordSetName;
            MapsTo = mapsTo;
            Required = required;
            Value = value;
            DefaultValue = defaultValue;
            EmptyToNull = emptyToNull;

            if(RecordSetName == string.Empty)
            {
                // ReSharper disable once DoNotCallOverridableMethodsInConstructor
// ReSharper disable DoNotCallOverridableMethodsInConstructor
                DisplayName = Name;
// ReSharper restore DoNotCallOverridableMethodsInConstructor
            }
            else
            {
                // ReSharper disable once DoNotCallOverridableMethodsInConstructor
// ReSharper disable DoNotCallOverridableMethodsInConstructor
                DisplayName = RecordSetName + "(*)." + Name;
// ReSharper restore DoNotCallOverridableMethodsInConstructor
            }
        }


        #region Methods
        public IDev2Definition GetGenerationTO()
        {
            IDev2Definition result = DataListFactory.CreateDefinition(Name, MapsTo, Value, RecordSetName, false, DefaultValue, Required, Value, EmptyToNull);

            return result;
        }


        public object Clone()
        {

            // ReSharper disable once ObjectCreationAsStatement
// ReSharper disable ObjectCreationAsStatement
            new ObjectCloner<IDataListItemModel>();
// ReSharper restore ObjectCreationAsStatement
            IInputOutputViewModel result = new InputOutputViewModel(Name, Value, MapsTo, DefaultValue, Required, RecordSetName, EmptyToNull);

            return result;
        }

        #endregion
    }
}
