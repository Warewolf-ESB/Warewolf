using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;
using System.Windows.Markup;
using System.Collections;
using System.Diagnostics;
using System.Collections.Specialized;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Automation.Peers.Ribbon;
using System.Windows.Automation.Peers;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Defines a grouping that organizes the display of <see cref="GalleryItem"/>'s in the <see cref="GalleryTool"/> dropdown.  
	/// </summary>
	/// <remarks>
	/// <p class="body">Adding a GalleryItemGroup to the <see cref="Infragistics.Windows.Ribbon.GalleryTool.Groups"/> collection will prevent the display of <see cref="GalleryItem"/>s
	/// in the <see cref="GalleryTool"/> dropdown, unless you add the key of at least 1 <see cref="GalleryItem"/> to the 
	/// <see cref="GalleryItemGroup.ItemKeys"/> collection of the GalleryItemGroup.</p>
	/// <p class="note"><b>Note: </b>Once a <see cref="GalleryItemGroup"/> has been added to the <see cref="Infragistics.Windows.Ribbon.GalleryTool.Groups"/>
	/// collection, the <see cref="GalleryTool"/> will not display <see cref="GalleryItem"/>s in the <see cref="GalleryTool"/> dropdown unless they are 
	/// assigned to <see cref="GalleryItemGroup"/>s by adding the <see cref="GalleryItem.Key"/> to the <see cref="GalleryItemGroup.ItemKeys"/> collection 
	/// of the Group in which the <see cref="GalleryItem "/> should be displayed.</p>
	/// <p class="note"><b>Note: </b>Adding <see cref="GalleryItemGroup"/>s to the <see cref="Infragistics.Windows.Ribbon.GalleryTool.Groups"/> collection 
	/// has no effect on the display of <see cref="GalleryItem"/>s in the <see cref="GalleryTool"/> preview area, since <see cref="GalleryItemGroup"/>s 
	/// are never displayed in the preview area.</p>
	/// </remarks>
	[ContentProperty("ItemKeys")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GalleryItemGroup : ItemsControl
	{
		#region Member Variables

		private ObservableCollection<string>						_itemKeys;
		private GalleryTool											_galleryTool;

		private ObservableCollection<GalleryItem>					_itemsInGroup = new ObservableCollection<GalleryItem>();
		private GroupItemsCollection								_groupItemsCollection;

		private int													_currentItemKeysVersion;
		private int													_currentGalleryToolItemsVersion;
		private int													_lastRefreshItemKeysVersion;
		private int													_lastRefreshGalleryToolItemsVersion;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes an instance of a <see cref="GalleryItemGroup"/> class.
		/// </summary>
		public GalleryItemGroup()
		{
			this._groupItemsCollection	= new GroupItemsCollection(this._itemsInGroup, this);
			this.ItemsSource			= this._groupItemsCollection;
		}

		static GalleryItemGroup()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GalleryItemGroup), new FrameworkPropertyMetadata(typeof(GalleryItemGroup)));

            // JJD 11/20/07 - BR27066
            // Default the FocusableProperty to false since the group can't take focus
            FrameworkElement.FocusableProperty.OverrideMetadata(typeof(GalleryItemGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region ClearContainerForItemOverride

		/// <summary>
		/// Undoes any preparation done by the container to 'host' the item.
		/// </summary>
		protected override void ClearContainerForItemOverride(DependencyObject element, object item)
		{
			base.ClearContainerForItemOverride(element, item);

			if (element is GalleryItemPresenter)
				((GalleryItemPresenter)element).ClearContainerForItem(item as GalleryItem);
		}

			#endregion //ClearContainerForItemOverride	
    
			#region GetContainerForItemOverride

		/// <summary>
		/// Creates the container to wrap an item in the list.
		/// </summary>
		/// <returns>The newly created container.</returns>
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new GalleryItemPresenter(false, this.GalleryToolInternal, this);
		}

			#endregion //GetContainerForItemOverride	
    
			#region IsItemItsOwnContainerOverride

		/// <summary>
		/// Determines if the item requires a separate container.
		/// </summary>
		/// <param name="item">The item in question.</param>
		/// <returns>True if the item does not require a wrapper</returns>
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is GalleryItemPresenter;
		}

			#endregion //IsItemItsOwnContainerOverride	

			#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			// AS 8/25/11 TFS83577
			this.VerifyCollectionVersions();

			return base.MeasureOverride(constraint);
		} 
			#endregion //MeasureOverride

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="GalleryItemGroup"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.GalleryItemGroupAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GalleryItemGroupAutomationPeer(this);
        }
            #endregion
        
            #region OnInitialized

		/// <summary>
		/// Called when the element is initialized.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
		}

			#endregion //OnInitialized	
    
			#region PrepareContainerForItemOverride

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <param name="element">The container that wraps the item.</param>
		/// <param name="item">The data item that is wrapped.</param>
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);

			if (element is GalleryItemPresenter)
				((GalleryItemPresenter)element).PrepareContainerForItem(item as GalleryItem);
		}

			#endregion //PrepareContainerForItemOverride	
    
		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region ItemKeys

		/// <summary>
		/// Returns a collection of <see cref="GalleryItem"/> keys that represents the <see cref="GalleryItem"/>s that have been assigned to this
		/// <see cref="GalleryItemGroup"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">Adding the key of a <see cref="GalleryItem"/> to this collection causes the <see cref="GalleryItem"/> to be displayed within 
		/// the <see cref="GalleryItemGroup "/> in the <see cref="GalleryTool"/> dropdown.  It does not affect the display of the item in the 
		/// <see cref="GalleryTool"/> preview area since <see cref="GalleryItemGroup"/>s are never displayed in the <see cref="GalleryTool"/> preview area.</p>
		/// <p class="note"><b>Note: </b>The key of a <see cref="GalleryItem"/> may be added to the ItemKeys collection of more than one 
		/// <see cref="GalleryItemGroup"/>. In this case the <see cref="GalleryItem"/> will be displayed in multiple <see cref="GalleryItemGroup"/>s 
		/// in the <see cref="GalleryTool"/> dropdown area.</p>
		/// </remarks>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItem.Key"/>
		//[Description("Returns a collection of GalleryItem keys that represents the GalleryItems that have been assigned to this GalleryItemGroup.")]
		//[Category("Ribbon Properties")]
		public ObservableCollection<string> ItemKeys
		{
			get
			{
				if (this._itemKeys == null)
				{
					this._itemKeys						= new ObservableCollection<string>();
					this._itemKeys.CollectionChanged	+= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnItemKeysCollectionChanged);
				}

				return this._itemKeys;
			}
		}

				#endregion //ItemKeys	

				#region ItemSettings

		/// <summary>
		/// Identifies the <see cref="ItemSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemSettingsProperty = DependencyProperty.Register("ItemSettings",
			typeof(GalleryItemSettings), typeof(GalleryItemGroup), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemSettingsChanged)));

		private static void OnItemSettingsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			GalleryItemGroup galleryItemGroup = target as GalleryItemGroup;
			if (galleryItemGroup != null)
			{
				if (e.OldValue != null)
					((GalleryItemSettings)e.OldValue).PropertyChanged -= new PropertyChangedEventHandler(galleryItemGroup.OnItemSettingsPropertyChanged);

				if (e.NewValue != null)
					((GalleryItemSettings)e.NewValue).PropertyChanged += new PropertyChangedEventHandler(galleryItemGroup.OnItemSettingsPropertyChanged);
			}
		}

		/// <summary>
		/// Returns/sets the settings that serve as the default settings for all <see cref="GalleryItem"/>s in this <see cref="GalleryItemGroup"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The various property values in the <see cref="GalleryItemSettings"/> specified at the <see cref="GalleryTool"/> level 
		/// (via the <see cref="Infragistics.Windows.Ribbon.GalleryTool.ItemSettings"/> property) serve as the ultimate defaults for all <see cref="GalleryItem"/>s.  These values 
		/// can be overridden at two lower levels:
		/// <ul>
		/// <li>at the <see cref="GalleryItemGroup"/> level via the <see cref="GalleryItemGroup.ItemSettings"/> property.  The values specified there
		/// will override corresponding values set at the <see cref="GalleryTool"/> level, but could be further overridden at the <see cref="GalleryItem"/>
		/// level (see next bullet)</li>
		/// <li>at the <see cref="GalleryItem"/> level via the <see cref="GalleryItem.Settings"/> property.  The values specified here will override corresponding 
		/// values set at the <see cref="GalleryTool"/> and <see cref="GalleryItemGroup"/> levels</li>
		/// </ul>
		/// </p>
		/// </remarks>
		/// <seealso cref="ItemSettingsProperty"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItem.Settings"/>
		/// <seealso cref="GalleryItemSettings"/>
		//[Description("Returns/sets the settings that serve as the default settings for all GalleryItems in this GalleryItemGroup.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public GalleryItemSettings ItemSettings
		{
			get
			{
				return (GalleryItemSettings)this.GetValue(GalleryItemGroup.ItemSettingsProperty);
			}
			set
			{
				this.SetValue(GalleryItemGroup.ItemSettingsProperty, value);
			}
		}

				#endregion //ItemSettings

				#region GalleryTool

		private static readonly DependencyPropertyKey GalleryToolPropertyKey =
			DependencyProperty.RegisterReadOnly("GalleryTool",
			typeof(GalleryTool), typeof(GalleryItemGroup), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="GalleryTool"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GalleryToolProperty =
			GalleryToolPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the owning GalleryTool
		/// </summary>
		/// <seealso cref="GalleryToolProperty"/>
		//[Description("Returns the owning GalleryTool")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public GalleryTool GalleryTool
		{
		    get { return (GalleryTool)this.GetValue(GalleryItemGroup.GalleryToolProperty); }
		}

				#endregion //GalleryTool

				#region Title

		/// <summary>
		/// Identifies the <see cref="Title"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title",
			typeof(string), typeof(GalleryItemGroup), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the Title that is displayed for the GalleryItemGroup.  
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b><see cref="GalleryItemGroup"/>s only appear in the <see cref="GalleryTool"/> dropdown - they do not appear in the
		/// <see cref="GalleryTool"/> preview area in the ribbon.</p>
		/// </remarks>
		/// <seealso cref="TitleProperty"/>
		/// <seealso cref="GalleryItemGroup"/>
		/// <seealso cref="GalleryTool"/>
		//[Description("Returns/sets the Title that is displayed for the GalleryItemGroup.  ")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string Title
		{
			get
			{
				return (string)this.GetValue(GalleryItemGroup.TitleProperty);
			}
			set
			{
				this.SetValue(GalleryItemGroup.TitleProperty, value);
			}
		}

				#endregion //Title

			#endregion //Public Properties

			#region Internal Properties

				#region GalleryTool

		internal GalleryTool GalleryToolInternal
		{
			get { return this._galleryTool; }
			set 
			{
				if (value != this._galleryTool)
				{
					if (value == null)
						this._galleryTool.Items.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnGalleryToolItemsCollectionChanged);

					this._galleryTool = value;
					this.SetValue(GalleryItemGroup.GalleryToolPropertyKey, value);

					if (this._galleryTool != null)
						this._galleryTool.Items.CollectionChanged += new NotifyCollectionChangedEventHandler(OnGalleryToolItemsCollectionChanged);
				}
			}
		}


				#endregion //GalleryTool	

				#region ItemSettingsVersion

		private static readonly DependencyPropertyKey ItemSettingsVersionPropertyKey =
			DependencyProperty.RegisterReadOnly("ItemSettingsVersion",
			typeof(int), typeof(GalleryItemGroup), new FrameworkPropertyMetadata(0));

		internal static readonly DependencyProperty ItemSettingsVersionProperty =
			ItemSettingsVersionPropertyKey.DependencyProperty;

				#endregion //ItemSettingsVersion
    
			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region OnGalleryToolItemsCollectionChanged

		private void OnGalleryToolItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this._currentGalleryToolItemsVersion++;

			// AS 8/25/11 TFS83577
			this.InvalidateMeasure();
		}

				#endregion //OnGalleryToolItemsCollectionChanged

				#region OnItemKeysCollectionChanged

		private void OnItemKeysCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this._currentItemKeysVersion++;
		}

				#endregion //OnItemKeysCollectionChanged	

				#region OnItemSettingsPropertyChanged

		private void OnItemSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			int currentVersion	= (int)this.GetValue(GalleryItemGroup.ItemSettingsVersionProperty);
			int newVersion		= ++currentVersion;
			this.SetValue(GalleryItemGroup.ItemSettingsVersionPropertyKey, newVersion);
		}

				#endregion //OnItemSettingsPropertyChanged	

				#region RefreshItemsList

		private void RefreshItemsList()
		{
			GalleryTool galleryTool = this.GalleryTool;
			if (galleryTool == null)
			{
				this.ItemsSource = null;
				return;
			}

			ObservableCollection<string>	groupItemKeys		= this.ItemKeys;
			GalleryItemCollection			galleryToolItems	= galleryTool.Items;

			this._itemsInGroup.Clear();
			for (int i = 0; i < groupItemKeys.Count; i++)
			{
				GalleryItem galleryItem = galleryToolItems[groupItemKeys[i]];
				if (galleryItem != null)
					this._itemsInGroup.Add(galleryItem);
			}

			this.ItemsSource = this._groupItemsCollection;
		}

				#endregion //RefreshItemsList

				#region VerifyCollectionVersions

		private void VerifyCollectionVersions()
		{
			if (this._currentItemKeysVersion			!= this._lastRefreshItemKeysVersion ||
				this._currentGalleryToolItemsVersion	!= this._lastRefreshGalleryToolItemsVersion)
			{
				this._lastRefreshItemKeysVersion			= this._currentItemKeysVersion;
				this._lastRefreshGalleryToolItemsVersion	= this._currentGalleryToolItemsVersion;

				this.RefreshItemsList();
			}
		}

				#endregion //VerifyCollectionVersions	
    
			#endregion //Private Methods

		#endregion //Methods

		#region Internal Nested Class GroupItemsCollection



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal class GroupItemsCollection : ReadOnlyObservableCollection<GalleryItem>,
											  IList<GalleryItem>,
											  IList
		{
			#region Member Variables

			private GalleryItemGroup							_galleryItemGroup;

			#endregion //Member Variables

			#region Constructor

			internal GroupItemsCollection(ObservableCollection<GalleryItem> list, GalleryItemGroup galleryItemGroup)
				: base(list)
			{
				Debug.Assert(galleryItemGroup != null, "galleryItemGroup is null!");

				this._galleryItemGroup = galleryItemGroup;
			}

			#endregion //Constructor

			#region IList<GalleryItem> Members

			GalleryItem IList<GalleryItem>.this[int index]
			{
				get
				{
					this._galleryItemGroup.VerifyCollectionVersions();

					return base[index];
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			#endregion

			#region ICollection<GalleryItem> Members

			int ICollection<GalleryItem>.Count
			{
				get
				{
					this._galleryItemGroup.VerifyCollectionVersions();

					return base.Count;
				}
			}

			#endregion

			#region ICollection Members

			int ICollection.Count
			{
				get
				{
					this._galleryItemGroup.VerifyCollectionVersions();

					return base.Count;
				}
			}

			#endregion

			#region IList Members

			object IList.this[int index]
			{
				get
				{
					this._galleryItemGroup.VerifyCollectionVersions();

					return base[index];
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				this._galleryItemGroup.VerifyCollectionVersions();

				return base.GetEnumerator();
			}

			#endregion

			#region IEnumerable<GalleryItem> Members

			IEnumerator<GalleryItem> IEnumerable<GalleryItem>.GetEnumerator()
			{
				this._galleryItemGroup.VerifyCollectionVersions();

				return base.GetEnumerator();
			}

			#endregion
		}

		#endregion //Internal Nested Class GroupItemsCollection
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