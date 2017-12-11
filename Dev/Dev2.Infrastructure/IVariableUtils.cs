using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dev2
{
    public interface IVariableUtils
    {
        void AddError(List<IActionableErrorInfo> errors, IActionableErrorInfo error);
        IActionableErrorInfo TryParseVariables(string inputValue, out string outputValue, Action onError);
        IActionableErrorInfo TryParseVariables(string inputValue, out string outputValue, Action onError, string variableValue);
        IActionableErrorInfo TryParseVariables(string inputValue, out string outputValue, Action onError, string variableValue, string labelText);
        IActionableErrorInfo TryParseVariables(string inputValue, out string outputValue, Action onError, string labelText, string variableValue, ObservableCollection<ObservablePair<string, string>> inputs);
        IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string value, string datalist);
        bool IsEvaluated(string value);
        bool IsValueRecordset(string value);
        List<string> SplitIntoRegions(string value);
        string RemoveLanguageBrackets(string region);
        IIntellisenseResult ValidateName(string name, string displayName);
    }
}
