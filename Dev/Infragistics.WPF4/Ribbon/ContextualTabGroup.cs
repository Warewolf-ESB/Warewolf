using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using Infragistics.Shared;
using Infragistics.Windows.Automation.Peers.Ribbon;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Represents a grouping of <see cref="RibbonTabItem"/> instances that relates to a specific context that has meaning to the application that is hosting 
	/// the <see cref="XamRibbon"/>.
	/// </summary>
	/// <remarks>
	/// <p class="body">ContextualTabGroup instances are added to the <see cref="XamRibbon.ContextualTabGroups"/> property. A contextual tab group 
	/// is made up of one or more <see cref="RibbonTabItem"/> instances. The tabs of a ContextualTabGroup are displayed after all the tabs that do not 
	/// belong to a ContextualTabGroup. When possible, the <see cref="Caption"/> of the group is displayed in the caption area of the XamRibbon above 
	/// the tab items it contains.</p>
	/// <p class="body">The <see cref="IsVisible"/> property may be used to show/hide the group and therefore the <see cref="Tabs"/> it contains.</p>
	/// <p class="body">The <see cref="BaseBackColor"/> can be used to control the base color that affects the appearance of the caption of the 
	/// ContextualTabGroup in the Ribbon's caption area as well as the general appearance of the tab item header and context area of the items within the 
	/// group's <see cref="Tabs"/> collection.</p>
	/// <p class="note">The ContextualTabGroup class is not a <see cref="Visual"/>. If you want to control the appearance of the group within the caption area 
	/// of the XamRibbon, you need to create a <see cref="DataTemplate"/> that targets the <see cref="ContextualTabGroup"/> type.</p>
	/// </remarks>
	/// <seealso cref="XamRibbon"/>
	/// <seealso cref="RibbonTabItem"/>
	/// <seealso cref="ContextualTabGroupCollection"/>
	/// <seealso cref="ContextualTabItemCollection"/>
	[ContentProperty("Tabs")]
	public class ContextualTabGroup : DependencyObjectNotifier
	{
		#region Member Variables

		private ContextualTabItemCollection		_tabs;
		private int _index = -1;

		private static readonly Color[] _baseColors = new Color[] { 
			Color.FromArgb(255, 243, 178, 024),  
			Color.FromArgb(255, 184, 142, 182),  
			Color.FromArgb(255, 114, 169, 184),  
			Color.FromArgb(255, 107, 184, 084),  
			Color.FromArgb(255, 252, 228, 029),  
			Color.FromArgb(255, 245, 081, 070),  
			};

        // AS 8/19/08 Automation support
        private WeakReference _captionElement;
        private ContextualTabGroupAutomationPeer _peer;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes an instance of the <see cref=" ContextualTabGroup"/> class.
		/// </summary>
		public ContextualTabGroup()
		{
			// AS 1/10/08 BR29564
			this.SetValue(System.Windows.Controls.ToolTipService.ShowDurationProperty, 20000);
		}

		/// <summary>
		/// Initializes an instance of the <see cref=" ContextualTabGroup"/> class with the specified key.
		/// </summary>
		/// <param name="key">The <paramref name="key"/> is a string that may be used to identify/locate the ContextualTabGroup within the <see cref="XamRibbon.ContextualTabGroups"/> collection.</param>
		/// <seealso cref="Key"/>
		public ContextualTabGroup(string key) : this()
		{
			this.Key = key;
		}

		#endregion //Constructor

        #region Base class overrides

            #region OnPropertyChanged

        /// <summary>
        /// Called when a property has been changed.
        /// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

			this.RaisePropertyChangedEvent(e.Property.Name);
        }

            #endregion //OnPropertyChanged

			#region ToString

		/// <summary>
		/// Returns a System.String that represents the current System.Object.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			// AS 1/9/08
			//return "ContextualTabGroup - Key: " + this.Key + ", Caption: " + this.Caption;
			return XamRibbon.GetString("ContextualTabGroupString", this.Key, this.Caption);
		}

			#endregion //ToString	
    
        #endregion //Base class overrides

		#region Commands

		/// <summary>
		/// Selects the first tab in the contextual tab group.
		/// </summary>
		internal static readonly RoutedCommand SelectFirstTabCommand = new RoutedCommand("SelectFirstTab", typeof(ContextualTabGroup));

		#endregion //Commands

		#region Properties

			#region Public Properties

				#region BaseBackColor

		/// <summary>
		/// Identifies the <see cref="BaseBackColor"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BaseBackColorProperty = DependencyProperty.Register("BaseBackColor",
			typeof(Color?), typeof(ContextualTabGroup), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBaseBackColorChanged)));


		private static void OnBaseBackColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ContextualTabGroup group = d as ContextualTabGroup;

			if (group != null)
			{
				// AS 10/22/07 BR27656
				// Always refresh the resolved color.
				//
				//if (e.NewValue == null)
					group.InitializeBaseBackColorResolved();
			}
		}

		/// <summary>
		/// Returns the color that should be used to determine the background for the caption of the group as well as the background of the tabs in the group.
		/// </summary>
		/// <remarks>
		/// <p class="body">The BaseBackColor is used to provide a single color that will be used to affect the appearance of the caption of the <see cref="ContextualTabGroup"/> 
		/// within the caption area of the <see cref="XamRibbon"/> as well as the appearance of the header and content area of the <see cref="RibbonTabItem"/> instances within the 
		/// group's <see cref="Tabs"/> collection.</p>
		/// <p class="body">By default, this property is set to null. When set to null, one of a set of default colors is used. The resolved value may be 
		/// obtained using the <see cref="BaseBackColorResolved"/>. For convenience, there is also a <see cref="BaseBackColorResolvedBrush"/> that will 
		/// return a <see cref="SolidColorBrush"/> which uses this resolved color as the color for the brush.</p>
		/// </remarks>
		/// <seealso cref="BaseBackColorProperty"/>
		/// <seealso cref="BaseBackColorResolved"/>
		/// <seealso cref="BaseBackColorResolvedBrush"/>
		//[Description("Returns the color that should be used to determine the background for the caption of the group as well as the background of the tabs in the group.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<Color>))] // AS 5/15/08 BR32816
		public Color? BaseBackColor
		{
			get
			{
				return (Color?)this.GetValue(ContextualTabGroup.BaseBackColorProperty);
			}
			set
			{
				this.SetValue(ContextualTabGroup.BaseBackColorProperty, value);
			}
		}

				#endregion //BaseBackColor

				#region BaseBackColorResolved

		private static readonly DependencyPropertyKey BaseBackColorResolvedPropertyKey =
			DependencyProperty.RegisterReadOnly("BaseBackColorResolved",
			typeof(Color), typeof(ContextualTabGroup), new FrameworkPropertyMetadata(Colors.Transparent, new PropertyChangedCallback(OnBaseBackColorResolvedChanged)));

		private static void OnBaseBackColorResolvedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Color c = (Color)e.NewValue;
			Brush newBrush = new SolidColorBrush(c);
			newBrush.Freeze();
			d.SetValue(BaseBackColorResolvedBrushPropertyKey, newBrush);
		}

		/// <summary>
		/// Identifies the <see cref="BaseBackColorResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BaseBackColorResolvedProperty =
			BaseBackColorResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the resolved <see cref="BaseBackColor"/> that should be used to render the tab group caption as well as the tab items in the group.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default, the <see cref="BaseBackColor"/> property is set to null (Nothing in VB). When that property is left set to null, 
		/// a default color is chosen for the ContextualTabGroup based on its index in the <see cref="XamRibbon.ContextualTabGroups"/> collection.</p>
		/// <p class="body">The <see cref="BaseBackColorResolvedBrush"/> may be used to obtain a <see cref="SolidColorBrush"/> which uses the color 
		/// from this property.</p>
		/// </remarks>
		/// <seealso cref="BaseBackColorResolvedProperty"/>
		/// <seealso cref="BaseBackColor"/>
		/// <seealso cref="BaseBackColorResolvedBrush"/>
		//[Description("Returns the resolved 'BaseBackColor' that should be used to render the tab group caption as well as the tab items in the group.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public Color BaseBackColorResolved
		{
			get
			{
				return (Color)this.GetValue(ContextualTabGroup.BaseBackColorResolvedProperty);
			}
		}

				#endregion //BaseBackColorResolved

				#region BaseBackColorResolvedBrush

		private static readonly DependencyPropertyKey BaseBackColorResolvedBrushPropertyKey =
			DependencyProperty.RegisterReadOnly("BaseBackColorResolvedBrush",
			typeof(Brush), typeof(ContextualTabGroup), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="BaseBackColorResolvedBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BaseBackColorResolvedBrushProperty =
			BaseBackColorResolvedBrushPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a <see cref="SolidColorBrush"/> using the <see cref="BaseBackColorResolved"/>
		/// </summary>
		/// <seealso cref="BaseBackColorResolvedBrushProperty"/>
		/// <seealso cref="BaseBackColor"/>
		/// <seealso cref="BaseBackColorResolved"/>
		//[Description("Returns a 'SolidColorBrush' using the 'BaseBackColorResolved'.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public Brush BaseBackColorResolvedBrush
		{
			get
			{
				return (Brush)this.GetValue(ContextualTabGroup.BaseBackColorResolvedBrushProperty);
			}
		}

				#endregion //BaseBackColorResolvedBrush

				#region Caption

		/// <summary>
		/// Identifies the <see cref="Caption"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption",
			typeof(string), typeof(ContextualTabGroup), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the caption of the <see cref="ContextualTabGroup"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The caption of the ContextualTabGroup is displayed above the associated tabs (when possible) within the 
		/// caption area of the <see cref="XamRibbon"/>.</p>
		/// </remarks>
		/// <seealso cref="CaptionProperty"/>
		/// <seealso cref="ContextualTabGroup"/>
		//[Description("Returns/sets the caption of the ContextualTabGroup.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string Caption
		{
			get
			{
				return (string)this.GetValue(ContextualTabGroup.CaptionProperty);
			}
			set
			{
				this.SetValue(ContextualTabGroup.CaptionProperty, value);
			}
		}

				#endregion //Caption

				#region IsVisible

		/// <summary>
		/// Identifies the <see cref="IsVisible"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register("IsVisible",
			typeof(bool), typeof(ContextualTabGroup), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets whether the <see cref="ContextualTabGroup"/> is Visible.  When set to false, any <see cref="RibbonTabItem"/>s in the 
		/// <see cref="ContextualTabGroup"/> will be hidden.
		/// </summary>
		/// <remarks>
		/// <p class="body">The IsVisible property is used to show/hide the <see cref="RibbonTabItem"/> instances in the group's <see cref="Tabs"/> 
		/// collection in a single operation.</p>
		/// <p class="note"><b>Note:</b> A RibbonTabItem will not be made visible if it has its <see cref="Visibility"/> set to Hidden or Collapsed regardless 
		/// of the value of the group's IsVisible property.</p>
		/// </remarks>
		/// <seealso cref="IsVisibleProperty"/>
		/// <seealso cref="ContextualTabGroup"/>
		/// <seealso cref="RibbonTabItem"/>
		//[Description("Returns/sets whether the ContextualTabGroup is Visible.  When set to false, any RibbonTabItems in the ContextualTabGroup will be hidden.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsVisible
		{
			get
			{
				return (bool)this.GetValue(ContextualTabGroup.IsVisibleProperty);
			}
			set
			{
				this.SetValue(ContextualTabGroup.IsVisibleProperty, value);
			}
		}

				#endregion //IsVisible

				#region Key

		/// <summary>
		/// Identifies the <see cref="Key"/> dependency property
		/// </summary>
		public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key",
			typeof(string), typeof(ContextualTabGroup), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the key of the <see cref="ContextualTabGroup"/> in the <see cref="ContextualTabGroupCollection"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The Key of the ContextualTabGroup is not required but provides a convenient way to locate a 
		/// group within the <see cref="XamRibbon.ContextualTabGroups"/> collection. If the Key is provided, it must be 
		/// unique with respect to the other ContextualTabGroup instances in the ContextualTabGroups collection of the Ribbon.</p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">If an attempt is made to add a <see cref="ContextualTabGroup"/> whose <see cref="Key"/> property value conflicts
		/// with the value of the <see cref="Key"/> property of an existing <see cref="ContextualTabGroup"/> in the collection.</exception>
		/// <seealso cref="KeyProperty"/>
		/// <seealso cref="ContextualTabGroup"/>
		/// <seealso cref="ContextualTabGroupCollection"/>
		//[Description("Returns/sets the key of the ContextualTabGroup in the ContextualTabGroupCollection.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string Key
		{
			get
			{
				return (string)this.GetValue(ContextualTabGroup.KeyProperty);
			}
			set
			{
				this.SetValue(ContextualTabGroup.KeyProperty, value);
			}
		}

				#endregion //Key

				#region Tabs

		/// <summary>
		/// Returns a collection of <see cref="RibbonTabItem"/>s that belong to the <see cref="ContextualTabGroup"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The Tabs property represents the collection of <see cref="RibbonTabItem"/> instances associated with 
		/// the <see cref="ContextualTabGroup"/>. Items added to the collection will have their <see cref="RibbonTabItem.ContextualTabGroup"/> 
		/// property set to this instance. This allows the tabs to access the <see cref="BaseBackColorResolved"/> within their templates.</p>
		/// <p class="note"><b>Note: </b>the <see cref="RibbonTabItem"/>s in this collection are not contained in the <see cref="Infragistics.Windows.Ribbon.XamRibbon.Tabs"/> 
		/// collection but ARE contained in the <see cref="Infragistics.Windows.Ribbon.XamRibbon.TabsInView"/> read-only collection if the <see cref="IsVisible"/> property 
		/// of the ContextualTabGroup is set to true.</p>
		/// </remarks>
		/// <seealso cref="ContextualTabGroup"/>
		/// <seealso cref="RibbonTabItem"/>
		/// <seealso cref="IsVisible"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.XamRibbon.Tabs"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.XamRibbon.TabsInView"/>
		//[Description("Returns a collection of RibbonTabItems that belong to the ContextualTabGroup.")]
		//[Category("Ribbon Properties")]
		[ReadOnly(true)]
		public ContextualTabItemCollection Tabs
		{
			get
			{
				if (this._tabs == null)
				{
					this._tabs = new ContextualTabItemCollection(this);
					this._tabs.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnTabsCollectionChanged);
				}

				return this._tabs;
			}
		}

				#endregion //Tabs	
		
			#endregion //Public Properties

			#region Internal Properties

                // AS 8/19/08 Automation support
                #region CaptionElement
        internal UIElement CaptionElement
        {
            get
            {
                return this._captionElement != null
                    ? Utilities.GetWeakReferenceTargetSafe(this._captionElement) as UIElement
                    : null;
            }

            set
            {
                if (value != this.CaptionElement)
                {
                    this._captionElement = value != null
                        ? new WeakReference(value)
                        : null;
                }
            }
        } 
                #endregion //CaptionElement

				#region FirstVisibleTabItem

		private static readonly DependencyPropertyKey FirstVisibleTabItemPropertyKey =
			DependencyProperty.RegisterReadOnly("FirstVisibleTabItem",
			typeof(RibbonTabItem), typeof(ContextualTabGroup), new FrameworkPropertyMetadata(null));

		internal static readonly DependencyProperty FirstVisibleTabItemProperty =
			FirstVisibleTabItemPropertyKey.DependencyProperty;

				#endregion //FirstVisibleTabItem

				#region Index
		internal int Index
		{
			get { return this._index; }
			set
			{
				if (value != this._index)
				{
					this._index = value;
					this.InitializeBaseBackColorResolved();
				}
			}
		} 
				#endregion //Index

				#region LastVisibleTabItem

		private static readonly DependencyPropertyKey LastVisibleTabItemPropertyKey =
			DependencyProperty.RegisterReadOnly("LastVisibleTabItem",
			typeof(RibbonTabItem), typeof(ContextualTabGroup), new FrameworkPropertyMetadata(null));

		internal static readonly DependencyProperty LastVisibleTabItemProperty =
			LastVisibleTabItemPropertyKey.DependencyProperty;

				#endregion //LastVisibleTabItem

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region OnTabsCollectionChanged

		void OnTabsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// AS 1/31/08
			// If you move or reorder tab items then the first/last tab item
			// in the contextual tab group will be incorrect. We were only 
			// setting that before when tabs were added/removed.
			//
			this.VerifyFirstLastTabItems();

			this.RaisePropertyChangedEvent("Tabs");
		}

				#endregion //OnTabsCollectionChanged	
    
			#endregion //Private Methods

			#region Internal Methods

                #region GetPeer
        internal ContextualTabGroupAutomationPeer GetPeer(bool createIfNull)
        {
            if (createIfNull && null == this._peer)
                this._peer = new ContextualTabGroupAutomationPeer(this);

            return this._peer;
        } 
                #endregion //GetPeer

				#region InitializeBaseBackColorResolved
		internal void InitializeBaseBackColorResolved()
		{
			Color? baseColor = this.BaseBackColor;

			if (baseColor == null)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(this);
				int index = this._index;

				if (index < 0)
					baseColor = Colors.Transparent;
				else
				{
					index = index % ContextualTabGroup._baseColors.Length;

					baseColor = ContextualTabGroup._baseColors[index];
				}
			}

			this.SetValue(BaseBackColorResolvedPropertyKey, baseColor);
		} 
				#endregion //InitializeBaseBackColorResolved

                // AS 8/19/08 Automation support
                #region SelectFirstTab
        internal void SelectFirstTab()
        {
            RibbonTabItem firstTab = (RibbonTabItem)this.GetValue(ContextualTabGroup.FirstVisibleTabItemProperty);

            if (null != firstTab)
                firstTab.IsSelected = true;
        } 
                #endregion //SelectFirstTab

				#region VerifyFirstLastTabItems





		internal void VerifyFirstLastTabItems()
		{
			RibbonTabItem firstTab = null;
			RibbonTabItem lastTab = null;

			for (int i = 0, end = this.Tabs.Count - 1; i <= end; i++)
			{
				RibbonTabItem tab = this.Tabs[i];

				tab.ClearValue(RibbonTabItem.IsFirstTabInContextualTabGroupPropertyKey);
				tab.ClearValue(RibbonTabItem.IsLastTabInContextualTabGroupPropertyKey);

				if (tab.Visibility == Visibility.Collapsed)
					continue;

				if (firstTab == null)
					firstTab = tab;

				lastTab = tab;
			}

			if (null != firstTab)
			{
				firstTab.SetValue(RibbonTabItem.IsFirstTabInContextualTabGroupPropertyKey, KnownBoxes.TrueBox);

				// no need to check for null here - if we have a first then we have a last
				lastTab.SetValue(RibbonTabItem.IsLastTabInContextualTabGroupPropertyKey, KnownBoxes.TrueBox);
			}

			this.SetValue(FirstVisibleTabItemPropertyKey, firstTab);
			this.SetValue(LastVisibleTabItemPropertyKey, lastTab);
		}
				#endregion //VerifyFirstLastTabItems

			#endregion //Internal Methods

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