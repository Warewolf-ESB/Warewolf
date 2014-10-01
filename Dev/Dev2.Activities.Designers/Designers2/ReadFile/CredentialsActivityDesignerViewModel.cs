
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
using Dev2.Activities.Designers2.Core;
using Dev2.Providers.Errors;

namespace Dev2.Activities.Designers2.ReadFile
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

        string UserName { get { return GetProperty<string>(); } }
        string Password { get { return GetProperty<string>(); } }


        public override void Validate()
        {
            Errors = null;
            ValidateInputs();
            ValidateUserNameAndPassword();
        }

        protected abstract void ValidateInputs();

        void ValidateUserNameAndPassword()
        {
            var errors = new List<IActionableErrorInfo>();

            Action onUserNameError = () => IsUserNameFocused = true;
            string userName;
            errors.AddRange(UserName.TryParseVariables(out userName, onUserNameError));

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

            if(errors.Count > 0)
            {
                if(Errors == null)
                {
                    Errors = errors;
                }
                else
                {
                    Errors.AddRange(errors);
                }
            }
        }
    }
}
