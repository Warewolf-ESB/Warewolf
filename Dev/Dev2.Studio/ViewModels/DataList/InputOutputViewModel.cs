using Dev2.DataList.Contract;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.ViewModels.Base;
using System;

namespace Dev2.Studio.ViewModels.DataList {
    public class InputOutputViewModel : SimpleBaseViewModel, IInputOutputViewModel, ICloneable {
       
        private bool _isSelected;
        private string _name;
        private string _value;
        private string _mapsTo;
        private string _defaultValue;
        private bool _required;
        private string _recordSetName;

        #region Properties
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public string DisplayDefaultValue
        {
            get
            {
                string result = string.Empty;

                if (DefaultValue == string.Empty)
                {
                    if (EmptyToNull)
                    {
                        result = "Empty to NULL";
                    }
                }
                else
                {
                    result =  "Default: " + DefaultValue;
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
                _mapsTo = value;

                base.OnPropertyChanged("MapsTo");
            }
        }

        //public ObservableCollection<IDataListItemModel> DataList
        //{
        //    get
        //    {
        //        return _dataList;
        //    }
        //}


        public string DefaultValue
        {
            get
            {
                return _defaultValue;
            }
            set
            {
                _defaultValue = value;
            }
        }

        public bool Required
        {
            get
            {
                return _required;
            }
            set
            {
                _required = value;
            }
        }

        public string RecordSetName
        {
            get
            {
                return _recordSetName;
            }
            set
            {
                _recordSetName = value;
            }
        }
        public bool EmptyToNull { get; private set; }

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


            if (RecordSetName == string.Empty)
            {
                DisplayName = Name;
            }
            else
            {
                DisplayName = RecordSetName + "(*)." + Name;
            }
        }
        

        #region Methods
        public IDev2Definition GetGenerationTO() {
            IDev2Definition result = DataListFactory.CreateDefinition(Name, MapsTo, Value, RecordSetName, false, DefaultValue, Required, Value, EmptyToNull);
            
            return result;
        }


        public object Clone() {
            
            IObjectCloner<IDataListItemModel> cloner = new ObjectCloner<IDataListItemModel>();                       
            //Collection<IDataListItemModel> tmpDataList = cloner.CloneObservableCollection(this.DataList);            
            IInputOutputViewModel result = new InputOutputViewModel(this.Name, this.Value, this.MapsTo, this.DefaultValue, this.Required, this.RecordSetName, this.EmptyToNull);
            
            return result;
        }

        #endregion
    }
}
