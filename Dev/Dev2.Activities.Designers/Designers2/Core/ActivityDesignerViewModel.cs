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
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core.Converters;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Activities.Designers2.Service;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Common;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class ActivityDesignerViewModel : DependencyObject, IClosable, IHelpSource, IValidator, IErrorsSource, IDisposable, IUpdatesHelp
    {
        readonly ModelItem _modelItem;
        Action _setInitialFocus;

        readonly ObservableCollection<ActivityDesignerToggle> _titleBarToggles = new ObservableCollection<ActivityDesignerToggle>();

        protected ActivityDesignerViewModel(ModelItem modelItem)
        {
            VerifyArgument.IsNotNull("modelItem", modelItem);
            _modelItem = modelItem;
            _modelItem.PropertyChanged += OnModelItemPropertyChanged;

            ShowExampleWorkflowLink = Visibility.Visible;
            IsValid = true;
            IsClosed = true;
            ShowHelpToggleCommand = new DelegateCommand(o => ShowHelp = !ShowHelp);
            ShowErrorsToggleCommand = new DelegateCommand(o => ClearErrors());
            OpenErrorsLinkCommand = new DelegateCommand(o =>
            {
                var actionableErrorInfo = o as IActionableErrorInfo;
                actionableErrorInfo?.Do();
            });

            BindingOperations.SetBinding(this, IsClosedProperty, new Binding(ShowLargeProperty.Name)
            {
                Source = this,
                Mode = BindingMode.TwoWay,
                Converter = new NegateBooleanConverter()
            });
        }

        public void SetIntialFocusAction(Action setInitialFocus)
        {
            _setInitialFocus = setInitialFocus;
        }

        public ModelItem ModelItem => _modelItem;

        public ICommand ShowHelpToggleCommand { get; private set; }

        public ICommand ShowErrorsToggleCommand { get; private set; }

        public ICommand OpenErrorsLinkCommand { get; private set; }

        public Visibility ShowExampleWorkflowLink { get; set; }

        public ObservableCollection<ActivityDesignerToggle> TitleBarToggles => _titleBarToggles;

        public bool IsClosed
        {
            get => (bool)GetValue(IsClosedProperty);
            set => SetValue(IsClosedProperty, value);
        }

        public static readonly DependencyProperty IsClosedProperty =
            DependencyProperty.Register("IsClosed", typeof(bool), typeof(ActivityDesignerViewModel), new PropertyMetadata(true));

        public List<IActionableErrorInfo> Errors
        {
            get => (List<IActionableErrorInfo>)GetValue(ErrorsProperty);
            set => SetValue(ErrorsProperty, value);
        }

        public static readonly DependencyProperty ErrorsProperty =
            DependencyProperty.Register("Errors", typeof(List<IActionableErrorInfo>), typeof(ActivityDesignerViewModel), new PropertyMetadata(null, OnErrorsChanged));

        static void OnErrorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ActivityDesignerViewModel)d;
            var errors = e.NewValue as IList<IActionableErrorInfo>;
            var isValid = errors == null || errors.Count == 0;
            viewModel.IsValid = isValid;
            viewModel.ShowErrors = !isValid;
            if (viewModel.ShowErrors)
            {
                viewModel.ShowHelp = false;
            }
        }

        public bool ShowLarge
        {
            get => (bool)GetValue(ShowLargeProperty);
            set => SetValue(ShowLargeProperty, value);
        }

        public bool IsMerge
        {
            get => (bool)GetValue(IsMergeProperty);
            set => SetValue(IsMergeProperty, value);
        }

        protected void RemoveHelpToggle()
        {
            var activityDesignerToggle = TitleBarToggles.FirstOrDefault(c => c.AutomationID == "HelpToggle");
            if (activityDesignerToggle != null)
            {
                TitleBarToggles.Remove(activityDesignerToggle);
                ShowHelp = false;
            }
        }

        public static readonly DependencyProperty IsMergeProperty =
            DependencyProperty.Register("IsMerge", typeof(bool), typeof(ActivityDesignerViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowLargeProperty =
            DependencyProperty.Register("ShowLarge", typeof(bool), typeof(ActivityDesignerViewModel), new PropertyMetadata(false, OnTitleBarToggleChanged));

        public bool ShowErrors
        {
            get => (bool)GetValue(ShowErrorsProperty);
            private set => SetValue(ShowErrorsProperty, value);
        }

        public static readonly DependencyProperty ShowErrorsProperty =
            DependencyProperty.Register("ShowErrors", typeof(bool), typeof(ActivityDesignerViewModel), new PropertyMetadata(false));

        public bool ShowHelp
        {
            get => (bool)GetValue(ShowHelpProperty);
            set => SetValue(ShowHelpProperty, value);
        }

        public static readonly DependencyProperty ShowHelpProperty =
            DependencyProperty.Register("ShowHelp", typeof(bool), typeof(ActivityDesignerViewModel), new PropertyMetadata(false, OnShowHelp));

        static void OnShowHelp(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ActivityDesignerViewModel vm && (bool)e.NewValue)
            {
                vm._setInitialFocus?.Invoke();
            }
        }

        public string HelpText
        {
            get => (string)GetValue(HelpTextProperty);
            set => SetValue(HelpTextProperty, value);
        }

        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register("HelpText", typeof(string), typeof(ActivityDesignerViewModel), new PropertyMetadata(null));

        public ZIndexPosition ZIndexPosition
        {
            get => (ZIndexPosition)GetValue(ZIndexPositionProperty);
            set => SetValue(ZIndexPositionProperty, value);
        }

        public static readonly DependencyProperty ZIndexPositionProperty =
            DependencyProperty.Register("ZIndexPosition", typeof(ZIndexPosition), typeof(ActivityDesignerViewModel), new PropertyMetadata(ZIndexPosition.Back));

        public Visibility ConnectorVisibility
        {
            get => (Visibility)GetValue(ConnectorVisibilityProperty);
            set => SetValue(ConnectorVisibilityProperty, value);
        }

        public static readonly DependencyProperty ConnectorVisibilityProperty =
            DependencyProperty.Register("ConnectorVisibility", typeof(Visibility), typeof(ActivityDesignerViewModel), new PropertyMetadata(Visibility.Visible));

        public Visibility TitleBarTogglesVisibility
        {
            get => (Visibility)GetValue(TitleBarTogglesVisibilityProperty);
            set => SetValue(TitleBarTogglesVisibilityProperty, value);
        }

        public static readonly DependencyProperty TitleBarTogglesVisibilityProperty =
            DependencyProperty.Register("TitleBarTogglesVisibility", typeof(Visibility), typeof(ActivityDesignerViewModel), new PropertyMetadata(Visibility.Collapsed));

        public Visibility ThumbVisibility
        {
            get => (Visibility)GetValue(ThumbVisibilityProperty);
            set => SetValue(ThumbVisibilityProperty, value);
        }

        public static readonly DependencyProperty ThumbVisibilityProperty =
            DependencyProperty.Register("ThumbVisibility", typeof(Visibility), typeof(ActivityDesignerViewModel), new PropertyMetadata(Visibility.Collapsed));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(ActivityDesignerViewModel), new PropertyMetadata(false, OnIsSelectedChanged));

        static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ActivityDesignerViewModel)d;
            viewModel.ToggleTitleBarVisibility();
        }

        public bool IsMouseOver
        {
            get => (bool)GetValue(IsMouseOverProperty);
            set => SetValue(IsMouseOverProperty, value);
        }

        public static readonly DependencyProperty IsMouseOverProperty =
            DependencyProperty.Register("IsMouseOver", typeof(bool), typeof(ActivityDesignerViewModel), new PropertyMetadata(false, OnIsMouseOverChanged));

        static void OnIsMouseOverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ActivityDesignerViewModel)d;
            viewModel.ToggleTitleBarVisibility();
        }

        public string PreviousView { get; set; }

        public void Expand()
        {
            ShowLarge = true;
            ShowLargeChanged = ShowLarge;
        }

        public bool ShowLargeChanged { get; set; }

        public virtual void Collapse()
        {
            ShowLarge = false;
            ShowLargeChanged = ShowLarge;
        }

        public virtual void Restore() => ShowLarge = PreviousView == ShowLargeProperty.Name && ShowLargeChanged != ShowLarge;

        protected virtual void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Dev2Logger.Info(sender.GetType() + " " + e.PropertyName + " changed", GlobalConstants.WarewolfInfo);
        }

        protected void AddTitleBarLargeToggle()
        {
            HasLargeView = true;
        }

        public bool HasLargeView { get; set; }

        bool IsSelectedOrMouseOver => IsSelected || IsMouseOver;

        public virtual bool ShowSmall => !ShowLarge;

        protected static void OnTitleBarToggleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ActivityDesignerViewModel)d;
            var isChecked = (bool)e.NewValue;
            viewModel.OnToggleCheckedChanged(e.Property.Name, isChecked);
        }

        protected virtual void OnToggleCheckedChanged(string propertyName, bool isChecked)
        {
            if (this is ServiceDesignerViewModel && propertyName == "ShowLarge")
            {
                if (isChecked)
                {
                    var activityDesignerToggle = TitleBarToggles.FirstOrDefault(c => c.AutomationID == "HelpToggle");
                }
                else
                {
                    RemoveHelpToggle();
                }

            }

            var isSelectedOrMouseOver = IsSelectedOrMouseOver;
            var showSmall = ShowSmall;
            ThumbVisibility = isSelectedOrMouseOver && !showSmall ? Visibility.Visible : Visibility.Collapsed;
            ConnectorVisibility = isSelectedOrMouseOver && showSmall ? Visibility.Visible : Visibility.Collapsed;

            if (!isChecked)
            {
                PreviousView = propertyName;
            }
            ClearErrors();
        }

        void ToggleTitleBarVisibility()
        {
            var parentContentPane = FindDependencyParent.FindParent<DesignerView>(ModelItem.View);
            var dataContext = parentContentPane?.DataContext;
            var isSelectedOrMouseOver = IsSelectedOrMouseOver;
            if (dataContext != null && (dataContext.GetType().Name == "ServiceTestViewModel"))
            {
                TitleBarTogglesVisibility = Visibility.Collapsed;
            }
            else
            {
                TitleBarTogglesVisibility = isSelectedOrMouseOver ? Visibility.Visible : Visibility.Collapsed;
                ZIndexPosition = isSelectedOrMouseOver ? ZIndexPosition.Front : ZIndexPosition.Back;
            }
        }

        protected string DisplayName { get => GetProperty<string>(); set => SetProperty(value); }

        #region Get/SetProperty

        /// <summary>
        /// <remarks><strong>DO NOT</strong> bind to properties that use this - use <see cref="ModelItem"/>.PropertyName instead!</remarks>
        /// </summary>
        protected T GetProperty<T>([CallerMemberName] string propertyName = null) => _modelItem.GetProperty<T>(propertyName);

        /// <summary>
        /// <remarks><strong>DO NOT</strong> bind to properties that use this - use <see cref="ModelItem"/>.PropertyName instead!</remarks>
        /// </summary>
        protected void SetProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            _modelItem.SetProperty(propertyName, value);
        }

        #endregion

        #region Implementation of IValidator

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            private set { SetValue(IsValidProperty, value); }
        }

        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(ActivityDesignerViewModel), new PropertyMetadata(true));

        public virtual void Validate() { }

        #endregion

        void ClearErrors()
        {
            // Clearing errors will set ShowErrors to false
            Errors = null;
        }

        public void Dispose()
        {
            TitleBarToggles.Clear();

            _modelItem.PropertyChanged -= OnModelItemPropertyChanged;

            OnDispose();
            CEventHelper.RemoveAllEventHandlers(this);
            CEventHelper.RemoveAllEventHandlers(TitleBarToggles);
            CEventHelper.RemoveAllEventHandlers(ModelItem);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnDispose() { }

        #region Implementation of IUpdatesHelp

        public abstract void UpdateHelpDescriptor(string helpText);

        #endregion
    }
}
