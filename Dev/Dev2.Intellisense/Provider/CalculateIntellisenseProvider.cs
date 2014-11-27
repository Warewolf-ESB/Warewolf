
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Parsing.Intellisense;
using System.Windows.Data;
using Dev2.Calculate;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Intellisense.Provider;
using Dev2.MathOperations;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.InterfaceImplementors
{
    public sealed class CalculateIntellisenseProvider : IIntellisenseProvider
    {
        readonly ISyntaxTreeBuilderHelper _syntaxTreeBuilderHelper;

        #region Static Members
        private static HashSet<string> _functionNames;
        private static readonly IList<IntellisenseProviderResult> EmptyResults = new List<IntellisenseProviderResult>();
        #endregion

        #region Instance Fields

        public IList<IntellisenseProviderResult> IntellisenseResult
        {
            get;
            private set;
        }

        #endregion

        #region Public Properties
        public bool Optional
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public bool HandlesResultInsertion
        {
            get
            {
                return false;
            }
            set
            {
            }
        }
        #endregion

        #region Constructors

        public CalculateIntellisenseProvider() : this(new SyntaxTreeBuilderHelper()) { }

        public CalculateIntellisenseProvider(ISyntaxTreeBuilderHelper syntaxTreeBuilderHelper)
        {
            _syntaxTreeBuilderHelper = syntaxTreeBuilderHelper;
            IntellisenseProviderType = IntellisenseProviderType.NonDefault;
            IFrameworkRepository<IFunction> functionList = MathOpsFactory.FunctionRepository();
            functionList.Load();
            bool creatingFunctions = false;

            if(_functionNames == null)
            {
                creatingFunctions = true;
                _functionNames = new HashSet<string>(StringComparer.Ordinal);
            }

            IntellisenseResult = functionList.All().Select(currentFunction =>
            {
                string description = currentFunction.Description;
                string dropDownDescription = description;
                if(description != null && description.Length > 80) dropDownDescription = description.Substring(0, 77) + "...";
                if(creatingFunctions) _functionNames.Add(currentFunction.FunctionName);
                IntellisenseProviderResult result = new IntellisenseProviderResult(this, currentFunction.FunctionName, dropDownDescription, description, currentFunction.arguments != null ? currentFunction.arguments.ToArray() : new string[0], currentFunction.ArgumentDescriptions != null ? currentFunction.ArgumentDescriptions.ToArray() : new string[0]);
                return result;
            }).OrderBy(p => p.Name).ToList();
        }
        #endregion

        public IntellisenseProviderType IntellisenseProviderType { get; private set; }

        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            throw new NotSupportedException();
        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            if(context == null)
            {
                return new List<IntellisenseProviderResult>();
            }

            var caretPosition = context.CaretPosition;
            var inputText = context.InputText ?? string.Empty;
            var parseEventLog = _syntaxTreeBuilderHelper.EventLog;
            var intellisenseDesiredResultSet = context.DesiredResultSet;

            if((caretPosition == 0 || string.IsNullOrEmpty(inputText))
                && intellisenseDesiredResultSet != IntellisenseDesiredResultSet.EntireSet)
            {
                return EmptyResults;
            }

            if(context.IsInCalculateMode)
            {
                if(intellisenseDesiredResultSet == IntellisenseDesiredResultSet.EntireSet)
                {
                    if(parseEventLog != null) parseEventLog.Clear();

                    if(_syntaxTreeBuilderHelper.EventLog != null && _syntaxTreeBuilderHelper.HasEventLogs)
                    {
                        List<IntellisenseProviderResult> tResults = new List<IntellisenseProviderResult>();
                        tResults.AddRange(IntellisenseResult);
                        return EvaluateEventLogs(tResults, inputText);
                    }

                    return IntellisenseResult;
                }

                Token[] tokens;
                if(intellisenseDesiredResultSet == IntellisenseDesiredResultSet.ClosestMatch)
                {
                    var searchText = context.FindTextToSearch();
                    _syntaxTreeBuilderHelper.Build(searchText, true, out tokens);
                    string sub = string.IsNullOrEmpty(searchText) ? inputText : searchText;

                    List<IntellisenseProviderResult> subResults = IntellisenseResult.Where(t => t.Name.StartsWith(sub)).ToList();

                    if(_syntaxTreeBuilderHelper.EventLog != null && _syntaxTreeBuilderHelper.HasEventLogs)
                    {
                        return EvaluateEventLogs(subResults, inputText);
                    }

                    return subResults;
                }

                _syntaxTreeBuilderHelper.Build(inputText, false, out tokens);

                if(_syntaxTreeBuilderHelper.HasEventLogs)
                {
                    return EvaluateEventLogs(inputText);
                }
            }

            return EmptyResults;
        }

        private IList<IntellisenseProviderResult> EvaluateEventLogs(IList<IntellisenseProviderResult> errors, string expression)
        {
            var parseEventLog = _syntaxTreeBuilderHelper.EventLog;
            parseEventLog.Clear();
            errors.Add(new IntellisenseProviderResult(this, "Syntax Error", null, "An error occurred while parsing { " + expression + " } It appears to be malformed", true, 0, expression.Length));
            return errors;
        }

        private IList<IntellisenseProviderResult> EvaluateEventLogs(string expression)
        {
            IList<IntellisenseProviderResult> errors = new List<IntellisenseProviderResult>();
            var parseEventLog = _syntaxTreeBuilderHelper.EventLog;
            parseEventLog.Clear();
            errors.Add(new IntellisenseProviderResult(this, "Syntax Error", null, "An error occurred while parsing { " + expression + " } It appears to be malformed", true, 0, expression.Length));
            return errors;
        }

        public void Dispose()
        {
            IntellisenseResult = null;
        }
    }

    #region CalculateIntellisenseTextConverter
    [ValueConversion(typeof(string), typeof(string), ParameterType = typeof(string))]
    public class CalculateIntellisenseTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null)
            {
                string text = (string)value;
                bool allowUserCalculateMode = (string)parameter == "True";

                if(allowUserCalculateMode && text.Length > 0)
                {
                    if(text.StartsWith(GlobalConstants.CalculateTextConvertPrefix))
                    {
                        if(text.EndsWith(GlobalConstants.CalculateTextConvertSuffix))
                        {
                            text = "=" + text.Substring(GlobalConstants.CalculateTextConvertPrefix.Length, text.Length - (GlobalConstants.CalculateTextConvertSuffix.Length + GlobalConstants.CalculateTextConvertPrefix.Length));
                        }
                    }
                }

                return text;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null)
            {
                string text = (string)value;
                bool allowUserCalculateMode = (string)parameter == "True";

                if(allowUserCalculateMode && text.Length > 0)
                {
                    if(text[0] == '=')
                    {
                        text = String.Format(GlobalConstants.CalculateTextConvertFormat, text.Substring(1));
                    }
                }

                return text;
            }

            return null;
        }
    }
    #endregion
}
