/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Utils;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;

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
        private bool _isObject;
        private string _jsonString;

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

                OnPropertyChanged("TypeName");
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

                OnPropertyChanged("Value");
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
                    OnPropertyChanged("MapsTo");

                    UpdateDataListWithJsonObject();

                    if (Required)
                    {
                        RequiredMissing = string.IsNullOrEmpty(_mapsTo);
                    }
                }
            }
        }

        private void UpdateDataListWithJsonObject()
        {
            if (IsObject && !string.IsNullOrEmpty(JsonString))
            {
                try
                {
                    var language = FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(_mapsTo);
                    if (language.IsJsonIdentifierExpression)
                    {
                        if (DataListSingleton.ActiveDataList != null)
                        {
                            var objToProcess = JsonConvert.DeserializeObject(JsonString) as JObject;
                            if (objToProcess != null)
                            {
                                var firstOrDefault = objToProcess.Properties().FirstOrDefault();
                                if (firstOrDefault != null)
                                {
                                    var processString = firstOrDefault.Value.ToString();
                                    DataListSingleton.ActiveDataList.GenerateComplexObjectFromJson(
                                        DataListUtil.RemoveLanguageBrackets(_mapsTo), processString);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //Is not an object identifier
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
                    OnPropertyChanged("Required");
                    NotifyOfPropertyChange(() => Required);
                    OnPropertyChanged("Required");
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

        public ICommand ViewComplexObjectsCommand { get; set; }

        public bool IsObject
        {
            get { return _isObject; }
            set
            {
                _isObject = value; 
                NotifyOfPropertyChange(() => IsObject);
            }
        }

        public string JsonString
        {
            get { return _jsonString; }
            set
            {
                _jsonString = value;
                UpdateDataListWithJsonObject();
            }
        }

        private void ViewJsonObjects()
        {
            if (!string.IsNullOrEmpty(JsonString))
            {
                var window = new JsonObjectsView();
                window.Height = 280;
                var contentPresenter = window.FindChild<TextBox>();
                if (contentPresenter != null)
                {
                    var json=JSONUtils.Format(JsonString);
                    contentPresenter.Text = json;
                }

                window.ShowDialog();
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
            ViewComplexObjectsCommand = new RelayCommand(item =>
            {
                ViewJsonObjects();
            });
        }


        #region Methods
        public IDev2Definition GetGenerationTO()
        {
            IDev2Definition result = DataListFactory.CreateDefinition(Name, MapsTo, Value, RecordSetName, false, DefaultValue, Required, Value, EmptyToNull);
            result.IsObject = IsObject;
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
