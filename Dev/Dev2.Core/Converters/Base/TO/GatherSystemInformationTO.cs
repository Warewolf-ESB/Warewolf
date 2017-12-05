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
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using Dev2.Common;

namespace Dev2
{
    public class GatherSystemInformationTO : ValidatableObject, IDev2TOFn
    {
        enTypeOfSystemInformationToGather _enTypeOfSystemInformation;
        string _result;
        bool _isResultFocused;

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

        /// <summary>
        ///     Type of system information to gather
        /// </summary>
        public enTypeOfSystemInformationToGather EnTypeOfSystemInformation
        {
            get { return _enTypeOfSystemInformation; }
            set
            {
                _enTypeOfSystemInformation = value;
                OnPropertyChanged("EnTypeOfSystemInformation");
            }
        }


        /// <summary>
        ///     Where to place the result, will be the same as From until wizards are created
        /// </summary>
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

        private void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }


        public bool IsResultFocused { get => _isResultFocused; set => OnPropertyChanged(ref _isResultFocused, value); }

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();
            if (propertyName == "Result")
            {

                if (!string.IsNullOrEmpty(Result))
                {
                    var inputExprRule = new IsValidExpressionRule(() => Result, datalist, "0", new VariableUtils());
                    ruleSet.Add(inputExprRule);
                }
                else
                {
                    ruleSet.Add(new IsStringEmptyRule(() => Result));
                }

            }
            return ruleSet;
        }



    }
}