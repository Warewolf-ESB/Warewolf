
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.InterfaceImplementors;

// ReSharper disable CheckNamespace
// ReSharper disable ConditionIsAlwaysTrueOrFalse
namespace Dev2.UI
{
    /// <summary>
    /// PBI 1214
    /// IntellisenseTextBox
    /// </summary>
    public class IntellisenseTextBox : TextBox, INotifyPropertyChanged, IHandle<UpdateAllIntellisenseMessage>
    {
        public bool IsEventFree { get; set; }

        #region Static Constructor
        static IntellisenseTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IntellisenseTextBox),
                new FrameworkPropertyMetadata(typeof(IntellisenseTextBox)));
        }
        #endregion Static Constructor

        #region Static Members
        private static void ScrollIntoViewCentered(ListBox listBox, object item)
        {
            FrameworkElement container = listBox.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;

            if(container != null)
            {
                if(ScrollViewer.GetCanContentScroll(listBox))
                {
                    IScrollInfo scrollInfo = VisualTreeHelper.GetParent(container) as IScrollInfo;

                    if(null != scrollInfo)
                    {
                        StackPanel stackPanel = scrollInfo as StackPanel;
                        VirtualizingStackPanel virtualizingStackPanel = scrollInfo as VirtualizingStackPanel;
                        int index = listBox.ItemContainerGenerator.IndexFromContainer(container);

                        if((stackPanel != null && Orientation.Horizontal == stackPanel.Orientation) || (virtualizingStackPanel != null && Orientation.Horizontal == virtualizingStackPanel.Orientation))
                        {
                            scrollInfo.SetHorizontalOffset(index - (int)(scrollInfo.ViewportWidth / 2));
                        }
                        else
                        {
                            scrollInfo.SetVerticalOffset(index - (int)(scrollInfo.ViewportHeight / 2));
                        }
                    }
                }
                else
                {
                    Rect rect = new Rect(new Point(), container.RenderSize);

                    FrameworkElement constrainingParent = container;
                    do
                    {
                        constrainingParent = VisualTreeHelper.GetParent(constrainingParent) as FrameworkElement;
                    }
                    // ReSharper disable PossibleUnintendedReferenceComparison
                    while(constrainingParent != null && listBox != constrainingParent && !(constrainingParent is ScrollContentPresenter));

                    if(null != constrainingParent)
                    {
                        rect.Inflate(Math.Max((constrainingParent.ActualWidth - rect.Width) / 2, 0), Math.Max((constrainingParent.ActualHeight - rect.Height) / 2, 0));
                    }

                    container.BringIntoView(rect);
                }
            }
        }
        #endregion

        #region Routed Events

        #region OpenedEvent
        public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent("Opened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IntellisenseTextBox));

        public event RoutedEventHandler Opened
        {
            add
            {
                AddHandler(OpenedEvent, value);
            }
            remove
            {
                RemoveHandler(OpenedEvent, value);
            }
        }
        #endregion OpenedEvent

        #region TabInsertedEvent
        public static readonly RoutedEvent TabInsertedEvent = EventManager.RegisterRoutedEvent("TabInserted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IntellisenseTextBox));

        public event RoutedEventHandler TabInserted
        {
            add
            {
                AddHandler(TabInsertedEvent, value);
            }
            remove
            {
                RemoveHandler(TabInsertedEvent, value);
            }
        }
        #endregion TabInsertedEvent

        #region ClosedEvent
        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IntellisenseTextBox));

        public event RoutedEventHandler Closed
        {
            add
            {
                AddHandler(ClosedEvent, value);
            }
            remove
            {
                RemoveHandler(ClosedEvent, value);
            }
        }
        #endregion ClosedEvent

        #endregion Routed Events

        #region Dependency Properties

        #region IsOnlyRecordsets
        public static readonly DependencyProperty IsOnlyRecordsetsProperty = DependencyProperty.Register("IsOnlyRecordsets", typeof(bool), typeof(IntellisenseTextBox), new PropertyMetadata(false));

        public bool IsOnlyRecordsets
        {
            get
            {
                return (bool)GetValue(IsOnlyRecordsetsProperty);
            }
            set
            {
                SetValue(IsOnlyRecordsetsProperty, value);
            }
        }
        #endregion

        #region HasError
        public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register("HasError", typeof(bool), typeof(IntellisenseTextBox), new PropertyMetadata(false));

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

        #endregion HasError

        #region SelectAllOnGotFocus
        public static readonly DependencyProperty SelectAllOnGotFocusProperty = DependencyProperty.Register("SelectAllOnGotFocus", typeof(bool), typeof(IntellisenseTextBox), new PropertyMetadata(false));

        public bool SelectAllOnGotFocus
        {
            get
            {
                return (bool)GetValue(SelectAllOnGotFocusProperty);
            }
            set
            {
                SetValue(SelectAllOnGotFocusProperty, value);
            }
        }

        #endregion SelectAllOnGotFocus

        #region AllowMultilinePaste
        public static readonly DependencyProperty AllowMultilinePasteProperty = DependencyProperty.Register("AllowMultilinePaste", typeof(bool), typeof(IntellisenseTextBox), new PropertyMetadata(true, OnAllowMultilinePasteChanged));

        public bool AllowMultilinePaste
        {
            get
            {
                return (bool)GetValue(AllowMultilinePasteProperty);
            }
            set
            {
                SetValue(AllowMultilinePasteProperty, value);
            }
        }

        private static void OnAllowMultilinePasteChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            IntellisenseTextBox box = sender as IntellisenseTextBox;

            if(box != null)
            {
                box.OnAllowMultilinePasteChanged((bool)args.OldValue, (bool)args.NewValue);
            }
        }
        #endregion AllowMultilinePaste

        #region AllowUserInsertLine
        public static readonly DependencyProperty AllowUserInsertLineProperty = DependencyProperty.Register("AllowUserInsertLine", typeof(bool), typeof(IntellisenseTextBox), new PropertyMetadata(true, OnAllowUserInsertLineChanged));

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

        private static void OnAllowUserInsertLineChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            IntellisenseTextBox box = sender as IntellisenseTextBox;

            if(box != null)
            {
                box.OnAllowUserInsertLineChanged((bool)args.OldValue, (bool)args.NewValue);
            }
        }
        #endregion AllowUserInsertLine

        #region AllowUserCalculateMode
        public static readonly DependencyProperty AllowUserCalculateModeProperty =
            DependencyProperty.Register("AllowUserCalculateMode", typeof(bool), typeof(IntellisenseTextBox),
            new PropertyMetadata(false, OnAllowUserCalculateModeChanged));

        public bool AllowUserCalculateMode
        {
            get
            {
                return (bool)GetValue(AllowUserCalculateModeProperty);
            }
            set
            {
                SetValue(AllowUserCalculateModeProperty, value);
            }
        }

        private static void OnAllowUserCalculateModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            IntellisenseTextBox box = sender as IntellisenseTextBox;

            if(box != null)
            {
                box.OnAllowUserCalculateModeChanged((bool)args.OldValue, (bool)args.NewValue);
            }
        }
        #endregion AllowUserCalculateMode

        #region IsInCalculateMode
        public static readonly DependencyProperty IsInCalculateModeProperty = DependencyProperty.Register("IsInCalculateMode", typeof(bool), typeof(IntellisenseTextBox), new PropertyMetadata(false, OnIsInCalculateModeChanged));

        public bool IsInCalculateMode
        {
            get
            {
                return (bool)GetValue(IsInCalculateModeProperty);
            }
            set
            {
                SetValue(IsInCalculateModeProperty, value);
            }
        }

        private static void OnIsInCalculateModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            IntellisenseTextBox box = sender as IntellisenseTextBox;

            if(box != null)
            {
                box.OnIsInCalculateModeChanged((bool)args.OldValue, (bool)args.NewValue);
            }
        }
        #endregion IsInCalculateMode

        #region DefaultText
        public static readonly DependencyProperty DefaultTextProperty = DependencyProperty.Register("DefaultText", typeof(object), typeof(IntellisenseTextBox), new UIPropertyMetadata(null));

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

        #endregion DefaultText

        #region DefaultTextTemplate
        public static readonly DependencyProperty DefaultTextTemplateProperty = DependencyProperty.Register("DefaultTextTemplate", typeof(DataTemplate), typeof(IntellisenseTextBox), new UIPropertyMetadata(null));

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
        #endregion DefaultTextTemplate

        #region FilterType
        public static readonly DependencyProperty FilterTypeProperty = DependencyProperty.Register("FilterType", typeof(enIntellisensePartType), typeof(IntellisenseTextBox), new UIPropertyMetadata(enIntellisensePartType.All));

        public enIntellisensePartType FilterType
        {
            get
            {
                return (enIntellisensePartType)GetValue(FilterTypeProperty);
            }
            set
            {
                SetValue(FilterTypeProperty, value);
            }
        }

        #endregion FilterType

        #region WrapInBrackets
        public static readonly DependencyProperty WrapInBracketsProperty = DependencyProperty.Register("WrapInBrackets", typeof(bool), typeof(IntellisenseTextBox), new UIPropertyMetadata(false));

        public bool WrapInBrackets
        {
            get
            {
                return (bool)GetValue(WrapInBracketsProperty);
            }
            set
            {
                SetValue(WrapInBracketsProperty, value);
            }
        }

        public static readonly DependencyProperty ErrorToolTipTextProperty = DependencyProperty.Register("ErrorToolTip", typeof(string), typeof(IntellisenseTextBox), new UIPropertyMetadata(string.Empty, ErrorTextChanged));

        static void ErrorTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var errorText = dependencyPropertyChangedEventArgs.NewValue as string;
            if(String.IsNullOrEmpty(errorText)) return;

            var box = dependencyObject as IntellisenseTextBox;
            if(box != null)
            {
                box._toolTip.Content = errorText;
            }
        }


        public string ErrorToolTip
        {
            get
            {
                return (string)GetValue(ErrorToolTipTextProperty);
            }
            set
            {
                SetValue(ErrorToolTipTextProperty, value);
            }
        }

        #endregion WrapInBracketse

        #region IntellisenseProvider
        public static readonly DependencyProperty IntellisenseProviderProperty = DependencyProperty.Register("IntellisenseProvider", typeof(IIntellisenseProvider), typeof(IntellisenseTextBox), new PropertyMetadata(null, OnIntellisenseProviderChanged));

        public IIntellisenseProvider IntellisenseProvider
        {
            get
            {
                return (IIntellisenseProvider)GetValue(IntellisenseProviderProperty);
            }
            set
            {
                SetValue(IntellisenseProviderProperty, value);
            }
        }

        private static void OnIntellisenseProviderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            IntellisenseTextBox box = sender as IntellisenseTextBox;

            if(box != null)
            {
                box.OnIntellisenseProviderChanged((IIntellisenseProvider)args.OldValue, (IIntellisenseProvider)args.NewValue);
            }
        }
        #endregion IntellisenseProvider

        #region ItemsPanel
        public static readonly DependencyProperty ItemsPanelProperty = ItemsControl.ItemsPanelProperty.AddOwner(typeof(IntellisenseTextBox));

        public ItemsPanelTemplate ItemsPanel
        {
            get
            {
                return (ItemsPanelTemplate)GetValue(ItemsPanelProperty);
            }
            set
            {
                SetValue(ItemsPanelProperty, value);
            }
        }
        #endregion ItemsPanel

        #region ItemTemplate
        public static readonly DependencyProperty ItemTemplateProperty = ItemsControl.ItemTemplateProperty.AddOwner(typeof(IntellisenseTextBox));

        public DataTemplate ItemTemplate
        {
            get
            {
                return (DataTemplate)GetValue(ItemTemplateProperty);
            }
            set
            {
                SetValue(ItemTemplateProperty, value);
            }
        }
        #endregion ItemTemplate

        #region ItemTemplateSelector
        public static readonly DependencyProperty ItemTemplateSelectorProperty = ItemsControl.ItemTemplateSelectorProperty.AddOwner(typeof(IntellisenseTextBox));

        public DataTemplateSelector ItemTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty);
            }
            set
            {
                SetValue(ItemTemplateSelectorProperty, value);
            }
        }
        #endregion ItemTemplateSelector

        #region ItemsSource
        public static readonly DependencyProperty ItemsSourceProperty = ItemsControl.ItemsSourceProperty.AddOwner(typeof(IntellisenseTextBox));

        public IEnumerable ItemsSource
        {
            get
            {
                return (IEnumerable)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }
        #endregion ItemsSource

        #region SelectionMode
        public static readonly DependencyProperty SelectionModeProperty = ListBox.SelectionModeProperty.AddOwner(typeof(IntellisenseTextBox));

        public SelectionMode SelectionMode
        {
            get
            {
                return (SelectionMode)GetValue(SelectionModeProperty);
            }
            set
            {
                SetValue(SelectionModeProperty, value);
            }
        }
        #endregion SelectionMode

        #region SelectedItems
        public static readonly DependencyProperty SelectedItemsProperty = ListBox.SelectedItemsProperty;

        public IList SelectedItems { get { return _listBox.SelectedItems; } }
        #endregion SelectedItems

        #region SelectedValue
        // ReSharper disable AccessToStaticMemberViaDerivedType
        public static readonly DependencyProperty SelectedValueProperty = ListBox.SelectedValueProperty.AddOwner(typeof(IntellisenseTextBox));

        public object SelectedValue
        {
            get
            {
                return GetValue(SelectedValueProperty);
            }
            set
            {
                SetValue(SelectedValueProperty, value);
            }
        }
        #endregion SelectedValue

        #region SelectedItem
        public static readonly DependencyProperty SelectedItemProperty = ListBox.SelectedItemProperty.AddOwner(typeof(IntellisenseTextBox));

        public object SelectedItem
        {
            get
            {
                return GetValue(SelectedItemProperty);
            }
            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }
        #endregion SelectedItem

        #region SelectedIndex
        public static readonly DependencyProperty SelectedIndexProperty = ListBox.SelectedIndexProperty.AddOwner(typeof(IntellisenseTextBox));

        public int SelectedIndex
        {
            get
            {
                return (int)GetValue(SelectedIndexProperty);
            }
            set
            {
                SetValue(SelectedIndexProperty, value);
            }
        }
        #endregion SelectedIndex

        #region DisplayMemberPath
        public static readonly DependencyProperty DisplayMemberPathProperty = ItemsControl.DisplayMemberPathProperty.AddOwner(typeof(IntellisenseTextBox));

        public string DisplayMemberPath
        {
            get
            {
                return (string)GetValue(DisplayMemberPathProperty);
            }
            set
            {
                SetValue(DisplayMemberPathProperty, value);
            }
        }
        #endregion DisplayMemberPath

        #region IsOpen


        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(IntellisenseTextBox), new UIPropertyMetadata(false));

        public bool IsOpen
        {
            get
            {
                return (bool)GetValue(IsOpenProperty);
            }
            set
            {
                SetValue(IsOpenProperty, value);
            }
        }
        //
        //        private static void OnIsOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        //        {
        //            IntellisenseTextBox box = o as IntellisenseTextBox;
        //
        //            if(box != null)
        //            {
        //                box.OnIsOpenChanged((bool)e.OldValue, (bool)e.NewValue);
        //            }
        //        }
        #endregion IsOpen

        #endregion Dependency Properties

        #region Instance Fields
        private ListBox _listBox;
        private KeyValuePair<int, int> _lastResultInputKey;
        private bool _lastResultHasError;
        private bool _expectOpen;
        private bool _suppressChangeOpen;
        private IntellisenseDesiredResultSet _desiredResultSet;
        private int _possibleCaretPositionOnPopup;
        private int _caretPositionOnPopup;
        private string _textOnPopup;
        private object _cachedState;
        private readonly List<Key> _wrapInBracketKey = new List<Key> { Key.F6, Key.F7 };
        readonly ToolTip _toolTip;
        private string _defaultToolTip;
        private bool _forcedOpen;
        private bool _fromPopup;

        #endregion

        #region Public Properties

        //public ItemCollection Items { get { return _listBox.Items; } }

        public ObservableCollection<IntellisenseProviderResult> Items { get; set; }

        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructor
        public IntellisenseTextBox()
        {
            Observable.FromEventPattern(this, "TextChanged")
                     .Throttle(TimeSpan.FromMilliseconds(200), System.Reactive.Concurrency.Scheduler.ThreadPool)
                     .ObserveOn(SynchronizationContext.Current)
                     .Subscribe(pattern => TheTextHasChanged());

            Items = new ObservableCollection<IntellisenseProviderResult>();
            DefaultStyleKey = typeof(IntellisenseTextBox);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
            Unloaded += OnUnloaded;
            EventPublishers.Aggregator.Subscribe(this);
            DataObject.AddPastingHandler(this, OnPaste);

            //08.04.2013: Ashley Lewis - To test for Bug 9238 Moved this from OnInitialized() to allow for a more contextless initialization
            _toolTip = new ToolTip();
            _listBox = new ListBox();
        }

        public IntellisenseTextBox(bool isEventFree)
        {
            IsEventFree = isEventFree;
            _toolTip = new ToolTip();
        }

        #region Overrides of TextBoxBase


        #endregion

        #endregion

        #region Load Handling
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _toolTip.ToolTipOpening += ToolTip_ToolTipOpening;
            _defaultToolTip = (string)ToolTip;
            _toolTip.Placement = PlacementMode.Right;
            _toolTip.PlacementTarget = this;
            EnsureIntellisenseProvider();
            EnsureIntellisenseResults(Text, true, IntellisenseDesiredResultSet.Default);
        }

        // ReSharper disable InconsistentNaming
        private void ToolTip_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if(!_fromPopup && IsOpen)
            {
                e.Handled = true;
            }
        }
        #endregion

        #region Unloaded Handeling

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Unloaded -= OnUnloaded;
            routedEventArgs.Handled = true;

        }

        public void ClearIntellisenseErrors()
        {

            EnsureIntellisenseResults(Text,true,IntellisenseDesiredResultSet.Default);
            LostFocusImpl();
        }
        #endregion Unloaded Handeling

        #region Template Handling
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //if (_listBox != null) _listBox.MouseLeftButtonUp -= new MouseButtonEventHandler(ListBox_MouseLeftButtonUp);

            _listBox = GetTemplateChild("PART_ItemList") as ListBox;
            //_listBox.MouseLeftButtonUp += new MouseButtonEventHandler(ListBox_MouseLeftButtonUp);
        }
        #endregion

        #region Paste Handeling

        void OnPaste(object sender, DataObjectPastingEventArgs dataObjectPastingEventArgs)
        {
            var isText = dataObjectPastingEventArgs.SourceDataObject.GetDataPresent(DataFormats.Text, true);
            if(!isText) return;

            var text = dataObjectPastingEventArgs.SourceDataObject.GetData(DataFormats.Text) as string;

            if(text != null && text.Contains("\t"))
            {
                RaiseRoutedEvent(TabInsertedEvent);
            }
        }

        #endregion

        #region Event Handling
        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            if(!_suppressChangeOpen)
            {
                _possibleCaretPositionOnPopup = CaretIndex;
            }
        }

        #region Overrides of TextBoxBase



        #endregion

        protected virtual void TheTextHasChanged()
        {
            try
            {
                if(CheckHasUnicodeInText(Text))
                {
                    return;
                }

                if(AllowUserInsertLine && GetLastVisibleLineIndex() + 1 == LineCount)
                {
                    double lineHeight = FontSize * FontFamily.LineSpacing;
                    double extentHeight = ExtentHeight;
                    double adjustedHeight = Height;

                    while(adjustedHeight - lineHeight > extentHeight)
                    {
                        adjustedHeight -= lineHeight;
                    }

                    Height = adjustedHeight;
                }

                if(!_suppressChangeOpen)
                {
                    bool wasOpen = IsOpen;

                    if(!wasOpen)
                    {
                        _caretPositionOnPopup = _possibleCaretPositionOnPopup;
                        _textOnPopup = Text;
                    }

                    EnsureIntellisenseResults(Text, true, IntellisenseDesiredResultSet.ClosestMatch);

                    if(IsKeyboardFocused && IsKeyboardFocusWithin)
                    {
                        if(!wasOpen && _listBox != null && _listBox.HasItems)
                        {
                            _expectOpen = true;
                            _desiredResultSet = (IntellisenseDesiredResultSet)4;
                            IsOpen = true;
                        }
                    }
                }
                else
                {
                    _suppressChangeOpen = false;
                }

                OnPropertyChanged("Text");

                int maxLines = AllowUserInsertLine ? MaxLines : 1;

                if(LineCount >= maxLines)
                {
                    int caretPosition = CaretIndex;
                    string nText = "";

                    for(int i = 0; i < maxLines; i++)
                    {
                        string lineText = GetLineText(i);

                        if(i + 1 >= maxLines)
                        {
                            if(lineText.EndsWith(Environment.NewLine))
                            {
                                lineText = lineText.Substring(0, lineText.Length - Environment.NewLine.Length);
                            }
                        }

                        nText += lineText;
                    }

                    if(caretPosition > nText.Length)
                    {
                        caretPosition = nText.Length;
                    }
                    Text = nText;
                    Select(caretPosition, 0);
                }
            }
            catch(InvalidOperationException)
            {
            }
        }

        public bool CheckHasUnicodeInText(string inputText)
        {
            var hasUnicode = inputText.ContainsUnicodeCharacter();
            if(hasUnicode)
            {
                var previousInput = inputText;
                Text = "";
                CustomContainer.Get<IPopupController>()
                               .ShowInvalidCharacterMessage(previousInput);

                return true;
            }
            return false;
        }

        protected virtual void OnAllowMultilinePasteChanged(bool oldValue, bool newValue)
        {
            AcceptsReturn = newValue;
        }

        protected virtual void OnAllowUserInsertLineChanged(bool oldValue, bool newValue)
        {
        }

        protected virtual void OnAllowUserCalculateModeChanged(bool oldValue, bool newValue)
        {
        }

        protected virtual void OnIsInCalculateModeChanged(bool oldValue, bool newValue)
        {
        }

        protected virtual void OnIsOpenChanged(bool oldValue, bool newValue)
        {
            if(newValue)
            {
                if(DesignerProperties.GetIsInDesignMode(this))
                {
                    IsOpen = false;
                    if(_toolTip != null) _toolTip.IsOpen = false;
                }
                else
                {
                    if(_listBox != null)
                    {
                        if(_expectOpen)
                        {
                            _expectOpen = false;
                            if((int)_desiredResultSet < 4) EnsureIntellisenseResults(Text, true, _desiredResultSet);
                        }
                        else
                        {
                            EnsureIntellisenseResults(Text, true, IntellisenseDesiredResultSet.Default);
                        }

                        if(_listBox.HasItems)
                        {
                            _caretPositionOnPopup = CaretIndex;
                            _textOnPopup = Text;
                            RaiseRoutedEvent(OpenedEvent);
                        }
                        else
                        {
                            IsOpen = false;
                        }
                    }
                    else IsOpen = false;
                }
            }
            else
            {
                EnsureErrorStatus();
                RaiseRoutedEvent(ClosedEvent);
            }
        }

        private void RaiseRoutedEvent(RoutedEvent routedEvent)
        {
            RoutedEventArgs args = new RoutedEventArgs(routedEvent, this);
            RaiseEvent(args);
        }
        #endregion

        #region Provider Handling
        public virtual void OnIntellisenseProviderChanged(IIntellisenseProvider oldValue, IIntellisenseProvider newValue)
        {
            if(oldValue != null)
            {
                oldValue.Dispose();
                _lastResultInputKey = new KeyValuePair<int, int>(0, 0);
                _lastResultHasError = false;
            }

            if(newValue == null)
            {
                EnsureIntellisenseProvider();
            }
            else
            {
                // What we need to do is cache the results from each provider. Only on a change to DL, text do we re gather results? ;)
                if(Text.Length > 0)
                {
                    EnsureIntellisenseResults(Text, true, IntellisenseDesiredResultSet.Default);
                }
            }
        }

        protected virtual IIntellisenseProvider CreateIntellisenseProviderInstance()
        {
            return new DefaultIntellisenseProvider();
        }

        public void EnsureIntellisenseResults(string text, bool forceUpdate, IntellisenseDesiredResultSet desiredResultSet)
        {
            if(!DesignerProperties.GetIsInDesignMode(this))
            {
                bool calculateMode = false;

                if(AllowUserCalculateMode)
                {
                    if(text.Length > 0 && text[0] == '=')
                    {
                        calculateMode = true;
                        text = text.Substring(1);
                    }

                    IsInCalculateMode = calculateMode;
                }
                else if(IsInCalculateMode)
                {
                    calculateMode = true;
                }


                KeyValuePair<int, int> currentResultInputKey = new KeyValuePair<int, int>(text.Length, text.GetHashCode());

                if(!forceUpdate)
                {
                    if(currentResultInputKey.Key != _lastResultInputKey.Key || currentResultInputKey.Value != _lastResultInputKey.Value)
                    {
                        forceUpdate = true;
                    }
                }

                _lastResultInputKey = currentResultInputKey;

                if(forceUpdate)
                {
                    IIntellisenseProvider provider = IntellisenseProvider;
                    var context = new IntellisenseProviderContext { TextBox = this, FilterType = FilterType, DesiredResultSet = desiredResultSet, InputText = text, CaretPosition = CaretIndex, CaretPositionOnPopup = _caretPositionOnPopup, TextOnPopup = _textOnPopup };

                    if((context.IsInCalculateMode = calculateMode) && AllowUserCalculateMode)
                    {
                        if(CaretIndex > 0)
                            context.CaretPosition = CaretIndex - 1;

                        if(_textOnPopup.Length > 0 && _textOnPopup[0] == '=')
                        {
                            context.TextOnPopup = _textOnPopup.Substring(1);

                            if(_caretPositionOnPopup > 0)
                            {
                                context.CaretPositionOnPopup = _caretPositionOnPopup - 1;
                            }
                        }
                    }

                    _cachedState = null;

                    IList<IntellisenseProviderResult> results = null;

                    try
                    {
                        results = provider.GetIntellisenseResults(context);
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                        //This try catch is to prevent the intellisense box from ever being crashed from a provider.
                        //This catch is intentionally blanks since if a provider throws an exception the intellisense
                        //box should simbly ignore that provider.
                    }

                    if(results != null && results.Count > 0)
                    {
                        _cachedState = context.State;
                        StringBuilder ttErrorBuilder = new StringBuilder();
                        StringBuilder ttValueBuilder = new StringBuilder();
                        bool cleared = false;
                        bool hasError = false;
                        int errorCount = 0;
                        IntellisenseProviderResult popup = null;

                        // ReSharper disable ForCanBeConvertedToForeach
                        for(var i = 0; i < results.Count; i++)
                        // ReSharper restore ForCanBeConvertedToForeach
                        {
                            IntellisenseProviderResult currentResult = results[i];

                            if(currentResult.IsError)
                            {
                                hasError = true;

                                ttErrorBuilder.Append(++errorCount);
                                ttErrorBuilder.Append(") ");
                                ttErrorBuilder.AppendLine(currentResult.Description);
                            }


                            if(!currentResult.IsError)
                            {
                                if(!currentResult.IsPopup)
                                {
                                    if(!cleared)
                                    {
                                        cleared = true;
                                        if(_listBox != null)
                                        {
                                            Items.Clear();
                                        }
                                    }

                                    if(_listBox != null)
                                    {
                                        Items.Add(currentResult);
                                    }
                                    ttValueBuilder.AppendLine(currentResult.Name);
                                }
                                else
                                {
                                    popup = currentResult;
                                }
                            }
                        }

                        _lastResultHasError = hasError;
                        ttValueBuilder.Clear();

                        if(popup == null)
                        {
                            _fromPopup = false;
                            _toolTip.Content = _lastResultHasError ? ttErrorBuilder.ToString()
                                                   : ttValueBuilder.ToString();
                        }

                        if(popup != null)
                        {
                            _fromPopup = true;
                            var description = popup.Description;

                            if(_lastResultHasError)
                            {
                                //ttValueBuilder.Clear();
                                ttValueBuilder.AppendLine();
                                ttValueBuilder.AppendLine();
                                description = description + ttValueBuilder + ttErrorBuilder;
                            }

                            _toolTip.Content = string.IsNullOrEmpty(description) ? _defaultToolTip : description;
                            _toolTip.IsOpen = _forcedOpen = true;
                            ToolTip = _toolTip;
                        }
                        else if(_forcedOpen)
                        {
                            _toolTip.IsOpen = _forcedOpen = false;
                        }

                        if(!cleared)
                        {
                            if(IsOpen)
                            {
                                IsOpen = false;

                                if(popup == null)
                                {
                                    _toolTip.IsOpen = _forcedOpen = false;
                                }
                            }

                            if(_listBox != null)
                            {
                                Items.Clear();
                            }
                        }
                    }
                    else
                    {
                        var ttErrorBuilder = new StringBuilder();
                        var hasError = false;
                        var errorCount = 0;

                        if(_listBox != null)
                        {
                            foreach(var currentResult in
                                Items.Where(currentResult => currentResult.IsError))
                            {
                                hasError = true;

                                ttErrorBuilder.Append(++errorCount);
                                ttErrorBuilder.Append(") ");
                                ttErrorBuilder.AppendLine(currentResult.Description);
                            }
                        }

                        if(text.Contains("[[") && text.Contains("]]"))
                        {
                            if(FilterType == enIntellisensePartType.RecordsetFields || FilterType == enIntellisensePartType.RecordsetsOnly)
                            {
                                if(!(text.Contains("(") && text.Contains(")")))
                                {
                                    hasError = true;
                                    ttErrorBuilder.AppendLine("Scalar is not allowed");
                                    _toolTip.IsOpen = true;
                                }
                            }
                            else if(FilterType == enIntellisensePartType.ScalarsOnly)
                            {
                                if(text.Contains("(") && text.Contains(")"))
                                {
                                    hasError = true;
                                    ttErrorBuilder.AppendLine("Recordset is not allowed");
                                    _toolTip.IsOpen = true;
                                }
                            }
                        }

                        _lastResultHasError = hasError;
                        _fromPopup = false;

                        string errorText = ttErrorBuilder.ToString();
                        _toolTip.Content = string.IsNullOrEmpty(errorText) ? _defaultToolTip : errorText;

                        ToolTip = _toolTip;

                        if(_forcedOpen)
                        {
                            _toolTip.IsOpen = _forcedOpen = false;
                        }

                        if(IsOpen)
                        {
                            IsOpen = false;
                        }

                        if(_listBox != null)
                        {
                            Items.Clear();
                        }
                    }

                    EnsureErrorStatus();
                }
            }
            if(_listBox != null)
            {
                _listBox.ItemsSource = Items;
            }
        }

        private void EnsureIntellisenseProvider()
        {
            if(IntellisenseProvider == null)
            {
                IIntellisenseProvider instance = CreateIntellisenseProviderInstance();

                if(instance == null)
                {
                    throw new InvalidOperationException("CreateIntellisenseProviderInstance cannot return null.");
                }

                IntellisenseProvider = instance;
            }
        }
        #endregion

        #region Validation Handling
        private void EnsureErrorStatus()
        {
            HasError = _lastResultHasError;
        }
        #endregion

        #region Dropdown Handling
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if(IsOpen)
            {
                bool originIsListbox = false;

                if(e.OriginalSource is DependencyObject)
                {
                    DependencyObject parent = e.OriginalSource as DependencyObject;


                    // ReSharper disable ConditionIsAlwaysTrueOrFalse
                    if(parent != null)
                    {
                        if(parent is System.Windows.Documents.Run)
                        {
                            parent = ((System.Windows.Documents.Run)parent).Parent;
                        }
                        else
                        {
                            parent = VisualTreeHelper.GetParent(parent);
                        }

                        while(parent != null && !(parent is ListBox))
                        {
                            parent = VisualTreeHelper.GetParent(parent);
                        }

                        if(parent != null)
                        {
                            if(parent == _listBox)
                            {
                                originIsListbox = true;
                            }
                        }
                    }
                }

                if(!originIsListbox)
                {
                    IsOpen = false;

                    if(_forcedOpen)
                    {
                        _toolTip.IsOpen = _forcedOpen = false;
                    }
                }
            }

            base.OnPreviewMouseWheel(e);
        }

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseDropDown(true);
        }

        /// <summary>
        /// Closes the drop down.
        /// </summary>
        private void CloseDropDown(bool closeToolTip)
        {
            if(IsOpen)
            {
                IsOpen = false;
            }

            if(closeToolTip && _toolTip != null)
            {
                _toolTip.IsOpen = _forcedOpen = false;
            }

            ReleaseMouseCapture();
        }
        #endregion

        #region Input Handling
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            BindingExpression be = BindingOperations.GetBindingExpression(this, TextProperty);
            if(be != null)
            {
                be.UpdateSource();
            }
            if(SelectAllOnGotFocus)
            {
                SelectAll();
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            LostFocusImpl();
        }

        void LostFocusImpl()
        {
            ExecWrapBrackets();

            CloseDropDown(true);

            BindingExpression be = BindingOperations.GetBindingExpression(this, TextProperty);
            if(be != null)
            {
                be.UpdateSource();
            }
        }

        private void ExecWrapBrackets()
        {
            if(_listBox != null && IsOpen && !IsKeyboardFocusWithin && !_listBox.IsKeyboardFocused &&
                !_listBox.IsKeyboardFocusWithin)
            {
                CloseDropDown(true);
            }

            if(WrapInBrackets && !string.IsNullOrWhiteSpace(Text))
            {
                Text = AddBracketsToExpression(Text);
            }

            if(IsOnlyRecordsets && !string.IsNullOrEmpty(Text))
            {
                Text = AddRecordsetNotationToExpresion(Text);
            }
        }

        public string AddRecordsetNotationToExpresion(string expression)
        {
            if(expression.EndsWith("]]"))
            {
                if(!expression.Contains("()"))
                {
                    expression = expression.Insert(expression.IndexOf("]", StringComparison.Ordinal), "()");
                }
            }
            else
            {
                expression += "()";
            }
            return expression;
        }

        public string AddBracketsToExpression(string expression)
        {
            string result = expression;

            if(!result.StartsWith("[["))
            {
                result = string.Concat(!result.StartsWith("[") ? "[[" : "[", result);
            }

            if(!expression.EndsWith("]]"))
            {
                result = string.Concat(result, !expression.EndsWith("]") ? "]]" : "]");
            }

            return result;
        }

        public void InsertItem(object item, bool force)
        {
            bool isOpen = IsOpen;
            string appendText = null;
            bool isInsert = false;
            int index = CaretIndex;
            IIntellisenseProvider currentProvider = new DefaultIntellisenseProvider();//Bug 8437

            if(isOpen || force)
            {
                IntellisenseProviderResult intellisenseProviderResult = item as IntellisenseProviderResult;
                if(intellisenseProviderResult != null)
                {
                    currentProvider = intellisenseProviderResult.Provider;//Bug 8437
                }

                object selectedItem = item;

                if(_listBox != null)
                {
                    // ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
                    if(selectedItem is IDataListVerifyPart)
                    // ReSharper restore CanBeReplacedWithTryCastAndCheckForNull
                    {
                        var part = selectedItem as IDataListVerifyPart;
                        appendText = part.DisplayValue;
                    }
                    else
                    {
                        if(selectedItem != null)
                        {
                            appendText = selectedItem.ToString();
                        }
                    }

                    isInsert = true;
                    CloseDropDown(false);
                    Focus();
                }
            }

            if(appendText != null)
            {
                string currentText = Text;

                int foundLength = 0;
                if(isInsert)
                {
                    if(currentProvider.HandlesResultInsertion)//Bug 8437
                    {
                        _suppressChangeOpen = true;
                        IntellisenseProviderContext context = new IntellisenseProviderContext { CaretPosition = index, InputText = currentText, State = _cachedState, TextBox = this };

                        try
                        {
                            Text = currentProvider.PerformResultInsertion(appendText, context);
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch
                        // ReSharper restore EmptyGeneralCatchClause
                        {
                            //This try catch is to prevent the intellisense box from ever being crashed from a provider.
                            //This catch is intentionally blanks since if a provider throws an exception the intellisense
                            //box should simbly ignore that provider.
                        }

                        Select(context.CaretPosition, 0);

                        IsOpen = false;
                        appendText = null;
                    }
                    else
                    {
                        int foundMinimum = -1;


                        for(int i = index - 1; i >= 0; i--)
                        {
                            if(appendText.StartsWith(currentText.Substring(i, index - i), StringComparison.OrdinalIgnoreCase))
                            {
                                foundMinimum = i;
                                foundLength = index - i;
                            }
                            else if(foundMinimum != -1 || appendText.IndexOf(currentText[i].ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) == -1)
                            {
                                i = -1;
                            }
                        }

                        if(foundMinimum != -1)
                        {
                            index = foundMinimum;
                            Text = currentText = currentText.Remove(foundMinimum, foundLength);
                            //appendText = appendText.Remove(0, foundLength);
                        }
                    }
                }

                if(appendText != null)
                {
                    _suppressChangeOpen = true;

                    if(currentText.Length == index)
                    {
                        AppendText(appendText);
                        Select(Text.Length, 0);
                    }
                    else
                    {
                        currentText = currentText.Insert(index, appendText);
                        Text = currentText;
                        Select(index + appendText.Length, 0);
                    }

                    IsOpen = false;
                }
            }

            double lineHeight = FontSize * FontFamily.LineSpacing;
            Height += lineHeight;

            UpdateErrorState();
            EnsureErrorStatus();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {

            base.OnPreviewKeyDown(e);
            bool isOpen = IsOpen;

            if(e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Tab)
            {
                object appendText = null;
                const bool isInsert = false;
                bool expand = false;

                if(AllowUserInsertLine && !isOpen && e.Key != Key.Tab && e.KeyboardDevice.Modifiers == ModifierKeys.None)
                {
                    if(LineCount < MaxLines)
                    {
                        appendText = Environment.NewLine;
                        expand = true;
                    }
                }

                if(isOpen && e.KeyboardDevice.Modifiers == ModifierKeys.None)
                {
                    object selectedItem;

                    if(_listBox != null && (selectedItem = _listBox.SelectedItem) != null)
                    {
                        // ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
                        if(selectedItem is IDataListVerifyPart)
                        // ReSharper restore CanBeReplacedWithTryCastAndCheckForNull
                        {
                            var part = selectedItem as IDataListVerifyPart;
                            appendText = part.DisplayValue;
                        }
                        else if(_listBox.SelectedItem is IntellisenseProviderResult)
                        {
                            appendText = _listBox.SelectedItem as IntellisenseProviderResult;
                        }
                        else
                        {
                            appendText = selectedItem.ToString();
                        }
                        Focus();
                    }
                }

                if(appendText != null && IsOpen)
                {
                    e.Handled = true;
                }
                InsertItem(appendText, isInsert);

                if(e.Key != Key.Tab)
                {
                    if((e.Key == Key.Enter || e.Key == Key.Return) && e.KeyboardDevice.Modifiers != ModifierKeys.Shift && AcceptsReturn)
                    {
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }

                if(expand)
                {
                    double lineHeight = FontSize * FontFamily.LineSpacing;
                    Height += lineHeight;
                }
            }
            else if(e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if(!isOpen)
                {
                    _expectOpen = true;
                    _desiredResultSet = IntellisenseDesiredResultSet.EntireSet;
                    IsOpen = true;
                }

                e.Handled = true;
            }
            else if(IsListBoxInputKey(e) && isOpen)
            {
                EmulateEventOnListBox(e);
            }
            else if(e.Key == Key.Home || e.Key == Key.End)
            {
                CloseDropDown(true);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            HandleWrapInBrackets(e.Key);

            base.OnKeyDown(e);

            if(e.Key == Key.Escape)
            {
                CloseDropDown(true);
            }
        }

        public void HandleWrapInBrackets(Key e)
        {
            if(_wrapInBracketKey.Contains(e))
            {
                ExecWrapBrackets();
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if(e.ClickCount >= 3)
            {
                SelectAll();
            }
            IInputElement directlyOver = Mouse.DirectlyOver;
            bool isItemSelected = false;
            object context = null;

            if(directlyOver is FrameworkElement)
            {
                FrameworkElement element = directlyOver as FrameworkElement;
                context = element.DataContext;
            }
            else if(directlyOver is FrameworkContentElement)
            {
                FrameworkContentElement element = directlyOver as FrameworkContentElement;
                context = element.DataContext;
            }

            if(context is IntellisenseProviderResult)
            {
                InsertItem(context, false);
                isItemSelected = true;
            }

            try
            {
                if(!isItemSelected)
                {
                    base.OnPreviewMouseLeftButtonDown(e);
                }
                else
                {
                    e.Handled = true;
                }

                Focus();
            }
            catch(Exception)
            {
                //bleh.
                e.Handled = true;
            }
        }

        internal void UpdateErrorState()
        {
            EnsureIntellisenseResults(Text, true, _desiredResultSet);
        }
        #endregion

        #region Listbox Handling
        private bool IsListBoxInputKey(KeyEventArgs e)
        {
            return (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp);
        }

        private void EmulateEventOnListBox(KeyEventArgs e)
        {
            if(_listBox != null)
            {
                int count = Items.Count;
                int index = _listBox.SelectedIndex;
                bool scrolled = false;

                if(e.Key == Key.Down || e.Key == Key.PageDown)
                {
                    if(count != 0)
                    {
                        if(index == -1)
                        {
                            _listBox.SelectedIndex = 0;
                        }
                        else
                        {
                            int adjust = e.Key == Key.Down ? 1 : 4;
                            adjust = Math.Min(index + adjust, count - 1);

                            if(adjust != index)
                            {
                                _listBox.SelectedIndex = adjust;
                            }
                        }

                        scrolled = true;
                    }
                }
                else
                {
                    if(count != 0)
                    {
                        if(index == -1)
                        {
                            _listBox.SelectedIndex = 0;
                        }
                        else
                        {
                            int adjust = e.Key == Key.Up ? -1 : -4;
                            adjust = Math.Max(index + adjust, 0);

                            if(adjust != index)
                            {
                                _listBox.SelectedIndex = adjust;
                            }
                        }

                        scrolled = true;
                    }
                }

                if(scrolled)
                {
                    ScrollIntoViewCentered(_listBox, _listBox.SelectedItem);

                    //FrameworkElement element = _listBox.ItemContainerGenerator.ContainerFromIndex(_listBox.SelectedIndex) as FrameworkElement;



                    //if (element != null)
                    //{
                    //    element.BringIntoView();
                    //}
                }

                e.Handled = true;
            }
        }
        #endregion


        public void Handle(UpdateAllIntellisenseMessage message)
        {
            ClearIntellisenseErrors();
        }
    }
}
