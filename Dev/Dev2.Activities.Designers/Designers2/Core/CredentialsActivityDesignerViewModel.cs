
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
using System.Windows;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Studio.Core;
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
            
            RuleSet credentialUserRuleSet = new RuleSet();
            IsValidExpressionRule isValidExpressionRule = new IsValidExpressionRule(() => userNameValue, DataListSingleton.ActiveDataList.Resource.DataList);
            credentialUserRuleSet.Add(isValidExpressionRule);
            errors.AddRange(credentialUserRuleSet.ValidateRules(userNameLabel, onUserNameError));
            RuleSet credentialPasswordRuleSet = new RuleSet();
            isValidExpressionRule = new IsValidExpressionRule(() => passwordValue, DataListSingleton.ActiveDataList.Resource.DataList);
            credentialPasswordRuleSet.Add(isValidExpressionRule);
            errors.AddRange(credentialPasswordRuleSet.ValidateRules(passwordLabel, onPasswordError));
            if(errors.Count == 0)
            {
                IsStringEmptyOrWhiteSpaceRule isStringEmptyOrWhiteSpaceRuleUserName = new IsStringEmptyOrWhiteSpaceRule(() => userNameValue)
                {
                    LabelText = userNameLabel, 
                    DoError = onUserNameError
                };
                var userNameBlankError = isStringEmptyOrWhiteSpaceRuleUserName.Check();
                var isStringEmptyOrWhiteSpaceRulePassword = new IsStringEmptyOrWhiteSpaceRule(() => passwordValue)
                {
                    LabelText = passwordLabel, 
                    DoError = onPasswordError
                };
                var passwordBlankError = isStringEmptyOrWhiteSpaceRulePassword.Check();

                if (userNameBlankError == null && passwordBlankError != null)
                {
                    errors.Add(passwordBlankError);
                }
                else
                {
                    if (passwordBlankError == null && userNameBlankError != null)
                    {
                        errors.Add(userNameBlankError);
                    }
                }
            }

            UpdateErrors(errors);
        }
    }
}
