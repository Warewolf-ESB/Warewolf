using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Dev2.Providers.Errors;

namespace Dev2.Activities.Designers2.Core
{
    public class FileActivityDesignerViewModel : CredentialsActivityDesignerViewModel
    {
        static readonly List<string> ValidUriSchemes =new List<string> { "file", "ftp", "ftps","sftp" };

        public FileActivityDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public bool IsInputPathFocused
        {
            get { return (bool)GetValue(IsInputPathFocusedProperty); }
            set { SetValue(IsInputPathFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsInputPathFocusedProperty =
            DependencyProperty.Register("IsInputPathFocused", typeof(bool), typeof(FileActivityDesignerViewModel), new PropertyMetadata(false));


        protected string InputPath { get { return GetProperty<string>(); } }

        protected virtual void ValidateInputs()
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

        #region Overrides of ActivityDesignerViewModel
        //Any common validation can go here
        public override void Validate()
        {
        }

        #endregion
    }
}