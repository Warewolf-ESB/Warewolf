/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace System.Windows.Controls
{
    [TemplatePart(Name = "SelectionAdapter", Type = typeof(ISelectionAdapter))]
    [TemplatePart(Name = "Selector", Type = typeof(Selector))]
    [TemplatePart(Name = "Text", Type = typeof(TextBox))]
    [TemplatePart(Name = "Popup", Type = typeof(Popup))]
    [StyleTypedProperty(Property = "TextBoxStyle", StyleTargetType = typeof(TextBox))]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ListBox))]
    [TemplateVisualState(Name = VisualStates.StateNormal, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateMouseOver, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StatePressed, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateDisabled, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateFocused, GroupName = VisualStates.GroupFocus)]
    [TemplateVisualState(Name = VisualStates.StateUnfocused, GroupName = VisualStates.GroupFocus)]
    [TemplateVisualState(Name = VisualStates.StatePopupClosed, GroupName = VisualStates.GroupPopup)]
    [TemplateVisualState(Name = VisualStates.StatePopupOpened, GroupName = VisualStates.GroupPopup)]
    [TemplateVisualState(Name = VisualStates.StateValid, GroupName = VisualStates.GroupValidation)]
    [TemplateVisualState(Name = VisualStates.StateInvalidFocused, GroupName = VisualStates.GroupValidation)]
    [TemplateVisualState(Name = VisualStates.StateInvalidUnfocused, GroupName = VisualStates.GroupValidation)]
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Large implementation keeps the components contained.")]
    [ContentProperty("ItemsSource")]
    public class AutoCompleteBox : Control, IUpdateVisualState
    {
        List<object> _items;
        ObservableCollection<object> _view;
        int _ignoreTextPropertyChange;
        bool _ignorePropertyChange;
        bool _ignoreTextSelectionChange;
        bool _skipSelectedItemTextUpdate;
        int _textSelectionStart;
        bool _userCalledPopulate;
        bool _popupHasOpened;
        DispatcherTimer _delayTimer;
        bool _allowWrite;
        internal InteractionHelper Interaction { get; set; }
        BindingEvaluator<string> _valueBindingEvaluator;

        public int MinimumPrefixLength
        {
            get => (int)GetValue(MinimumPrefixLengthProperty);
            set => SetValue(MinimumPrefixLengthProperty, value);
        }

        public static readonly DependencyProperty MinimumPrefixLengthProperty =
            DependencyProperty.Register(nameof(MinimumPrefixLength), typeof(int), typeof(AutoCompleteBox), new PropertyMetadata(1, OnMinimumPrefixLengthPropertyChanged));

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "MinimumPrefixLength is the name of the actual dependency property.")]
        static void OnMinimumPrefixLengthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (int)e.NewValue;

            if (newValue < 0 && newValue != -1)
            {
                throw new ArgumentOutOfRangeException(nameof(MinimumPrefixLength));
            }
        }

        [ExcludeFromCodeCoverage]
        public int MinimumPopulateDelay
        {
            get => (int)GetValue(MinimumPopulateDelayProperty);
            set => SetValue(MinimumPopulateDelayProperty, value);
        }

        public static readonly DependencyProperty MinimumPopulateDelayProperty =
            DependencyProperty.Register(nameof(MinimumPopulateDelay), typeof(int), typeof(AutoCompleteBox), new PropertyMetadata(OnMinimumPopulateDelayPropertyChanged));

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The exception is most likely to be called through the CLR property setter.")]
        [ExcludeFromCodeCoverage]
        static void OnMinimumPopulateDelayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;

            if (source != null && source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            var newValue = (int)e.NewValue;
            if (newValue < 0)
            {
                if (source != null)
                {
                    source._ignorePropertyChange = true;
                }
                d.SetValue(e.Property, e.OldValue);

                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    Dev2.Runtime.Configuration.Properties.Resources.AutoComplete_OnMinimumPopulateDelayPropertyChanged_InvalidValue, newValue));
            }

            SetupNewDelayTimer(source, newValue);
        }

        private static void SetupNewDelayTimer(AutoCompleteBox source, int newValue)
        {
            if (source?._delayTimer != null)
            {
                source._delayTimer.Stop();

                if (newValue == 0)
                {
                    source._delayTimer = null;
                }
            }

            if (source != null && newValue > 0 && source._delayTimer == null)
            {
                source._delayTimer = new DispatcherTimer();
                source._delayTimer.Tick += source.PopulateDropDown;
            }

            if (source != null && newValue > 0 && source._delayTimer != null)
            {
                source._delayTimer.Interval = TimeSpan.FromMilliseconds(newValue);
            }
        }

        public static readonly DependencyProperty DefaultTextTemplateProperty =
            DependencyProperty.Register(nameof(DefaultTextTemplate), typeof(DataTemplate), typeof(AutoCompleteBox), new UIPropertyMetadata(null));

        [ExcludeFromCodeCoverage]
        public DataTemplate DefaultTextTemplate
        {
            get => (DataTemplate)GetValue(DefaultTextTemplateProperty);
            set => SetValue(DefaultTextTemplateProperty, value);
        }

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register(nameof(DefaultText), typeof(object), typeof(AutoCompleteBox), new UIPropertyMetadata(null));

        public object DefaultText
        {
            get => GetValue(DefaultTextProperty);
            set => SetValue(DefaultTextProperty, value);
        }

        public static readonly DependencyProperty AllowUserInsertLineProperty =
            DependencyProperty.Register(nameof(AllowUserInsertLine), typeof(bool), typeof(AutoCompleteBox), new PropertyMetadata(true));

        public bool AllowUserInsertLine
        {
            get => (bool)GetValue(AllowUserInsertLineProperty);
            set => SetValue(AllowUserInsertLineProperty, value);
        }

        public bool IsTextCompletionEnabled
        {
            get => (bool)GetValue(IsTextCompletionEnabledProperty);
            set => SetValue(IsTextCompletionEnabledProperty, value);
        }

        public static readonly DependencyProperty IsTextCompletionEnabledProperty =
            DependencyProperty.Register(nameof(IsTextCompletionEnabled), typeof(bool), typeof(AutoCompleteBox), new PropertyMetadata(false, null));

        [ExcludeFromCodeCoverage]
        public DataTemplate ItemTemplate
        {
            get => GetValue(ItemTemplateProperty) as DataTemplate;
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(AutoCompleteBox), new PropertyMetadata(null));

        [ExcludeFromCodeCoverage]
        public Style ItemContainerStyle
        {
            get => GetValue(ItemContainerStyleProperty) as Style;
            set => SetValue(ItemContainerStyleProperty, value);
        }

        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register(nameof(ItemContainerStyle), typeof(Style), typeof(AutoCompleteBox), new PropertyMetadata(null, null));

        [ExcludeFromCodeCoverage]
        public Style TextBoxStyle
        {
            get => GetValue(TextBoxStyleProperty) as Style;
            set => SetValue(TextBoxStyleProperty, value);
        }

        public static readonly DependencyProperty TextBoxStyleProperty =
            DependencyProperty.Register(nameof(TextBoxStyle), typeof(Style), typeof(AutoCompleteBox), new PropertyMetadata(null));

        [ExcludeFromCodeCoverage]
        public double MaxDropDownHeight
        {
            get => (double)GetValue(MaxDropDownHeightProperty);
            set => SetValue(MaxDropDownHeightProperty, value);
        }

        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(AutoCompleteBox), new PropertyMetadata(double.PositiveInfinity, OnMaxDropDownHeightPropertyChanged));

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The exception will be called through a CLR setter in most cases.")]
        static void OnMaxDropDownHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;
            if (source != null && source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            var newValue = (double)e.NewValue;

            if (newValue < 0)
            {
                if (source != null)
                {
                    source._ignorePropertyChange = true;
                    source.SetValue(e.Property, e.OldValue);
                }

                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Dev2.Runtime.Configuration.Properties.Resources.AutoComplete_OnMaxDropDownHeightPropertyChanged_InvalidValue, e.NewValue));
            }

            source?.OnMaxDropDownHeightChanged(newValue);
        }

        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(AutoCompleteBox), new PropertyMetadata(false, OnIsDropDownOpenPropertyChanged));

        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        static void OnIsDropDownOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;

            if (source != null && source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (source != null)
            {
                if (newValue)
                {
                    source.TextUpdated(source.Text, true);
                }
                else
                {
                    source.ClosingDropDown(oldValue);
                }

                source.UpdateVisualState(true);
            }
        }

        public IEnumerable ItemsSource
        {
            get => GetValue(ItemsSourceProperty) as IEnumerable;
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(AutoCompleteBox), new PropertyMetadata(OnItemsSourcePropertyChanged));

        static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var autoComplete = d as AutoCompleteBox;
            autoComplete?.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(AutoCompleteBox), new PropertyMetadata(OnSelectedItemPropertyChanged));

        static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;
            if (source != null)
            {
                if (source._ignorePropertyChange)
                {
                    source._ignorePropertyChange = false;
                    return;
                }

                if (source._skipSelectedItemTextUpdate)
                {
                    source._skipSelectedItemTextUpdate = false;
                }
                else
                {
                    source.OnSelectedItemChanged(e.NewValue);
                }
            }

            var removed = new List<object>();
            if (e.OldValue != null)
            {
                removed.Add(e.OldValue);
            }

            var added = new List<object>();
            if (e.NewValue != null)
            {
                added.Add(e.NewValue);
            }

            source?.OnSelectionChanged(new SelectionChangedEventArgs(SelectionChangedEvent, removed, added));
        }

        void OnSelectedItemChanged(object newItem)
        {
            if (CustomSelection)
            {
                return;
            }
            var text = newItem == null ? SearchText : FormatValue(newItem, true);

            UpdateTextValue(text);

            if (TextBox != null && Text != null)
            {
                TextBox.SelectionStart = Text.Length;
            }
        }

        public bool CustomSelection { get; set; }

        public string Text
        {
            get => GetValue(TextProperty) as string;
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(AutoCompleteBox), new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;
            source?.TextUpdated((string)e.NewValue, false);
        }

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            private set
            {
                try
                {
                    _allowWrite = true;
                    SetValue(SearchTextProperty, value);
                }
                finally
                {
                    _allowWrite = false;
                }
            }
        }

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(AutoCompleteBox), new PropertyMetadata(string.Empty, OnSearchTextPropertyChanged));

        static void OnSearchTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;
            if (source != null && source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            if (source != null && !source._allowWrite)
            {
                source._ignorePropertyChange = true;
                source.SetValue(e.Property, e.OldValue);

                throw new InvalidOperationException(Dev2.Runtime.Configuration.Properties.Resources.AutoComplete_OnSearchTextPropertyChanged_InvalidWrite);
            }
        }

        public AutoCompleteFilterMode FilterMode
        {
            get => (AutoCompleteFilterMode)GetValue(FilterModeProperty);
            set => SetValue(FilterModeProperty, value);
        }

        public static readonly DependencyProperty FilterModeProperty =
            DependencyProperty.Register(nameof(FilterMode), typeof(AutoCompleteFilterMode), typeof(AutoCompleteBox), new PropertyMetadata(AutoCompleteFilterMode.StartsWith, OnFilterModePropertyChanged));

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The exception will be thrown when the CLR setter is used in most situations.")]
        [ExcludeFromCodeCoverage]
        static void OnFilterModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;
            var mode = (AutoCompleteFilterMode)e.NewValue;

            var modeNotContainingFilterMode = mode != AutoCompleteFilterMode.Contains;
            modeNotContainingFilterMode &= mode != AutoCompleteFilterMode.EqualsCaseSensitive;
            modeNotContainingFilterMode &= mode != AutoCompleteFilterMode.StartsWith;
            modeNotContainingFilterMode &= mode != AutoCompleteFilterMode.Custom;
            modeNotContainingFilterMode &= mode != AutoCompleteFilterMode.None;

            if (modeNotContainingFilterMode)
            {
                source?.SetValue(e.Property, e.OldValue);
                throw new ArgumentException(Dev2.Runtime.Configuration.Properties.Resources.AutoComplete_OnFilterModePropertyChanged_InvalidValue);
            }

            var newValue = (AutoCompleteFilterMode)e.NewValue;
            if (source != null)
            {
                source.TextFilter = AutoCompleteSearch.GetFilter(newValue);
            }
        }

        public AutoCompleteFilterPredicate<object> ItemFilter
        {
            get => GetValue(ItemFilterProperty) as AutoCompleteFilterPredicate<object>;
            set => SetValue(ItemFilterProperty, value);
        }

        public static readonly DependencyProperty ItemFilterProperty =
            DependencyProperty.Register(nameof(ItemFilter), typeof(AutoCompleteFilterPredicate<object>), typeof(AutoCompleteBox), new PropertyMetadata(OnItemFilterPropertyChanged));

        static void OnItemFilterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var autoCompleteBox = d as AutoCompleteBox;

            if (e.NewValue is AutoCompleteFilterPredicate<object> value)
            {
                if (autoCompleteBox != null)
                {
                    autoCompleteBox.FilterMode = AutoCompleteFilterMode.Custom;
                    autoCompleteBox.TextFilter = null;
                }
                return;
            }
            if (autoCompleteBox != null)
            {
                autoCompleteBox.FilterMode = AutoCompleteFilterMode.None;
            }
        }

        public AutoCompleteFilterPredicate<string> TextFilter
        {
            get => GetValue(TextFilterProperty) as AutoCompleteFilterPredicate<string>;
            set => SetValue(TextFilterProperty, value);
        }

        public static readonly DependencyProperty TextFilterProperty =
            DependencyProperty.Register(nameof(TextFilter), typeof(AutoCompleteFilterPredicate<string>), typeof(AutoCompleteBox), new PropertyMetadata(AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.StartsWith)));

        PopupHelper DropDownPopup { get; set; }
        TextBox _text;
        ISelectionAdapter _adapter;

        public TextBox TextBox
        {
            get => _text;
            set
            {
                if (_text != null)
                {
                    _text.SelectionChanged -= OnTextBoxSelectionChanged;
                    _text.TextChanged -= OnTextBoxTextChanged;
                }
                _text = value;
                if (_text != null)
                {
                    _text.SelectionChanged += OnTextBoxSelectionChanged;
                    _text.TextChanged += OnTextBoxTextChanged;

                    if (Text != null)
                    {
                        UpdateTextValue(Text);
                    }
                }
            }
        }

        protected internal ISelectionAdapter SelectionAdapter
        {
            get => _adapter;
            set
            {
                if (_adapter != null)
                {
                    _adapter.SelectionChanged -= OnAdapterSelectionChanged;
                    _adapter.Commit -= OnAdapterSelectionComplete;
                    _adapter.Cancel -= OnAdapterSelectionCanceled;
                    _adapter.Cancel -= OnAdapterSelectionComplete;
                    _adapter.ItemsSource = null;
                }

                _adapter = value;

                if (_adapter != null)
                {
                    _adapter.SelectionChanged += OnAdapterSelectionChanged;
                    _adapter.Commit += OnAdapterSelectionComplete;
                    _adapter.Cancel += OnAdapterSelectionCanceled;
                    _adapter.Cancel += OnAdapterSelectionComplete;
                    _adapter.ItemsSource = _view;
                }
            }
        }

        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(AutoCompleteBox), new PropertyMetadata(false));

        public bool HasError
        {
            get => (bool)GetValue(HasErrorProperty);
            set => SetValue(HasErrorProperty, value);
        }

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent(nameof(TextChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AutoCompleteBox));

        public event RoutedEventHandler TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        public static readonly RoutedEvent PopulatingEvent = EventManager.RegisterRoutedEvent(nameof(Populating), RoutingStrategy.Bubble, typeof(PopulatingEventHandler), typeof(AutoCompleteBox));

        public event PopulatingEventHandler Populating
        {
            add { AddHandler(PopulatingEvent, value); }
            remove { RemoveHandler(PopulatingEvent, value); }
        }

        public static readonly RoutedEvent PopulatedEvent = EventManager.RegisterRoutedEvent(nameof(Populated), RoutingStrategy.Bubble, typeof(PopulatedEventHandler), typeof(AutoCompleteBox));

        public event PopulatedEventHandler Populated
        {
            add { AddHandler(PopulatedEvent, value); }
            remove { RemoveHandler(PopulatedEvent, value); }
        }

        public static readonly RoutedEvent DropDownOpeningEvent = EventManager.RegisterRoutedEvent(nameof(DropDownOpening), RoutingStrategy.Bubble, typeof(RoutedPropertyChangingEventHandler<bool>), typeof(AutoCompleteBox));

        public event RoutedPropertyChangingEventHandler<bool> DropDownOpening
        {
            add { AddHandler(PopulatedEvent, value); }
            remove { RemoveHandler(PopulatedEvent, value); }
        }

        public static readonly RoutedEvent DropDownOpenedEvent = EventManager.RegisterRoutedEvent(nameof(DropDownOpened), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(AutoCompleteBox));

        public event RoutedPropertyChangedEventHandler<bool> DropDownOpened
        {
            add { AddHandler(DropDownOpenedEvent, value); }
            remove { RemoveHandler(DropDownOpenedEvent, value); }
        }

        public static readonly RoutedEvent DropDownClosingEvent = EventManager.RegisterRoutedEvent(nameof(DropDownClosing), RoutingStrategy.Bubble, typeof(RoutedPropertyChangingEventHandler<bool>), typeof(AutoCompleteBox));

        public event RoutedPropertyChangingEventHandler<bool> DropDownClosing
        {
            add { AddHandler(DropDownClosingEvent, value); }
            remove { RemoveHandler(DropDownClosingEvent, value); }
        }

        public static readonly RoutedEvent DropDownClosedEvent = EventManager.RegisterRoutedEvent(nameof(DropDownClosed), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(AutoCompleteBox));

        public event RoutedPropertyChangedEventHandler<bool> DropDownClosed
        {
            add { AddHandler(DropDownClosedEvent, value); }
            remove { RemoveHandler(DropDownClosedEvent, value); }
        }

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(AutoCompleteBox));

        public event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        public Binding ValueMemberBinding
        {
            get => _valueBindingEvaluator?.ValueBinding;
            set => _valueBindingEvaluator = new BindingEvaluator<string>(value);
        }

        public string ValueMemberPath
        {
            get => ValueMemberBinding?.Path.Path;
            set => ValueMemberBinding = value == null ? null : new Binding(value);
        }

        public ObservableCollection<object> View => _view;

        static AutoCompleteBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox), new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
        }

        public AutoCompleteBox()
        {
            IsEnabledChanged += ControlIsEnabledChanged;
            Interaction = new InteractionHelper(this);

            ClearView();
            if (Application.Current != null)
            {
                Style = Application.Current.TryFindResource("AutoCompleteBoxStyle") as Style;
            }
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var r = base.ArrangeOverride(arrangeBounds);
            DropDownPopup?.Arrange();
            return r;
        }

        public override void OnApplyTemplate()
        {
            if (TextBox != null)
            {
                TextBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
            }

            if (DropDownPopup != null)
            {
                DropDownPopup.Closed -= DropDownPopupClosed;
                DropDownPopup.FocusChanged -= OnDropDownFocusChanged;
                DropDownPopup.UpdateVisualStates -= OnDropDownPopupUpdateVisualStates;
                DropDownPopup.BeforeOnApplyTemplate();
                DropDownPopup = null;
            }

            base.OnApplyTemplate();

            if (GetTemplateChild("Popup") is Popup popup)
            {
                DropDownPopup = new PopupHelper(this, popup) { MaxDropDownHeight = MaxDropDownHeight };
                DropDownPopup.AfterOnApplyTemplate();
                DropDownPopup.Closed += DropDownPopupClosed;
                DropDownPopup.FocusChanged += OnDropDownFocusChanged;
                DropDownPopup.UpdateVisualStates += OnDropDownPopupUpdateVisualStates;
            }
            SelectionAdapter = GetSelectionAdapterPart();
            TextBox = GetTemplateChild("Text") as TextBox;

            if (TextBox != null)
            {
                TextBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
            }

            Interaction.OnApplyTemplateBase();

            if (IsDropDownOpen && DropDownPopup != null && !DropDownPopup.IsOpen)
            {
                OpeningDropDown(false);
            }
        }

        void OnDropDownPopupUpdateVisualStates(object sender, EventArgs e)
        {
            UpdateVisualState(true);
        }

        void OnDropDownFocusChanged(object sender, EventArgs e)
        {
            FocusChanged(HasFocus());
        }

        void ClosingDropDown(bool oldValue)
        {
            var delayedClosingVisual = false;
            if (DropDownPopup != null)
            {
                delayedClosingVisual = DropDownPopup.UsesClosingVisualState;
            }

            var args = new RoutedPropertyChangingEventArgs<bool>(IsDropDownOpenProperty, oldValue, false, true, DropDownClosingEvent);
            OnDropDownClosing(args);

            if (_view == null || _view.Count == 0)
            {
                delayedClosingVisual = false;
            }

            if (args.Cancel)
            {
                _ignorePropertyChange = true;
                SetValue(IsDropDownOpenProperty, oldValue);
            }
            else
            {
                RaiseExpandCollapseAutomationEvent(oldValue, false);
                if (!delayedClosingVisual)
                {
                    CloseDropDown(oldValue, false);
                }
            }

            UpdateVisualState(true);
        }

        void OpeningDropDown(bool oldValue)
        {
            var args = new RoutedPropertyChangingEventArgs<bool>(IsDropDownOpenProperty, oldValue, true, true, DropDownOpeningEvent);
            OnDropDownOpening(args);

            if (args.Cancel)
            {
                _ignorePropertyChange = true;
                SetValue(IsDropDownOpenProperty, oldValue);
            }
            else
            {
                RaiseExpandCollapseAutomationEvent(oldValue, true);
                OpenDropDown(oldValue, true);
            }

            UpdateVisualState(true);
        }

        void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
        {
            var peer = UIElementAutomationPeer.FromElement(this) as AutoCompleteBoxAutomationPeer;
            peer?.RaiseExpandCollapseAutomationEvent(oldValue, newValue);
        }

        void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        void DropDownPopupClosed(object sender, EventArgs e)
        {
            if (IsDropDownOpen)
            {
                IsDropDownOpen = false;
            }

            if (_popupHasOpened)
            {
                OnDropDownClosed(new RoutedPropertyChangedEventArgs<bool>(true, false, DropDownClosedEvent));
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer() => new AutoCompleteBoxAutomationPeer(this);

        void FocusChanged(bool hasFocus)
        {
            if (!hasFocus)
            {
                IsDropDownOpen = false;
                _userCalledPopulate = false;
                TextBox?.Select(TextBox.Text.Length, 0);
            }
        }

        protected bool HasFocus()
        {
            var focused = IsKeyboardFocusWithin ? Keyboard.FocusedElement as DependencyObject : FocusManager.GetFocusedElement(this) as DependencyObject;
            while (focused != null)
            {
                if (ReferenceEquals(focused, this))
                {
                    return true;
                }

                var parent = VisualTreeHelper.GetParent(focused);
                if (parent == null && focused is FrameworkElement element)
                {
                    parent = element.Parent;
                }

                focused = parent;
            }
            return false;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            FocusChanged(HasFocus());
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);
            if (!IsKeyboardFocusWithin)
            {
                IsDropDownOpen = false;
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            FocusChanged(HasFocus());
        }

        void ControlIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isEnabled = (bool)e.NewValue;
            if (!isEnabled)
            {
                IsDropDownOpen = false;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Following the GetTemplateChild pattern for the method.")]
        [ExcludeFromCodeCoverage]
        protected virtual ISelectionAdapter GetSelectionAdapterPart()
        {
            ISelectionAdapter adapter = null;
            if (GetTemplateChild("Selector") is Selector selector)
            {
                adapter = selector as ISelectionAdapter ?? new SelectorSelectionAdapter(selector);
            }

            return adapter ?? GetTemplateChild("SelectionAdapter") as ISelectionAdapter;
        }

        [ExcludeFromCodeCoverage]
        void PopulateDropDown(object sender, EventArgs e)
        {
            _delayTimer?.Stop();
            SearchText = Text;

            var populating = new PopulatingEventArgs(SearchText, PopulatingEvent);

            OnPopulating(populating);
            if (!populating.Cancel)
            {
                PopulateComplete();
            }
        }

        protected virtual void OnPopulating(PopulatingEventArgs e)
        {
            RaiseEvent(e);
        }

        [ExcludeFromCodeCoverage]
        protected virtual void OnPopulated(PopulatedEventArgs e)
        {
            RaiseEvent(e);
        }

        [ExcludeFromCodeCoverage]
        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            RaiseEvent(e);

            var box = (AutoCompleteBox)e.Source;
            var innerListBox = (ListBox)box?.Template?.FindName("Selector", box);
            innerListBox?.ScrollIntoView(innerListBox.SelectedItem);

            RaiseEvent(e);
        }

        [ExcludeFromCodeCoverage]
        protected virtual void OnDropDownOpening(RoutedPropertyChangingEventArgs<bool> e)
        {
            RaiseEvent(e);
        }

        [ExcludeFromCodeCoverage]
        protected virtual void OnDropDownOpened(RoutedPropertyChangedEventArgs<bool> e)
        {
            RaiseEvent(e);
        }

        [ExcludeFromCodeCoverage]
        protected virtual void OnDropDownClosing(RoutedPropertyChangingEventArgs<bool> e)
        {
            RaiseEvent(e);
        }

        protected virtual void OnDropDownClosed(RoutedPropertyChangedEventArgs<bool> e)
        {
            RaiseEvent(e);
        }

        string FormatValue(object value, bool clearDataContext)
        {
            var str = FormatValue(value);
            if (clearDataContext)
            {
                _valueBindingEvaluator?.ClearDataContext();
            }
            return str;
        }

        protected virtual string FormatValue(object value)
        {
            if (_valueBindingEvaluator != null)
            {
                return _valueBindingEvaluator.GetDynamicValue(value) ?? string.Empty;
            }

            return value?.ToString() ?? string.Empty;
        }

        protected virtual void OnTextChanged(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            TextUpdated(_text.Text, true);
        }

        void OnTextBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_ignoreTextSelectionChange)
            {
                return;
            }

            _textSelectionStart = _text.SelectionStart;
        }

        void UpdateTextValue(string value)
        {
            UpdateTextValue(value, null);
        }

        void UpdateTextValue(string value, bool? userInitiated)
        {
            var textUpdated = false;

            if ((userInitiated is null || userInitiated == true) && Text != value)
            {
                _ignoreTextPropertyChange++;
                Text = value;
                textUpdated = true;
            }

            if (TextBox is null)
            {
                Text = value;
                textUpdated = true;
            }

            var textBoxUpdated = UpdateTextBoxValue(value, userInitiated);

            if (textUpdated || textBoxUpdated)
            {
                OnTextChanged(new RoutedEventArgs(TextChangedEvent));
            }
        }

        private bool UpdateTextBoxValue(string value, bool? userInitiated)
        {
            if ((userInitiated is null || userInitiated == false) && TextBox != null && TextBox.Text != value)
            {
                _ignoreTextPropertyChange++;
                TextBox.Text = value ?? string.Empty;

                if (Text == value || Text is null)
                {
                    return true;
                }
            }
            return false;
        }

        void TextUpdated(string newText, bool userInitiated)
        {
            if (_ignoreTextPropertyChange > 0)
            {
                _ignoreTextPropertyChange--;
                return;
            }

            var text = newText;

            if (text == null)
            {
                text = string.Empty;
            }

            if (IsTextCompletionEnabled && TextBox != null && TextBox.SelectionLength > 0 && TextBox.SelectionStart != TextBox.Text.Length)
            {
                return;
            }

            var populateReady = text.Length >= MinimumPrefixLength && MinimumPrefixLength >= 0;
            _userCalledPopulate = populateReady && userInitiated;

            UpdateTextValue(text, userInitiated);

            if (populateReady)
            {
                PopulateReady();
                return;
            }
            PopulateNotReady();
        }

        private void PopulateReady()
        {
            _ignoreTextSelectionChange = true;

            if (_delayTimer != null)
            {
                _delayTimer.Start();
            }
            else
            {
                PopulateDropDown(this, EventArgs.Empty);
            }
        }

        private void PopulateNotReady()
        {
            SearchText = string.Empty;
            if (SelectedItem != null)
            {
                _skipSelectedItemTextUpdate = true;
            }
            SelectedItem = null;
            if (IsDropDownOpen)
            {
                IsDropDownOpen = false;
            }
        }

        public void PopulateComplete()
        {
            RefreshView();

            var populated = new PopulatedEventArgs(new ReadOnlyCollection<object>(_view), PopulatedEvent);
            OnPopulated(populated);

            if (SelectionAdapter != null && !SelectionAdapter.ItemsSource.Equals(_view))
            {
                SelectionAdapter.ItemsSource = _view;
            }

            var isDropDownOpen = _userCalledPopulate && _view.Count > 0;
            if (isDropDownOpen != IsDropDownOpen)
            {
                _ignorePropertyChange = true;
                IsDropDownOpen = isDropDownOpen;
            }
            if (IsDropDownOpen)
            {
                OpeningDropDown(false);
                DropDownPopup?.Arrange();
            }
            else
            {
                ClosingDropDown(true);
            }

            UpdateTextCompletion(_userCalledPopulate);
        }

        void UpdateTextCompletion(bool userInitiated)
        {
            object newSelectedItem = null;
            var text = Text;
            if (_view.Count > 0)
            {
                if (IsTextCompletionEnabled && TextBox != null && userInitiated)
                {
                    var currentLength = TextBox.Text.Length;
                    var selectionStart = TextBox.SelectionStart;
                    newSelectedItem = UpdateSelection(newSelectedItem, text, currentLength, selectionStart);
                }
                else
                {
                    newSelectedItem = TryGetMatch(text, _view, AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.EqualsCaseSensitive));
                }
            }
            if (SelectedItem != newSelectedItem)
            {
                _skipSelectedItemTextUpdate = true;
            }
            SelectedItem = newSelectedItem;
            if (_ignoreTextSelectionChange)
            {
                _ignoreTextSelectionChange = false;
                if (TextBox != null)
                {
                    _textSelectionStart = TextBox.SelectionStart;
                }
            }
        }

        private object UpdateSelection(object newSelectedItem, string text, int currentLength, int selectionStart)
        {
            var selectedItem = newSelectedItem;
            if (selectionStart == text.Length && selectionStart > _textSelectionStart)
            {
                var top = FilterMode == AutoCompleteFilterMode.StartsWith || FilterMode == AutoCompleteFilterMode.StartsWithCaseSensitive
                    ? _view[0]
                    : TryGetMatch(text, _view, AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.StartsWith));
                if (top != null)
                {
                    selectedItem = top;
                    var topString = FormatValue(top, true);
                    var minLength = Math.Min(topString.Length, Text.Length);
                    if (AutoCompleteSearch.Equals(Text.Substring(0, minLength), topString.Substring(0, minLength)))
                    {
                        UpdateTextValue(topString);
                        TextBox.SelectionStart = currentLength;
                        TextBox.SelectionLength = topString.Length - currentLength;
                    }
                }
            }

            return selectedItem;
        }

        object TryGetMatch(string searchText, ObservableCollection<object> view, AutoCompleteFilterPredicate<string> predicate)
        {
            if (view != null && view.Count > 0)
            {
                return view.FirstOrDefault(o => predicate?.Invoke(searchText, FormatValue(o)) ?? default(bool));
            }

            return null;
        }

        void ClearView()
        {
            if (_view == null)
            {
                _view = new ObservableCollection<object>();
            }
            else
            {
                _view.Clear();
            }
        }

        void RefreshView()
        {
            if (_items == null)
            {
                ClearView();
                return;
            }

            _view.Clear();

            var filteredItems = GetFilteredItems();
            foreach (var item in filteredItems)
            {
                _view.Add(item);
            }

            _valueBindingEvaluator?.ClearDataContext();
        }

        private IEnumerable<object> GetFilteredItems()
        {
            var text = Text ?? string.Empty;
            var stringFiltering = TextFilter != null;
            var objectFiltering = FilterMode == AutoCompleteFilterMode.Custom && TextFilter == null;

            var filteredItems = _items.Where(item =>
            {
                var isAutoCompleteMatch = !(stringFiltering || objectFiltering);
                if (!isAutoCompleteMatch)
                {
                    isAutoCompleteMatch = stringFiltering ? TextFilter?.Invoke(text, FormatValue(item)) ?? default(bool) : ItemFilter?.Invoke(text, item) ?? default(bool);
                }
                return isAutoCompleteMatch;
            });
            return filteredItems;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldValue", Justification = "This makes it easy to add validation or other changes in the future.")]
        [ExcludeFromCodeCoverage]
        void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            _items = newValue == null ? null : new List<object>(newValue.Cast<object>().ToList());
            ClearView();
            if (SelectionAdapter != null && !SelectionAdapter.ItemsSource.Equals(_view))
            {
                SelectionAdapter.ItemsSource = _view;
            }
            if (IsDropDownOpen)
            {
                RefreshView();
            }
        }

        [ExcludeFromCodeCoverage]
        void OnAdapterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = _adapter.SelectedItem;
        }

        [ExcludeFromCodeCoverage]
        void OnAdapterSelectionComplete(object sender, RoutedEventArgs e)
        {
            IsDropDownOpen = false;
            UpdateTextCompletion(false);
            TextBox?.Select(TextBox.Text.Length, 0);

            if (TextBox != null)
            {
                Keyboard.Focus(TextBox);
            }
            else
            {
                Focus();
            }
        }

        [ExcludeFromCodeCoverage]
        void OnAdapterSelectionCanceled(object sender, RoutedEventArgs e)
        {
            UpdateTextValue(SearchText);
            UpdateTextCompletion(false);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newValue", Justification = "This makes it easy to add validation or other changes in the future.")]
        [ExcludeFromCodeCoverage]
        void OnMaxDropDownHeightChanged(double newValue)
        {
            if (DropDownPopup != null)
            {
                DropDownPopup.MaxDropDownHeight = newValue;
                DropDownPopup.Arrange();
            }
            UpdateVisualState(true);
        }

        void OpenDropDown(bool oldValue, bool newValue)
        {
            if (DropDownPopup != null)
            {
                DropDownPopup.IsOpen = true;
            }
            _popupHasOpened = true;
            OnDropDownOpened(new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue, DropDownOpenedEvent));
        }

        protected void CloseDropDown(bool oldValue, bool newValue)
        {
            if (_popupHasOpened)
            {
                if (SelectionAdapter != null)
                {
                    SelectionAdapter.SelectedItem = null;
                }
                if (DropDownPopup != null)
                {
                    DropDownPopup.IsOpen = false;
                }
                OnDropDownClosed(new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue, DropDownClosedEvent));
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException();
            }

            base.OnKeyDown(e);

            if (e.Handled || !IsEnabled)
            {
                return;
            }
            if (IsDropDownOpen)
            {
                if (SelectionAdapter != null)
                {
                    SelectionAdapter.HandleKeyDown(e);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                switch (e.Key)
                {
                    case Key.F4:
                        IsDropDownOpen = !IsDropDownOpen;
                        e.Handled = true;
                        break;
                    case Key.Enter:
                    case Key.Escape:
                        OnAdapterSelectionComplete(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    default:
                        e.Handled = false;
                        break;
                }
            }
        }

        void IUpdateVisualState.UpdateVisualState(bool useTransitions)
        {
            UpdateVisualState(useTransitions);
        }

        internal virtual void UpdateVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsDropDownOpen ? VisualStates.StatePopupOpened : VisualStates.StatePopupClosed, useTransitions);
            Interaction.UpdateVisualStateBase(useTransitions);
        }
    }
}