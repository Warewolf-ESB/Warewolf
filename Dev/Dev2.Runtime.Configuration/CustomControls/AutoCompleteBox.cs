/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

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
// ReSharper disable NonLocalizedString
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable VirtualMemberNeverOverriden.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

// ReSharper disable CheckNamespace
namespace System.Windows.Controls
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Represents a control that provides a text box for user input and a
    /// drop-down that contains possible matches based on the input in the text
    /// box.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
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

        /// <summary>
        /// Specifies the name of the selection adapter TemplatePart.
        /// </summary>
        private const string ElementSelectionAdapter = "SelectionAdapter";

        /// <summary>
        /// Specifies the name of the Selector TemplatePart.
        /// </summary>
        private const string ElementSelector = "Selector";

        /// <summary>
        /// Specifies the name of the Popup TemplatePart.
        /// </summary>
        private const string ElementPopup = "Popup";

        /// <summary>
        /// The name for the text box part.
        /// </summary>
        private const string ElementTextBox = "Text";

        /// <summary>
        /// The name for the text box style.
        /// </summary>
        private const string ElementTextBoxStyle = "TextBoxStyle";

        /// <summary>
        /// The name for the adapter's item container style.
        /// </summary>
        private const string ElementItemContainerStyle = "ItemContainerStyle";

        #endregion

        /// <summary>
        /// Gets or sets a local cached copy of the items data.
        /// </summary>
        private List<object> _items;

        /// <summary>
        /// Gets or sets the observable collection that contains references to 
        /// all of the items in the generated view of data that is provided to 
        /// the selection-style control adapter.
        /// </summary>
        private ObservableCollection<object> _view;

        /// <summary>
        /// Gets or sets a value to ignore a number of pending change handlers. 
        /// The value is decremented after each use. This is used to reset the 
        /// value of properties without performing any of the actions in their 
        /// change handlers.
        /// </summary>
        /// <remarks>The int is important as a value because the TextBox 
        /// TextChanged event does not immediately fire, and this will allow for
        /// nested property changes to be ignored.</remarks>
        private int _ignoreTextPropertyChange;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore calling a pending 
        /// change handlers. 
        /// </summary>
        private bool _ignorePropertyChange;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the selection 
        /// changed event.
        /// </summary>
        private bool _ignoreTextSelectionChange;

        /// <summary>
        /// Gets or sets a value indicating whether to skip the text update 
        /// processing when the selected item is updated.
        /// </summary>
        private bool _skipSelectedItemTextUpdate;

        /// <summary>
        /// Gets or sets the last observed text box selection start location.
        /// </summary>
        private int _textSelectionStart;

        /// <summary>
        /// Gets or sets a value indicating whether the user initiated the 
        /// current populate call.
        /// </summary>
        private bool _userCalledPopulate;

        /// <summary>
        /// A value indicating whether the popup has been opened at least once.
        /// </summary>
        private bool _popupHasOpened;

        /// <summary>
        /// Gets or sets the DispatcherTimer used for the MinimumPopulateDelay 
        /// condition for auto completion.
        /// </summary>
        private DispatcherTimer _delayTimer;

        /// <summary>
        /// Gets or sets a value indicating whether a read-only dependency 
        /// property change handler should allow the value to be set.  This is 
        /// used to ensure that read-only properties cannot be changed via 
        /// SetValue, etc.
        /// </summary>
        private bool _allowWrite;

        /// <summary>
        /// Gets or sets the helper that provides all of the standard
        /// interaction functionality. Making it internal for subclass access.
        /// </summary>
        internal InteractionHelper Interaction { get; set; }

        /// <summary>
        /// Gets or sets the BindingEvaluator, a framework element that can
        /// provide updated string values from a single binding.
        /// </summary>
        private BindingEvaluator<string> _valueBindingEvaluator;

        /// <summary>
        /// A weak event listener for the collection changed event.
        /// </summary>
        private WeakEventListener<AutoCompleteBox, object, NotifyCollectionChangedEventArgs> _collectionChangedWeakEventListener;

        #region public int MinimumPrefixLength
        /// <summary>
        /// Gets or sets the minimum number of characters required to be entered
        /// in the text box before the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> displays
        /// possible matches.
        /// matches.
        /// </summary>
        /// <value>
        /// The minimum number of characters to be entered in the text box
        /// before the <see cref="T:System.Windows.Controls.AutoCompleteBox" />
        /// displays possible matches. The default is 1.
        /// </value>
        /// <remarks>
        /// If you set MinimumPrefixLength to -1, the AutoCompleteBox will
        /// not provide possible matches. There is no maximum value, but
        /// setting MinimumPrefixLength to value that is too large will
        /// prevent the AutoCompleteBox from providing possible matches as well.
        /// </remarks>
        public int MinimumPrefixLength
        {
            get { return (int)GetValue(MinimumPrefixLengthProperty); }
            set { SetValue(MinimumPrefixLengthProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.MinimumPrefixLength" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.MinimumPrefixLength" />
        /// dependency property.</value>
        public static readonly DependencyProperty MinimumPrefixLengthProperty =
            DependencyProperty.Register(
                "MinimumPrefixLength",
                typeof(int),
                typeof(AutoCompleteBox),
                new PropertyMetadata(1, OnMinimumPrefixLengthPropertyChanged));

        /// <summary>
        /// MinimumPrefixLengthProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteBox that changed its MinimumPrefixLength.</param>
        /// <param name="e">Event arguments.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "MinimumPrefixLength is the name of the actual dependency property.")]
        private static void OnMinimumPrefixLengthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int newValue = (int)e.NewValue;

            if(newValue < 0 && newValue != -1)
            {
                // ReSharper disable once NotResolvedInText
                throw new ArgumentOutOfRangeException("MinimumPrefixLength");
            }
        }
        #endregion public int MinimumPrefixLength

        #region public int MinimumPopulateDelay
        /// <summary>
        /// Gets or sets the minimum delay, in milliseconds, after text is typed
        /// in the text box before the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control
        /// populates the list of possible matches in the drop-down.
        /// </summary>
        /// <value>The minimum delay, in milliseconds, after text is typed in
        /// the text box, but before the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> populates
        /// the list of possible matches in the drop-down. The default is 0.</value>
        /// <exception cref="T:System.ArgumentException">The set value is less than 0.</exception>
        [ExcludeFromCodeCoverage]
        public int MinimumPopulateDelay
        {
            get { return (int)GetValue(MinimumPopulateDelayProperty); }
            set { SetValue(MinimumPopulateDelayProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.MinimumPopulateDelay" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.MinimumPopulateDelay" />
        /// dependency property.</value>
        
        public static readonly DependencyProperty MinimumPopulateDelayProperty =
            DependencyProperty.Register(
                "MinimumPopulateDelay",
                typeof(int),
                typeof(AutoCompleteBox),
                new PropertyMetadata(OnMinimumPopulateDelayPropertyChanged));

        /// <summary>
        /// MinimumPopulateDelayProperty property changed handler. Any current 
        /// dispatcher timer will be stopped. The timer will not be restarted 
        /// until the next TextUpdate call by the user.
        /// </summary>
        /// <param name="d">AutoCompleteTextBox that changed its 
        /// MinimumPopulateDelay.</param>
        /// <param name="e">Event arguments.</param>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The exception is most likely to be called through the CLR property setter.")]
        [ExcludeFromCodeCoverage]
        private static void OnMinimumPopulateDelayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox source = d as AutoCompleteBox;

            if(source != null && source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            int newValue = (int)e.NewValue;
            if(newValue < 0)
            {
                if(source != null)
                {
                    source._ignorePropertyChange = true;
                }
                d.SetValue(e.Property, e.OldValue);

                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    // ReSharper disable once NotResolvedInText
                    Dev2.Runtime.Configuration.Properties.Resources.AutoComplete_OnMinimumPopulateDelayPropertyChanged_InvalidValue, newValue), "value");
            }

            // Stop any existing timer
            if(source?._delayTimer != null)
            {
                source._delayTimer.Stop();

                if(newValue == 0)
                {
                    source._delayTimer = null;
                }
            }

            // Create or clear a dispatcher timer instance
            if(source != null && newValue > 0 && source._delayTimer == null)
            {
                source._delayTimer = new DispatcherTimer();
                source._delayTimer.Tick += source.PopulateDropDown;
            }

            // Set the new tick interval
            if(source != null && newValue > 0 && source._delayTimer != null)
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
        /// <summary>
        /// Gets or sets a value indicating whether the first possible match
        /// found during the filtering process will be displayed automatically
        /// in the text box.
        /// </summary>
        /// <value>
        /// True if the first possible match found will be displayed
        /// automatically in the text box; otherwise, false. The default is
        /// false.
        /// </value>
        public bool IsTextCompletionEnabled
        {
            get { return (bool)GetValue(IsTextCompletionEnabledProperty); }
            set { SetValue(IsTextCompletionEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.IsTextCompletionEnabled" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.IsTextCompletionEnabled" />
        /// dependency property.</value>
        public static readonly DependencyProperty IsTextCompletionEnabledProperty =
            DependencyProperty.Register(
                "IsTextCompletionEnabled",
                typeof(bool),
                typeof(AutoCompleteBox),
                new PropertyMetadata(false, null));

        #endregion public bool IsTextCompletionEnabled

        #region public DataTemplate ItemTemplate
        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.DataTemplate" /> used
        /// to display each item in the drop-down portion of the control.
        /// </summary>
        /// <value>The <see cref="T:System.Windows.DataTemplate" /> used to
        /// display each item in the drop-down. The default is null.</value>
        /// <remarks>
        /// You use the ItemTemplate property to specify the visualization 
        /// of the data objects in the drop-down portion of the AutoCompleteBox 
        /// control. If your AutoCompleteBox is bound to a collection and you 
        /// do not provide specific display instructions by using a 
        /// DataTemplate, the resulting UI of each item is a string 
        /// representation of each object in the underlying collection. 
        /// </remarks>
        [ExcludeFromCodeCoverage]
        public DataTemplate ItemTemplate
        {
            get { return GetValue(ItemTemplateProperty) as DataTemplate; }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemTemplate" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemTemplate" />
        /// dependency property.</value>
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                "ItemTemplate",
                typeof(DataTemplate),
                typeof(AutoCompleteBox),
                new PropertyMetadata(null));

        #endregion public DataTemplate ItemTemplate

        #region public Style ItemContainerStyle
        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.Style" /> that is
        /// applied to the selection adapter contained in the drop-down portion
        /// of the <see cref="T:System.Windows.Controls.AutoCompleteBox" />
        /// control.
        /// </summary>
        /// <value>The <see cref="T:System.Windows.Style" /> applied to the
        /// selection adapter contained in the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// The default is null.</value>
        /// <remarks>
        /// The default selection adapter contained in the drop-down is a 
        /// ListBox control. 
        /// </remarks>
        [ExcludeFromCodeCoverage]
        public Style ItemContainerStyle
        {
            get { return GetValue(ItemContainerStyleProperty) as Style; }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemContainerStyle" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemContainerStyle" />
        /// dependency property.</value>
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register(
                ElementItemContainerStyle,
                typeof(Style),
                typeof(AutoCompleteBox),
                new PropertyMetadata(null, null));

        #endregion public Style ItemContainerStyle

        #region public Style TextBoxStyle
        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.Style" /> applied to
        /// the text box portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// </summary>
        /// <value>The <see cref="T:System.Windows.Style" /> applied to the text
        /// box portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// The default is null.</value>
        [ExcludeFromCodeCoverage]
        public Style TextBoxStyle
        {
            get { return GetValue(TextBoxStyleProperty) as Style; }
            set { SetValue(TextBoxStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.TextBoxStyle" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.TextBoxStyle" />
        /// dependency property.</value>
        public static readonly DependencyProperty TextBoxStyleProperty =
            DependencyProperty.Register(
                ElementTextBoxStyle,
                typeof(Style),
                typeof(AutoCompleteBox),
                new PropertyMetadata(null));

        #endregion public Style TextBoxStyle

        #region public double MaxDropDownHeight
        /// <summary>
        /// Gets or sets the maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// </summary>
        /// <value>The maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// The default is <see cref="F:System.Double.PositiveInfinity" />.</value>
        /// <exception cref="T:System.ArgumentException">The specified value is less than 0.</exception>
        [ExcludeFromCodeCoverage]
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register(
                "MaxDropDownHeight",
                typeof(double),
                typeof(AutoCompleteBox),
                new PropertyMetadata(double.PositiveInfinity, OnMaxDropDownHeightPropertyChanged));

        /// <summary>
        /// MaxDropDownHeightProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteTextBox that changed its MaxDropDownHeight.</param>
        /// <param name="e">Event arguments.</param>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The exception will be called through a CLR setter in most cases.")]
        private static void OnMaxDropDownHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox source = d as AutoCompleteBox;
            if(source != null && source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            double newValue = (double)e.NewValue;

            // Revert to the old value if invalid (negative)
            if(newValue < 0)
            {
                if(source != null)
                {
                    source._ignorePropertyChange = true;
                    source.SetValue(e.Property, e.OldValue);
                }

                // ReSharper disable once NotResolvedInText
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Dev2.Runtime.Configuration.Properties.Resources.AutoComplete_OnMaxDropDownHeightPropertyChanged_InvalidValue, e.NewValue), "value");
            }

            source?.OnMaxDropDownHeightChanged(newValue);
        }
        #endregion public double MaxDropDownHeight

        #region public bool IsDropDownOpen
        /// <summary>
        /// Gets or sets a value indicating whether the drop-down portion of
        /// the control is open.
        /// </summary>
        /// <value>
        /// True if the drop-down is open; otherwise, false. The default is
        /// false.
        /// </value>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.IsDropDownOpen" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.IsDropDownOpen" />
        /// dependency property.</value>
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(
                "IsDropDownOpen",
                typeof(bool),
                typeof(AutoCompleteBox),
                new PropertyMetadata(false, OnIsDropDownOpenPropertyChanged));

        /// <summary>
        /// IsDropDownOpenProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteTextBox that changed its IsDropDownOpen.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsDropDownOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox source = d as AutoCompleteBox;

            // Ignore the change if requested
            if(source != null && source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            if(source != null)
            {
                if(newValue)
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
        /// <summary>
        /// Gets or sets a collection that is used to generate the items for the
        /// drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// </summary>
        /// <value>The collection that is used to generate the items of the
        /// drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.</value>
        public IEnumerable ItemsSource
        {
            get { return GetValue(ItemsSourceProperty) as IEnumerable; }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemsSource" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemsSource" />
        /// dependency property.</value>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(AutoCompleteBox),
                new PropertyMetadata(OnItemsSourcePropertyChanged));

        /// <summary>
        /// ItemsSourceProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteBox that changed its ItemsSource.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoComplete = d as AutoCompleteBox;
            autoComplete?.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        #endregion public IEnumerable ItemsSource

        #region public object SelectedItem
        /// <summary>
        /// Gets or sets the selected item in the drop-down.
        /// </summary>
        /// <value>The selected item in the drop-down.</value>
        /// <remarks>
        /// If the IsTextCompletionEnabled property is true and text typed by 
        /// the user matches an item in the ItemsSource collection, which is 
        /// then displayed in the text box, the SelectedItem property will be 
        /// a null reference.
        /// </remarks>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.SelectedItem" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.SelectedItem" />
        /// dependency property.</value>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(object),
                typeof(AutoCompleteBox),
                new PropertyMetadata(OnSelectedItemPropertyChanged));

        /// <summary>
        /// SelectedItemProperty property changed handler. Fires the 
        /// SelectionChanged event. The event data will contain any non-null
        /// removed items and non-null additions.
        /// </summary>
        /// <param name="d">AutoCompleteBox that changed its SelectedItem.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox source = d as AutoCompleteBox;
            if(source != null)
            {
                if(source._ignorePropertyChange)
                {
                    source._ignorePropertyChange = false;
                    return;
                }

                // Update the text display
                if(source._skipSelectedItemTextUpdate)
                {
                    source._skipSelectedItemTextUpdate = false;
                }
                else
                {
                    source.OnSelectedItemChanged(e.NewValue);
                }
            }


            // Fire the SelectionChanged event
            List<object> removed = new List<object>();
            if(e.OldValue != null)
            {
                removed.Add(e.OldValue);
            }

            List<object> added = new List<object>();
            if(e.NewValue != null)
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

        /// <summary>
        /// Called when the selected item is changed, updates the text value
        /// that is displayed in the text box part.
        /// </summary>
        /// <param name="newItem">The new item.</param>
        private void OnSelectedItemChanged(object newItem)
        {
            if (CustomSelection)
            {
                return;
            }
            var text = newItem == null ? SearchText : FormatValue(newItem, true);

            // Update the Text property and the TextBox values
            UpdateTextValue(text);

            // Move the caret to the end of the text box
            if(TextBox != null && Text != null)
            {
                TextBox.SelectionStart = Text.Length;
            }
        }

        #endregion public object SelectedItem
        
        public bool CustomSelection { get; set; }
        #region public string Text
        /// <summary>
        /// Gets or sets the text in the text box portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// </summary>
        /// <value>The text in the text box portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.</value>
        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.Text" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.Text" />
        /// dependency property.</value>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(AutoCompleteBox),
                new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        /// <summary>
        /// TextProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteBox that changed its Text.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox source = d as AutoCompleteBox;
            source?.TextUpdated((string)e.NewValue, false);
        }

        #endregion public string Text

        #region public string SearchText
        /// <summary>
        /// Gets the text that is used to filter items in the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemsSource" />
        /// item collection.
        /// </summary>
        /// <value>The text that is used to filter items in the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemsSource" />
        /// item collection.</value>
        /// <remarks>
        /// The SearchText value is typically the same as the 
        /// Text property, but is set after the TextChanged event occurs 
        /// and before the Populating event.
        /// </remarks>
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

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.SearchText" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.SearchText" />
        /// dependency property.</value>
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(
                "SearchText",
                typeof(string),
                typeof(AutoCompleteBox),
                new PropertyMetadata(string.Empty, OnSearchTextPropertyChanged));

        /// <summary>
        /// OnSearchTextProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteBox that changed its SearchText.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSearchTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox source = d as AutoCompleteBox;
            if(source != null && source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            // Ensure the property is only written when expected
            if(source != null && !source._allowWrite)
            {
                // Reset the old value before it was incorrectly written
                source._ignorePropertyChange = true;
                source.SetValue(e.Property, e.OldValue);

                throw new InvalidOperationException(Dev2.Runtime.Configuration.Properties.Resources.AutoComplete_OnSearchTextPropertyChanged_InvalidWrite);
            }
        }
        #endregion public string SearchText

        #region public AutoCompleteFilterMode FilterMode
        /// <summary>
        /// Gets or sets how the text in the text box is used to filter items
        /// specified by the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemsSource" />
        /// property for display in the drop-down.
        /// </summary>
        /// <value>One of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteFilterMode" />
        /// values The default is
        /// <see cref="F:System.Windows.Controls.AutoCompleteFilterMode.StartsWith" />.</value>
        /// <exception cref="T:System.ArgumentException">The specified value is
        /// not a valid
        /// <see cref="T:System.Windows.Controls.AutoCompleteFilterMode" />.</exception>
        /// <remarks>
        /// Use the FilterMode property to specify how possible matches are 
        /// filtered. For example, possible matches can be filtered in a 
        /// predefined or custom way. The search mode is automatically set to 
        /// Custom if you set the ItemFilter property. 
        /// </remarks>
        public AutoCompleteFilterMode FilterMode
        {
            get { return (AutoCompleteFilterMode)GetValue(FilterModeProperty); }
            set { SetValue(FilterModeProperty, value); }
        }

        /// <summary>
        /// Gets the identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.FilterMode" />
        /// dependency property.
        /// </summary>
        public static readonly DependencyProperty FilterModeProperty =
            DependencyProperty.Register(
                "FilterMode",
                typeof(AutoCompleteFilterMode),
                typeof(AutoCompleteBox),
                new PropertyMetadata(AutoCompleteFilterMode.StartsWith, OnFilterModePropertyChanged));

        /// <summary>
        /// FilterModeProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteBox that changed its FilterMode.</param>
        /// <param name="e">Event arguments.</param>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The exception will be thrown when the CLR setter is used in most situations.")]
        [ExcludeFromCodeCoverage]
        // ReSharper disable once CyclomaticComplexity
        private static void OnFilterModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox source = d as AutoCompleteBox;
            AutoCompleteFilterMode mode = (AutoCompleteFilterMode)e.NewValue;

            if(mode != AutoCompleteFilterMode.Contains &&
                mode != AutoCompleteFilterMode.ContainsCaseSensitive &&
                mode != AutoCompleteFilterMode.ContainsOrdinal &&
                mode != AutoCompleteFilterMode.ContainsOrdinalCaseSensitive &&
                mode != AutoCompleteFilterMode.Custom &&
                mode != AutoCompleteFilterMode.Equals &&
                mode != AutoCompleteFilterMode.EqualsCaseSensitive &&
                mode != AutoCompleteFilterMode.EqualsOrdinal &&
                mode != AutoCompleteFilterMode.EqualsOrdinalCaseSensitive &&
                mode != AutoCompleteFilterMode.None &&
                mode != AutoCompleteFilterMode.StartsWith &&
                mode != AutoCompleteFilterMode.StartsWithCaseSensitive &&
                mode != AutoCompleteFilterMode.StartsWithOrdinal &&
                mode != AutoCompleteFilterMode.StartsWithOrdinalCaseSensitive)
            {
                source?.SetValue(e.Property, e.OldValue);

                // ReSharper disable once NotResolvedInText
                throw new ArgumentException(Dev2.Runtime.Configuration.Properties.Resources.AutoComplete_OnFilterModePropertyChanged_InvalidValue, "value");
            }

            // Sets the filter predicate for the new value
            AutoCompleteFilterMode newValue = (AutoCompleteFilterMode)e.NewValue;
            if(source != null)
            {
                source.TextFilter = AutoCompleteSearch.GetFilter(newValue);
            }
        }
        #endregion public AutoCompleteFilterMode FilterMode

        #region public AutoCompleteFilterPredicate ItemFilter
        /// <summary>
        /// Gets or sets the custom method that uses user-entered text to filter
        /// the items specified by the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemsSource" />
        /// property for display in the drop-down.
        /// </summary>
        /// <value>The custom method that uses the user-entered text to filter
        /// the items specified by the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemsSource" />
        /// property. The default is null.</value>
        /// <remarks>
        /// The filter mode is automatically set to Custom if you set the 
        /// ItemFilter property. 
        /// </remarks>
        public AutoCompleteFilterPredicate<object> ItemFilter
        {
            get { return GetValue(ItemFilterProperty) as AutoCompleteFilterPredicate<object>; }
            set { SetValue(ItemFilterProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemFilter" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemFilter" />
        /// dependency property.</value>
        public static readonly DependencyProperty ItemFilterProperty =
            DependencyProperty.Register(
                "ItemFilter",
                typeof(AutoCompleteFilterPredicate<object>),
                typeof(AutoCompleteBox),
                new PropertyMetadata(OnItemFilterPropertyChanged));

        /// <summary>
        /// ItemFilterProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteBox that changed its ItemFilter.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnItemFilterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox source = d as AutoCompleteBox;
            AutoCompleteFilterPredicate<object> value = e.NewValue as AutoCompleteFilterPredicate<object>;

            // If null, revert to the "None" predicate
            if(value == null)
            {
                if(source != null)
                {
                    source.FilterMode = AutoCompleteFilterMode.None;
                }
            }
            else
            {
                if(source != null)
                {
                    source.FilterMode = AutoCompleteFilterMode.Custom;
                    source.TextFilter = null;
                }
            }
        }
        #endregion public AutoCompleteFilterPredicate ItemFilter

        #region public AutoCompleteStringFilterPredicate TextFilter
        /// <summary>
        /// Gets or sets the custom method that uses the user-entered text to
        /// filter items specified by the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemsSource" />
        /// property in a text-based way for display in the drop-down.
        /// </summary>
        /// <value>The custom method that uses the user-entered text to filter
        /// items specified by the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemsSource" />
        /// property in a text-based way for display in the drop-down.</value>
        /// <remarks>
        /// The search mode is automatically set to Custom if you set the 
        /// TextFilter property. 
        /// </remarks>
        public AutoCompleteFilterPredicate<string> TextFilter
        {
            get { return GetValue(TextFilterProperty) as AutoCompleteFilterPredicate<string>; }
            set { SetValue(TextFilterProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.TextFilter" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.TextFilter" />
        /// dependency property.</value>
        public static readonly DependencyProperty TextFilterProperty =
            DependencyProperty.Register(
                "TextFilter",
                typeof(AutoCompleteFilterPredicate<string>),
                typeof(AutoCompleteBox),
                new PropertyMetadata(AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.StartsWith)));
        #endregion public AutoCompleteStringFilterPredicate TextFilter

        #region Template parts

        /// <summary>
        /// Gets or sets the drop down popup control.
        /// </summary>
        private PopupHelper DropDownPopup { get; set; }

        /// <summary>
        /// The TextBox template part.
        /// </summary>
        private TextBox _text;

        /// <summary>
        /// The SelectionAdapter.
        /// </summary>
        private ISelectionAdapter _adapter;

        /// <summary>
        /// Gets or sets the Text template part.
        /// </summary>
        public TextBox TextBox
        {
            get { return _text; }
            set
            {
                // Detach existing handlers
                if(_text != null)
                {
                    _text.SelectionChanged -= OnTextBoxSelectionChanged;
                    _text.TextChanged -= OnTextBoxTextChanged;
                }

                _text = value;

                // Attach handlers
                if(_text != null)
                {
                    _text.SelectionChanged += OnTextBoxSelectionChanged;
                    _text.TextChanged += OnTextBoxTextChanged;

                    if(Text != null)
                    {
                        UpdateTextValue(Text);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the selection adapter used to populate the drop-down
        /// with a list of selectable items.
        /// </summary>
        /// <value>The selection adapter used to populate the drop-down with a
        /// list of selectable items.</value>
        /// <remarks>
        /// You can use this property when you create an automation peer to 
        /// use with AutoCompleteBox or deriving from AutoCompleteBox to 
        /// create a custom control.
        /// </remarks>
        protected internal ISelectionAdapter SelectionAdapter
        {
            get { return _adapter; }
            set
            {
                if(_adapter != null)
                {
                    _adapter.SelectionChanged -= OnAdapterSelectionChanged;
                    _adapter.Commit -= OnAdapterSelectionComplete;
                    _adapter.Cancel -= OnAdapterSelectionCanceled;
                    _adapter.Cancel -= OnAdapterSelectionComplete;
                    _adapter.ItemsSource = null;
                }

                _adapter = value;

                if(_adapter != null)
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

        /// <summary>
        /// Occurs when the text in the text box portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> changes.
        /// </summary>
#if SILVERLIGHT
        public event RoutedEventHandler TextChanged;
#else
        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AutoCompleteBox));

        /// <summary>
        /// Occurs when the text in the text box portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> changes.
        /// </summary>
        public event RoutedEventHandler TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }
#endif

        /// <summary>
        /// Occurs when the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> is
        /// populating the drop-down with possible matches based on the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.Text" />
        /// property.
        /// </summary>
        /// <remarks>
        /// If the event is canceled, by setting the PopulatingEventArgs.Cancel 
        /// property to true, the AutoCompleteBox will not automatically 
        /// populate the selection adapter contained in the drop-down. 
        /// In this case, if you want possible matches to appear, you must 
        /// provide the logic for populating the selection adapter.
        /// </remarks>
#if SILVERLIGHT
        public event PopulatingEventHandler Populating;
#else
        public static readonly RoutedEvent PopulatingEvent = EventManager.RegisterRoutedEvent("Populating", RoutingStrategy.Bubble, typeof(PopulatingEventHandler), typeof(AutoCompleteBox));

        /// <summary>
        /// Occurs when the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> is
        /// populating the drop-down with possible matches based on the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.Text" />
        /// property.
        /// </summary>
        /// <remarks>
        /// If the event is canceled, by setting the PopulatingEventArgs.Cancel 
        /// property to true, the AutoCompleteBox will not automatically 
        /// populate the selection adapter contained in the drop-down. 
        /// In this case, if you want possible matches to appear, you must 
        /// provide the logic for populating the selection adapter.
        /// </remarks>
        public event PopulatingEventHandler Populating
        {
            add { AddHandler(PopulatingEvent, value); }
            remove { RemoveHandler(PopulatingEvent, value); }
        }
#endif

        /// <summary>
        /// Occurs when the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> has
        /// populated the drop-down with possible matches based on the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.Text" />
        /// property.
        /// </summary>
#if SILVERLIGHT
        public event PopulatedEventHandler Populated;
#else
        public static readonly RoutedEvent PopulatedEvent = EventManager.RegisterRoutedEvent("Populated", RoutingStrategy.Bubble, typeof(PopulatedEventHandler), typeof(AutoCompleteBox));

        /// <summary>
        /// Occurs when the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> has
        /// populated the drop-down with possible matches based on the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.Text" />
        /// property.
        /// </summary>
        public event PopulatedEventHandler Populated
        {
            add { AddHandler(PopulatedEvent, value); }
            remove { RemoveHandler(PopulatedEvent, value); }
        }
#endif

        /// <summary>
        /// Occurs when the value of the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.IsDropDownOpen" />
        /// property is changing from false to true.
        /// </summary>
#if SILVERLIGHT
        public event RoutedPropertyChangingEventHandler<bool> DropDownOpening;
#else
        public static readonly RoutedEvent DropDownOpeningEvent = EventManager.RegisterRoutedEvent("DropDownOpening", RoutingStrategy.Bubble, typeof(RoutedPropertyChangingEventHandler<bool>), typeof(AutoCompleteBox));

        /// <summary>
        /// Occurs when the value of the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.IsDropDownOpen" />
        /// property is changing from false to true.
        /// </summary>
        public event RoutedPropertyChangingEventHandler<bool> DropDownOpening
        {
            add { AddHandler(PopulatedEvent, value); }
            remove { RemoveHandler(PopulatedEvent, value); }
        }
#endif

        /// <summary>
        /// Occurs when the value of the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.IsDropDownOpen" />
        /// property has changed from false to true and the drop-down is open.
        /// </summary>
#if SILVERLIGHT
        public event RoutedPropertyChangedEventHandler<bool> DropDownOpened;
#else
        public static readonly RoutedEvent DropDownOpenedEvent = EventManager.RegisterRoutedEvent("DropDownOpened", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(AutoCompleteBox));

        /// <summary>
        /// Occurs when the value of the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.IsDropDownOpen" />
        /// property has changed from false to true and the drop-down is open.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<bool> DropDownOpened
        {
            add { AddHandler(DropDownOpenedEvent, value); }
            remove { RemoveHandler(DropDownOpenedEvent, value); }
        }
#endif

        /// <summary>
        /// Occurs when the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.IsDropDownOpen" />
        /// property is changing from true to false.
        /// </summary>
#if SILVERLIGHT
        public event RoutedPropertyChangingEventHandler<bool> DropDownClosing;
#else
        public static readonly RoutedEvent DropDownClosingEvent = EventManager.RegisterRoutedEvent("DropDownClosing", RoutingStrategy.Bubble, typeof(RoutedPropertyChangingEventHandler<bool>), typeof(AutoCompleteBox));

        /// <summary>
        /// Occurs when the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.IsDropDownOpen" />
        /// property is changing from true to false.
        /// </summary>
        public event RoutedPropertyChangingEventHandler<bool> DropDownClosing
        {
            add { AddHandler(DropDownClosingEvent, value); }
            remove { RemoveHandler(DropDownClosingEvent, value); }
        }
#endif

        /// <summary>
        /// Occurs when the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.IsDropDownOpen" />
        /// property was changed from true to false and the drop-down is open.
        /// </summary>
#if SILVERLIGHT
        public event RoutedPropertyChangedEventHandler<bool> DropDownClosed;
#else
        public static readonly RoutedEvent DropDownClosedEvent = EventManager.RegisterRoutedEvent("DropDownClosed", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(AutoCompleteBox));

        /// <summary>
        /// Occurs when the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.IsDropDownOpen" />
        /// property was changed from true to false and the drop-down is open.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<bool> DropDownClosed
        {
            add { AddHandler(DropDownClosedEvent, value); }
            remove { RemoveHandler(DropDownClosedEvent, value); }
        }
#endif

        /// <summary>
        /// Occurs when the selected item in the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> has
        /// changed.
        /// </summary>
#if SILVERLIGHT
        public event SelectionChangedEventHandler SelectionChanged;
#else
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(AutoCompleteBox));

        /// <summary>
        /// Occurs when the selected item in the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> has
        /// changed.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }
#endif

        /// <summary>
        /// Gets or sets the  <see cref="T:System.Windows.Data.Binding" /> that
        /// is used to get the values for display in the text portion of
        /// the <see cref="T:System.Windows.Controls.AutoCompleteBox" />
        /// control.
        /// </summary>
        /// <value>The <see cref="T:System.Windows.Data.Binding" /> object used
        /// when binding to a collection property.</value>
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

        /// <summary>
        /// Gets or sets the property path that is used to get values for
        /// display in the text portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// </summary>
        /// <value>The property path that is used to get values for display in
        /// the text portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.</value>
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
        /// <summary>
        /// Gets or sets the observable collection that contains references to 
        /// all of the items in the generated view of data that is provided to 
        /// the selection-style control adapter.
        /// </summary>
        public ObservableCollection<object> View => _view;

#if !SILVERLIGHT
        /// <summary>
        /// Initializes the static members of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> class.
        /// </summary>
        static AutoCompleteBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox), new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
        }
#endif

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> class.
        /// </summary>
        public AutoCompleteBox()
        {
#if SILVERLIGHT  
            DefaultStyleKey = typeof(AutoCompleteBox);

            Loaded += (sender, e) => ApplyTemplate();
#endif
            IsEnabledChanged += ControlIsEnabledChanged;

            Interaction = new InteractionHelper(this);

            // Creating the view here ensures that View is always != null
            ClearView();
            if (Application.Current != null)
                Style = Application.Current.TryFindResource("AutoCompleteBoxStyle") as Style;
        }

        /// <summary>
        /// Arranges and sizes the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" />
        /// control and its contents.
        /// </summary>
        /// <param name="finalSize">The size allowed for the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.</param>
        /// <returns>The <paramref name="finalSize" />, unchanged.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Size r = base.ArrangeOverride(finalSize);
            DropDownPopup?.Arrange();
            return r;
        }

        /// <summary>
        /// Builds the visual tree for the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
#if !SILVERLIGHT
            if(TextBox != null)
            {
                TextBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
            }
#endif
            if(DropDownPopup != null)
            {
                DropDownPopup.Closed -= DropDownPopupClosed;
                DropDownPopup.FocusChanged -= OnDropDownFocusChanged;
                DropDownPopup.UpdateVisualStates -= OnDropDownPopupUpdateVisualStates;
                DropDownPopup.BeforeOnApplyTemplate();
                DropDownPopup = null;
            }

            base.OnApplyTemplate();

            // Set the template parts. Individual part setters remove and add 
            // any event handlers.
            Popup popup = GetTemplateChild(ElementPopup) as Popup;
            if(popup != null)
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
            if(TextBox != null)
            {
                TextBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
            }
#endif
            Interaction.OnApplyTemplateBase();

            // If the drop down property indicates that the popup is open,
            // flip its value to invoke the changed handler.
            if(IsDropDownOpen && DropDownPopup != null && !DropDownPopup.IsOpen)
            {
                OpeningDropDown(false);
            }
        }

        /// <summary>
        /// Allows the popup wrapper to fire visual state change events.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnDropDownPopupUpdateVisualStates(object sender, EventArgs e)
        {
            UpdateVisualState(true);
        }

        /// <summary>
        /// Allows the popup wrapper to fire the FocusChanged event.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnDropDownFocusChanged(object sender, EventArgs e)
        {
            FocusChanged(HasFocus());
        }

        /// <summary>
        /// Begin closing the drop-down.
        /// </summary>
        /// <param name="oldValue">The original value.</param>
        private void ClosingDropDown(bool oldValue)
        {
            bool delayedClosingVisual = false;
            if(DropDownPopup != null)
            {
                delayedClosingVisual = DropDownPopup.UsesClosingVisualState;
            }

#if SILVERLIGHT
            RoutedPropertyChangingEventArgs<bool> args = new RoutedPropertyChangingEventArgs<bool>(IsDropDownOpenProperty, oldValue, false, true);
#else
            RoutedPropertyChangingEventArgs<bool> args = new RoutedPropertyChangingEventArgs<bool>(IsDropDownOpenProperty, oldValue, false, true, DropDownClosingEvent);
#endif

            OnDropDownClosing(args);

            if(_view == null || _view.Count == 0)
            {
                delayedClosingVisual = false;
            }

            if(args.Cancel)
            {
                _ignorePropertyChange = true;
                SetValue(IsDropDownOpenProperty, oldValue);
            }
            else
            {
                // Immediately close the drop down window:
                // When a popup closed visual state is present, the code path is 
                // slightly different and the actual call to CloseDropDown will 
                // be called only after the visual state's transition is done
                RaiseExpandCollapseAutomationEvent(oldValue, false);
                if(!delayedClosingVisual)
                {
                    CloseDropDown(oldValue, false);
                }
            }

            UpdateVisualState(true);
        }

        /// <summary>
        /// Begin opening the drop down by firing cancelable events, opening the
        /// drop-down or reverting, depending on the event argument values.
        /// </summary>
        /// <param name="oldValue">The original value, if needed for a revert.</param>
        private void OpeningDropDown(bool oldValue)
        {
#if SILVERLIGHT
            RoutedPropertyChangingEventArgs<bool> args = new RoutedPropertyChangingEventArgs<bool>(IsDropDownOpenProperty, oldValue, true, true);
#else
            RoutedPropertyChangingEventArgs<bool> args = new RoutedPropertyChangingEventArgs<bool>(IsDropDownOpenProperty, oldValue, true, true, DropDownOpeningEvent);
#endif
            // Opening
            OnDropDownOpening(args);

            if(args.Cancel)
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

        /// <summary>
        /// Raise an expand/collapse event through the automation peer.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
        {
            AutoCompleteBoxAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as AutoCompleteBoxAutomationPeer;
            peer?.RaiseExpandCollapseAutomationEvent(oldValue, newValue);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Handles the PreviewKeyDown event on the TextBox for WPF. This method
        /// is not implemented for Silverlight.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }
#endif

        /// <summary>
        /// Connects to the DropDownPopup Closed event.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void DropDownPopupClosed(object sender, EventArgs e)
        {
            // Force the drop down dependency property to be false.
            if(IsDropDownOpen)
            {
                IsDropDownOpen = false;
            }

            // Fire the DropDownClosed event
            if(_popupHasOpened)
            {
#if SILVERLIGHT
                OnDropDownClosed(new RoutedPropertyChangedEventArgs<bool>(true, false));
#else
                OnDropDownClosed(new RoutedPropertyChangedEventArgs<bool>(true, false, DropDownClosedEvent));
#endif
            }
        }

        /// <summary>
        /// Creates an
        /// <see cref="T:System.Windows.Automation.Peers.AutoCompleteBoxAutomationPeer" />
        /// </summary>
        /// <returns>A
        /// <see cref="T:System.Windows.Automation.Peers.AutoCompleteBoxAutomationPeer" />
        /// for the <see cref="T:System.Windows.Controls.AutoCompleteBox" />
        /// object.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AutoCompleteBoxAutomationPeer(this);
        }

        #region Focus

        /// <summary>
        /// Handles the FocusChanged event.
        /// </summary>
        /// <param name="hasFocus">A value indicating whether the control 
        /// currently has the focus.</param>
        private void FocusChanged(bool hasFocus)
        {
            // The OnGotFocus & OnLostFocus are asynchronously and cannot 
            // reliably tell you that have the focus.  All they do is let you 
            // know that the focus changed sometime in the past.  To determine 
            // if you currently have the focus you need to do consult the 
            // FocusManager (see HasFocus()).

            if(hasFocus)
            {
                
            }
            else
            {
                IsDropDownOpen = false;
                _userCalledPopulate = false;
                TextBox?.Select(TextBox.Text.Length, 0);
            }
        }

        /// <summary>
        /// Determines whether the text box or drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control has
        /// focus.
        /// </summary>
        /// <returns>true to indicate the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> has focus;
        /// otherwise, false.</returns>
        protected bool HasFocus()
        {
            DependencyObject focused =
#if SILVERLIGHT
                FocusManager.GetFocusedElement() as DependencyObject;
#else
                // For WPF, check if the element that has focus is within the control, as
                // FocusManager.GetFocusedElement(this) will return null in such a case.
                IsKeyboardFocusWithin ? Keyboard.FocusedElement as DependencyObject : FocusManager.GetFocusedElement(this) as DependencyObject;
#endif
            while(focused != null)
            {
                if(ReferenceEquals(focused, this))
                {
                    return true;
                }

                // This helps deal with popups that may not be in the same 
                // visual tree
                DependencyObject parent = VisualTreeHelper.GetParent(focused);
                if(parent == null)
                {
                    // Try the logical parent.
                    FrameworkElement element = focused as FrameworkElement;
                    if(element != null)
                    {
                        parent = element.Parent;
                    }
                }
                focused = parent;
            }
            return false;
        }

        /// <summary>
        /// Provides handling for the
        /// <see cref="E:System.Windows.UIElement.GotFocus" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs" />
        /// that contains the event data.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            FocusChanged(HasFocus());
        }

#if !SILVERLIGHT
        /// <summary>
        /// Handles change of keyboard focus, which is treated differently than control focus
        /// </summary>
        /// <param name="e"></param>
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);
            FocusChanged((bool)e.NewValue);
        }
#endif

        /// <summary>
        /// Provides handling for the
        /// <see cref="E:System.Windows.UIElement.LostFocus" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs" />
        /// that contains the event data.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            FocusChanged(HasFocus());
        }

        #endregion

        /// <summary>
        /// Handle the change of the IsEnabled property.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void ControlIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool isEnabled = (bool)e.NewValue;
            if(!isEnabled)
            {
                IsDropDownOpen = false;
                //if (TextBox != null)
                //{
                //    TextBox.Text = string.Empty;
                //}
            }
        }

        /// <summary>
        /// Returns the
        /// <see cref="T:System.Windows.Controls.ISelectionAdapter" /> part, if
        /// possible.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Windows.Controls.ISelectionAdapter" /> object,
        /// if possible. Otherwise, null.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Following the GetTemplateChild pattern for the method.")]
        // ReSharper disable once VirtualMemberNeverOverriden.Global
        [ExcludeFromCodeCoverage]
        protected virtual ISelectionAdapter GetSelectionAdapterPart()
        {
            ISelectionAdapter adapter = null;
            Selector selector = GetTemplateChild(ElementSelector) as Selector;
            if(selector != null)
            {
                // Check if it is already an IItemsSelector
                // ReSharper disable once SuspiciousTypeConversion.Global
                adapter = selector as ISelectionAdapter ?? new SelectorSelectionAdapter(selector);
            }
            // ReSharper disable once SuspiciousTypeConversion.Global
            return adapter ?? GetTemplateChild(ElementSelectionAdapter) as ISelectionAdapter;
        }

        /// <summary>
        /// Handles the timer tick when using a populate delay.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event arguments.</param>
        [ExcludeFromCodeCoverage]
        private void PopulateDropDown(object sender, EventArgs e)
        {
            _delayTimer?.Stop();

            // Update the prefix/search text.
            SearchText = Text;

            // The Populated event enables advanced, custom filtering. The 
            // client needs to directly update the ItemsSource collection or
            // call the Populate method on the control to continue the 
            // display process if Cancel is set to true.
#if SILVERLIGHT
            PopulatingEventArgs populating = new PopulatingEventArgs(SearchText);
#else
            PopulatingEventArgs populating = new PopulatingEventArgs(SearchText, PopulatingEvent);
#endif

            OnPopulating(populating);
            if(!populating.Cancel)
            {
                PopulateComplete();
            }
        }

        /// <summary>
        /// Raises the
        /// <see cref="E:System.Windows.Controls.AutoCompleteBox.Populating" />
        /// event.
        /// </summary>
        /// <param name="e">A
        /// <see cref="T:System.Windows.Controls.PopulatingEventArgs" /> that
        /// contains the event data.</param>
        // ReSharper disable once VirtualMemberNeverOverriden.Global
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

        /// <summary>
        /// Raises the
        /// <see cref="E:System.Windows.Controls.AutoCompleteBox.Populated" />
        /// event.
        /// </summary>
        /// <param name="e">A
        /// <see cref="T:System.Windows.Controls.PopulatedEventArgs" />
        /// that contains the event data.</param>
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

        /// <summary>
        /// Raises the
        /// <see cref="E:System.Windows.Controls.AutoCompleteBox.SelectionChanged" />
        /// event.
        /// </summary>
        /// <param name="e">A
        /// <see cref="T:System.Windows.Controls.SelectionChangedEventArgs" />
        /// that contains the event data.</param>
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

            AutoCompleteBox box = (AutoCompleteBox)e.Source;
            ListBox innerListBox = (ListBox)box?.Template?.FindName("Selector", box);
            innerListBox?.ScrollIntoView(innerListBox.SelectedItem);

            RaiseEvent(e);
        }

        /// <summary>
        /// Raises the
        /// <see cref="E:System.Windows.Controls.AutoCompleteBox.DropDownOpening" />
        /// event.
        /// </summary>
        /// <param name="e">A
        /// <see cref="T:System.Windows.Controls.RoutedPropertyChangingEventArgs`1" />
        /// that contains the event data.</param>
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

        /// <summary>
        /// Raises the
        /// <see cref="E:System.Windows.Controls.AutoCompleteBox.DropDownOpened" />
        /// event.
        /// </summary>
        /// <param name="e">A
        /// <see cref="T:System.Windows.RoutedPropertyChangedEventArgs`1" />
        /// that contains the event data.</param>
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

        /// <summary>
        /// Raises the
        /// <see cref="E:System.Windows.Controls.AutoCompleteBox.DropDownClosing" />
        /// event.
        /// </summary>
        /// <param name="e">A
        /// <see cref="T:System.Windows.Controls.RoutedPropertyChangingEventArgs`1" />
        /// that contains the event data.</param>
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

        /// <summary>
        /// Raises the
        /// <see cref="E:System.Windows.Controls.AutoCompleteBox.DropDownClosed" />
        /// event.
        /// </summary>
        /// <param name="e">A
        /// <see cref="T:System.Windows.RoutedPropertyChangedEventArgs`1" />
        /// which contains the event data.</param>
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

        /// <summary>
        /// Formats an Item for text comparisons based on Converter 
        /// and ConverterCulture properties.
        /// </summary>
        /// <param name="value">The object to format.</param>
        /// <param name="clearDataContext">A value indicating whether to clear
        /// the data context after the lookup is performed.</param>
        /// <returns>Formatted Value.</returns>
        private string FormatValue(object value, bool clearDataContext)
        {
            string str = FormatValue(value);
            if(clearDataContext)
            {
                _valueBindingEvaluator?.ClearDataContext();
            }
            return str;
        }

        /// <summary>
        /// Converts the specified object to a string by using the
        /// <see cref="P:System.Windows.Data.Binding.Converter" /> and
        /// <see cref="P:System.Windows.Data.Binding.ConverterCulture" /> values
        /// of the binding object specified by the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ValueMemberBinding" />
        /// property.
        /// </summary>
        /// <param name="value">The object to format as a string.</param>
        /// <returns>The string representation of the specified object.</returns>
        /// <remarks>
        /// Override this method to provide a custom string conversion.
        /// </remarks>
        protected virtual string FormatValue(object value)
        {
            if(_valueBindingEvaluator != null)
            {
                return _valueBindingEvaluator.GetDynamicValue(value) ?? string.Empty;
            }

            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Raises the
        /// <see cref="E:System.Windows.Controls.AutoCompleteBox.TextChanged" />
        /// event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs" />
        /// that contains the event data.</param>
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

        /// <summary>
        /// Handle the TextChanged event that is directly attached to the 
        /// TextBox part. This ensures that only user initiated actions will 
        /// result in an AutoCompleteBox suggestion and operation.
        /// </summary>
        /// <param name="sender">The source TextBox object.</param>
        /// <param name="e">The TextChanged event data.</param>
        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            // Call the central updated text method as a user-initiated action
            TextUpdated(_text.Text, true);
        }

        /// <summary>
        /// When selection changes, save the location of the selection start.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnTextBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
            // If ignoring updates. This happens after text is updated, and 
            // before the PopulateComplete method is called. Required for the 
            // IsTextCompletionEnabled feature.
            if(_ignoreTextSelectionChange)
            {
                return;
            }

            _textSelectionStart = _text.SelectionStart;
        }

        /// <summary>
        /// Updates both the text box value and underlying text dependency 
        /// property value if and when they change. Automatically fires the 
        /// text changed events when there is a change.
        /// </summary>
        /// <param name="value">The new string value.</param>
        private void UpdateTextValue(string value)
        {
            // ReSharper disable once IntroduceOptionalParameters.Local
            UpdateTextValue(value, null);
        }

        /// <summary>
        /// Updates both the text box value and underlying text dependency 
        /// property value if and when they change. Automatically fires the 
        /// text changed events when there is a change.
        /// </summary>
        /// <param name="value">The new string value.</param>
        /// <param name="userInitiated">A nullable bool value indicating whether
        /// the action was user initiated. In a user initiated mode, the 
        /// underlying text dependency property is updated. In a non-user 
        /// interaction, the text box value is updated. When user initiated is 
        /// null, all values are updated.</param>
        private void UpdateTextValue(string value, bool? userInitiated)
        {
            // Update the Text dependency property
            if((userInitiated == null || userInitiated == true) && Text != value)
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

            // Update the TextBox's Text dependency property
            if((userInitiated == null || userInitiated == false) && TextBox != null && TextBox.Text != value)
            {
                _ignoreTextPropertyChange++;
                TextBox.Text = value ?? string.Empty;

                // Text dependency property value was set, fire event
                if(Text == value || Text == null)
                {
#if SILVERLIGHT
                    OnTextChanged(new RoutedEventArgs());
#else
                    OnTextChanged(new RoutedEventArgs(TextChangedEvent));
#endif
                }
            }
        }

        /// <summary>
        /// Handle the update of the text for the control from any source, 
        /// including the TextBox part and the Text dependency property.
        /// </summary>
        /// <param name="newText">The new text.</param>
        /// <param name="userInitiated">A value indicating whether the update 
        /// is a user-initiated action. This should be a True value when the 
        /// TextUpdated method is called from a TextBox event handler.</param>
        private void TextUpdated(string newText, bool userInitiated)
        {
            // Only process this event if it is coming from someone outside 
            // setting the Text dependency property directly.
            if(_ignoreTextPropertyChange > 0)
            {
                _ignoreTextPropertyChange--;
                return;
            }

            if(newText == null)
            {
                newText = string.Empty;
            }

            // The TextBox.TextChanged event was not firing immediately and 
            // was causing an immediate update, even with wrapping. If there is 
            // a selection currently, no update should happen.
            if(IsTextCompletionEnabled && TextBox != null && TextBox.SelectionLength > 0 && TextBox.SelectionStart != TextBox.Text.Length)
            {
                return;
            }

            // Evaluate the conditions needed for completion.
            // 1. Minimum prefix length
            // 2. If a delay timer is in use, use it
            bool populateReady = newText.Length >= MinimumPrefixLength && MinimumPrefixLength >= 0;
            _userCalledPopulate = populateReady && userInitiated;

            // Update the interface and values only as necessary
            UpdateTextValue(newText, userInitiated);

            if(populateReady)
            {
                _ignoreTextSelectionChange = true;

                if(_delayTimer != null)
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
                if(SelectedItem != null)
                {
                    _skipSelectedItemTextUpdate = true;
                }
                SelectedItem = null;
                if(IsDropDownOpen)
                {
                    IsDropDownOpen = false;
                }
            }
        }

        /// <summary>
        /// Notifies the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> that the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemsSource" />
        /// property has been set and the data can be filtered to provide
        /// possible matches in the drop-down.
        /// </summary>
        /// <remarks>
        /// Call this method when you are providing custom population of 
        /// the drop-down portion of the AutoCompleteBox, to signal the control 
        /// that you are done with the population process. 
        /// Typically, you use PopulateComplete when the population process 
        /// is a long-running process and you want to cancel built-in filtering
        ///  of the ItemsSource items. In this case, you can handle the 
        /// Populated event and set PopulatingEventArgs.Cancel to true. 
        /// When the long-running process has completed you call 
        /// PopulateComplete to indicate the drop-down is populated.
        /// </remarks>
        public void PopulateComplete()
        {
            // Apply the search filter
            RefreshView();

            // Fire the Populated event containing the read-only view data.
#if SILVERLIGHT
            PopulatedEventArgs populated = new PopulatedEventArgs(new ReadOnlyCollection<object>(_view));
#else
            PopulatedEventArgs populated = new PopulatedEventArgs(new ReadOnlyCollection<object>(_view), PopulatedEvent);
#endif
            OnPopulated(populated);

            // ReSharper disable once PossibleUnintendedReferenceComparison
            if(SelectionAdapter != null && SelectionAdapter.ItemsSource != _view)
            {
                SelectionAdapter.ItemsSource = _view;
            }

            bool isDropDownOpen = _userCalledPopulate && _view.Count > 0;
            if(isDropDownOpen != IsDropDownOpen)
            {
                _ignorePropertyChange = true;
                IsDropDownOpen = isDropDownOpen;
            }
            if(IsDropDownOpen)
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

        /// <summary>
        /// Performs text completion, if enabled, and a lookup on the underlying
        /// item values for an exact match. Will update the SelectedItem value.
        /// </summary>
        /// <param name="userInitiated">A value indicating whether the operation
        /// was user initiated. Text completion will not be performed when not 
        /// directly initiated by the user.</param>
        private void UpdateTextCompletion(bool userInitiated)
        {
            // By default this method will clear the selected value
            object newSelectedItem = null;
            string text = Text;

            // Text search is StartsWith explicit and only when enabled, in 
            // line with WPF's ComboBox lookup. When in use it will associate 
            // a Value with the Text if it is found in ItemsSource. This is 
            // only valid when there is data and the user initiated the action.
            if(_view.Count > 0)
            {
                if(IsTextCompletionEnabled && TextBox != null && userInitiated)
                {
                    int currentLength = TextBox.Text.Length;
                    int selectionStart = TextBox.SelectionStart;
                    if(selectionStart == text.Length && selectionStart > _textSelectionStart)
                    {
                        // When the FilterMode dependency property is set to 
                        // either StartsWith or StartsWithCaseSensitive, the 
                        // first item in the view is used. This will improve 
                        // performance on the lookup. It assumes that the 
                        // FilterMode the user has selected is an acceptable 
                        // case sensitive matching function for their scenario.
                        object top = FilterMode == AutoCompleteFilterMode.StartsWith || FilterMode == AutoCompleteFilterMode.StartsWithCaseSensitive
                            ? _view[0]
                            : TryGetMatch(text, _view, AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.StartsWith));

                        // If the search was successful, update SelectedItem
                        if(top != null)
                        {
                            newSelectedItem = top;
                            string topString = FormatValue(top, true);

                            // Only replace partially when the two words being the same
                            int minLength = Math.Min(topString.Length, Text.Length);
                            if(AutoCompleteSearch.Equals(Text.Substring(0, minLength), topString.Substring(0, minLength)))
                            {
                                // Update the text
                                UpdateTextValue(topString);

                                // Select the text past the user's caret
                                TextBox.SelectionStart = currentLength;
                                TextBox.SelectionLength = topString.Length - currentLength;
                            }
                        }
                    }
                }
                else
                {
                    // Perform an exact string lookup for the text. This is a 
                    // design change from the original Toolkit release when the 
                    // IsTextCompletionEnabled property behaved just like the 
                    // WPF ComboBox's IsTextSearchEnabled property.
                    //
                    // This change provides the behavior that most people expect
                    // to find: a lookup for the value is always performed.
                    newSelectedItem = TryGetMatch(text, _view, AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.EqualsCaseSensitive));
                }
            }

            // Update the selected item property

            if(SelectedItem != newSelectedItem)
            {
                _skipSelectedItemTextUpdate = true;
            }
            SelectedItem = newSelectedItem;

            // Restore updates for TextSelection
            if(_ignoreTextSelectionChange)
            {
                _ignoreTextSelectionChange = false;
                if(TextBox != null)
                {
                    _textSelectionStart = TextBox.SelectionStart;
                }
            }
        }

        /// <summary>
        /// Attempts to look through the view and locate the specific exact 
        /// text match.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="view">The view reference.</param>
        /// <param name="predicate">The predicate to use for the partial or 
        /// exact match.</param>
        /// <returns>Returns the object or null.</returns>
        private object TryGetMatch(string searchText, ObservableCollection<object> view, AutoCompleteFilterPredicate<string> predicate)
        {
            if(view != null && view.Count > 0)
            {
                return view.FirstOrDefault(o => predicate(searchText, FormatValue(o)));
            }

            return null;
        }

        /// <summary>
        /// A simple helper method to clear the view and ensure that a view 
        /// object is always present and not null.
        /// </summary>
        private void ClearView()
        {
            if(_view == null)
            {
                _view = new ObservableCollection<object>();
            }
            else
            {
                _view.Clear();
            }
        }

        /// <summary>
        /// Walks through the items enumeration. Performance is not going to be 
        /// perfect with the current implementation.
        /// </summary>
        // ReSharper disable once CyclomaticComplexity
        private void RefreshView()
        {
            if(_items == null)
            {
                ClearView();
                return;
            }

            // Cache the current text value
            string text = Text ?? string.Empty;

            // Determine if any filtering mode is on
            bool stringFiltering = TextFilter != null;
            bool objectFiltering = FilterMode == AutoCompleteFilterMode.Custom && TextFilter == null;

            int viewIndex = 0;
            int viewCount = _view.Count;
            List<object> items = _items;
            foreach(object item in items)
            {
                bool inResults = !(stringFiltering || objectFiltering);
                if(!inResults)
                {
                    inResults = stringFiltering ? TextFilter(text, FormatValue(item)) : ItemFilter(text, item);
                }

                if(viewCount > viewIndex && inResults && _view[viewIndex] == item)
                {
                    // Item is still in the view
                    viewIndex++;
                }
                else if(inResults)
                {
                    // Insert the item
                    if(viewCount > viewIndex && _view[viewIndex] != item)
                    {
                        // Replace item
                        // Unfortunately replacing via index throws a fatal 
                        // Cost: O(n) vs O(1)
                        _view.RemoveAt(viewIndex);
                        _view.Insert(viewIndex, item);
                        viewIndex++;
                    }
                    else
                    {
                        // Add the item
                        if(viewIndex == viewCount)
                        {
                            // Constant time is preferred (Add).
                            _view.Add(item);
                        }
                        else
                        {
                            _view.Insert(viewIndex, item);
                        }
                        viewIndex++;
                        viewCount++;
                    }
                }
                else if(viewCount > viewIndex && _view[viewIndex] == item)
                {
                    // Remove the item
                    _view.RemoveAt(viewIndex);
                    viewCount--;
                }
            }

            // Clear the evaluator to discard a reference to the last item
            _valueBindingEvaluator?.ClearDataContext();
        }

        /// <summary>
        /// Handle any change to the ItemsSource dependency property, update 
        /// the underlying ObservableCollection view, and set the selection 
        /// adapter's ItemsSource to the view if appropriate.
        /// </summary>
        /// <param name="oldValue">The old enumerable reference.</param>
        /// <param name="newValue">The new enumerable reference.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldValue", Justification = "This makes it easy to add validation or other changes in the future.")]
        [ExcludeFromCodeCoverage]
        private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            // Remove handler for oldValue.CollectionChanged (if present)
            INotifyCollectionChanged oldValueINotifyCollectionChanged = oldValue as INotifyCollectionChanged;
            if(null != oldValueINotifyCollectionChanged && null != _collectionChangedWeakEventListener)
            {
                _collectionChangedWeakEventListener.Detach();
                _collectionChangedWeakEventListener = null;
            }

            // Add handler for newValue.CollectionChanged (if possible)
            INotifyCollectionChanged newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
            if(null != newValueINotifyCollectionChanged)
            {
                _collectionChangedWeakEventListener = new WeakEventListener<AutoCompleteBox, object, NotifyCollectionChangedEventArgs>(this) { OnEventAction = (instance, source, eventArgs) => instance.ItemsSourceCollectionChanged(eventArgs), OnDetachAction = weakEventListener => newValueINotifyCollectionChanged.CollectionChanged -= weakEventListener.OnEvent };
                newValueINotifyCollectionChanged.CollectionChanged += _collectionChangedWeakEventListener.OnEvent;
            }

            // Store a local cached copy of the data
            _items = newValue == null ? null : new List<object>(newValue.Cast<object>().ToList());

            // Clear and set the view on the selection adapter
            ClearView();
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if(SelectionAdapter != null && SelectionAdapter.ItemsSource != _view)
            {
                SelectionAdapter.ItemsSource = _view;
            }
            if(IsDropDownOpen)
            {
                RefreshView();
            }
        }

        /// <summary>
        /// Method that handles the ObservableCollection.CollectionChanged event for the ItemsSource property.
        /// </summary>
        /// <param name="e">The event data.</param>
        // ReSharper disable once CyclomaticComplexity
        [ExcludeFromCodeCoverage]
        private void ItemsSourceCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Update the cache
            if(e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                for(int index = 0; index < e.OldItems.Count; index++)
                {
                    _items.RemoveAt(e.OldStartingIndex);
                }
            }
            if(e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && _items.Count >= e.NewStartingIndex)
            {
                for(int index = 0; index < e.NewItems.Count; index++)
                {
                    _items.Insert(e.NewStartingIndex + index, e.NewItems[index]);
                }
            }
            if(e.Action == NotifyCollectionChangedAction.Replace && e.NewItems != null && e.OldItems != null)
            {
                foreach(object t in e.NewItems)
                {
                    _items[e.NewStartingIndex] = t;
                }
            }

            // Update the view
            if(e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                if(e.OldItems != null)
                {
                    foreach(object t in e.OldItems)
                    {
                        _view.Remove(t);
                    }
                }
            }

            if(e.Action == NotifyCollectionChangedAction.Reset)
            {
                // Significant changes to the underlying data.
                ClearView();
                if(ItemsSource != null)
                {
                    _items = new List<object>(ItemsSource.Cast<object>().ToList());
                }
            }

            // Refresh the observable collection used in the selection adapter.
            RefreshView();
        }

        #region Selection Adapter

        /// <summary>
        /// Handles the SelectionChanged event of the selection adapter.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The selection changed event data.</param>
        [ExcludeFromCodeCoverage]
        private void OnAdapterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = _adapter.SelectedItem;
        }

        /// <summary>
        /// Handles the Commit event on the selection adapter.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        [ExcludeFromCodeCoverage]
        private void OnAdapterSelectionComplete(object sender, RoutedEventArgs e)
        {
            IsDropDownOpen = false;

            // Completion will update the selected value
            UpdateTextCompletion(false);

            // Text should not be selected
            TextBox?.Select(TextBox.Text.Length, 0);

#if SILVERLIGHT
            Focus();
#else
            // Focus is treated differently in SL and WPF.
            // This forces the textbox to get keyboard focus, in the case where
            // another part of the control may have temporarily received focus.
            if(TextBox != null)
            {
                Keyboard.Focus(TextBox);
            }
            else
            {
                Focus();
            }
#endif
        }

        /// <summary>
        /// Handles the Cancel event on the selection adapter.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        [ExcludeFromCodeCoverage]
        private void OnAdapterSelectionCanceled(object sender, RoutedEventArgs e)
        {
            UpdateTextValue(SearchText);

            // Completion will update the selected value
            UpdateTextCompletion(false);
        }

        #endregion

        #region Popup

        /// <summary>
        /// Handles MaxDropDownHeightChanged by re-arranging and updating the 
        /// popup arrangement.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newValue", Justification = "This makes it easy to add validation or other changes in the future.")]
        [ExcludeFromCodeCoverage]
        private void OnMaxDropDownHeightChanged(double newValue)
        {
            if(DropDownPopup != null)
            {
                DropDownPopup.MaxDropDownHeight = newValue;
                DropDownPopup.Arrange();
            }
            UpdateVisualState(true);
        }

        /// <summary>
        /// Private method that directly opens the popup, checks the expander 
        /// button, and then fires the Opened event.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private void OpenDropDown(bool oldValue, bool newValue)
        {
            if(DropDownPopup != null)
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

        /// <summary>
        /// Private method that directly closes the popup, flips the Checked 
        /// value, and then fires the Closed event.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected void CloseDropDown(bool oldValue, bool newValue)
        {
            if(_popupHasOpened)
            {
                if(SelectionAdapter != null)
                {
                    SelectionAdapter.SelectedItem = null;
                }
                if(DropDownPopup != null)
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

        /// <summary>
        /// Provides handling for the
        /// <see cref="E:System.Windows.UIElement.KeyDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Input.KeyEventArgs" />
        /// that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if(e == null)
            {
                // ReSharper disable once UseNameofExpression
                throw new ArgumentNullException("e");
            }

            base.OnKeyDown(e);

            if(e.Handled || !IsEnabled)
            {
                return;
            }

            // The drop down is open, pass along the key event arguments to the
            // selection adapter. If it isn't handled by the adapter's logic,
            // then we handle some simple navigation scenarios for controlling
            // the drop down.
            if(IsDropDownOpen)
            {
                if(SelectionAdapter != null)
                {
                    SelectionAdapter.HandleKeyDown(e);
                    if(e.Handled)
                    {
                        return;
                    }
                }

                if(e.Key == Key.Escape)
                {
                    OnAdapterSelectionCanceled(this, new RoutedEventArgs());
                    e.Handled = true;
                }

                switch (e.Key)
                {
                    case Key.F4:
                        IsDropDownOpen = !IsDropDownOpen;
                        e.Handled = true;
                        break;

                    case Key.Enter:
                        OnAdapterSelectionComplete(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                }
            }
            else
            {
                // The drop down is not open, the Down key will toggle it open.
//                if(e.Key == Key.Down)
//                {
//                    IsDropDownOpen = true;
//                    e.Handled = true;
//                }
            }

            // Standard drop down navigation
            
        }

        /// <summary>
        /// Update the visual state of the control.
        /// </summary>
        /// <param name="useTransitions">
        /// A value indicating whether to automatically generate transitions to
        /// the new state, or instantly transition to the new state.
        /// </param>
        void IUpdateVisualState.UpdateVisualState(bool useTransitions)
        {
            UpdateVisualState(useTransitions);
        }

        /// <summary>
        /// Update the current visual state of the button.
        /// </summary>
        /// <param name="useTransitions">
        /// True to use transitions when updating the visual state, false to
        /// snap directly to the new visual state.
        /// </param>
        internal virtual void UpdateVisualState(bool useTransitions)
        {
            // Popup
            VisualStateManager.GoToState(this, IsDropDownOpen ? VisualStates.StatePopupOpened : VisualStates.StatePopupClosed, useTransitions);

            // Handle the Common and Focused states
            Interaction.UpdateVisualStateBase(useTransitions);
        }
    }
}
