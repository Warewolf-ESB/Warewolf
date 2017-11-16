/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;

namespace Dev2
{
    public class GatherSystemInformationTO : IDev2TOFn, IPerformsValidation
    {
        #region Fields

        private enTypeOfSystemInformationToGather _enTypeOfSystemInformation;
        private Dictionary<string, List<IActionableErrorInfo>> _errors;
        private string _result;

        #endregion

        #region Ctor

        public GatherSystemInformationTO()
        {
        }

        public GatherSystemInformationTO(enTypeOfSystemInformationToGather enTypeOfSystemInformation, string result,
            int indexNumber)
            : this(enTypeOfSystemInformation, result, indexNumber, false)
        {
        }

        public GatherSystemInformationTO(enTypeOfSystemInformationToGather enTypeOfSystemInformation, string result,
            int indexNumber, bool inserted)
        {
            Inserted = inserted;
            EnTypeOfSystemInformation = enTypeOfSystemInformation;
            Result = result;
            IndexNumber = indexNumber;
        }

        #endregion

        #region Properties
        
        public enTypeOfSystemInformationToGather EnTypeOfSystemInformation
        {
            get { return _enTypeOfSystemInformation; }
            set
            {
                _enTypeOfSystemInformation = value;
                OnPropertyChanged("EnTypeOfSystemInformation");
            }
        }
        
        [FindMissing]
        public string Result
        {
            get { return _result; }
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

        public bool Inserted { get; set; }

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

        private void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Implementation of IPerformsValidation

        public Dictionary<string, List<IActionableErrorInfo>> Errors
        {
            get { return _errors; }
            set
            {
                _errors = value;
                OnPropertyChanged("Errors");
            }
        }

        public bool Validate(string propertyName, IRuleSet ruleSet)
        {
            if (ruleSet == null)
            {
                Errors[propertyName] = new List<IActionableErrorInfo>();
            }
            else
            {
                List<IActionableErrorInfo> errorsTos = ruleSet.ValidateRules();
                List<IActionableErrorInfo> actionableErrorInfos =
                    errorsTos.ConvertAll<IActionableErrorInfo>(input => new ActionableErrorInfo(input, () =>
                    {
                        //
                    }));
                Errors[propertyName] = actionableErrorInfos;
            }
            OnPropertyChanged("Errors");
            if (Errors.TryGetValue(propertyName, out List<IActionableErrorInfo> errorList))
            {
                return errorList.Count == 0;
            }
            return false;
        }

        public bool Validate(string propertyName, string datalist)
        {
            RuleSet ruleSet = null;
            if (propertyName == "FieldName")
            {
                ruleSet = new RuleSet();
            }
            return Validate(propertyName, ruleSet);
        }

        #endregion

        #region Implementation of IDataErrorInfo
        
        public string this[string columnName] => null;
        
        public string Error { get; private set; }
        
        #endregion
    }
}