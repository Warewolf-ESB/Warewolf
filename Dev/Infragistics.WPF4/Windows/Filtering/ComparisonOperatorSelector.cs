using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Controls
{
    /// <summary>
    /// An element used for selecting from a filtered list of <see cref="ComparisonOperator"/>s 
    /// </summary>

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,          GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,       GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,        GroupName = VisualStateUtilities.GroupCommon)]

    [TemplateVisualState(Name = VisualStateUtilities.StateFocused,         GroupName = VisualStateUtilities.GroupFocus)]
    [TemplateVisualState(Name = VisualStateUtilities.StateFocusedDropDown, GroupName = VisualStateUtilities.GroupFocus)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnfocused,       GroupName = VisualStateUtilities.GroupFocus)]

    [TemplatePart(Name = "PART_ComboBox", Type = typeof(ComboBox))]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ComparisonOperatorSelector : Control
    {
		#region Member Variables

        private ComparisonOperatorListItemCollection _items;
        private List<ComparisonOperatorListItem> _allOperatorList;
        private ObservableCollection<ComparisonOperatorListItem> _filteredList;


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		#endregion //Member Variables

		#region Constructor

		/// <summary>
        /// Initializes an instance of the <see cref="ComparisonOperatorSelector"/> class.
		/// </summary>
		public ComparisonOperatorSelector()
		{
            Array enumvalues = Enum.GetValues(typeof(ComparisonOperator));

            this._allOperatorList = new List<ComparisonOperatorListItem>(enumvalues.Length);

            foreach (ComparisonOperator oper in enumvalues)
            {
				// JJD 3/25/11 - TFS70334 
				// Moved creation and image binding logic into CreateOperatorListItem
				// helper method
				ComparisonOperatorListItem item = CreateOperatorListItem(oper);
                this._allOperatorList.Add(item);
            }

            this._filteredList = new ObservableCollection<ComparisonOperatorListItem>(this._allOperatorList);
            this._items = new ComparisonOperatorListItemCollection(this._filteredList);
		}

        static ComparisonOperatorSelector()
		{
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(typeof(ComparisonOperatorSelector)));
            Control.IsTabStopProperty.OverrideMetadata(typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
            FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

		#endregion //Constructor

        #region Base class overrides

        #region OnApplyTemplate

        /// <summary>
        /// Invoked when the template for the control has been changed.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

        #endregion //OnApplyTemplate	
           
        #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="ComparisonOperatorSelector"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.ComparisonOperatorSelectorAutomationPeer"/></returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new Infragistics.Windows.Automation.Peers.ComparisonOperatorSelectorAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer

        #region OnIsKeyboardFocusWithinChanged

        /// <summary>
        /// Invoked just before the System.Windows.UIElement.IsKeyboardFocusWithinChanged event is raised by this element. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

        #endregion //OnIsKeyboardFocusWithinChanged	

        #region OnMouseEnter

        /// <summary>
        /// Invoked when the mouse is moved within the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

        #endregion //OnMouseEnter


        #region OnMouseLeave

        /// <summary>
        /// Invoked when the mouse is moved outside the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

        #endregion //OnMouseLeave	
    
        #endregion //Base class overrides

		#region Properties

			#region Public Properties

                #region AllowableOperators

        /// <summary>
        /// Identifies the <see cref="AllowableOperators"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowableOperatorsProperty = DependencyProperty.Register("AllowableOperators",
            typeof(ComparisonOperatorFlags), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(ComparisonOperatorFlags.All, new PropertyChangedCallback(OnAllowableOperatorsChanged)));

        private static void OnAllowableOperatorsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ComparisonOperatorSelector chooser = target as ComparisonOperatorSelector;

            if (chooser != null)
            {
                ComparisonOperatorFlags newFlags = (ComparisonOperatorFlags)e.NewValue;
                ComparisonOperator[] newAllowableOperators = ComparisonCondition.GetComparisonOperators(newFlags);

                int newcount = newAllowableOperators.Length;
                int oldcount = chooser._filteredList.Count;

                #region Remove items from the filtered list that aren't in the mew set

                if (newcount < chooser._allOperatorList.Count)
                {
                    int j = newcount - 1;

                    // loop over the old values backwards and remove any that are not in the new list
                    for (int i = oldcount - 1; i >= 0; i--)
                    {
                        ComparisonOperator operOld = chooser._filteredList[i].Operator;
                        bool matchFound = false;

                        for (; j >= 0; j--)
                        {
                            ComparisonOperator operNew = newAllowableOperators[j];

                            if (operNew == operOld)
                            {
                                matchFound = true;
                                j--;
                                break;
                            }

                            if (operNew < operOld)
                                break;
                        }

                        if (!matchFound)
                            chooser._filteredList.RemoveAt(i);
                    }
                }

                #endregion //Remove items from the filtered list that aren't in the mew set	
    
                #region Add items to the filtered list if necessary

                oldcount = chooser._filteredList.Count;

                if (newcount > oldcount)
                {
                    int j = oldcount - 1;

                    // loop over new values backwards for efficiency sake and
                    // add ones that are missing
                    for (int i = newcount - 1; i >= 0; i--)
                    {
                        ComparisonOperator operNew = newAllowableOperators[i];
                        bool matchFound = false;

                        for (; j >= 0; j--)
                        {
                            ComparisonOperator operOld = chooser._filteredList[j].Operator;

                            if (operNew == operOld)
                            {
                                matchFound = true;
                                break;
                            }

                            if (operNew > operOld)
                                break;
                        }

                        if (!matchFound)
                            chooser._filteredList.Insert(j + 1, chooser._allOperatorList[(int)operNew]);

                    }
                }

                #endregion //Add items to the filtered list if necessary	
    
            }
        }

        /// <summary>
        /// Gets or sets a flagged enumeration that defines which operators are allowed.
        /// </summary>
        /// <seealso cref="AllowableOperatorsProperty"/>
        //[Description("Gets or sets a flagged enumeration that defines which operators are allowed.")]
        //[Category("Behavior")]
        [Bindable(true)]
        public ComparisonOperatorFlags AllowableOperators
        {
            get
            {
                return (ComparisonOperatorFlags)this.GetValue(ComparisonOperatorSelector.AllowableOperatorsProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.AllowableOperatorsProperty, value);
            }
        }

                #endregion //AllowableOperators

                #region DropDownButtonStyle

        /// <summary>
        /// Identifies the <see cref="DropDownButtonStyle"/> dependency property
        /// </summary>
        /// <seealso cref="DropDownButtonStyle"/>
        public static readonly DependencyProperty DropDownButtonStyleProperty = DependencyProperty.Register("DropDownButtonStyle",
            typeof(Style), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the style used for the ToggleButton used inside the template`
        /// </summary>
        /// <seealso cref="DropDownButtonStyleProperty"/>
        /// <seealso cref="DropDownButtonStyleKey"/>
        //[Description("Gets or sets the style used for the ToggleButton used inside the template`")]
        //[Category("Appearance")]
        [Bindable(true)]
        public Style DropDownButtonStyle
        {
            get
            {
                return (Style)this.GetValue(ComparisonOperatorSelector.DropDownButtonStyleProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.DropDownButtonStyleProperty, value);
            }
        }

                #endregion //DropDownButtonStyle

                #region DropDownButtonStyleKey

        /// <summary>
        /// The key that identifies a resource to be used as the DropDownButtonStyle.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey DropDownButtonStyleKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "DropDownButtonStyleKey");

                #endregion //DropDownButtonStyleKey	

                #region IsDropDownOpen

        /// <summary>
        /// Identifies the <see cref="IsDropDownOpen"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen",
            typeof(bool), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
                , new PropertyChangedCallback(OnVisualStatePropertyChanged)

                ));

        /// <summary>
        /// Gets or sets whether the list of operators is dropped down
        /// </summary>
        /// <seealso cref="IsDropDownOpenProperty"/>
        //[Description("Gets or sets whether the list of operators is dropped down")]
        //[Category("Appearance")]
        [Browsable(false)]
        public bool IsDropDownOpen
        {
            get
            {
                return (bool)this.GetValue(ComparisonOperatorSelector.IsDropDownOpenProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.IsDropDownOpenProperty, value);
            }
        }

                #endregion //IsDropDownOpen
    	
                #region Items

        /// <summary>
        /// Returns a read-only collection of <see cref="ComparisonOperatorListItem"/>s that are filtered based on the <see cref="AllowableOperators"/> setting.
        /// </summary>
        /// <seealso cref="AllowableOperatorsProperty"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        [Bindable(true)]
        [Browsable(false)]
        public ComparisonOperatorListItemCollection Items
        {
            get
            {
                return this._items;
            }
        }

                #endregion //Items

                #region SelectedIndex

        /// <summary>
        /// Identifies the 'SelectedIndex' dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex",
                typeof(int), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(-1, new PropertyChangedCallback(OnSelectedIndexChanged)));

        private static void OnSelectedIndexChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ComparisonOperatorSelector control = target as ComparisonOperatorSelector;

            if (control != null)
            {
                control._cachedSelectedIndex = (int)e.NewValue;

                // sync up the selected items with the new index
                if (control._cachedSelectedIndex >= 0)
                {
                    control.SelectedOperator = control._items[control._cachedSelectedIndex].Operator;
                }
            }
        }

        private int _cachedSelectedIndex = -1;

        /// <summary>
        /// Gets or sets the zero-based index of the selected item
        /// </summary>
        /// <value>The zero-based index of the selected item in the <see cref="Items"/> collection or -1 if no item is selected.</value>
        /// <seealso cref="SelectedOperatorInfo"/>
        /// <seealso cref="SelectedOperator"/>
        //[Description("Gets or sets the zwero-based index of the selected item")]
        //[Category("Behavior")]
        [Bindable(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get
            {
                return this._cachedSelectedIndex;
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.SelectedIndexProperty, value);
            }
        }
                #endregion //SelectedIndex

                #region SelectedOperator

        /// <summary>
        /// Identifies the 'SelectedOperator' dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedOperatorProperty = DependencyProperty.Register("SelectedOperator",
                typeof(ComparisonOperator?), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedOperatorChanged)));

        /// <summary>
        /// Event ID for the 'SelectedOperatorChanged' routed event
        /// </summary>
        public static readonly RoutedEvent SelectedOperatorChangedEvent =
                EventManager.RegisterRoutedEvent("SelectedOperatorChanged", RoutingStrategy.Bubble, typeof(EventHandler<SelectedOperatorChangedEventArgs>), typeof(ComparisonOperatorSelector));

        private static void OnSelectedOperatorChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ComparisonOperatorSelector control = target as ComparisonOperatorSelector;

            if (control != null)
            {
                control._cachedSelectedOperator = (ComparisonOperator?)e.NewValue;
 
                // sync up the SelectedOperator property
                if (control._cachedSelectedOperator == null)
                {
                    control.ClearValue(SelectedOperatorInfoPropertyKey);

                    // JJD 12/21/09 - TFS25870
                    // Since nothing is selected sync up the SelectedIndex property
                    control.SetValue(SelectedIndexProperty, (int)-1);
                }
                else
                {
                    int index = control._items.IndexOfOperator(control._cachedSelectedOperator.Value);

					if (index >= 0)
						control.SetValue(SelectedOperatorInfoPropertyKey, control._items[index]);
					else
					{
						// JJD 3/25/11 - TFS70334 
						// Instead of creating a new ComparisonOperatorListItem first try to
						// find it in the _allOperatorList
						//control.SetValue(SelectedOperatorInfoPropertyKey, new ComparisonOperatorListItem(control._cachedSelectedOperator.Value));
						ComparisonOperator oper = control._cachedSelectedOperator.Value;
						int allItemIndex = -1;
						int count = control._allOperatorList.Count;

						for (int i = 0; i < count; i++)
						{
							if (control._allOperatorList[i].Operator == oper)
							{
								allItemIndex = i;
								break;
							}
						}

						// JJD 3/25/11 - TFS70334 
						// Instead of creating a new ComparisonOperatorListItem first try to
						// find it in the _allOperatorList.
						// If it still wasn't found then create a new one using the CreateOperatorListItem
						// helper method
						if ( allItemIndex >= 0 )
							control.SetValue(SelectedOperatorInfoPropertyKey, control._allOperatorList[allItemIndex]);
						else
							control.SetValue(SelectedOperatorInfoPropertyKey, control.CreateOperatorListItem(control._cachedSelectedOperator.Value));
					}
                    
                    // JJD 12/21/09 - TFS25870
                    // Sync up the SelectedIndex property
                    control.SetValue(SelectedIndexProperty, index);
                }

               control.OnSelectedOperatorChanged((ComparisonOperator?)e.OldValue, (ComparisonOperator?)e.NewValue);
            }
        }

        private ComparisonOperator? _cachedSelectedOperator = null;

        /// <summary>
        /// Gets or sets the selected operator
        /// </summary>
        //[Description("Gets or sets the selected operator")]
        //[Category("Behavior")]
        [Bindable(true)]
        [Browsable(false)]
        public ComparisonOperator? SelectedOperator
        {
            get
            {
                return this._cachedSelectedOperator;
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.SelectedOperatorProperty, value);
            }
        }

        /// <summary>
        /// Called when property 'SelectedOperator' changes
        /// </summary>
        protected virtual void OnSelectedOperatorChanged(ComparisonOperator? previousValue, ComparisonOperator? currentValue)
        {
            SelectedOperatorChangedEventArgs newEvent = new SelectedOperatorChangedEventArgs(previousValue, currentValue);
            newEvent.RoutedEvent = ComparisonOperatorSelector.SelectedOperatorChangedEvent;
            newEvent.Source = this;
            RaiseEvent(newEvent);
        }

        /// <summary>
        /// Occurs when property 'SelectedOperator' changes
        /// </summary>
        //[Description("Occurs when property 'SelectedOperator' changes")]
        //[Category("Behavior")]
        public event EventHandler<SelectedOperatorChangedEventArgs> SelectedOperatorChanged
        {
            add
            {
                base.AddHandler(ComparisonOperatorSelector.SelectedOperatorChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(ComparisonOperatorSelector.SelectedOperatorChangedEvent, value);
            }
        }

                #endregion //SelectedOperator

                #region SelectedOperatorInfo

        private static readonly DependencyPropertyKey SelectedOperatorInfoPropertyKey =
            DependencyProperty.RegisterReadOnly("SelectedOperatorInfo",
            typeof(ComparisonOperatorListItem), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="SelectedOperatorInfo"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedOperatorInfoProperty =
            SelectedOperatorInfoPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns an object that exposes information about the selected Operator (read-only).
        /// </summary>
        /// <seealso cref="ComparisonOperatorListItem"/>
        /// <seealso cref="Items"/>
        /// <seealso cref="SelectedOperatorInfoProperty"/>
        /// <seealso cref="SelectedOperator"/>
        //[Description("Returns an object that exposes information about the selected operator (read-only).")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ComparisonOperatorListItem SelectedOperatorInfo
        {
            get
            {
                return (ComparisonOperatorListItem)this.GetValue(ComparisonOperatorSelector.SelectedOperatorInfoProperty);
            }
        }

                #endregion //SelectedOperatorInfo

                #region Operator Images and ImageKeys

                #region OperatorEqualsImage

        /// <summary>
        /// Identifies the <see cref="OperatorEqualsImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorEqualsImageProperty = DependencyProperty.Register("OperatorEqualsImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'Equals' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorEqualsImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'Equals' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorEqualsImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorEqualsImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorEqualsImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorEqualsImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorEqualsImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorEqualsImageKey");

                #endregion //OperatorEqualsImage

                #region OperatorNotEqualsImage

        /// <summary>
        /// Identifies the <see cref="OperatorNotEqualsImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorNotEqualsImageProperty = DependencyProperty.Register("OperatorNotEqualsImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'NotEquals' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorNotEqualsImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'NotEquals' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorNotEqualsImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorNotEqualsImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorNotEqualsImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorNotEqualsImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorNotEqualsImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorNotEqualsImageKey");

                #endregion //OperatorNotEqualsImage

                #region OperatorLessThanImage

        /// <summary>
        /// Identifies the <see cref="OperatorLessThanImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorLessThanImageProperty = DependencyProperty.Register("OperatorLessThanImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'LessThan' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorLessThanImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'LessThan' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorLessThanImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorLessThanImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorLessThanImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorLessThanImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorLessThanImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorLessThanImageKey");

                #endregion //OperatorLessThanImage

                #region OperatorLessThanOrEqualToImage

        /// <summary>
        /// Identifies the <see cref="OperatorLessThanOrEqualToImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorLessThanOrEqualToImageProperty = DependencyProperty.Register("OperatorLessThanOrEqualToImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'LessThanOrEqualTo' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorLessThanOrEqualToImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'LessThanOrEqualTo' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorLessThanOrEqualToImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorLessThanOrEqualToImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorLessThanOrEqualToImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorLessThanOrEqualToImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorLessThanOrEqualToImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorLessThanOrEqualToImageKey");

                #endregion //OperatorLessThanOrEqualToImage

                #region OperatorGreaterThanImage

        /// <summary>
        /// Identifies the <see cref="OperatorGreaterThanImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorGreaterThanImageProperty = DependencyProperty.Register("OperatorGreaterThanImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'GreaterThan' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorGreaterThanImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'GreaterThan' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorGreaterThanImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorGreaterThanImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorGreaterThanImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorGreaterThanImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorGreaterThanImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorGreaterThanImageKey");

                #endregion //OperatorGreaterThanImage

                #region OperatorGreaterThanOrEqualToImage

        /// <summary>
        /// Identifies the <see cref="OperatorGreaterThanOrEqualToImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorGreaterThanOrEqualToImageProperty = DependencyProperty.Register("OperatorGreaterThanOrEqualToImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'GreaterThanOrEqualTo' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorGreaterThanOrEqualToImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'GreaterThanOrEqualTo' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorGreaterThanOrEqualToImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorGreaterThanOrEqualToImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorGreaterThanOrEqualToImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorGreaterThanOrEqualToImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorGreaterThanOrEqualToImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorGreaterThanOrEqualToImageKey");

                #endregion //OperatorGreaterThanOrEqualToImage

                #region OperatorContainsImage

        /// <summary>
        /// Identifies the <see cref="OperatorContainsImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorContainsImageProperty = DependencyProperty.Register("OperatorContainsImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'Contains' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorContainsImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'Contains' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorContainsImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorContainsImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorContainsImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorContainsImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorContainsImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorContainsImageKey");

                #endregion //OperatorContainsImage

                #region OperatorDoesNotContainImage

        /// <summary>
        /// Identifies the <see cref="OperatorDoesNotContainImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorDoesNotContainImageProperty = DependencyProperty.Register("OperatorDoesNotContainImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'DoesNotContain' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorDoesNotContainImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'DoesNotContain' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorDoesNotContainImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorDoesNotContainImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorDoesNotContainImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorDoesNotContainImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorDoesNotContainImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorDoesNotContainImageKey");

                #endregion //OperatorDoesNotContainImage

                #region OperatorLikeImage

        /// <summary>
        /// Identifies the <see cref="OperatorLikeImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorLikeImageProperty = DependencyProperty.Register("OperatorLikeImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'Like' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorLikeImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'Like' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorLikeImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorLikeImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorLikeImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorLikeImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorLikeImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorLikeImageKey");

                #endregion //OperatorLikeImage

                #region OperatorNotLikeImage

        /// <summary>
        /// Identifies the <see cref="OperatorNotLikeImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorNotLikeImageProperty = DependencyProperty.Register("OperatorNotLikeImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'NotLike' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorNotLikeImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'NotLike' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorNotLikeImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorNotLikeImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorNotLikeImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorNotLikeImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorNotLikeImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorNotLikeImageKey");

                #endregion //OperatorNotLikeImage

                #region OperatorMatchImage

        /// <summary>
        /// Identifies the <see cref="OperatorMatchImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorMatchImageProperty = DependencyProperty.Register("OperatorMatchImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'Match' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorMatchImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'Match' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorMatchImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorMatchImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorMatchImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorMatchImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorMatchImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorMatchImageKey");

                #endregion //OperatorMatchImage

                #region OperatorDoesNotMatchImage

        /// <summary>
        /// Identifies the <see cref="OperatorDoesNotMatchImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorDoesNotMatchImageProperty = DependencyProperty.Register("OperatorDoesNotMatchImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'DoesNotMatch' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorDoesNotMatchImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'DoesNotMatch' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorDoesNotMatchImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorDoesNotMatchImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorDoesNotMatchImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorDoesNotMatchImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorDoesNotMatchImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorDoesNotMatchImageKey");

                #endregion //OperatorDoesNotMatchImage

                #region OperatorStartsWithImage

        /// <summary>
        /// Identifies the <see cref="OperatorStartsWithImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorStartsWithImageProperty = DependencyProperty.Register("OperatorStartsWithImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'StartsWith' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorStartsWithImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'StartsWith' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorStartsWithImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorStartsWithImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorStartsWithImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorStartsWithImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorStartsWithImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorStartsWithImageKey");

                #endregion //OperatorStartsWithImage

                #region OperatorDoesNotStartWithImage

        /// <summary>
        /// Identifies the <see cref="OperatorDoesNotStartWithImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorDoesNotStartWithImageProperty = DependencyProperty.Register("OperatorDoesNotStartWithImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'DoesNotStartWith' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorDoesNotStartWithImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'DoesNotStartWith' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorDoesNotStartWithImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorDoesNotStartWithImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorDoesNotStartWithImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorDoesNotStartWithImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorDoesNotStartWithImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorDoesNotStartWithImageKey");

                #endregion //OperatorDoesNotStartWithImage

                #region OperatorEndsWithImage

        /// <summary>
        /// Identifies the <see cref="OperatorEndsWithImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorEndsWithImageProperty = DependencyProperty.Register("OperatorEndsWithImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'EndsWith' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorEndsWithImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'EndsWith' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorEndsWithImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorEndsWithImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorEndsWithImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorEndsWithImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorEndsWithImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorEndsWithImageKey");

                #endregion //OperatorEndsWithImage

                #region OperatorDoesNotEndWithImage

        /// <summary>
        /// Identifies the <see cref="OperatorDoesNotEndWithImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorDoesNotEndWithImageProperty = DependencyProperty.Register("OperatorDoesNotEndWithImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'DoesNotEndWith' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorDoesNotEndWithImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'DoesNotEndWith' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorDoesNotEndWithImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorDoesNotEndWithImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorDoesNotEndWithImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorDoesNotEndWithImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorDoesNotEndWithImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorDoesNotEndWithImageKey");

                #endregion //OperatorDoesNotEndWithImage

                #region OperatorTopImage

        /// <summary>
        /// Identifies the <see cref="OperatorTopImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorTopImageProperty = DependencyProperty.Register("OperatorTopImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'Top' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorTopImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'Top' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorTopImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorTopImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorTopImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorTopImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorTopImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorTopImageKey");

                #endregion //OperatorTopImage

                #region OperatorBottomImage

        /// <summary>
        /// Identifies the <see cref="OperatorBottomImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorBottomImageProperty = DependencyProperty.Register("OperatorBottomImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'Bottom' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorBottomImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'Bottom' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorBottomImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorBottomImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorBottomImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorBottomImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorBottomImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorBottomImageKey");

                #endregion //OperatorBottomImage

                #region OperatorTopPercentileImage

        /// <summary>
        /// Identifies the <see cref="OperatorTopPercentileImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorTopPercentileImageProperty = DependencyProperty.Register("OperatorTopPercentileImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'TopPercentile' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorTopPercentileImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'TopPercentile' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorTopPercentileImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorTopPercentileImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorTopPercentileImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorTopPercentileImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorTopPercentileImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorTopPercentileImageKey");

                #endregion //OperatorTopPercentileImage

                #region OperatorBottomPercentileImage

        /// <summary>
        /// Identifies the <see cref="OperatorBottomPercentileImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorBottomPercentileImageProperty = DependencyProperty.Register("OperatorBottomPercentileImage",
            typeof(ImageSource), typeof(ComparisonOperatorSelector), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource used to represent the 'BottomPercentile' operator
        /// </summary>
        /// <seealso cref="ComparisonOperator"/>
        /// <seealso cref="OperatorBottomPercentileImageProperty"/>
        //[Description("Gets or sets the ImageSource used to represent the 'BottomPercentile' operator")]
        //[Category("Appearance")]
        [Bindable(true)]
        public ImageSource OperatorBottomPercentileImage
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorSelector.OperatorBottomPercentileImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorSelector.OperatorBottomPercentileImageProperty, value);
            }
        }

        /// <summary>
        /// The key that identifies a resource to be used as the OperatorBottomPercentileImageKey.  Look here <see cref="PrimitivesBrushKeys" /> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey OperatorBottomPercentileImageKey = new StaticPropertyResourceKey(typeof(ComparisonOperatorSelector), "OperatorBottomPercentileImageKey");

                #endregion //OperatorBottomPercentileImage

                #endregion //Operator Images and ImageKeys	
    	
			#endregion //Public Properties

		#endregion //Properties

        #region Methods

        #region VisualState... Methods


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            if (this.IsEnabled)
            {
                if (this.IsMouseOver)
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
            }
            else
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateDisabled, VisualStateUtilities.StateNormal);

            if (this.IsDropDownOpen)
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateFocusedDropDown, VisualStateUtilities.StateFocused);
            else
                if (this.IsKeyboardFocusWithin)
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateFocused, useTransitions);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateUnfocused, useTransitions);
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ComparisonOperatorSelector selector = target as ComparisonOperatorSelector;

            selector.UpdateVisualStates();
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



        #endregion //VisualState... Methods	

			#region Private Methods

				#region CreateOperatorListItem

		// JJD 3/25/11 - TFS70334 
		// Moved creation and image binding logic into CreateOperatorListItem
		private ComparisonOperatorListItem CreateOperatorListItem(ComparisonOperator oper)
		{
			ComparisonOperatorListItem item = new ComparisonOperatorListItem(oper);
			BindingOperations.SetBinding(item, ComparisonOperatorListItem.ImageProperty, Utilities.CreateBindingObject("Operator" + oper.ToString() + "Image", BindingMode.OneWay, this));
			return item;
		}

				#endregion //CreateOperatorListItem

			#endregion //Private Methods	
    
		#endregion //Methods	
        	
	}
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved