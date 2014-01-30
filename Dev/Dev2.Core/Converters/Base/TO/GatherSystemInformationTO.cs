using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Data.Enums;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;

namespace Dev2
{
    public class GatherSystemInformationTO : IDev2TOFn, IPerformsValidation
    {
        #region Fields

        enTypeOfSystemInformationToGather _enTypeOfSystemInformation;
        string _result;
        Dictionary<string, List<IActionableErrorInfo>> _errors;

        #endregion

        #region Ctor

        public GatherSystemInformationTO()
        {

        }

        public GatherSystemInformationTO(enTypeOfSystemInformationToGather enTypeOfSystemInformation, string result, int indexNumber,bool inserted = false)
        {
            Inserted = inserted;
            EnTypeOfSystemInformation = enTypeOfSystemInformation;
            Result = result;
            IndexNumber = indexNumber;
        }

        #endregion

        #region Properties

        public bool Inserted { get; set; }
        
        /// <summary>
        /// Type of system information to gather
        /// </summary>       
        public enTypeOfSystemInformationToGather EnTypeOfSystemInformation
        {
            get
            {
                return _enTypeOfSystemInformation;
            }
            set
            {
                _enTypeOfSystemInformation = value;
                OnPropertyChanged("EnTypeOfSystemInformation");
            }
        }


        /// <summary>
        /// Where to place the result, will be the same as From until wizards are created
        /// </summary>
        [FindMissing]
        public string Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
                OnPropertyChanged("Result");
                RaiseCanAddRemoveChanged();
            }
        }

        public IList<string> Expressions { get; set; }

        public string WatermarkTextVariable { get; set; }

        public string WatermarkText { get; set; }

        public int IndexNumber { get; set; }

        #endregion

        #region Public Methods

        public bool CanRemove()
        {
            return string.IsNullOrWhiteSpace(Result);            
        }

        public bool CanAdd()
        {
            return !string.IsNullOrWhiteSpace(Result);            
        }

        public void ClearRow()
        {
            Result = "";
        }        

        #endregion

        #region Private Methods
        void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }
        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Implementation of IPerformsValidation

        public Dictionary<string, List<IActionableErrorInfo>> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
                OnPropertyChanged("Errors");
            }
        }

        public bool Validate(string propertyName, RuleSet ruleSet)
        {
            if (ruleSet == null)
            {
                Errors[propertyName] = new List<IActionableErrorInfo>();
            }
            else
            {
                var errorsTos = ruleSet.ValidateRules();
                var actionableErrorInfos = errorsTos.ConvertAll<IActionableErrorInfo>(input => new ActionableErrorInfo(input, () =>
                {
                    //
                }));
                Errors[propertyName] = actionableErrorInfos;
            }
            OnPropertyChanged("Errors");
            List<IActionableErrorInfo> errorList;
            if (Errors.TryGetValue(propertyName, out errorList))
            {
                return errorList.Count == 0;
            }
            return false;
        }

        public bool Validate(string propertyName)
        {
            RuleSet ruleSet = null;
            switch (propertyName)
            {
                case "FieldName":
                    ruleSet = GetFieldNameRuleSet();
                    break;
                case "FieldValue":
                    break;
            }
            return Validate(propertyName, ruleSet);
        }

        public void Validate()
        {
        }

        RuleSet GetFieldNameRuleSet()
        {
            var ruleSet = new RuleSet();
            return ruleSet;
        }
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
    }
}