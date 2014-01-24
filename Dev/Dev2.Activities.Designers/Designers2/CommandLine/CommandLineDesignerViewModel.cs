using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Providers.Errors;

namespace Dev2.Activities.Designers2.CommandLine
{
    public class CommandLineDesignerViewModel : ActivityDesignerViewModel
    {
        public CommandLineDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();
            InitializeCommandPriorities();
        }

        public List<KeyValuePair<ProcessPriorityClass, string>> CommandPriorities { get; private set; }

        public bool IsCommandFileNameFocused { get { return (bool)GetValue(IsCommandFileNameFocusedProperty); } set { SetValue(IsCommandFileNameFocusedProperty, value); } }

        public static readonly DependencyProperty IsCommandFileNameFocusedProperty =
            DependencyProperty.Register("IsCommandFileNameFocused", typeof(bool), typeof(CommandLineDesignerViewModel), new PropertyMetadata(false));

        string CommandFileName { get { return GetProperty<string>(); } }

        public override void Validate()
        {
            Errors = null;
            var errors = new List<IActionableErrorInfo>();

            System.Action onError = () => IsCommandFileNameFocused = true;

            string commandValue;
            errors.AddRange(CommandFileName.TryParseVariables(out commandValue, onError));

            if(string.IsNullOrWhiteSpace(commandValue))
            {
                errors.Add(new ActionableErrorInfo(onError) { ErrorType = ErrorType.Critical, Message = "Command must have a value" });
            }

            // Always assign property otherwise binding does not update!
            Errors = errors;
        }

        void InitializeCommandPriorities()
        {
            CommandPriorities = new List<KeyValuePair<ProcessPriorityClass, string>>
            {
                new KeyValuePair<ProcessPriorityClass, string>(ProcessPriorityClass.BelowNormal, "Below Normal"),
                new KeyValuePair<ProcessPriorityClass, string>(ProcessPriorityClass.Normal, "Normal"),
                new KeyValuePair<ProcessPriorityClass, string>(ProcessPriorityClass.AboveNormal, "Above Normal"),
                new KeyValuePair<ProcessPriorityClass, string>(ProcessPriorityClass.Idle, "Idle"),
                new KeyValuePair<ProcessPriorityClass, string>(ProcessPriorityClass.High, "High"),
                new KeyValuePair<ProcessPriorityClass, string>(ProcessPriorityClass.RealTime, "Real Time"),
            };
        }

    }
}