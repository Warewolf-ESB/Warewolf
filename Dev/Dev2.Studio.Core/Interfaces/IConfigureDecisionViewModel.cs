using System;
using System.Windows.Input;
using Dev2.Studio.Core.AppResources;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IConfigureDecisionViewModel
    {
        event OperatorTypeEventHandler OnExpressionBuilt;
        event EventHandler OnUserClose;
        dynamic DecisionTypes { get; }
        ICommand OkCommand { get; }
        bool CanSelect { get; }
    }
}