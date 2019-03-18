#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
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
        string _outputVariable;
        string _xPath;
        int _indexNum;
        bool _isOutputVariableFocused;
        bool _isXpathVariableFocused;

        public XPathDTO()
        {
        }

        public XPathDTO(string outputVariable, string xPath, int indexNum)
            : this(outputVariable, xPath, indexNum, false)
        {
        }

        public XPathDTO(string outputVariable, string xPath, int indexNum, bool inserted)
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
            get => _indexNum;
            set
            {
                _indexNum = value;
                OnPropertyChanged();
            }
        }

        [FindMissing]
        public string OutputVariable
        {
            get => _outputVariable;
            set
            {
                if (_outputVariable != value)
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
            get => _xPath;
            set
            {
                if (_xPath != value)
                {
                    _xPath = value;
                    OnPropertyChanged(ref _xPath, value);
                }
            }
        }

        public bool CanRemove()
        {
            if (string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(XPath))
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

        public bool IsOutputVariableFocused { get => _isOutputVariableFocused; set => OnPropertyChanged(ref _isOutputVariableFocused, value); }
        public bool IsXpathVariableFocused { get => _isXpathVariableFocused; set => OnPropertyChanged(ref _isXpathVariableFocused, value); }

        public bool IsEmpty() => string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(XPath);

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();

            if (IsEmpty())
            {
                return ruleSet;
            }

            switch (propertyName)
            {
                case "OutputVariable":
                    var outputExprRule = new IsValidExpressionRule(() => OutputVariable, datalist, "1", new VariableUtils());
                    ruleSet.Add(outputExprRule);
                    ruleSet.Add(new IsValidExpressionRule(() => outputExprRule.ExpressionValue, datalist, new VariableUtils()));

                    if (!string.IsNullOrEmpty(XPath))
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => OutputVariable));
                    }
                    break;

                case "XPath":
                    if (!string.IsNullOrEmpty(OutputVariable))
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => XPath));

                        if (!string.IsNullOrEmpty(XPath) && !DataListUtil.IsEvaluated(XPath))
                        {
                            ruleSet.Add(new IsValidXpathRule(() => XPath));
                        }
                    }
                    break;
                default:
                    Dev2Logger.Info("No Rule Set for the XPath DTO Property Name: " + propertyName, GlobalConstants.WarewolfInfo);
                    break;
            }
            return ruleSet;
        }

        public bool Equals(XPathDTO other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var eq = string.Equals(XPath, other.XPath);
            eq &= string.Equals(OutputVariable, other.OutputVariable);
            eq &= IndexNumber == other.IndexNumber;
            eq &= Inserted == other.Inserted;
            eq &= IsXpathVariableFocused == other.IsXpathVariableFocused;
            eq &= IsOutputVariableFocused == other.IsOutputVariableFocused;
            eq &= string.Equals(WatermarkTextVariable, other.WatermarkTextVariable);

            return eq;
        }

        public override bool Equals(object obj)
        {
            if (obj is XPathDTO xPathDTO)
            {
                return Equals(xPathDTO);
            }

            return false;
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
