
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
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.Services.Security;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Security
{
    public class AuthorizeCommand<T> : DependencyObject, ICommand
    {
        readonly Action<T> _action;
        readonly Predicate<T> _canExecute;
        IAuthorizationService _authorizationService;

        public AuthorizeCommand(AuthorizationContext authorizationContext, Action<T> action, Predicate<T> canExecute)
        {
            VerifyArgument.IsNotNull("action", action);
            AuthorizationContext = authorizationContext;
            _action = action;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public string UnauthorizedReason
        {
            get { return (string)GetValue(UnauthorizedReasonProperty); }
            set { SetValue(UnauthorizedReasonProperty, value); }
        }

        public static readonly DependencyProperty UnauthorizedReasonProperty =
            DependencyProperty.Register("UnauthorizedReason", typeof(string), typeof(AuthorizeCommand<T>), new PropertyMetadata(null));

        public Visibility UnauthorizedVisibility
        {
            get { return (Visibility)GetValue(UnauthorizedVisibilityProperty); }
            set { SetValue(UnauthorizedVisibilityProperty, value); }
        }

        public static readonly DependencyProperty UnauthorizedVisibilityProperty =
            DependencyProperty.Register("UnauthorizedVisibility", typeof(Visibility), typeof(AuthorizeCommand<T>), new PropertyMetadata(Visibility.Collapsed));

        public AuthorizationContext AuthorizationContext { get; private set; }

        string ResourceId { get; set; }
        bool IsVersionResource { get; set; }

        public IAuthorizationService AuthorizationService
        {
            get { return _authorizationService; }
            private set
            {
                if(Equals(value, _authorizationService))
                {
                    return;
                }
                if(_authorizationService != null)
                {
                    _authorizationService.PermissionsChanged -= OnPermissionsChanged;
                }
                _authorizationService = value;
                if(_authorizationService != null)
                {
                    _authorizationService.PermissionsChanged += OnPermissionsChanged;
                }
                OnPermissionsChanged(this, EventArgs.Empty);
            }
        }

        public void UpdateContext(IEnvironmentModel environment, IContextualResourceModel resourceModel = null)
        {
            // MUST set ResourceID first as setting AuthorizationService triggers IsAuthorized() query
            if(resourceModel != null)
            {
                ResourceId = resourceModel.ID.ToString();
                IsVersionResource = resourceModel.IsVersionResource;
            }
            AuthorizationService = environment == null ? null : environment.AuthorizationService;
        }

        public void Execute(object parameter)
        {
            _action((T)parameter);
        }

        public bool CanExecute(object parameter)
        {
            var authorized = true;
            var canExecute = _canExecute != null && _canExecute((T)parameter);
            if(canExecute)
            {
                authorized = IsAuthorized();
                canExecute = authorized;
            }
            UnauthorizedVisibility = authorized ? Visibility.Collapsed : Visibility.Visible;
            UnauthorizedReason = AuthorizationContext.ToReason(authorized);
            return canExecute;
        }

        bool IsAuthorized()
        {
            return !IsVersionResource && AuthorizationService != null && AuthorizationService.IsAuthorized(AuthorizationContext, ResourceId);
        }

        static void OnPermissionsChanged(object sender, EventArgs eventArgs)
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class AuthorizeCommand : AuthorizeCommand<object>
    {
        public AuthorizeCommand(AuthorizationContext authorizationContext, Action<object> action, Predicate<object> canExecute)
            : base(authorizationContext, action, canExecute)
        {
        }
    }
}
