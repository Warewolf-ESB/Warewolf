
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
using Dev2.Providers.Validation.Rules;
using Dev2.Studio.Core;
using Dev2.Validation;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class FileActivityDesignerViewModel : CredentialsActivityDesignerViewModel
    {
        public static readonly List<string> ValidUriSchemes = new List<string> { "file", "ftp", "ftps", "sftp" };
        public Func<string> GetDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;

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


        public string FileContentValue { get; private set; }
        public string ArchivePasswordValue { get; private set; }


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
            if (!pathIsRequired && string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            var errors = new List<IActionableErrorInfo>();

            RuleSet fileActivityRuleSet = new RuleSet();
            IsValidExpressionRule isValidExpressionRule = new IsValidExpressionRule(() => path, DataListSingleton.ActiveDataList.Resource.DataList);
            fileActivityRuleSet.Add(isValidExpressionRule);
            errors.AddRange(fileActivityRuleSet.ValidateRules(label, onError));


            string pathValue;
            bool isVariable = false;
            if (errors.Count == 0)
            {
                pathValue = path;
                isVariable = true;
            }
            else
            {
                errors.AddError(path.TryParseVariables(out pathValue, onError, variableValue: ValidUriSchemes[0] + "://temp"));
            }


            if (errors.Count == 0)
            {

                IsStringEmptyOrWhiteSpaceRule isStringEmptyOrWhiteSpaceRuleUserName = new IsStringEmptyOrWhiteSpaceRule(() => path)
                {
                    LabelText = label,
                    DoError = onError
                };

                fileActivityRuleSet.Add(isStringEmptyOrWhiteSpaceRuleUserName);
                errors.AddRange(fileActivityRuleSet.ValidateRules(label, onError));

                var pathBlankError = isStringEmptyOrWhiteSpaceRuleUserName.Check();
                if (pathBlankError != null)
                {
                    //errors.Add(pathBlankError);
                    errors.Add(new ActionableErrorInfo(onError) { ErrorType = ErrorType.Critical, Message = label + " must have a value" });
                }
                else
                {
                    if (!isVariable)
                    {
                        Uri uriResult;
                        var isValid = Uri.TryCreate(pathValue, UriKind.Absolute, out uriResult)
                            ? ValidUriSchemes.Contains(uriResult.Scheme)
                            : File.Exists(pathValue);

                        if (!isValid)
                        {
                            errors.Add(new ActionableErrorInfo(onError) { ErrorType = ErrorType.Critical, Message = "Please supply a valid " + label });
                        }
                    }
                }
            }


            UpdateErrors(errors);

            return pathValue;
        }


        protected virtual void ValidateFileContent(string content, string label)
        {
            FileContentValue = ValidateFileContent(content, label, () => FileHasContent = true);
        }

        public bool FileHasContent
        {
            get { return (bool)GetValue(FileHasContentProperty); }
            set { SetValue(FileHasContentProperty, value); }
        }


        public static readonly DependencyProperty FileHasContentProperty =
    DependencyProperty.Register("HasContent", typeof(bool), typeof(FileActivityDesignerViewModel), new PropertyMetadata(false));


        protected virtual string ValidateFileContent(string content, string label, Action onError, bool contentIsRequired = true)
        {

            var errors = new List<IActionableErrorInfo>();
            RuleSet fileActivityRuleSet = new RuleSet();

            IsValidExpressionRule isValidExpressionRule = new IsValidExpressionRule(() => content, DataListSingleton.ActiveDataList.Resource.DataList);
            fileActivityRuleSet.Add(isValidExpressionRule);
            errors.AddRange(fileActivityRuleSet.ValidateRules(label, onError));

            UpdateErrors(errors);
            return content;

        }


        protected virtual void ValidateArchivePassword(string password, string label)
        {
            ArchivePasswordValue = ValidateArchivePassword(password, label, () => PasswordExists = true);
        }

        public bool PasswordExists
        {
            get { return (bool)GetValue(PasswordExistsProperty); }
            set { SetValue(PasswordExistsProperty, value); }
        }

        public static readonly DependencyProperty PasswordExistsProperty =
DependencyProperty.Register("HasPassword", typeof(bool), typeof(FileActivityDesignerViewModel), new PropertyMetadata(false));



        protected virtual string ValidateArchivePassword(string password, string label, Action onError, bool contentIsRequired = true)
        {

            var errors = new List<IActionableErrorInfo>();
            RuleSet fileActivityRuleSet = new RuleSet();

            IsValidExpressionRule isValidExpressionRule = new IsValidExpressionRule(() => password, DataListSingleton.ActiveDataList.Resource.DataList);
            fileActivityRuleSet.Add(isValidExpressionRule);
            errors.AddRange(fileActivityRuleSet.ValidateRules(label, onError));

            UpdateErrors(errors);
            return password;

        }


    }
}
