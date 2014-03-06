using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Base class for a panel that displays the elements created for items that implement <see cref="ISupportRecycling"/>
	/// </summary>
	[DesignTimeVisible(false)]
	public abstract class ScheduleItemsPanel : Panel
		, IRecyclableElementHost
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ScheduleItemsPanel"/>
		/// </summary>
		protected ScheduleItemsPanel()
		{

		} 
		#endregion // Constructor

		#region Properties

		#region IsFirstItem

		/// <summary>
		/// Identifies the IsFirstItem attached dependency property
		/// </summary>
		public static readonly DependencyProperty IsFirstItemProperty = DependencyProperty.RegisterAttached("IsFirstItem",
			typeof(bool), typeof(ScheduleItemsPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsBoolPropertyChanged))
			);

		/// <summary>
		/// Returns a boolean indicating if the child is the first item in the source collection.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="IsFirstItemProperty"/>
		/// <seealso cref="SetIsFirstItem"/>

		[AttachedPropertyBrowsableForChildren()]

		public static bool GetIsFirstItem(DependencyObject d)
		{
			return (bool)d.GetValue(ScheduleItemsPanel.IsFirstItemProperty);
		}

		/// <summary>
		/// Sets the value of the attached IsFirstItem DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="IsFirstItemProperty"/>
		/// <seealso cref="GetIsFirstItem"/>
		internal static void SetIsFirstItem(DependencyObject d, bool value)
		{
			d.SetValue(ScheduleItemsPanel.IsFirstItemProperty, value);
		}

		#endregion //IsFirstItem

		#region IsLastItem

		/// <summary>
		/// Identifies the IsLastItem attached dependency property
		/// </summary>
		public static readonly DependencyProperty IsLastItemProperty = DependencyProperty.RegisterAttached("IsLastItem",
			typeof(bool), typeof(ScheduleItemsPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsBoolPropertyChanged))
			);

		/// <summary>
		/// Returns a boolean indicating if the child is the first item in the source collection.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="IsLastItemProperty"/>
		/// <seealso cref="SetIsLastItem"/>

		[AttachedPropertyBrowsableForChildren()]

		public static bool GetIsLastItem(DependencyObject d)
		{
			return (bool)d.GetValue(ScheduleItemsPanel.IsLastItemProperty);
		}

		/// <summary>
		/// Sets the value of the attached IsLastItem DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="IsLastItemProperty"/>
		/// <seealso cref="GetIsLastItem"/>
		internal static void SetIsLastItem(DependencyObject d, bool value)
		{
			d.SetValue(ScheduleItemsPanel.IsLastItemProperty, value);
		}

		#endregion //IsLastItem

		#region IsVertical
		internal bool IsVertical
		{
			get { return this.Orientation == Orientation.Vertical; }
		}
		#endregion //IsVertical

		#region Items

		private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.InvalidateMeasure();
		}

		private IList _items;
		private CoreUtilities.TypedList<ISupportRecycling> _itemsWrapper;

		/// <summary>
		/// Returns or sets the recyclable items in the specified collection
		/// </summary>
		internal IList Items
		{
			get
			{
				return _items;
			}
			set
			{
				if (value != _items)
				{
					if (_items is INotifyCollectionChanged)
						((INotifyCollectionChanged)_items).CollectionChanged -= new NotifyCollectionChangedEventHandler(OnItemsCollectionChanged);

					// AS 12/13/10 TFS61517
					var oldItems = _items;

					_items = value;

					if (_items is INotifyCollectionChanged)
						((INotifyCollectionChanged)_items).CollectionChanged += new NotifyCollectionChangedEventHandler(OnItemsCollectionChanged);

					_itemsWrapper = null != value ? new CoreUtilities.TypedList<ISupportRecycling>(value) : null;
					this.InvalidateMeasure();

					// AS 12/13/10 TFS61517
					this.OnItemsChanged(oldItems, value);
				}
			}
		}

		// AS 12/13/10 TFS61517
		internal virtual void OnItemsChanged( IList oldList, IList newList )
		{

		}

		internal IList<ISupportRecycling> RecyclableItems
		{
			get { return _itemsWrapper; }
		}
		#endregion //Items

		#region Orientation

		private Orientation _orientation = Orientation.Vertical;

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty OrientationProperty = DependencyPropertyUtilities.Register("Orientation",
			typeof(Orientation), typeof(ScheduleItemsPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.OrientationVerticalBox, new PropertyChangedCallback(OnOrientationChanged))
			);

		private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleItemsPanel item = (ScheduleItemsPanel)d;

			item._orientation = (Orientation)e.NewValue;
			item.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets the orientation in which the items will be arranged.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		internal Orientation Orientation
		{
			get
			{
				return _orientation;
			}
			set
			{
				this.SetValue(ScheduleItemsPanel.OrientationProperty, value);
			}
		}

		#endregion //Orientation

		#endregion // Properties

		#region Methods
		
		#region OnIsBoolPropertyChanged
		private static void OnIsBoolPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			IReceivePropertyChange<bool> rpc = d as IReceivePropertyChange<bool>;

			if (null != rpc)
				rpc.OnPropertyChanged(e.Property, (bool)e.OldValue, (bool)e.NewValue);
		}
		#endregion // OnIsBoolPropertyChanged

		#region OnElementAttached
		internal virtual void OnElementAttached( ISupportRecycling item, FrameworkElement element, bool isNewlyRealized )
		{
			if ( isNewlyRealized )
				ScheduleControlBase.SetControl(element, ScheduleControlBase.GetControl(this));
		}
		#endregion // OnElementAttached

		#region OnElementReleased
		internal virtual void OnElementReleased( ISupportRecycling item, FrameworkElement element, bool isRemoved )
		{
			if ( isRemoved )
			{
				// we set the control reference so we should clear it when the element is removed
				element.ClearValue(ScheduleControlBase.ControlProperty);
			}
		}
		#endregion // OnElementReleased

		#endregion // Methods

		#region IRecyclableElementHost Members

		void IRecyclableElementHost.OnElementAttached(ISupportRecycling item, FrameworkElement element, bool isNewlyRealized)
		{
			this.OnElementAttached(item, element, isNewlyRealized);
		}

		void IRecyclableElementHost.OnElementReleased(ISupportRecycling item, FrameworkElement element, bool isRemoved)
		{
			this.OnElementReleased(item, element, isRemoved);
		}

		bool IRecyclableElementHost.ShouldRemove(ISupportRecycling item, FrameworkElement element)
		{
			return false;
		}

		#endregion //IRecyclableElementHost Members
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