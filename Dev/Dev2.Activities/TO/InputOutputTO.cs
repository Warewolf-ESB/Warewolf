using System.Collections.Generic;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Providers.Validation.Rules;

namespace Dev2.TO
{
    public class InputOutputTO : ObservableObject, IInputOutputViewModel, IDev2TOFn, IPerformsValidation
    {
        string _name;
        bool _isSelected;
        string _value;
        string _mapsTo;
        string _defaultValue;
        bool _required;
        string _recordSetName;
        string _displayName;
        string _displayDefaultValue;
        bool _isNew;
        bool _requiredMissing;
        int _indexNumber;
        string _typeName;

        #region Implementation of IInputOutputViewModel

        public string Name { get { return _name; } set { OnPropertyChanged(ref _name, value); } }

        public bool IsSelected { get { return _isSelected; } set { OnPropertyChanged(ref _isSelected, value); } }

        public string Value { get { return _value; } set { OnPropertyChanged(ref _value, value); } }

        public string MapsTo { get { return _mapsTo; } set { OnPropertyChanged(ref _mapsTo, value); } }

        public string DefaultValue { get { return _defaultValue; } set { OnPropertyChanged(ref _defaultValue, value); } }

        public bool Required { get { return _required; } set { OnPropertyChanged(ref _required, value); } }

        public string RecordSetName { get { return _recordSetName; } set { OnPropertyChanged(ref _recordSetName, value); } }

        public string DisplayName { get { return _displayName; } set { OnPropertyChanged(ref _displayName, value); } }

        public string DisplayDefaultValue { get { return _displayDefaultValue; } private set { OnPropertyChanged(ref _displayDefaultValue, value); } }

        public bool IsNew { get { return _isNew; } set { OnPropertyChanged(ref _isNew, value); } }

        public bool RequiredMissing { get { return _requiredMissing; } set { OnPropertyChanged(ref _requiredMissing, value); } }

        public string TypeName { get { return _typeName; } set { OnPropertyChanged(ref _typeName, value); } }

        public IDev2Definition GetGenerationTO()
        {
            return null;
        }

        #endregion

        #region Implementation of IDev2TOFn

        public int IndexNumber { get { return _indexNumber; } set { OnPropertyChanged(ref _indexNumber, value); } }

        public bool CanRemove()
        {
            return false;
        }

        public bool CanAdd()
        {
            return false;
        }

        public void ClearRow()
        {
        }

        public bool Inserted { get; set; }

        #endregion

        #region Implementation of IDataErrorInfo

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        public string this[string columnName] { get { return null; } }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error { get; private set; }

        #endregion

        #region Implementation of IPerformsValidation

        public Dictionary<string, List<IActionableErrorInfo>> Errors { get; set; }

        public bool Validate(string propertyName, RuleSet ruleSet)
        {
            return false;
        }

        public bool Validate(string propertyName)
        {
            return false;
        }

        #endregion
    }
}
