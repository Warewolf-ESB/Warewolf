using System;
using System.Windows.Input;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Models;

namespace Dev2.Studio.Core.Interfaces {
    public interface IConfigureDecisionViewModel {
        event OperatorTypeEventHandler OnExpressionBuilt;
        event EventHandler OnUserClose;
        dynamic DecisionTypes { get; }
        DecisionType SelectedDecisionType { get; set; }
        ICommand OkCommand { get; }
        bool CanSelect { get; }
    }
}