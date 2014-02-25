using System;
using System.Activities.Presentation.Model;
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
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Utils;
using Dev2.Utils;

namespace Dev2.Activities.Designers2.Core
{
    /// <summary>
    /// <remarks>
    /// <strong>DO NOT</strong> bind to properties that use <see cref="GetProperty{T}"/> and <see cref="SetProperty{T}"/>.
    /// Rather bind to <see cref="ModelItem"/>.PropertyName - this will ensure that the built-in undo/redo framework just works.
    /// </remarks>
    /// </summary>
    public abstract class ActivityDesignerViewModel : DependencyObject, IClosable, IHelpSource, IValidator, IErrorsSource
    {
        static Action<Type> CreateShowExampleWorkflowAction()
        {
            return type => WorkflowDesignerUtils.ShowExampleWorkflow(type.Name, ServerUtil.GetLocalhostServer(), null);
        }

        readonly ModelItem _modelItem;
        Action _setInitialFocus;

        readonly ObservableCollection<ActivityDesignerToggle> _titleBarToggles = new ObservableCollection<ActivityDesignerToggle>();

        protected ActivityDesignerViewModel(ModelItem modelItem)
            : this(modelItem, CreateShowExampleWorkflowAction())
        {
        }

        protected ActivityDesignerViewModel(ModelItem modelItem, Action<Type> showExampleWorkflow)
        {
            VerifyArgument.IsNotNull("modelItem", modelItem);
            _modelItem = modelItem;
            _modelItem.PropertyChanged += OnModelItemPropertyChanged;
            VerifyArgument.IsNotNull("showExampleWorkflow", showExampleWorkflow);

            ShowExampleWorkflowLink = Visibility.Visible;
            IsValid = true;
            IsClosed = true;
            ShowItemHelpCommand = new Runtime.Configuration.ViewModels.Base.RelayCommand(o => showExampleWorkflow(modelItem.ItemType), o => true);
            ShowHelpToggleCommand = new Runtime.Configuration.ViewModels.Base.RelayCommand(o => ShowHelp = !ShowHelp, o => true);
            ShowErrorsToggleCommand = new Runtime.Configuration.ViewModels.Base.RelayCommand(o => ClearErrors(), o => true);
            OpenErrorsLinkCommand = new Runtime.Configuration.ViewModels.Base.RelayCommand(o =>
            {
                var actionableErrorInfo = o as IActionableErrorInfo;
                if(actionableErrorInfo != null)
                {
                    actionableErrorInfo.Do();
                }
            }, o => true);

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

        public ModelItem ModelItem { get { return _modelItem; } }

        public ICommand ShowItemHelpCommand { get; private set; }

        public ICommand ShowHelpToggleCommand { get; private set; }

        public ICommand ShowErrorsToggleCommand { get; private set; }

        public ICommand OpenErrorsLinkCommand { get; private set; }

        public Visibility ShowExampleWorkflowLink { get; set; }

        public ObservableCollection<ActivityDesignerToggle> TitleBarToggles { get { return _titleBarToggles; } }

        public bool IsClosed
        {
            get { return (bool)GetValue(IsClosedProperty); }
            set { SetValue(IsClosedProperty, value); }
        }

        public static readonly DependencyProperty IsClosedProperty =
            DependencyProperty.Register("IsClosed", typeof(bool), typeof(ActivityDesignerViewModel), new PropertyMetadata(true));

        public List<IActionableErrorInfo> Errors
        {
            get { return (List<IActionableErrorInfo>)GetValue(ErrorsProperty); }
            set { SetValue(ErrorsProperty, value); }
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
            if(viewModel.ShowErrors)
            {
                viewModel.ShowHelp = false;
            }
        }

        public bool ShowLarge
        {
            get
            {
                return (bool)GetValue(ShowLargeProperty);
            }
            set
            {
                SetValue(ShowLargeProperty, value);
            }
        }

        protected void RemoveHelpToggle()
        {
            ActivityDesignerToggle activityDesignerToggle = TitleBarToggles.FirstOrDefault(c => c.AutomationID == "HelpToggle");
            if(activityDesignerToggle != null)
            {
                TitleBarToggles.Remove(activityDesignerToggle);
                ShowHelp = false;
            }
        }

        public static readonly DependencyProperty ShowLargeProperty =
            DependencyProperty.Register("ShowLarge", typeof(bool), typeof(ActivityDesignerViewModel), new PropertyMetadata(false, OnTitleBarToggleChanged));

        public bool ShowErrors
        {
            get { return (bool)GetValue(ShowErrorsProperty); }
            private set { SetValue(ShowErrorsProperty, value); }
        }

        public static readonly DependencyProperty ShowErrorsProperty =
            DependencyProperty.Register("ShowErrors", typeof(bool), typeof(ActivityDesignerViewModel), new PropertyMetadata(false));

        public bool ShowHelp
        {
            get { return (bool)GetValue(ShowHelpProperty); }
            set { SetValue(ShowHelpProperty, value); }
        }

        public static readonly DependencyProperty ShowHelpProperty =
            DependencyProperty.Register("ShowHelp", typeof(bool), typeof(ActivityDesignerViewModel), new PropertyMetadata(false, OnShowHelp));

        static void OnShowHelp(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vm = d as ActivityDesignerViewModel;

            if(vm != null && (bool)e.NewValue)
            {
                if(vm._setInitialFocus != null)
                {
                    vm._setInitialFocus();
                }
            }
        }

        public string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register("HelpText", typeof(string), typeof(ActivityDesignerViewModel), new PropertyMetadata(null));

        public ZIndexPosition ZIndexPosition
        {
            get { return (ZIndexPosition)GetValue(ZIndexPositionProperty); }
            set { SetValue(ZIndexPositionProperty, value); }
        }

        public static readonly DependencyProperty ZIndexPositionProperty =
            DependencyProperty.Register("ZIndexPosition", typeof(ZIndexPosition), typeof(ActivityDesignerViewModel), new PropertyMetadata(ZIndexPosition.Back));

        public Visibility ConnectorVisibility
        {
            get { return (Visibility)GetValue(ConnectorVisibilityProperty); }
            set { SetValue(ConnectorVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ConnectorVisibilityProperty =
            DependencyProperty.Register("ConnectorVisibility", typeof(Visibility), typeof(ActivityDesignerViewModel), new PropertyMetadata(Visibility.Visible));

        public Visibility TitleBarTogglesVisibility
        {
            get { return (Visibility)GetValue(TitleBarTogglesVisibilityProperty); }
            set { SetValue(TitleBarTogglesVisibilityProperty, value); }
        }

        public static readonly DependencyProperty TitleBarTogglesVisibilityProperty =
            DependencyProperty.Register("TitleBarTogglesVisibility", typeof(Visibility), typeof(ActivityDesignerViewModel), new PropertyMetadata(Visibility.Collapsed));

        public Visibility ThumbVisibility
        {
            get { return (Visibility)GetValue(ThumbVisibilityProperty); }
            set { SetValue(ThumbVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ThumbVisibilityProperty =
            DependencyProperty.Register("ThumbVisibility", typeof(Visibility), typeof(ActivityDesignerViewModel), new PropertyMetadata(Visibility.Collapsed));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
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
            get { return (bool)GetValue(IsMouseOverProperty); }
            set { SetValue(IsMouseOverProperty, value); }
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
        }

        public virtual void Collapse()
        {
            ShowLarge = false;
        }

        public virtual void Restore()
        {
            ShowLarge = PreviousView == ShowLargeProperty.Name;
        }

        protected virtual void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        protected void AddTitleBarHelpToggle()
        {
            var toggle = ActivityDesignerToggle.Create(
                collapseImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png",
                collapseToolTip: "Close Help",
                expandImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png",
                expandToolTip: "Open Help",
                automationID: "HelpToggle",
                target: this,
                dp: ShowHelpProperty
                );
            TitleBarToggles.Add(toggle);
        }

        protected void AddTitleBarLargeToggle()
        {
            var toggle = ActivityDesignerToggle.Create(
                collapseImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceCollapseMapping-32.png",
                collapseToolTip: "Close Large View",
                expandImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceExpandMapping-32.png",
                expandToolTip: "Open Large View",
                automationID: "LargeViewToggle",
                target: this,
                dp: ShowLargeProperty
                );
            TitleBarToggles.Add(toggle);
        }

        bool IsSelectedOrMouseOver { get { return IsSelected || IsMouseOver; } }

        public virtual bool ShowSmall { get { return !ShowLarge; } }

        protected static void OnTitleBarToggleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ActivityDesignerViewModel)d;
            var isChecked = (bool)e.NewValue;
            viewModel.OnToggleCheckedChanged(e.Property.Name, isChecked);
        }

        protected virtual void OnToggleCheckedChanged(string propertyName, bool isChecked)
        {
            if(this is ServiceDesignerViewModel && propertyName == "ShowLarge")
            {
                if(isChecked)
                {
                    ActivityDesignerToggle activityDesignerToggle = TitleBarToggles.FirstOrDefault(c => c.AutomationID == "HelpToggle");
                    if(activityDesignerToggle == null)
                    {
                        AddTitleBarHelpToggle();
                    }
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

            if(!isChecked)
            {
                PreviousView = propertyName;
            }
            ClearErrors();
        }

        void ToggleTitleBarVisibility()
        {
            var isSelectedOrMouseOver = IsSelectedOrMouseOver;
            TitleBarTogglesVisibility = isSelectedOrMouseOver ? Visibility.Visible : Visibility.Collapsed;
            ZIndexPosition = isSelectedOrMouseOver ? ZIndexPosition.Front : ZIndexPosition.Back;
        }

        protected string DisplayName { get { return GetProperty<string>(); } set { SetProperty(value); } }

        #region Get/SetProperty

        /// <summary>
        /// <remarks><strong>DO NOT</strong> bind to properties that use this - use <see cref="ModelItem"/>.PropertyName instead!</remarks>
        /// </summary>
        protected T GetProperty<T>([CallerMemberName] string propertyName = null)
        {
            return _modelItem.GetProperty<T>(propertyName);
        }

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

        public abstract void Validate();

        #endregion

        void ClearErrors()
        {
            // Clearing errors will set ShowErrors to false
            Errors = null;
        }
    }
}