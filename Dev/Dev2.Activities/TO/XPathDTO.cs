
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using Dev2.Util;
using Dev2.Validation;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class XPathDTO : ValidatedObject, IDev2TOFn
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
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
            // ReSharper restore ExplicitCallerInfoArgument
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
            }
            return ruleSet;
        }
    }
}
