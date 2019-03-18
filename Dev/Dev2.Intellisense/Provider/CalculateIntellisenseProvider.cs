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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Parsing.Intellisense;
using System.Windows.Data;
using Dev2.Calculate;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.MathOperations;
using Dev2.Intellisense.Provider;
using Dev2.MathOperations;
using Dev2.Studio.Interfaces;
using Warewolf.Resource.Errors;


namespace Dev2.Studio.InterfaceImplementors
{
    public sealed class CalculateIntellisenseProvider : IIntellisenseProvider
    {
        readonly ISyntaxTreeBuilderHelper _syntaxTreeBuilderHelper;

        #region Static Members
        static HashSet<string> _functionNames = new HashSet<string>(StringComparer.Ordinal);
        static readonly IList<IntellisenseProviderResult> EmptyResults = new List<IntellisenseProviderResult>();
        #endregion

        #region Instance Fields

        public IList<IntellisenseProviderResult> IntellisenseResult
        {
            get;
            private set;
        }

        #endregion

        #region Public Properties
        public bool Optional => false;

        public bool HandlesResultInsertion => false;
        #endregion

        #region Constructors

        public CalculateIntellisenseProvider() : this(new SyntaxTreeBuilderHelper()) { }

        public CalculateIntellisenseProvider(ISyntaxTreeBuilderHelper syntaxTreeBuilderHelper)
        {
            _syntaxTreeBuilderHelper = syntaxTreeBuilderHelper;
            IntellisenseProviderType = IntellisenseProviderType.NonDefault;
            var functionList = MathOpsFactory.FunctionRepository();
            functionList.Load();
            IntellisenseResult = functionList.All().Select(currentFunction =>
            {
                var description = currentFunction.Description;
                var dropDownDescription = description;
                if (description != null && description.Length > 80)
                {
                    dropDownDescription = description.Substring(0, 77) + "...";
                }
                _functionNames.Add(currentFunction.FunctionName);

                var result = new IntellisenseProviderResult(this, currentFunction.FunctionName, dropDownDescription, description, currentFunction.arguments?.ToArray() ?? new string[0], currentFunction.ArgumentDescriptions?.ToArray() ?? new string[0]);
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

            if(context.IsInCalculateMode)
            {
                if(intellisenseDesiredResultSet == IntellisenseDesiredResultSet.EntireSet && (caretPosition == 0 || string.IsNullOrEmpty(inputText)))
                {
                    parseEventLog?.Clear();

                    if(_syntaxTreeBuilderHelper.EventLog != null && _syntaxTreeBuilderHelper.HasEventLogs)
                    {
                        var tResults = new List<IntellisenseProviderResult>();
                        tResults.AddRange(IntellisenseResult);
                        return EvaluateEventLogs(tResults, inputText);
                    }

                    return IntellisenseResult;
                }

                var searchText = context.FindTextToSearch();
                _syntaxTreeBuilderHelper.Build(searchText, true, out Token[] tokens);
                var sub = string.IsNullOrEmpty(searchText) ? inputText : searchText;

                var subResults = IntellisenseResult.Where(t => t.Name.StartsWith(sub)).ToList();

                return subResults;
            }

            return EmptyResults;
        }

        IList<IntellisenseProviderResult> EvaluateEventLogs(IList<IntellisenseProviderResult> errors, string expression)
        {
            var parseEventLog = _syntaxTreeBuilderHelper.EventLog;
            parseEventLog.Clear();
            errors.Add(new IntellisenseProviderResult(this, "Syntax Error", null, string.Format(ErrorResource.MalformedExpression, expression), true, 0, expression.Length));
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
                var text = (string)value;
                var allowUserCalculateMode = (string)parameter == "True";

                if (allowUserCalculateMode && text.Length > 0 && text.StartsWith(GlobalConstants.CalculateTextConvertPrefix) && text.EndsWith(GlobalConstants.CalculateTextConvertSuffix))
                {
                    text = "=" + text.Substring(GlobalConstants.CalculateTextConvertPrefix.Length, text.Length - (GlobalConstants.CalculateTextConvertSuffix.Length + GlobalConstants.CalculateTextConvertPrefix.Length));
                }



                return text;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null)
            {
                var text = (string)value;
                var allowUserCalculateMode = (string)parameter == "True";

                if (allowUserCalculateMode && text.Length > 0 && text[0] == '=')
                {
                    text = String.Format(GlobalConstants.CalculateTextConvertFormat, text.Substring(1));
                }


                return text;
            }

            return null;
        }
    }
    #endregion
}
