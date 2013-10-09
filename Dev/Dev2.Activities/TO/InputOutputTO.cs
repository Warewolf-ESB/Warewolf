using System.Collections.Generic;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Providers.Validation.Rules;

namespace Dev2.TO
{
    public class SqlBulkInsertTO : ObservableObject, IDev2TOFn, IPerformsValidation
    {
        string _inputData;
        string _outputColumn;
        string _outputColumnType;
        int _indexNumber;

        public string InputData { get { return _inputData; } set { OnPropertyChanged(ref _inputData, value); } }

        public string OutputColumn { get { return _outputColumn; } set { OnPropertyChanged(ref _outputColumn, value); } }

        public string OutputColumnType { get { return _outputColumnType; } set { OnPropertyChanged(ref _outputColumnType, value); } }

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
