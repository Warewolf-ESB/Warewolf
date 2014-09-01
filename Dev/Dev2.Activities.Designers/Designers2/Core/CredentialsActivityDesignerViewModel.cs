using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Validation;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class CredentialsActivityDesignerViewModel : ActivityDesignerViewModel
    {
        public static readonly DependencyProperty IsUserNameFocusedProperty =
            DependencyProperty.Register("IsUserNameFocused", typeof(bool), typeof(CredentialsActivityDesignerViewModel), new PropertyMetadata(false));
        public static readonly DependencyProperty IsPasswordFocusedProperty =
            DependencyProperty.Register("IsPasswordFocused", typeof(bool), typeof(CredentialsActivityDesignerViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty IsDestinationUsernameFocusedProperty =
           DependencyProperty.Register("IsDestinationUsernameFocused", typeof(bool), typeof(CredentialsActivityDesignerViewModel), new PropertyMetadata(false));
        public static readonly DependencyProperty IsDestinationPasswordFocusedProperty =
            DependencyProperty.Register("IsDestinationPasswordFocused", typeof(bool), typeof(CredentialsActivityDesignerViewModel), new PropertyMetadata(false));

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

        public bool IsDestinationUsernameFocused
        {
            get { return (bool)GetValue(IsDestinationUsernameFocusedProperty); }
            set { SetValue(IsDestinationUsernameFocusedProperty, value); }
        }

        public bool IsDestinationPasswordFocused
        {
            get { return (bool)GetValue(IsDestinationPasswordFocusedProperty); }
            set { SetValue(IsDestinationPasswordFocusedProperty, value); }
        }

        string Username { get { return GetProperty<string>(); } }
        string Password { get { return GetProperty<string>(); } }
        string DestinationUsername { get { return GetProperty<string>(); } }
        string DestinationPassword { get { return GetProperty<string>(); } }

        protected virtual void ValidateUserNameAndPassword()
        {
            ValidateUserNameAndPassword(Username, "Username", () => IsUserNameFocused = true, Password, "Password", () => IsPasswordFocused = true);
        }

        protected virtual void ValidateDestinationUsernameAndPassword()
        {
            ValidateUserNameAndPassword(DestinationUsername, "Destination Username", () => IsDestinationUsernameFocused = true, DestinationPassword, "Destination Password", () => IsDestinationPasswordFocused = true);
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

        void ValidateUserNameAndPassword(string userNameValue, string userNameLabel, Action onUserNameError, string passwordValue, string passwordLabel, Action onPasswordError)
        {
            var errors = new List<IActionableErrorInfo>();

            string userName;
            errors.AddError(userNameValue.TryParseVariables(out userName, onUserNameError));

            if(errors.Count == 0)
            {
                string password;
                errors.AddError(passwordValue.TryParseVariables(out password, onPasswordError));

                if(!string.IsNullOrWhiteSpace(userName))
                {
                    if(string.IsNullOrWhiteSpace(password))
                    {
                        errors.Add(new ActionableErrorInfo(onPasswordError) { ErrorType = ErrorType.Critical, Message = passwordLabel + " must have a value" });
                    }
                }
                else
                {
                    if(!string.IsNullOrWhiteSpace(password))
                    {
                        errors.Add(new ActionableErrorInfo(onUserNameError) { ErrorType = ErrorType.Critical, Message = userNameLabel + " must have a value" });
                    }
                }
            }

            UpdateErrors(errors);
        }
    }
}