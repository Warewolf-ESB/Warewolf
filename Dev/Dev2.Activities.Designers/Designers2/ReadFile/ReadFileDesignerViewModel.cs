using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Preview;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;

namespace Dev2.Activities.Designers2.ReadFile
{
    public class ReadFileDesignerViewModel : ActivityDesignerViewModel
    {
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

        public bool IsUserNameFocused
        {
            get { return (bool)GetValue(IsUserNameFocusedProperty); }
            set { SetValue(IsUserNameFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsUserNameFocusedProperty =
            DependencyProperty.Register("IsUserNameFocused", typeof(bool), typeof(ReadFileDesignerViewModel), new PropertyMetadata(false));

        public bool IsPasswordFocused
        {
            get { return (bool)GetValue(IsPasswordFocusedProperty); }
            set { SetValue(IsPasswordFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsPasswordFocusedProperty =
            DependencyProperty.Register("IsPasswordFocused", typeof(bool), typeof(ReadFileDesignerViewModel), new PropertyMetadata(false));


        // DO NOT bind to these properties - these are here for convenience only!!!
        string InputPath { get { return GetProperty<string>(); } }
        string UserName { get { return GetProperty<string>(); } }
        string Password { get { return GetProperty<string>(); } }

        public override void Validate()
        {
            Errors = null;
            if(string.IsNullOrWhiteSpace(InputPath))
            {
                return;
            }

            var inputPath = ParseVariables(InputPath, () => IsInputPathFocused = true);
            if(IsValid)
            {
                ValidateInputPath(inputPath);
            }

            var userName = ParseVariables(UserName, () => IsUserNameFocused = true);
            if(IsValid)
            {
                ValidateUserName(userName);
            }

            var password = ParseVariables(Password, () => IsPasswordFocused = true);
            if(IsValid)
            {
                ValidatePassword(userName, password);
            }
        }

        string ParseVariables(string original, Action onError, ObservableCollection<ObservablePair<string, string>> inputs = null)
        {
            var value = original;

            if(string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var isValid = true;
            var variableList = DataListCleaningUtils.SplitIntoRegions(value);
            foreach(var v in variableList)
            {
                if(v != null)
                {
                    string s;
                    if(inputs != null)
                    {
                        var input = inputs.FirstOrDefault(p => p.Key == v);
                        s = input == null ? string.Empty : input.Value;
                    }
                    else
                    {
                        s = "a"; // random text to replace variable
                    }
                    value = value.Replace(v, s);
                }
                else
                {
                    isValid = false;
                }
            }

            if(!isValid)
            {
                Errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo(onError) { ErrorType = ErrorType.Critical, Message = "Invalid expression: opening and closing brackets don't match." }
                };
            }

            return value;
        }

        void ValidateInputPath(string inputPathValue)
        {
            if(string.IsNullOrWhiteSpace(inputPathValue))
            {
                Errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo(() => IsInputPathFocused = true) { ErrorType = ErrorType.Critical, Message = "InputPath must have a value" }
                };
            }
            else
            {
                Uri uriResult;
                var isValid = Uri.TryCreate(inputPathValue, UriKind.Absolute, out uriResult) && (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps);
                if(!isValid)
                {
                    Errors = new List<IActionableErrorInfo>
                    {
                        new ActionableErrorInfo(() => IsInputPathFocused = true) { ErrorType = ErrorType.Critical, Message = "Please supply a valid InputPath" }
                    };
                }
            }
        }

        void ValidateUserName(string userName)
        {
            if(string.IsNullOrWhiteSpace(userName))
            {
                Errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo(() => IsUserNameFocused = true) { ErrorType = ErrorType.Critical, Message = "User Name must have a value" }
                };
            }
        }

        void ValidatePassword(string userName, string password)
        {
            if(!string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(password))
            {
                Errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo(() => IsPasswordFocused = true) { ErrorType = ErrorType.Critical, Message = "Password must have a value" }
                };
            }
        }
    }
}