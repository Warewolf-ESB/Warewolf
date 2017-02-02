/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using Dev2.Validation;

namespace Dev2.TO
{
    public class JsonMappingTo : ValidatedObject, IDev2TOFn
    {
        string _sourceName, _destinationName;

        [FindMissing]
        public string SourceName
        {
            get { return _sourceName; }
            set
            {
                OnPropertyChanged(ref _sourceName, value);
                RaiseCanAddRemoveChanged();
            }
        }

        // ReSharper disable MemberCanBePrivate.Global
        public string GetDestinationWithName(string sourceName)
            // ReSharper restore MemberCanBePrivate.Global
        {
            string destName = null;
            if (DataListUtil.IsFullyEvaluated(sourceName))
            {

                if (DataListUtil.IsValueRecordset(sourceName) || DataListUtil.IsValueRecordsetWithFields(sourceName))
                {
                    destName = DataListUtil.ExtractRecordsetNameFromValue(sourceName);
                }
                else
                {
                    destName = DataListUtil.StripBracketsFromValue(sourceName);
                }
            }
            return destName;
        }

        public string DestinationName
        {
            get
            {
                return _destinationName;
            }
            set
            {
                OnPropertyChanged(ref _destinationName, value);
                RaiseCanAddRemoveChanged();
            }
        }

        #region Implementation of IDev2TOFn
        int _indexNumber;
        bool _isSourceNameFocused;
        bool _isDestinationNameFocused;
        public int IndexNumber { get { return _indexNumber; } set { OnPropertyChanged(ref _indexNumber, value); } }

        public JsonMappingTo()
        {
        }

        public JsonMappingTo(string sourceName, int indexNumber, bool inserted)
        {
            SourceName = sourceName;
            _indexNumber = indexNumber;
            Inserted = inserted;
            DestinationName = GetDestinationWithName(SourceName);
        }

        // ReSharper disable UnusedMember.Global
        public bool IsSourceNameFocused { get { return _isSourceNameFocused; } set { OnPropertyChanged(ref _isSourceNameFocused, value); } }


        public bool IsDestinationNameFocused { get { return _isDestinationNameFocused; } set { OnPropertyChanged(ref _isDestinationNameFocused, value); } }
        // ReSharper restore UnusedMember.Global
        public bool CanRemove()
        {
            return string.IsNullOrEmpty(DestinationName) && string.IsNullOrEmpty(SourceName);
        }

        public bool CanAdd()
        {
            return !CanRemove();
        }

        public void ClearRow()
        {
            SourceName = string.Empty;
            DestinationName = string.Empty;
        }

        void RaiseCanAddRemoveChanged()
        {
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
            // ReSharper restore ExplicitCallerInfoArgument
        }
        public bool Inserted { get; set; }

        #endregion

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            RuleSet ruleSet = new RuleSet();
            if (String.IsNullOrEmpty( SourceName))
            {
                return ruleSet;
            }
            if(propertyName == "SourceName")
            {
                ruleSet.Add(new IsValidJsonCreateMappingSourceExpression(() => SourceName));
            }
            if(propertyName == "DestinationName")
            {
                ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(()=>DestinationName));
                ruleSet.Add(new ShouldNotBeVariableRule(()=>DestinationName));
            }
            return ruleSet;
        }
    }
}
