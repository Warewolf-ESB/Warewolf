using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Providers.Errors;

namespace Dev2.Activities.Designers2.ReadFile
{
    public class ReadFileDesignerViewModel : CredentialsActivityDesignerViewModel
    {
        static readonly string[] ValidUriSchemes = { "file", "ftp", "ftps" };

        public ReadFileDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();
        }

        public bool IsInputPathFocused
        {
            get { return (bool)GetValue(IsInputPathFocusedProperty); }
            set { SetValue(IsInputPathFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsInputPathFocusedProperty =
            DependencyProperty.Register("IsInputPathFocused", typeof(bool), typeof(ReadFileDesignerViewModel), new PropertyMetadata(false));

        // DO NOT bind to these properties - these are here for convenience only!!!
        string InputPath { get { return GetProperty<string>(); } }

        protected override void ValidateInputs()
        {
            if(string.IsNullOrWhiteSpace(InputPath))
            {
                return;
            }

            ValidateInputPath();
        }

        void ValidateInputPath()
        {
            Action onError = () => IsInputPathFocused = true;
            string inputPathValue;
            Errors = InputPath.TryParseVariables(out inputPathValue, onError);
            if(!IsValid)
            {
                return;
            }

            if(string.IsNullOrWhiteSpace(inputPathValue))
            {
                Errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo(onError) { ErrorType = ErrorType.Critical, Message = "InputPath must have a value" }
                };
            }
            else
            {
                Uri uriResult;
                var isValid = Uri.TryCreate(inputPathValue, UriKind.Absolute, out uriResult)
                    ? ValidUriSchemes.Contains(uriResult.Scheme)
                    : File.Exists(inputPathValue);

                if(!isValid)
                {
                    Errors = new List<IActionableErrorInfo>
                    {
                        new ActionableErrorInfo(onError) { ErrorType = ErrorType.Critical, Message = "Please supply a valid InputPath" }
                    };
                }
            }
        }

    }
}