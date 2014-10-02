
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
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Validation;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class FileActivityDesignerViewModel : CredentialsActivityDesignerViewModel
    {
        public static readonly List<string> ValidUriSchemes = new List<string> { "file", "ftp", "ftps", "sftp" };

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

        protected virtual void ValidateInputPath()
        {
            InputPathValue = ValidatePath(InputPathLabel, InputPath, () => IsInputPathFocused = true, true);
        }

        protected virtual void ValidateOutputPath()
        {
            OutputPathValue = ValidatePath(OutputPathLabel, OutputPath, () => IsOutputPathFocused = true, true);
        }

        protected virtual void ValidateInputAndOutputPaths()
        {
            ValidateOutputPath();
            ValidateInputPath();
        }

        protected virtual string ValidatePath(string label, string path, Action onError, bool pathIsRequired)
        {
            if(!pathIsRequired && string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            var errors = new List<IActionableErrorInfo>();

            string pathValue;
            errors.AddError(path.TryParseVariables(out pathValue, onError, variableValue: ValidUriSchemes[0] + "://temp"));

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
