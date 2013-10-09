using System;
using System.Collections.Generic;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Providers.Validation.Rules;

namespace Dev2.TO
{
    public class DataColumnMapping : ObservableObject, IDev2TOFn, IPerformsValidation
    {
        int _indexNumber;
        string _inputColumn;
        string _outputColumnName;
        Type _outputColumnDataType;
        int _outputColumnMaxLength;

        public string InputColumn { get { return _inputColumn; } set { OnPropertyChanged(ref _inputColumn, value); } }

        public string OutputColumnName { get { return _outputColumnName; } set { OnPropertyChanged(ref _outputColumnName, value); } }

        public Type OutputColumnDataType { get { return _outputColumnDataType; } set { OnPropertyChanged(ref _outputColumnDataType, value); } }

        public int OutputColumnMaxLength { get { return _outputColumnMaxLength; } set { OnPropertyChanged(ref _outputColumnMaxLength, value); } }

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
