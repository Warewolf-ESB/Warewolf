#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;





namespace Dev2.Studio.ViewModels.DataList

{
    public class InputOutputViewModel : SimpleBaseViewModel, IInputOutputViewModel, ICloneable
    {
        string _value;
        string _mapsTo;
        bool _required;
        bool _isNew;
        bool _requiredMissing;
        string _typeName;
        bool _isMapsToFocused;
        bool _isValueFocused;
        bool _isObject;
        string _jsonString;

        #region Properties

        public string TypeName
        {
            get => _typeName;
            set
            {
                _typeName = value;
                OnPropertyChanged("TypeName");
            }
        }
        public bool RequiredMissing
        {
            get => _requiredMissing;
            set
            {
                if (value.Equals(_requiredMissing))
                {
                    return;
                }
                _requiredMissing = value;
                NotifyOfPropertyChange(() => RequiredMissing);
            }
        }

        public bool IsNew
        {
            get => _isNew;
            set
            {
                if (value.Equals(_isNew))
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
                var result = string.Empty;

                if (DefaultValue == string.Empty)
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
            get => _value;
            set
            {
                if (!value.Equals(_value))
                {
                    _value = value;
                    OnPropertyChanged("Value");

                    TryUpdateDataListWithJsonObject(_value);

                    if (Required)
                    {
                        RequiredMissing = string.IsNullOrEmpty(_value);
                    }
                }
            }
        }

        public string MapsTo
        {
            get => _mapsTo;
            set
            {
                if (!value.Equals(_mapsTo))
                {
                    _mapsTo = value;
                    OnPropertyChanged("MapsTo");

                    TryUpdateDataListWithJsonObject(_mapsTo);

                    if (Required)
                    {
                        RequiredMissing = string.IsNullOrEmpty(_mapsTo);
                    }
                }
            }
        }

        void TryUpdateDataListWithJsonObject(string expression)
        {
            if (IsObject && !string.IsNullOrEmpty(JsonString))
            {
                try
                {
                    UpdateDataListWithJsonObject(expression);
                }
                catch (Exception)
                {
                    //Is not an object identifier
                }
            }
        }

        private void UpdateDataListWithJsonObject(string expression)
        {
            var language = FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(expression);
            if (language.IsJsonIdentifierExpression && DataListSingleton.ActiveDataList != null)
            {
                var objToProcess = JsonConvert.DeserializeObject(JsonString) as JObject;
                var firstOrDefault = objToProcess?.Properties().FirstOrDefault();
                if (firstOrDefault != null)
                {
                    var processString = firstOrDefault.Value.ToString();
                    DataListSingleton.ActiveDataList.GenerateComplexObjectFromJson(
                        DataListUtil.RemoveLanguageBrackets(expression), processString);
                }
            }
        }

        public string DefaultValue { get; set; }

        public bool Required
        {
            get => _required;
            set
            {
                if (!value.Equals(_required))
                {
                    _required = value;
                    OnPropertyChanged("Required");
                    NotifyOfPropertyChange(() => Required);
                    OnPropertyChanged("Required");
                }
            }
        }

        public string RecordSetName { get; set; }
        bool EmptyToNull { get; set; }

        public bool IsMapsToFocused
        {
            get => _isMapsToFocused;
            set
            {
                if (value.Equals(_isMapsToFocused))
                {
                    return;
                }
                _isMapsToFocused = value;
                NotifyOfPropertyChange(() => IsMapsToFocused);
            }
        }

        public bool IsValueFocused
        {
            get => _isValueFocused;
            set
            {
                if (value.Equals(_isValueFocused))
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
            get => _isObject;
            set
            {
                _isObject = value;
                NotifyOfPropertyChange(() => IsObject);
            }
        }

        public string JsonString
        {
            get => _jsonString;
            set
            {
                _jsonString = value;
                if (!string.IsNullOrEmpty(_mapsTo))
                {
                    TryUpdateDataListWithJsonObject(_mapsTo);
                }
                if (!string.IsNullOrEmpty(_value))
                {
                    TryUpdateDataListWithJsonObject(_value);
                }
            }
        }

        void ViewJsonObjects()
        {
            if (!string.IsNullOrEmpty(JsonString))
            {
                var window = new JsonObjectsView { Height = 280 };
                var contentPresenter = window.FindChild<TextBox>();
                if (contentPresenter != null)
                {
                    var json = JsonUtils.Format(JsonString);
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

            DisplayName = RecordSetName == string.Empty ? Name : RecordSetName + "(*)." + Name;
            ViewComplexObjectsCommand = new RelayCommand(item =>
            {
                ViewJsonObjects();
            });
        }


        #region Methods
        public IDev2Definition GetGenerationTO()
        {
            var result = DataListFactory.CreateDefinition_Recordset(Name, MapsTo, Value, RecordSetName, false, DefaultValue, Required, Value, EmptyToNull);
            result.IsObject = IsObject;
            return result;
        }


        public object Clone()
        {
            IInputOutputViewModel result = new InputOutputViewModel(Name, Value, MapsTo, DefaultValue, Required, RecordSetName, EmptyToNull);

            return result;
        }

        #endregion
    }
}
