#pragma warning disable RECS009, S104, CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
    [TemplatePart(Name = ElementSelectionAdapter, Type = typeof(ISelectionAdapter))]
    [TemplatePart(Name = ElementSelector, Type = typeof(Selector))]
    [TemplatePart(Name = ElementTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = ElementPopup, Type = typeof(Popup))]
    [StyleTypedProperty(Property = ElementTextBoxStyle, StyleTargetType = typeof(TextBox))]
    [StyleTypedProperty(Property = ElementItemContainerStyle, StyleTargetType = typeof(ListBox))]
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
        #region Template part and style names

        const string ElementSelectionAdapter = "SelectionAdapter";
        const string ElementSelector = "Selector";
        const string ElementPopup = "Popup";
        const string ElementTextBox = "Text";
        const string ElementTextBoxStyle = "TextBoxStyle";
        const string ElementItemContainerStyle = "ItemContainerStyle";

        #endregion

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

        #region public int MinimumPrefixLength

        public int MinimumPrefixLength
        {
            get { return (int)GetValue(MinimumPrefixLengthProperty); }
            set { SetValue(MinimumPrefixLengthProperty, value); }
        }

        public static readonly DependencyProperty MinimumPrefixLengthProperty =
            DependencyProperty.Register(
                "MinimumPrefixLength",
                typeof(int),
                typeof(AutoCompleteBox),
                new PropertyMetadata(1, OnMinimumPrefixLengthPropertyChanged));

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "MinimumPrefixLength is the name of the actual dependency property.")]
        static void OnMinimumPrefixLengthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (int)e.NewValue;

            if (newValue < 0 && newValue != -1)
            {

                throw new ArgumentOutOfRangeException("MinimumPrefixLength");
            }
        }
        #endregion public int MinimumPrefixLength

        #region public int MinimumPopulateDelay

        [ExcludeFromCodeCoverage]
        public int MinimumPopulateDelay
        {
            get { return (int)GetValue(MinimumPopulateDelayProperty); }
            set { SetValue(MinimumPopulateDelayProperty, value); }
        }

        public static readonly DependencyProperty MinimumPopulateDelayProperty =
            DependencyProperty.Register(
                "MinimumPopulateDelay",
                typeof(int),
                typeof(AutoCompleteBox),
                new PropertyMetadata(OnMinimumPopulateDelayPropertyChanged));

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

                    Dev2.Runtime.Configuration.Properties.Resources.AutoComplete_OnMinimumPopulateDelayPropertyChanged_InvalidValue, newValue), "value");
            }

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
        #endregion public int MinimumPopulateDelay

        public static readonly DependencyProperty DefaultTextTemplateProperty = DependencyProperty.Register("DefaultTextTemplate", typeof(DataTemplate), typeof(AutoCompleteBox), new UIPropertyMetadata(null));
        [ExcludeFromCodeCoverage]
        public DataTemplate DefaultTextTemplate
        {
            get
            {
                return (DataTemplate)GetValue(DefaultTextTemplateProperty);
            }
            set
            {
                SetValue(DefaultTextTemplateProperty, value);
            }
        }

        public static readonly DependencyProperty DefaultTextProperty = DependencyProperty.Register("DefaultText", typeof(object), typeof(AutoCompleteBox), new UIPropertyMetadata(null));

        public object DefaultText
        {
            get
            {
                return GetValue(DefaultTextProperty);
            }
            set
            {
                SetValue(DefaultTextProperty, value);
            }
        }

        public static readonly DependencyProperty AllowUserInsertLineProperty = DependencyProperty.Register("AllowUserInsertLine", typeof(bool), typeof(AutoCompleteBox), new PropertyMetadata(true));

        public bool AllowUserInsertLine
        {
            get
            {
                return (bool)GetValue(AllowUserInsertLineProperty);
            }
            set
            {
                SetValue(AllowUserInsertLineProperty, value);
            }
        }

        #region public bool IsTextCompletionEnabled

        public bool IsTextCompletionEnabled
        {
            get { return (bool)GetValue(IsTextCompletionEnabledProperty); }
            set { SetValue(IsTextCompletionEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsTextCompletionEnabledProperty =
            DependencyProperty.Register(
                "IsTextCompletionEnabled",
                typeof(bool),
                typeof(AutoCompleteBox),
                new PropertyMetadata(false, null));

        #endregion public bool IsTextCompletionEnabled

        #region public DataTemplate ItemTemplate

        [ExcludeFromCodeCoverage]
        public DataTemplate ItemTemplate
        {
            get { return GetValue(ItemTemplateProperty) as DataTemplate; }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                "ItemTemplate",
                typeof(DataTemplate),
                typeof(AutoCompleteBox),
                new PropertyMetadata(null));

        #endregion public DataTemplate ItemTemplate

        #region public Style ItemContainerStyle

        [ExcludeFromCodeCoverage]
        public Style ItemContainerStyle
        {
            get { return GetValue(ItemContainerStyleProperty) as Style; }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register(
                ElementItemContainerStyle,
                typeof(Style),
                typeof(AutoCompleteBox),
                new PropertyMetadata(null, null));

        #endregion public Style ItemContainerStyle

        #region public Style TextBoxStyle

        [ExcludeFromCodeCoverage]
        public Style TextBoxStyle
        {
            get { return GetValue(TextBoxStyleProperty) as Style; }
            set { SetValue(TextBoxStyleProperty, value); }
        }

        public static readonly DependencyProperty TextBoxStyleProperty =
            DependencyProperty.Register(
                ElementTextBoxStyle,
                typeof(Style),
                typeof(AutoCompleteBox),
                new PropertyMetadata(null));

        #endregion public Style TextBoxStyle

        #region public double MaxDropDownHeight

        [ExcludeFromCodeCoverage]
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register(
                "MaxDropDownHeight",
                typeof(double),
                typeof(AutoCompleteBox),
                new PropertyMetadata(double.PositiveInfinity, OnMaxDropDownHeightPropertyChanged));

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


                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Dev2.Runtime.Configuration.Properties.Resources.AutoComplete_OnMaxDropDownHeightPropertyChanged_InvalidValue, e.NewValue), "value");
            }

            source?.OnMaxDropDownHeightChanged(newValue);
        }
        #endregion public double MaxDropDownHeight

        #region public bool IsDropDownOpen

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(
                "IsDropDownOpen",
                typeof(bool),
                typeof(AutoCompleteBox),
                new PropertyMetadata(false, OnIsDropDownOpenPropertyChanged));

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
        #endregion public bool IsDropDownOpen

        #region public IEnumerable ItemsSource

        public IEnumerable ItemsSource
        {
            get { return GetValue(ItemsSourceProperty) as IEnumerable; }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(AutoCompleteBox),
                new PropertyMetadata(OnItemsSourcePropertyChanged));

        static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var autoComplete = d as AutoCompleteBox;
            autoComplete?.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        #endregion public IEnumerable ItemsSource

        #region public object SelectedItem

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(object),
                typeof(AutoCompleteBox),
                new PropertyMetadata(OnSelectedItemPropertyChanged));

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

            source?.OnSelectionChanged(new SelectionChangedEventArgs(
#if !SILVERLIGHT
                SelectionChangedEvent,
#endif
                removed,
                added));
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

        #endregion public object SelectedItem

        public bool CustomSelection { get; set; }

        #region public string Text

        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(AutoCompleteBox),
                new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;
            source?.TextUpdated((string)e.NewValue, false);
        }

        #endregion public string Text

        #region public string SearchText

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }

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
            DependencyProperty.Register(
                "SearchText",
                typeof(string),
                typeof(AutoCompleteBox),
                new PropertyMetadata(string.Empty, OnSearchTextPropertyChanged));

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
        #endregion public string SearchText

        #region public AutoCompleteFilterMode FilterMode

        public AutoCompleteFilterMode FilterMode
        {
            get { return (AutoCompleteFilterMode)GetValue(FilterModeProperty); }
            set { SetValue(FilterModeProperty, value); }
        }

        public static readonly DependencyProperty FilterModeProperty =
            DependencyProperty.Register(
                "FilterMode",
                typeof(AutoCompleteFilterMode),
                typeof(AutoCompleteBox),
                new PropertyMetadata(AutoCompleteFilterMode.StartsWith, OnFilterModePropertyChanged));

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The exception will be thrown when the CLR setter is used in most situations.")]
        [ExcludeFromCodeCoverage]
        static void OnFilterModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;
            var mode = (AutoCompleteFilterMode)e.NewValue;

            if (mode != AutoCompleteFilterMode.Contains &&
                mode != AutoCompleteFilterMode.EqualsCaseSensitive &&
                mode != AutoCompleteFilterMode.StartsWith &&
                mode != AutoCompleteFilterMode.Custom &&
                mode != AutoCompleteFilterMode.None)
            {
                source?.SetValue(e.Property, e.OldValue);
                throw new ArgumentException(Dev2.Runtime.Configuration.Properties.Resources.AutoComplete_OnFilterModePropertyChanged_InvalidValue, "value");
            }

            var newValue = (AutoCompleteFilterMode)e.NewValue;
            if (source != null)
            {
                source.TextFilter = AutoCompleteSearch.GetFilter(newValue);
            }
        }
        #endregion public AutoCompleteFilterMode FilterMode

        #region public AutoCompleteFilterPredicate ItemFilter

        public AutoCompleteFilterPredicate<object> ItemFilter
        {
            get { return GetValue(ItemFilterProperty) as AutoCompleteFilterPredicate<object>; }
            set { SetValue(ItemFilterProperty, value); }
        }

        public static readonly DependencyProperty ItemFilterProperty =
            DependencyProperty.Register(
                "ItemFilter",
                typeof(AutoCompleteFilterPredicate<object>),
                typeof(AutoCompleteBox),
                new PropertyMetadata(OnItemFilterPropertyChanged));

        static void OnItemFilterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as AutoCompleteBox;
            var value = e.NewValue as AutoCompleteFilterPredicate<object>;

            if (value == null)
            {
                if (source != null)
                {
                    source.FilterMode = AutoCompleteFilterMode.None;
                }
            }
            else
            {
                if (source != null)
                {
                    source.FilterMode = AutoCompleteFilterMode.Custom;
                    source.TextFilter = null;
                }
            }
        }
        #endregion public AutoCompleteFilterPredicate ItemFilter

        #region public AutoCompleteStringFilterPredicate TextFilter

        public AutoCompleteFilterPredicate<string> TextFilter
        {
            get { return GetValue(TextFilterProperty) as AutoCompleteFilterPredicate<string>; }
            set { SetValue(TextFilterProperty, value); }
        }

        public static readonly DependencyProperty TextFilterProperty =
            DependencyProperty.Register(
                "TextFilter",
                typeof(AutoCompleteFilterPredicate<string>),
                typeof(AutoCompleteBox),
                new PropertyMetadata(AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.StartsWith)));
        #endregion public AutoCompleteStringFilterPredicate TextFilter

        #region Template parts

        PopupHelper DropDownPopup { get; set; }
        TextBox _text;
        ISelectionAdapter _adapter;
        public TextBox TextBox
        {
            get { return _text; }
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
            get { return _adapter; }
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

        #endregion

        public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register("HasError", typeof(bool), typeof(AutoCompleteBox), new PropertyMetadata(false));

        public bool HasError
        {
            get
            {
                return (bool)GetValue(HasErrorProperty);
            }
            set
            {
                SetValue(HasErrorProperty, value);
            }
        }

#if SILVERLIGHT
        public event RoutedEventHandler TextChanged;
#else
        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AutoCompleteBox));

        public event RoutedEventHandler TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }
#endif

#if SILVERLIGHT
        public event PopulatingEventHandler Populating;
#else
        public static readonly RoutedEvent PopulatingEvent = EventManager.RegisterRoutedEvent("Populating", RoutingStrategy.Bubble, typeof(PopulatingEventHandler), typeof(AutoCompleteBox));

        public event PopulatingEventHandler Populating
        {
            add { AddHandler(PopulatingEvent, value); }
            remove { RemoveHandler(PopulatingEvent, value); }
        }
#endif

#if SILVERLIGHT
        public event PopulatedEventHandler Populated;
#else
        public static readonly RoutedEvent PopulatedEvent = EventManager.RegisterRoutedEvent("Populated", RoutingStrategy.Bubble, typeof(PopulatedEventHandler), typeof(AutoCompleteBox));

        public event PopulatedEventHandler Populated
        {
            add { AddHandler(PopulatedEvent, value); }
            remove { RemoveHandler(PopulatedEvent, value); }
        }
#endif

#if SILVERLIGHT
        public event RoutedPropertyChangingEventHandler<bool> DropDownOpening;
#else
        public static readonly RoutedEvent DropDownOpeningEvent = EventManager.RegisterRoutedEvent("DropDownOpening", RoutingStrategy.Bubble, typeof(RoutedPropertyChangingEventHandler<bool>), typeof(AutoCompleteBox));

        public event RoutedPropertyChangingEventHandler<bool> DropDownOpening
        {
            add { AddHandler(PopulatedEvent, value); }
            remove { RemoveHandler(PopulatedEvent, value); }
        }
#endif

#if SILVERLIGHT
        public event RoutedPropertyChangedEventHandler<bool> DropDownOpened;
#else
        public static readonly RoutedEvent DropDownOpenedEvent = EventManager.RegisterRoutedEvent("DropDownOpened", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(AutoCompleteBox));

        public event RoutedPropertyChangedEventHandler<bool> DropDownOpened
        {
            add { AddHandler(DropDownOpenedEvent, value); }
            remove { RemoveHandler(DropDownOpenedEvent, value); }
        }
#endif

#if SILVERLIGHT
        public event RoutedPropertyChangingEventHandler<bool> DropDownClosing;
#else
        public static readonly RoutedEvent DropDownClosingEvent = EventManager.RegisterRoutedEvent("DropDownClosing", RoutingStrategy.Bubble, typeof(RoutedPropertyChangingEventHandler<bool>), typeof(AutoCompleteBox));

        public event RoutedPropertyChangingEventHandler<bool> DropDownClosing
        {
            add { AddHandler(DropDownClosingEvent, value); }
            remove { RemoveHandler(DropDownClosingEvent, value); }
        }
#endif

#if SILVERLIGHT
        public event RoutedPropertyChangedEventHandler<bool> DropDownClosed;
#else
        public static readonly RoutedEvent DropDownClosedEvent = EventManager.RegisterRoutedEvent("DropDownClosed", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(AutoCompleteBox));

        public event RoutedPropertyChangedEventHandler<bool> DropDownClosed
        {
            add { AddHandler(DropDownClosedEvent, value); }
            remove { RemoveHandler(DropDownClosedEvent, value); }
        }
#endif

#if SILVERLIGHT
        public event SelectionChangedEventHandler SelectionChanged;
#else
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(AutoCompleteBox));

        public event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }
#endif

        public Binding ValueMemberBinding
        {
            get
            {
                return _valueBindingEvaluator?.ValueBinding;
            }
            set
            {
                _valueBindingEvaluator = new BindingEvaluator<string>(value);
            }
        }

        public string ValueMemberPath
        {
            get
            {
                return ValueMemberBinding?.Path.Path;
            }
            set
            {
                ValueMemberBinding = value == null ? null : new Binding(value);
            }
        }

        public ObservableCollection<object> View => _view;

#if !SILVERLIGHT

        static AutoCompleteBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox), new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
        }
#endif

        public AutoCompleteBox()
        {
#if SILVERLIGHT  
            DefaultStyleKey = typeof(AutoCompleteBox);

            Loaded += (sender, e) => ApplyTemplate();
#endif
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
#if !SILVERLIGHT
            if (TextBox != null)
            {
                TextBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
            }
#endif
            if (DropDownPopup != null)
            {
                DropDownPopup.Closed -= DropDownPopupClosed;
                DropDownPopup.FocusChanged -= OnDropDownFocusChanged;
                DropDownPopup.UpdateVisualStates -= OnDropDownPopupUpdateVisualStates;
                DropDownPopup.BeforeOnApplyTemplate();
                DropDownPopup = null;
            }

            base.OnApplyTemplate();

            if (GetTemplateChild(ElementPopup) is Popup popup)
            {
                DropDownPopup = new PopupHelper(this, popup) { MaxDropDownHeight = MaxDropDownHeight };
                DropDownPopup.AfterOnApplyTemplate();
                DropDownPopup.Closed += DropDownPopupClosed;
                DropDownPopup.FocusChanged += OnDropDownFocusChanged;
                DropDownPopup.UpdateVisualStates += OnDropDownPopupUpdateVisualStates;
            }
            SelectionAdapter = GetSelectionAdapterPart();
            TextBox = GetTemplateChild(ElementTextBox) as TextBox;
#if !SILVERLIGHT
            if (TextBox != null)
            {
                TextBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
            }
#endif
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

#if SILVERLIGHT
            RoutedPropertyChangingEventArgs<bool> args = new RoutedPropertyChangingEventArgs<bool>(IsDropDownOpenProperty, oldValue, false, true);
#else
            var args = new RoutedPropertyChangingEventArgs<bool>(IsDropDownOpenProperty, oldValue, false, true, DropDownClosingEvent);
#endif

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
#if SILVERLIGHT
            RoutedPropertyChangingEventArgs<bool> args = new RoutedPropertyChangingEventArgs<bool>(IsDropDownOpenProperty, oldValue, true, true);
#else
            var args = new RoutedPropertyChangingEventArgs<bool>(IsDropDownOpenProperty, oldValue, true, true, DropDownOpeningEvent);
#endif
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

#if !SILVERLIGHT

        void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

#endif

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

        #region Focus

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

        #endregion

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
            if (GetTemplateChild(ElementSelector) is Selector selector)
            {
                adapter = selector as ISelectionAdapter ?? new SelectorSelectionAdapter(selector);
            }

            return adapter ?? GetTemplateChild(ElementSelectionAdapter) as ISelectionAdapter;
        }

        [ExcludeFromCodeCoverage]
        void PopulateDropDown(object sender, EventArgs e)
        {
            _delayTimer?.Stop();
            SearchText = Text;
#if SILVERLIGHT
            PopulatingEventArgs populating = new PopulatingEventArgs(SearchText);
#else
            var populating = new PopulatingEventArgs(SearchText, PopulatingEvent);
#endif

            OnPopulating(populating);
            if (!populating.Cancel)
            {
                PopulateComplete();
            }
        }

        protected virtual void OnPopulating(PopulatingEventArgs e)
        {
#if SILVERLIGHT
            PopulatingEventHandler handler = Populating;
            if (handler != null)
            {
                handler(this, e);
            }
#else
            RaiseEvent(e);
#endif
        }

        [ExcludeFromCodeCoverage]
        protected virtual void OnPopulated(PopulatedEventArgs e)
        {
#if SILVERLIGHT
            PopulatedEventHandler handler = Populated;
            if (handler != null)
            {
                handler(this, e);
            }
#else
            RaiseEvent(e);
#endif
        }

        [ExcludeFromCodeCoverage]
        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
#if SILVERLIGHT
            SelectionChangedEventHandler handler = SelectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
#else
            RaiseEvent(e);
#endif

            var box = (AutoCompleteBox)e.Source;
            var innerListBox = (ListBox)box?.Template?.FindName("Selector", box);
            innerListBox?.ScrollIntoView(innerListBox.SelectedItem);

            RaiseEvent(e);
        }

        [ExcludeFromCodeCoverage]
        protected virtual void OnDropDownOpening(RoutedPropertyChangingEventArgs<bool> e)
        {
#if SILVERLIGHT
            RoutedPropertyChangingEventHandler<bool> handler = DropDownOpening;
            if (handler != null)
            {
                handler(this, e);
            }
#else
            RaiseEvent(e);
#endif
        }

        [ExcludeFromCodeCoverage]
        protected virtual void OnDropDownOpened(RoutedPropertyChangedEventArgs<bool> e)
        {
#if SILVERLIGHT
            RoutedPropertyChangedEventHandler<bool> handler = DropDownOpened;
            if (handler != null)
            {
                handler(this, e);
            }
#else
            RaiseEvent(e);
#endif
        }

        [ExcludeFromCodeCoverage]
        protected virtual void OnDropDownClosing(RoutedPropertyChangingEventArgs<bool> e)
        {
#if SILVERLIGHT
            RoutedPropertyChangingEventHandler<bool> handler = DropDownClosing;
            if (handler != null)
            {
                handler(this, e);
            }
#else
            RaiseEvent(e);
#endif
        }

        protected virtual void OnDropDownClosed(RoutedPropertyChangedEventArgs<bool> e)
        {
#if SILVERLIGHT
            RoutedPropertyChangedEventHandler<bool> handler = DropDownClosed;
            if (handler != null)
            {
                handler(this, e);
            }
#else
            RaiseEvent(e);
#endif
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
#if SILVERLIGHT
            RoutedEventHandler handler = TextChanged;
            if (handler != null)
            {
                handler(this, e);
            }
#else
            RaiseEvent(e);
#endif
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
            if ((userInitiated == null || userInitiated == true) && Text != value)
            {
                _ignoreTextPropertyChange++;
                Text = value;
#if SILVERLIGHT
                OnTextChanged(new RoutedEventArgs());
#else
                OnTextChanged(new RoutedEventArgs(TextChangedEvent));
#endif
            }

            if (TextBox == null)
            {
                Text = value;
                OnTextChanged(new RoutedEventArgs(TextChangedEvent));
            }

            if ((userInitiated == null || userInitiated == false) && TextBox != null && TextBox.Text != value)
            {
                _ignoreTextPropertyChange++;
                TextBox.Text = value ?? string.Empty;

                if (Text == value || Text == null)
                {
#if SILVERLIGHT
                    OnTextChanged(new RoutedEventArgs());
#else
                    OnTextChanged(new RoutedEventArgs(TextChangedEvent));
#endif
                }
            }
        }

        void TextUpdated(string newText, bool userInitiated)
        {
            if (_ignoreTextPropertyChange > 0)
            {
                _ignoreTextPropertyChange--;
                return;
            }

            if (newText == null)
            {
                newText = string.Empty;
            }

            if (IsTextCompletionEnabled && TextBox != null && TextBox.SelectionLength > 0 && TextBox.SelectionStart != TextBox.Text.Length)
            {
                return;
            }

            var populateReady = newText.Length >= MinimumPrefixLength && MinimumPrefixLength >= 0;
            _userCalledPopulate = populateReady && userInitiated;

            UpdateTextValue(newText, userInitiated);

            if (populateReady)
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
            else
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
        }

        public void PopulateComplete()
        {
            RefreshView();

#if SILVERLIGHT
            PopulatedEventArgs populated = new PopulatedEventArgs(new ReadOnlyCollection<object>(_view));
#else
            var populated = new PopulatedEventArgs(new ReadOnlyCollection<object>(_view), PopulatedEvent);
#endif
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
            if (selectionStart == text.Length && selectionStart > _textSelectionStart)
            {
                var top = FilterMode == AutoCompleteFilterMode.StartsWith || FilterMode == AutoCompleteFilterMode.StartsWithCaseSensitive
                    ? _view[0]
                    : TryGetMatch(text, _view, AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.StartsWith));
                if (top != null)
                {
                    newSelectedItem = top;
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

            return newSelectedItem;
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

            var text = Text ?? string.Empty;
            var stringFiltering = TextFilter != null;
            var objectFiltering = FilterMode == AutoCompleteFilterMode.Custom && TextFilter == null;

            var viewIndex = 0;
            var viewCount = _view.Count;
            var items = _items;
            foreach (object item in items)
            {
                RefreshItem(text, stringFiltering, objectFiltering, item, ref viewIndex, ref viewCount);
            }
            _valueBindingEvaluator?.ClearDataContext();
        }

        void RefreshItem(string text, bool stringFiltering, bool objectFiltering, object item, ref int viewIndex, ref int viewCount)
        {
            var inResults = !(stringFiltering || objectFiltering);
            if (!inResults)
            {
                inResults = stringFiltering ? TextFilter?.Invoke(text, FormatValue(item)) ?? default(bool) : ItemFilter?.Invoke(text, item) ?? default(bool);
            }

            if (viewCount > viewIndex && inResults && _view[viewIndex] == item)
            {
                viewIndex++;
            }
            else if (inResults)
            {
                if (viewCount > viewIndex && _view[viewIndex] != item)
                {
                    _view.RemoveAt(viewIndex);
                    _view.Insert(viewIndex, item);
                    viewIndex++;
                }
                else
                {
                    AddOrInsertItem(viewIndex, item);
                    viewIndex++;
                    viewCount++;
                }
            }
            else
            {
                if (viewCount > viewIndex && _view[viewIndex] == item)
                {
                    _view.RemoveAt(viewIndex);
                    viewCount--;
                }
            }
        }

        private void AddOrInsertItem(int viewIndex, object item)
        {
            _view.Insert(viewIndex, item);
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
        void ItemsSourceCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                for (int index = 0; index < e.OldItems.Count; index++)
                {
                    _items.RemoveAt(e.OldStartingIndex);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && _items.Count >= e.NewStartingIndex)
            {
                for (int index = 0; index < e.NewItems.Count; index++)
                {
                    _items.Insert(e.NewStartingIndex + index, e.NewItems[index]);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Replace && e.NewItems != null && e.OldItems != null)
            {
                foreach (object t in e.NewItems)
                {
                    _items[e.NewStartingIndex] = t;
                }
            }
            if ((e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace) && e.OldItems != null)
            {
                foreach (object t in e.OldItems)
                {
                    _view.Remove(t);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ClearView();
                if (ItemsSource != null)
                {
                    _items = new List<object>(ItemsSource.Cast<object>().ToList());
                }
            }
            RefreshView();
        }

        #region Selection Adapter

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

#if SILVERLIGHT
            Focus();
#else
            if (TextBox != null)
            {
                Keyboard.Focus(TextBox);
            }
            else
            {
                Focus();
            }
#endif
        }

        [ExcludeFromCodeCoverage]
        void OnAdapterSelectionCanceled(object sender, RoutedEventArgs e)
        {
            UpdateTextValue(SearchText);
            UpdateTextCompletion(false);
        }

        #endregion

        #region Popup

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
#if SILVERLIGHT
            OnDropDownOpened(new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue));
#else
            OnDropDownOpened(new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue, DropDownOpenedEvent));
#endif
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
#if SILVERLIGHT
                OnDropDownClosed(new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue));
#else
                OnDropDownClosed(new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue, DropDownClosedEvent));
#endif
            }
        }

        #endregion

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
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