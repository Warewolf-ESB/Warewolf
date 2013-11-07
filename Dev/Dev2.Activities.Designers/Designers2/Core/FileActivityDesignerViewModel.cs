using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Dev2.Providers.Errors;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class FileActivityDesignerViewModel : CredentialsActivityDesignerViewModel
    {
        static readonly List<string> ValidUriSchemes = new List<string> { "file", "ftp", "ftps", "sftp" };

        protected FileActivityDesignerViewModel(ModelItem modelItem, string inputPathLabel, string outputPathLabel)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("inputPathLabel", inputPathLabel);
            VerifyArgument.IsNotNull("outputPathLabel", outputPathLabel);
            InputPathLabel = inputPathLabel;
            OutputPathLabel = outputPathLabel;
        }

        public string InputPathLabel { get; private set; }
        public string OutputPathLabel { get; private set; }

        public string InputPathValue { get; private set; }
        public string OutputPathValue { get; set; }

        public bool IsInputPathFocused
        {
            get { return (bool)GetValue(IsInputPathFocusedProperty); }
            set { SetValue(IsInputPathFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsInputPathFocusedProperty =
            DependencyProperty.Register("IsInputPathFocused", typeof(bool), typeof(FileActivityDesignerViewModel), new PropertyMetadata(false));

        public bool IsOutputPathFocused
        {
            get { return (bool)GetValue(IsOutputPathFocusedProperty); }
            set { SetValue(IsOutputPathFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsOutputPathFocusedProperty =
            DependencyProperty.Register("IsOutputPathFocused", typeof(bool), typeof(FileActivityDesignerViewModel), new PropertyMetadata(false));

        string InputPath { get { return GetProperty<string>(); } }
        string OutputPath { get { return GetProperty<string>(); } }

        protected virtual void ValidateInputPath(bool isRequired = false)
        {
            InputPathValue = ValidatePath(InputPathLabel, InputPath, () => IsInputPathFocused = true, isRequired);
        }

        protected virtual void ValidateOutputPath(bool isRequired = false)
        {
            OutputPathValue = ValidatePath(OutputPathLabel, OutputPath, () => IsOutputPathFocused = true, isRequired);
        }

        protected virtual void ValidInputAndOutputPaths(bool isOutputPathRequired = false)
        {
            ValidateOutputPath(isOutputPathRequired);
            ValidateInputPath();

            if(!string.IsNullOrWhiteSpace(OutputPathValue) && string.IsNullOrWhiteSpace(InputPathValue))
            {
                var errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo(() => IsInputPathFocused = true)
                    {
                        ErrorType = ErrorType.Critical,
                        Message = InputPathLabel + " must have a value if " + OutputPathLabel + " has a value"
                    }
                };
                UpdateErrors(errors);
            }
        }

        string ValidatePath(string label, string path, Action onError, bool pathIsRequired = false)
        {
            if(!pathIsRequired && string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            var errors = new List<IActionableErrorInfo>();

            string pathValue;
            errors.AddRange(path.TryParseVariables(out pathValue, onError));

            if(errors.Count == 0)
            {
                if(string.IsNullOrWhiteSpace(pathValue))
                {
                    errors.Add(new ActionableErrorInfo(onError) { ErrorType = ErrorType.Critical, Message = label + " must have a value" });
                }
                else
                {
                    Uri uriResult;
                    var isValid = Uri.TryCreate(pathValue, UriKind.Absolute, out uriResult)
                        ? ValidUriSchemes.Contains(uriResult.Scheme)
                        : File.Exists(pathValue);

                    if(!isValid)
                    {
                        errors.Add(new ActionableErrorInfo(onError) { ErrorType = ErrorType.Critical, Message = "Please supply a valid " + label });
                    }
                }
            }

            UpdateErrors(errors);

            return pathValue;
        }
    }
}