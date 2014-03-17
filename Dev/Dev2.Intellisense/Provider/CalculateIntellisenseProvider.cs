using Dev2.Common;
using Dev2.MathOperations;
using Dev2.Studio.Core.Interfaces;
using Infragistics.Calculations.CalcManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Parsing;
using System.Parsing.Intellisense;
using System.Text;
using System.Windows.Data;

namespace Dev2.Studio.InterfaceImplementors
{
    public sealed class CalculateIntellisenseProvider : IIntellisenseProvider
    {
        #region Static Members
        private static HashSet<string> _functionNames;
        //private static IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        //private static string _emptyADL = "<ADL></ADL>";
        private static IList<IntellisenseProviderResult> _emptyResults = new List<IntellisenseProviderResult>();
        #endregion

        #region Instance Fields
        private IList<IntellisenseProviderResult> _intellisenseResult;
        private readonly List<IntellisenseProviderResult> _errors = new List<IntellisenseProviderResult>();
        private readonly SyntaxTreeBuilder _builder = new SyntaxTreeBuilder();
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
        public CalculateIntellisenseProvider()
        {
            IFrameworkRepository<IFunction> functionList = MathOpsFactory.FunctionRepository();
            functionList.Load();
            bool creatingFunctions = false;

            if(_functionNames == null)
            {
                creatingFunctions = true;
                _functionNames = new HashSet<string>(StringComparer.Ordinal);
            }

            _intellisenseResult = functionList.All().Select(currentFunction =>
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

        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            throw new NotSupportedException();
        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            if(context.IsInCalculateMode)
            {
                Token[] tokens;
                Node[] nodes;

                if(context.DesiredResultSet == IntellisenseDesiredResultSet.EntireSet)
                {
                    if(_builder.EventLog != null) _builder.EventLog.Clear();
                    nodes = _builder.Build(context.InputText, out tokens);

                    if(_builder.EventLog.HasEventLogs)
                    {
                        List<IntellisenseProviderResult> tResults = new List<IntellisenseProviderResult>();
                        tResults.AddRange(_intellisenseResult);
                        return EvaluateEventLogs(tResults, _builder.EventLog.GetEventLogs(), context.InputText);
                    }

                    return _intellisenseResult;
                }
                else if(context.DesiredResultSet == IntellisenseDesiredResultSet.ClosestMatch)
                {
                    int start = -1;
                    int length = 0;

                    for(int i = context.CaretPosition - 1; i >= 0; i--)
                    {
                        if(Char.IsWhiteSpace(context.InputText[i])) break;
                        if(!Char.IsLetter(context.InputText[i])) break;
                        start = i;
                        length++;
                    }

                    if(start != -1)
                    {
                        if(start - 1 >= 0)
                        {
                            if(_builder.EventLog != null) _builder.EventLog.Clear();
                            nodes = _builder.Build(context.InputText, out tokens);

                            if(tokens == null && _builder.EventLog.HasEventLogs)
                            {
                                ParseEventLogEntry[] examineEntries = _builder.EventLog.GetEventLogs();

                                if(examineEntries != null && examineEntries.Length > 0)
                                {
                                    if(examineEntries[examineEntries.Length - 1].Component == "StringLiteralTokenizationHandler")
                                    {
                                        _builder.EventLog.Clear();
                                        _builder.Build(context.InputText, true, out tokens);
                                        return EvaluateEventLogs(EvaluateMethodContext(context, false, tokens), examineEntries, context.InputText);
                                    }
                                }
                            }
                        }

                        string sub = context.InputText.Substring(start, length);
                        List<IntellisenseProviderResult> subResults = new List<IntellisenseProviderResult>();

                        for(int i = 0; i < _intellisenseResult.Count; i++)
                            if(_intellisenseResult[i].Name.StartsWith(sub))
                                subResults.Add(_intellisenseResult[i]);

                        if(_builder.EventLog != null && _builder.EventLog.HasEventLogs && subResults.Count == 0) return EvaluateEventLogs(_builder.EventLog.GetEventLogs(), context.InputText);

                        if(_builder.EventLog != null && _builder.EventLog.HasEventLogs)
                        {
                            _builder.EventLog.Clear(); 
                            subResults.Add(new IntellisenseProviderResult(this, "Syntax Error", null, "An error occurred while parsing { " + context.InputText + " } It appears to be malformed", true, 0, context.InputText.Length)); //Bug 6733
                        }

                        return subResults;
                    }
                }

                if(_builder.EventLog != null) _builder.EventLog.Clear();
                nodes = _builder.Build(context.InputText, out tokens);

                if(_builder.EventLog.HasEventLogs)
                {
                    ParseEventLogEntry[] examineEntries = _builder.EventLog.GetEventLogs();
                    if(tokens == null) return EvaluateMethodContext(context, true, tokens);
                    else
                    {
                        _builder.EventLog.Clear();
                        return EvaluateEventLogs(EvaluateMethodContext(context, false, tokens), examineEntries, context.InputText);
                    }
                }

                if(nodes != null && nodes.Length == 1)
                {
                    bool hasError = false;

                    List<Node> allNodes = new List<Node>();
                    nodes[0].CollectNodes(allNodes);

                    for(int i = 0; i < allNodes.Count; i++)
                    {
                        if(allNodes[i] is MethodInvocationNode)
                        {
                            string methodName = allNodes[i].Identifier.Content;

                            if(!_functionNames.Contains(methodName))
                            {
                                if(!hasError)
                                {
                                    hasError = true;
                                    _errors.Clear();
                                }

                                _errors.Add(new IntellisenseProviderResult(this, allNodes[i].Identifier.Content, null, "An error occurred while parsing { " + methodName + " }, the function does not exist.", true, allNodes[i].Identifier.Start.SourceIndex, allNodes[i].Identifier.End.SourceIndex + allNodes[i].Identifier.End.SourceLength));
                            }
                            else
                            {
                                if(!hasError)
                                {
                                    _errors.Clear();
                                }

                                MethodInvocationNode mNode = allNodes[i] as MethodInvocationNode;
                                if(!VerifyMethodInvocationArguments(mNode)) hasError = true;
                            }
                        }
                        else if(allNodes[i] is BinaryOperatorNode && allNodes[i].Identifier.Start.Definition == TokenKind.Colon)
                        {
                            BinaryOperatorNode biNode = (BinaryOperatorNode)allNodes[i];
                            bool escape = false;

                            if(!(biNode.Left is DatalistRecordSetFieldNode))
                            {
                                if(!hasError)
                                {
                                    hasError = true;
                                    _errors.Clear();
                                }

                                escape = true;
                                _errors.Add(new IntellisenseProviderResult(this, biNode.Left.Declaration.Content, null, "An error occurred while parsing { " + context.InputText + " } Range operator can only be used with record set fields.", true, biNode.Left.Identifier.Start.SourceIndex, biNode.Left.Identifier.End.SourceIndex + biNode.Left.Identifier.End.SourceLength));
                            }

                            if(!(biNode.Right is DatalistRecordSetFieldNode))
                            {
                                if(!hasError)
                                {
                                    hasError = true;
                                    _errors.Clear();
                                }

                                escape = true;
                                _errors.Add(new IntellisenseProviderResult(this, biNode.Right.Declaration.Content, null, "An error occurred while parsing { " + context.InputText + " } Range operator can only be used with record set fields.", true, biNode.Right.Identifier.Start.SourceIndex, biNode.Right.Identifier.End.SourceIndex + biNode.Right.Identifier.End.SourceLength));
                            }

                            if(!escape)
                            {
                                DatalistRecordSetFieldNode fieldLeft = (DatalistRecordSetFieldNode)biNode.Left;
                                DatalistRecordSetFieldNode fieldRight = (DatalistRecordSetFieldNode)biNode.Right;

                                if(fieldLeft.Field == null && fieldRight.Field == null)
                                {
                                    string evaluateFieldLeft = fieldLeft.Identifier.Content;
                                    string evaluateFieldRight = fieldRight.Identifier.Content;

                                    if(!String.Equals(evaluateFieldLeft, evaluateFieldRight, StringComparison.Ordinal))
                                    {
                                        if(!hasError)
                                        {
                                            hasError = true;
                                            _errors.Clear();
                                        }

                                        _errors.Add(new IntellisenseProviderResult(this, biNode.Declaration.Content, null, "An error occurred while parsing { " + context.InputText + " } Range operator must be used with the same record set fields.", true, biNode.Declaration.Start.SourceIndex, biNode.Declaration.End.SourceIndex + biNode.Declaration.End.SourceLength));
                                    }
                                }

                                if(fieldLeft.RecordSet.NestedIdentifier == null && fieldRight.RecordSet.NestedIdentifier == null)
                                {

                                    string evaluateRecordLeft = fieldLeft.RecordSet.GetRepresentationForEvaluation();
                                    evaluateRecordLeft = evaluateRecordLeft.Substring(2, evaluateRecordLeft.IndexOf('(') - 2);
                                    string evaluateRecordRight = fieldRight.RecordSet.GetRepresentationForEvaluation();
                                    evaluateRecordRight = evaluateRecordRight.Substring(2, evaluateRecordRight.IndexOf('(') - 2);

                                    if(!String.Equals(evaluateRecordLeft, evaluateRecordRight, StringComparison.Ordinal))
                                    {
                                        if(!hasError)
                                        {
                                            hasError = true;
                                            _errors.Clear();
                                        }

                                        _errors.Add(new IntellisenseProviderResult(this, biNode.Declaration.Content, null, "An error occurred while parsing { " + context.InputText + " } Range operator must be used with the same record sets.", true, biNode.Declaration.Start.SourceIndex, biNode.Declaration.End.SourceIndex + biNode.Declaration.End.SourceLength));
                                    }
                                }
                            }
                        }
                    }

                    if(hasError) return _errors;
                }

                _errors.Clear();
                return _intellisenseResult;
            }

                return _emptyResults;
            }

        private bool VerifyMethodInvocationArguments(MethodInvocationNode mNode)
        {
            int paramCount = 0;

            if(mNode != null && mNode.Parameters != null && mNode.Parameters.Items != null)
            {
                paramCount = mNode.Parameters.Items.Length;
            }

            string methodName = mNode.Identifier.Content;
            IntellisenseProviderResult templateResult = null;

            for(int i = 0; i < _intellisenseResult.Count; i++)
            {
                if(String.Equals(methodName, _intellisenseResult[i].Name, StringComparison.Ordinal))
                {
                        templateResult = _intellisenseResult[i];
                        break;
                }
            }

            bool result = true;

            if(templateResult == null)
            {
                _errors.Add(new IntellisenseProviderResult(this, mNode.Identifier.Content, null, "An error occured while parsing { " + methodName + " }, the function does not exist.", true, mNode.Identifier.Start.SourceIndex, mNode.Identifier.End.SourceIndex + mNode.Identifier.End.SourceLength));
                result = false;
            }
            else
            {
                int minArguments = -1;
                int maxArguments = -1;

                if(templateResult.Arguments != null && templateResult.Arguments.Count() > 0)
                {
                    for(int i = 0; i < templateResult.Arguments.Length; i++)
                    {
                        string currentName = templateResult.Arguments[i];

                        int startIndex = -1;

                        if((startIndex = currentName.IndexOf("{")) != -1)
                        {
                            minArguments = i + 1;
                            int endIndex = currentName.IndexOf("}", startIndex + 1);

                            if(endIndex != -1)
                            {
                                maxArguments = 65535;
                                currentName = currentName.Substring(0, startIndex);
                            }
                            else
                            {
                                currentName = currentName.Substring(0, startIndex);
                            }
                        }
                        else
                        {
                            minArguments = i + 1;
                        }
                    }
                }
                else
                {
                    if(mNode.Parameters.Items.Length > 0)
                    {
                        minArguments = -1;
                    }

                }

                if(minArguments != -1)
                {
                    if(paramCount < minArguments)
                    {
                        _errors.Add(new IntellisenseProviderResult(this, mNode.Identifier.Content, null, "An error occured while parsing { " + methodName + " }, the function needs at least " + minArguments.ToString() + " arguments.", true, mNode.Identifier.Start.SourceIndex, mNode.Identifier.End.SourceIndex + mNode.Identifier.End.SourceLength));
                        result = false;
                    }
                    else if(maxArguments == -1 && paramCount != minArguments)
                    {
                            _errors.Add(new IntellisenseProviderResult(this, mNode.Identifier.Content, null, "An error occured while parsing { " + methodName + " }, the function needs exactly " + minArguments.ToString() + " arguments.", true, mNode.Identifier.Start.SourceIndex, mNode.Identifier.End.SourceIndex + mNode.Identifier.End.SourceLength));
                            result = false;
                    }
                }
                else if(paramCount > 0 && minArguments == -1)
                {
                    _errors.Add(new IntellisenseProviderResult(this, mNode.Identifier.Content, null, "An error occured while parsing { " + methodName + " }, the function must be called without arguments", true, mNode.Identifier.Start.SourceIndex, mNode.Identifier.End.SourceIndex + mNode.Identifier.End.SourceLength));
                    result = false;
                }
            }

            return result;
        }

        private IList<IntellisenseProviderResult> EvaluateMethodContext(IntellisenseProviderContext context, bool tryRebuildNullTokens, Token[] tokens)
        {
            Token start = null;
            bool usedStart = false;

            if(tokens == null && tryRebuildNullTokens && _builder.EventLog.HasEventLogs)
            {
                ParseEventLogEntry[] examineEntries = _builder.EventLog.GetEventLogs();

                if(examineEntries != null && examineEntries.Length > 0)
                {
                    if(examineEntries[examineEntries.Length - 1].Component == "StringLiteralTokenizationHandler")
                    {
                        _builder.EventLog.Clear();
                        _builder.Build(context.InputText, true, out tokens);
                        return EvaluateEventLogs(EvaluateMethodContext(context, false, tokens), examineEntries, context.InputText);
                    }
                }
            }

            if(tokens != null && tokens.Length > 0)
            {
                for(int i = 0; i < tokens.Length; i++)
                    if(tokens[i].TokenIndex == 0)
                    {
                        start = tokens[i];
                        break;
                    }

                if(start != null)
                {
                    if(context.CaretPosition > 0 && context.InputText.Length > 0)
                    {
                        int targetPosition = context.CaretPosition - 1;
                        Token from = null;

                        for(Token k = start; k != null; k = k.Next)
                        {
                            if(k.SourceIndex >= 0 && k.SourceIndex <= targetPosition && targetPosition < (k.SourceIndex + k.SourceLength))
                            {
                                from = k;
                                break;
                            }
                        }

                        if(from != null)
                        {
                            Token methodIdentifier = null;
                            int argumentNumber = 0;
                            bool fromComma = false;


                            for(Token k = from; k != null; k = k.Previous)
                            {
                                if(k.Definition == TokenKind.LeftParenthesis)
                                {
                                    if(k.PreviousNWS != null && k.PreviousNWS.Definition.IsUnknown)
                                    {
                                        if(k.PreviousNWS.PreviousNWS == null || k.PreviousNWS.PreviousNWS.Definition != TokenKind.OpenDL)
                                        {
                                            methodIdentifier = k.PreviousNWS;
                                            break;
                                        }
                                    }

                                    argumentNumber = 0;
                                    fromComma = false;
                                }
                                else if(k.Definition == TokenKind.Comma)
                                {
                                    argumentNumber++;
                                    fromComma = true;
                                }
                                else
                                {
                                    if(k.Definition == TokenKind.RightParenthesis)
                                    {
                                        TokenPair body = TokenUtility.BuildReverseGroupSimple(k, TokenKind.LeftParenthesis);
                                        if(body.Start != null) k = body.Start;
                                    }
                                }
                            }

                            if(methodIdentifier != null)
                            {
                                usedStart = true;
                                string methodName = methodIdentifier.Content;
                                IntellisenseProviderResult templateResult = null;

                                for(int i = 0; i < _intellisenseResult.Count; i++)
                                    if(String.Equals(methodName, _intellisenseResult[i].Name, StringComparison.Ordinal))
                                    {
                                        templateResult = _intellisenseResult[i];
                                        break;
                                    }

                                if(templateResult != null)
                                {
                                    List<IntellisenseProviderResult> popupresults = new List<IntellisenseProviderResult>();

                                    StringBuilder builder = new StringBuilder();

                                    if(templateResult.Arguments != null && templateResult.Arguments.Length > 0)
                                    {
                                        builder.Append(templateResult.Name + "(");
                                        int paramsIndex = -1;
                                        string paramsInsert = null;
                                        string currentName = null;
                                        string paramsDescription = null;

                                        for(int i = 0; i < templateResult.Arguments.Length; i++)
                                        {
                                            if(i != 0) builder.Append(", ");
                                            currentName = templateResult.Arguments[i];
                                            int startIndex = -1;
                                            string rangeStart = null;

                                            if((startIndex = currentName.IndexOf("{")) != -1)
                                            {
                                                int endIndex = currentName.IndexOf("}", startIndex + 1);

                                                if(endIndex != -1)
                                                {
                                                    rangeStart = currentName.Substring(startIndex + 1, endIndex - (startIndex + 1));
                                                    paramsIndex = i;
                                                    if(templateResult.ArgumentDescriptions != null && templateResult.ArgumentDescriptions.Length > i) paramsDescription = templateResult.ArgumentDescriptions[i];
                                                    paramsInsert = currentName = currentName.Substring(0, startIndex);
                                                }
                                                else
                                                {
                                                    currentName = currentName.Substring(0, startIndex);
                                                }
                                            }

                                            if(rangeStart != null)
                                            {
                                                builder.Append(currentName + "{" + rangeStart + "}, " + currentName + "{N}");
                                            }
                                            else
                                            {
                                                builder.Append(currentName);
                                            }
                                        }

                                        builder.AppendLine(")");
                                        builder.AppendLine();
                                        builder.Append(templateResult.Description);

                                        if(templateResult.Arguments.Length > argumentNumber)
                                        {
                                            builder.AppendLine();

                                            currentName = templateResult.Arguments[argumentNumber];

                                            int startIndex = -1;
                                            if((startIndex = currentName.IndexOf("{")) != -1) currentName = currentName.Substring(0, startIndex);

                                            builder.AppendLine();

                                            if(templateResult.ArgumentDescriptions != null && templateResult.ArgumentDescriptions.Length > argumentNumber)
                                            {
                                                builder.AppendLine("Argument: " + currentName);
                                                builder.Append(templateResult.ArgumentDescriptions[argumentNumber]);
                                            }
                                            else
                                            {
                                                builder.Append("Argument: " + currentName);
                                            }
                                        }
                                        else
                                        {
                                            if(paramsIndex != -1 && argumentNumber > paramsIndex)
                                            {
                                                builder.AppendLine();
                                                builder.AppendLine();

                                                if(paramsDescription != null)
                                                {
                                                    builder.AppendLine("Argument: " + paramsInsert);
                                                    builder.Append(paramsDescription);
                                                }
                                                else
                                                {
                                                    builder.Append("Argument: " + paramsInsert);
                                                }
                                            }
                                            else if(fromComma)
                                            {
                                                _builder.EventLog.Clear();
                                                _errors.Clear();
                                                _errors.Add(new IntellisenseProviderResult(this, "Syntax Error", null, "Function \"" + methodName + "\" only accepts " + templateResult.Arguments.Length.ToString() + " arguments.", true, methodIdentifier.SourceIndex, methodIdentifier.SourceIndex + methodIdentifier.SourceLength));
                                                return _errors;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        builder.AppendLine(templateResult.Name);
                                        builder.AppendLine();
                                        builder.Append(templateResult.Description);
                                    }
                                    
                                    popupresults.Add(new IntellisenseProviderResult(this, templateResult.Name, builder.ToString()));
                                    return popupresults;
                                }
                            }
                        }
                    }
                }
            }

            if(!usedStart && context.CaretPosition > 0 && context.InputText.Length > 0 && tokens != null)
            {
                char letter = context.InputText[context.CaretPosition - 1];

                if(letter == '(' || letter == ',')
                {
                    return _intellisenseResult;
                }
            }

            return EvaluateEventLogs(_builder.EventLog.GetEventLogs(), context.InputText);
        }

        private IList<IntellisenseProviderResult> EvaluateEventLogs(ParseEventLogEntry[] parseEventLogEntry, string expression)
        {
            _errors.Clear();
            return EvaluateEventLogs(_errors, parseEventLogEntry, expression);
        }

        private IList<IntellisenseProviderResult> EvaluateEventLogs(IList<IntellisenseProviderResult> errors, ParseEventLogEntry[] parseEventLogEntry, string expression)
        {
            if(errors == _intellisenseResult)
            {
                List<IntellisenseProviderResult> clone = new List<IntellisenseProviderResult>();
                clone.AddRange(errors);
                errors = clone;
            }

            _builder.EventLog.Clear();
            errors.Add(new IntellisenseProviderResult(this, "Syntax Error", null, "An error occurred while parsing { " + expression + " } It appears to be malformed", true, 0, expression.Length));
            return errors;
        }

        public void Dispose()
        {
            _intellisenseResult = null;
        }
    }

    #region CalculateIntellisenseTextConverter
    [ValueConversion(typeof(string), typeof(string), ParameterType = typeof(string))]
    public class CalculateIntellisenseTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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

            return value;
        }
    }
    #endregion
}
