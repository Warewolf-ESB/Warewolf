/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using System.Collections.Generic;

namespace Dev2.TO
{
    public class AssignObjectDTO : ValidatedObject, IDev2TOFn
    {
        private string fieldName;
        private string fieldValue;
        private int indexNumber;
        private bool isFieldNameFocused;
        private bool isFieldValueFocused;

        public AssignObjectDTO()
            : this("[[Variable]]", "Expression", 0)
        {
        }

        public AssignObjectDTO(string fieldName, string fieldValue, int indexNumber, bool inserted = false)
        {
            this.fieldName = fieldName;
            this.fieldValue = fieldValue;
            this.indexNumber = indexNumber;
            this.Inserted = inserted;
            OutList = new List<string>();
        }

        [FindMissing]
        public string FieldName
        {
            get
            {
                return fieldName;
            }
            set
            {
                if (fieldName != value)
                {
                    fieldName = value;
                    OnPropertyChanged();
                    RaiseCanAddRemoveChanged();
                }
                //DO NOT call validation here as it will cause issues during serialization
            }
        }

        [FindMissing]
        public string FieldValue
        {
            get
            {
                return fieldValue;
            }
            set
            {
                if (fieldValue != value)
                {
                    fieldValue = value;
                    OnPropertyChanged();
                    RaiseCanAddRemoveChanged();
                }
                //DO NOT call validation here as it will cause issues during serialization
            }
        }

        public int IndexNumber
        {
            get
            {
                return indexNumber;
            }
            set
            {
                indexNumber = value;
                OnPropertyChanged();
            }
        }

        public bool IsFieldNameFocused
        {
            get
            {
                return isFieldNameFocused;
            }
            set
            {
                OnPropertyChanged(ref isFieldNameFocused, value);
            }
        }

        public bool IsFieldValueFocused
        {
            get
            {
                return isFieldValueFocused;
            }
            set
            {
                OnPropertyChanged(ref isFieldValueFocused, value);
            }
        }

        public string WatermarkTextVariable { get; set; }

        public string WatermarkTextValue { get; set; }

        public bool Inserted { get; set; }

        public List<string> OutList { get; set; }

        public OutputTO ConvertToOutputTO()
        {
            return DataListFactory.CreateOutputTO(FieldName, OutList);
        }

        public bool CanRemove()
        {
            bool result = string.IsNullOrEmpty(FieldName) && string.IsNullOrEmpty(FieldValue);
            return result;
        }


        public bool CanAdd()
        {
            bool result = !(string.IsNullOrEmpty(FieldName) && string.IsNullOrEmpty(FieldValue));
            return result;
        }

        public void ClearRow()
        {
            FieldName = string.Empty;
            FieldValue = string.Empty;
        }

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            IRuleSet ruleSet = new RuleSet();
            return ruleSet;
        }

        private void RaiseCanAddRemoveChanged()
        {
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
            // ReSharper restore ExplicitCallerInfoArgument
        }
    }
}