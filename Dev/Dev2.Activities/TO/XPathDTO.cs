/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Data.Util;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using Dev2.Util;
using Dev2.Validation;
using System;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class XPathDTO : ValidatedObject, IDev2TOFn, IEquatable<XPathDTO>
    {
        private string _outputVariable;
        private string _xPath;
        private int _indexNum;
        bool _isOutputVariableFocused;
        bool _isXpathVariableFocused;

        public XPathDTO()
        {

        }

        public XPathDTO(string outputVariable, string xPath, int indexNum, bool inserted = false)
        {
            Inserted = inserted;
            OutputVariable = outputVariable;
            XPath = xPath;
            IndexNumber = indexNum;
        }

        public string WatermarkTextVariable { get; set; }

        void RaiseCanAddRemoveChanged()
        {
            
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
            
        }

        public int IndexNumber
        {
            get
            {
                return _indexNum;
            }
            set
            {
                _indexNum = value;
                OnPropertyChanged();
            }
        }

        [FindMissing]
        public string OutputVariable
        {
            get
            {
                return _outputVariable;
            }
            set
            {
                if(_outputVariable != value)
                {
                    _outputVariable = value;
                    OnPropertyChanged(ref _outputVariable, value);
                    RaiseCanAddRemoveChanged();
                }
            }
        }

        [FindMissing]
        public string XPath
        {
            get
            {
                return _xPath;
            }
            set
            {
                if(_xPath != value)
                {
                    _xPath = value;
                    OnPropertyChanged(ref _xPath, value);
                }
            }
        }

        public bool CanRemove()
        {
            if(string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(XPath))
            {
                return true;
            }
            return false;
        }

        public bool CanAdd()
        {
            var result = !string.IsNullOrEmpty(OutputVariable);
            return result;
        }

        public void ClearRow()
        {
            OutputVariable = string.Empty;
            XPath = "";
        }

        public bool Inserted { get; set; }

        public bool IsOutputVariableFocused { get { return _isOutputVariableFocused; } set { OnPropertyChanged(ref _isOutputVariableFocused, value); } }
        public bool IsXpathVariableFocused { get { return _isXpathVariableFocused; } set { OnPropertyChanged(ref _isXpathVariableFocused, value); } }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(XPath);
        }

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();

            if(IsEmpty())
            {
                return ruleSet;
            }

            switch(propertyName)
            {
                case "OutputVariable":
                    var outputExprRule = new IsValidExpressionRule(() => OutputVariable,datalist, "1");
                    ruleSet.Add(outputExprRule);
                    ruleSet.Add(new IsValidExpressionRule(() => outputExprRule.ExpressionValue, datalist));

                    if(!string.IsNullOrEmpty(XPath))
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => OutputVariable));
                    }
                    break;

                case "XPath":
                    if(!string.IsNullOrEmpty(OutputVariable))
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => XPath));

                        if(!string.IsNullOrEmpty(XPath) && !DataListUtil.IsEvaluated(XPath))
                        {
                            ruleSet.Add(new IsValidXpathRule(() => XPath));
                        }
                    }
                    break;
                default:
                    throw new ArgumentException("Unrecognized property name: " + propertyName);
            }
            return ruleSet;
        }

        public bool Equals(XPathDTO other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(XPath, other.XPath)
                && string.Equals(OutputVariable, other.OutputVariable)
                && IndexNumber == other.IndexNumber
                && Inserted == other.Inserted
                && IsXpathVariableFocused == other.IsXpathVariableFocused
                && IsOutputVariableFocused == other.IsOutputVariableFocused
                && string.Equals(WatermarkTextVariable, other.WatermarkTextVariable);                
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ActivityDTO)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (XPath != null ? XPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutputVariable != null ? OutputVariable.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IndexNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ Inserted.GetHashCode();
                hashCode = (hashCode * 397) ^ IsXpathVariableFocused.GetHashCode();
                hashCode = (hashCode * 397) ^ IsOutputVariableFocused.GetHashCode();
                hashCode = (hashCode * 397) ^ (WatermarkTextVariable != null ? WatermarkTextVariable.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
