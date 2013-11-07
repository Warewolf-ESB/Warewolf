using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Providers.Errors;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class CredentialsActivityDesignerViewModel : ActivityDesignerViewModel
    {
        public static readonly DependencyProperty IsUserNameFocusedProperty =
            DependencyProperty.Register("IsUserNameFocused", typeof(bool), typeof(CredentialsActivityDesignerViewModel), new PropertyMetadata(false));
        public static readonly DependencyProperty IsPasswordFocusedProperty =
            DependencyProperty.Register("IsPasswordFocused", typeof(bool), typeof(CredentialsActivityDesignerViewModel), new PropertyMetadata(false));

        protected CredentialsActivityDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public bool IsUserNameFocused
        {
            get { return (bool)GetValue(IsUserNameFocusedProperty); }
            set { SetValue(IsUserNameFocusedProperty, value); }
        }

        public bool IsPasswordFocused
        {
            get { return (bool)GetValue(IsPasswordFocusedProperty); }
            set { SetValue(IsPasswordFocusedProperty, value); }
        }

        string Username { get { return GetProperty<string>(); } }
        string Password { get { return GetProperty<string>(); } }

        protected virtual void ValidateUserNameAndPassword()
        {
            var errors = new List<IActionableErrorInfo>();

            Action onUserNameError = () => IsUserNameFocused = true;
            string userName;
            errors.AddRange(Username.TryParseVariables(out userName, onUserNameError));

            if(errors.Count == 0)
            {
                if(!string.IsNullOrWhiteSpace(userName))
                {
                    Action onPasswordError = () => IsPasswordFocused = true;
                    string password;
                    errors.AddRange(Password.TryParseVariables(out password, onPasswordError));

                    if(string.IsNullOrWhiteSpace(password))
                    {
                        errors.Add(new ActionableErrorInfo(onPasswordError) { ErrorType = ErrorType.Critical, Message = "Password must have a value" });
                    }
                }
            }

            UpdateErrors(errors);
        }

        protected void UpdateErrors(List<IActionableErrorInfo> errors)
        {
            if(errors != null && errors.Count > 0)
            {
                if(Errors != null)
                {
                    errors.AddRange(Errors);
                }

                // Always assign property otherwise binding does not update!
                Errors = errors;
            }
        }
    }
}