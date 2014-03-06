using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Base class for an element representing a portion of a <see cref="CalendarGroupBase"/>
	/// </summary>
	public class CalendarGroupPresenterBase : ResourceCalendarElementBase
        , IReceivePropertyChange<bool>
	{
		#region Member Variables

		private bool _hasCalendarAreaInControl;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CalendarGroupPresenterBase"/>
		/// </summary>
		protected CalendarGroupPresenterBase()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region CalendarGroup

		private static readonly DependencyPropertyKey CalendarGroupPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CalendarGroup",
			typeof(CalendarGroupBase), typeof(CalendarGroupPresenterBase),
			null,
			new PropertyChangedCallback(OnCalendarGroupChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="CalendarGroup"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalendarGroupProperty = CalendarGroupPropertyKey.DependencyProperty;

		private static void OnCalendarGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarGroupPresenterBase instance = (CalendarGroupPresenterBase)d;
			instance.OnCalendarGroupChanged(e.OldValue as CalendarGroupBase, e.NewValue as CalendarGroupBase);
		}

		internal virtual void OnCalendarGroupChanged(CalendarGroupBase oldValue, CalendarGroupBase newValue)
		{

		}

		/// <summary>
		/// Returns the <see cref="CalendarGroupBase"/> that the element represents
		/// </summary>
		/// <seealso cref="CalendarGroupProperty"/>
		public CalendarGroupBase CalendarGroup
		{
			get
			{
				return (CalendarGroupBase)this.GetValue(CalendarGroupPresenterBase.CalendarGroupProperty);
			}
			internal set
			{
				this.SetValue(CalendarGroupPresenterBase.CalendarGroupPropertyKey, value);
			}
		}

		#endregion //CalendarGroup

		#region ComputedBorderThickness

		private static readonly DependencyPropertyKey ComputedBorderThicknessPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBorderThickness",
			typeof(Thickness), typeof(CalendarGroupPresenterBase), new Thickness(), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBorderThickness"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBorderThicknessProperty = ComputedBorderThicknessPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the border thickness to use for the BorderBrush based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedBorderThicknessProperty"/>
		public Thickness ComputedBorderThickness
		{
			get
			{
				return (Thickness)this.GetValue(CalendarGroupPresenterBase.ComputedBorderThicknessProperty);
			}
			internal set
			{
				this.SetValue(CalendarGroupPresenterBase.ComputedBorderThicknessPropertyKey, value);
			}
		}

		#endregion //ComputedBorderThickness

		#region ComputedMargin

		private static readonly DependencyPropertyKey ComputedMarginPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedMargin",
			typeof(Thickness), typeof(CalendarGroupPresenterBase), new Thickness(), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedMargin"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedMarginProperty = ComputedMarginPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the margin to use for the BorderBrush based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedMarginProperty"/>
		public Thickness ComputedMargin
		{
			get
			{
				return (Thickness)this.GetValue(CalendarGroupPresenterBase.ComputedMarginProperty);
			}
			internal set
			{
				this.SetValue(CalendarGroupPresenterBase.ComputedMarginPropertyKey, value);
			}
		}

		#endregion //ComputedMargin

		#endregion // Public Properties

		#region Internal Properties

		#region HasCalendarAreaInControl
		internal bool HasCalendarAreaInControl
		{
			get { return _hasCalendarAreaInControl; }
			set
			{
				if (value != _hasCalendarAreaInControl)
				{
					_hasCalendarAreaInControl = value;
					this.SetProviderBrushes();
				}
			}
		}
		#endregion // HasCalendarAreaInControl

		#endregion // Internal Properties
		
		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region GetBorderThickness

		internal virtual Thickness GetBorderThickness(double borderSize)
		{
			return new Thickness();
		}

		#endregion //GetBorderThickness	

		#region SetProviderBrushes

		internal override void SetProviderBrushes()
		{
			if (!this.IsBrushVersionBindingInitialized)
				return;

			var ctrl = ScheduleControlBase.GetControl(this);

			if (ctrl == null || ctrl.CalendarGroupOrientation == Orientation.Vertical)
				return;

			CalendarBrushProvider brushProvider = ScheduleUtilities.GetCalendarGroupBrushProvider(this, ctrl);

			if (brushProvider == null)
				return;

			bool isFirstItem = ScheduleItemsPanel.GetIsFirstItem(this);
			bool isLastItem = ScheduleItemsPanel.GetIsLastItem(this);

			#region Set border brush

			Brush br = null;

			if (brushProvider != null)
			{
				CalendarBrushId brushId;

				brushId = CalendarBrushId.CalendarBorder;

				br = brushProvider.GetBrush(brushId);
			}

			if (br != null)
			{
				this.ComputedBorderBrush = br;
			}

			#endregion //Set border brush

			if (!_hasCalendarAreaInControl && isFirstItem && isLastItem)
			{
				this.ClearValue(ComputedBorderThicknessPropertyKey);
				this.ClearValue(ComputedMarginPropertyKey);
			}
			else
			{
				Thickness margin;

				if (isFirstItem)
				{
					if (isLastItem)
						margin = new Thickness();
					else
						margin = new Thickness(0, 0, 2, 0);
				}
				else
				{
					if (isLastItem)
						margin = new Thickness(2, 0, 0, 0);
					else
						margin = new Thickness(2, 0, 2, 0);
				}

				this.ComputedMargin = margin;

				this.ComputedBorderThickness = this.GetBorderThickness(3);
			}
		}

		#endregion //SetProviderBrushes

		#endregion //Internal Methods

		#endregion //Methods

		#region IReceivePropertyChange<bool> Members

		void IReceivePropertyChange<bool>.OnPropertyChanged(DependencyProperty property, bool oldValue, bool newValue)
		{
			this.SetProviderBrushes();
		}

		#endregion //IReceivePropertyChange<bool> Members
	}

	/// <summary>
	/// Base class for an element representing a portion of a <see cref="CalendarGroupBase"/> that displays one or more items in a <see cref="ScheduleItemsPanel"/>.
	/// </summary>
	[TemplatePart(Name = PartItemsPanel, Type = typeof(ScheduleItemsPanel))]
	public class CalendarGroupItemsPresenterBase : CalendarGroupPresenterBase
	{
		#region Member Variables

		private const string PartItemsPanel = "ItemsPanel";

		private ScheduleItemsPanel _itemsPanel;
		private IList _items;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CalendarGroupPresenterBase"/>
		/// </summary>
		protected CalendarGroupItemsPresenterBase()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			ScheduleItemsPanel oldPanel = _itemsPanel;
			_itemsPanel = this.GetTemplateChild(PartItemsPanel) as ScheduleItemsPanel;

			if (oldPanel != _itemsPanel)
				this.OnItemsPanelChanged(oldPanel, _itemsPanel);

			base.OnApplyTemplate();
		}
		#endregion //OnApplyTemplate

		#endregion //Base class overrides

		#region Properties

		#region Internal Properties

		#region Items

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
					_items = value;

					if (_itemsPanel != null)
						_itemsPanel.Items = value;
				}
			}
		}
		#endregion //Items

		#region ItemsPanel
		internal ScheduleItemsPanel ItemsPanel
		{
			get { return _itemsPanel; }
		}
		#endregion // ItemsPanel

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region OnItemsPanelChanged
		internal virtual void OnItemsPanelChanged(ScheduleItemsPanel oldPanel, ScheduleItemsPanel newPanel)
		{
			if (oldPanel != null)
			{
				oldPanel.ClearValue(ScheduleControlBase.ControlProperty);
				oldPanel.Items = null;
			}

			if (null != newPanel)
			{
				Debug.Assert(null != ScheduleControlBase.GetControl(this) || System.ComponentModel.DesignerProperties.GetIsInDesignMode(this));
				newPanel.SetValue(ScheduleControlBase.ControlProperty, ScheduleControlBase.GetControl(this));
				newPanel.Items = this.Items;
			}
		}
		#endregion // OnItemsPanelChanged

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