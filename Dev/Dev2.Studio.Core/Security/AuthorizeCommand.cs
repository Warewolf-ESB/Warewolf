#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.Enums;
using Dev2.Services.Security;
using Dev2.Studio.Interfaces;



namespace Dev2.Security
{
    public class AuthorizeCommand<T> : DependencyObject, IAuthorizeCommand<T>
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
        IContextualResourceModel _resourceModel;

        public AuthorizationContext AuthorizationContext { get;  set; }

        string ResourceId { get; set; }
        bool IsVersionResource { get; set; }

        public IAuthorizationService AuthorizationService
        {
            get { return _authorizationService; }
            private set
            {
                if(Equals(value, _authorizationService))
                {
                    OnPermissionsChanged(this, EventArgs.Empty);
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

        public void UpdateContext(IServer environment) => UpdateContext(environment, null);

        public void UpdateContext(IServer environment, IContextualResourceModel resourceModel)
        {
            // MUST set ResourceID first as setting AuthorizationService triggers IsAuthorized() query
            if(resourceModel != null)
            {
                _resourceModel = resourceModel;
                ResourceId = resourceModel.ID.ToString();
                IsVersionResource = resourceModel.IsVersionResource;
            }
            AuthorizationService = environment?.AuthorizationService;
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
            if (AuthorizationService == null)
            {
                return true;
            }
            if (_resourceModel != null)
            {
                var perms = _resourceModel.UserPermissions;
                return (perms & AuthorizationContext.ToPermissions()) != 0;
            }
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

        public void RaiseCanExecuteChanged()
        {
             CommandManager.InvalidateRequerySuggested();
        }
    }
}
