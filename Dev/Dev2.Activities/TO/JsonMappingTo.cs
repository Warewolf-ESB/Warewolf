
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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

                if (String.IsNullOrEmpty(DestinationName))
                {
                    if (DataListUtil.IsFullyEvaluated(_sourceName))
                    {
                        string destName;
                        if (DataListUtil.IsValueRecordset(value) || DataListUtil.IsValueRecordsetWithFields(value))
                        {
                            destName = DataListUtil.ExtractRecordsetNameFromValue(_sourceName);
                        }
                        else
                        {
                            destName = DataListUtil.StripBracketsFromValue(_sourceName);
                        }
                        DestinationName = destName;
                    }
                    RaiseCanAddRemoveChanged();
                }                
            }
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
        public int IndexNumber { get { return _indexNumber; } set { OnPropertyChanged(ref _indexNumber, value); } }

        public JsonMappingTo()
        {
        }

        public JsonMappingTo(string sourceName, int indexNumber, bool inserted)
        {
            SourceName = sourceName;
            _indexNumber = indexNumber;
            Inserted = inserted;
        }

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
                ruleSet.Add(new IsValidJsonCreateMappingSourceExpression(() => SourceName));
            return ruleSet;
        }
    }
}
