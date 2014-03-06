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
using Infragistics.AutomationPeers;
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	
	
	/// <summary>
	/// Element used to indicate that there are more <see cref="ActivityBase"/> instances that are not currently in view.
	/// </summary>
	[TemplateVisualState(Name = VisualStateUtilities.StateUp, GroupName = VisualStateUtilities.GroupDirection)]
	[TemplateVisualState(Name = VisualStateUtilities.StateDown, GroupName = VisualStateUtilities.GroupDirection)]
	[TemplateVisualState(Name = VisualStateUtilities.StateMouseOver, GroupName = VisualStateUtilities.GroupCommon)]
	[TemplateVisualState(Name = VisualStateUtilities.StateNormal, GroupName = VisualStateUtilities.GroupCommon)]
	[DesignTimeVisible(false)]
	public class MoreActivityIndicator : Control
	{
		#region Member Variables

		private bool _isMouseOver;
		private Action<MoreActivityIndicator> _action;
		private object _context;

		#endregion // Member Variables

		#region Constructor
		static MoreActivityIndicator()
		{

			MoreActivityIndicator.DefaultStyleKeyProperty.OverrideMetadata(typeof(MoreActivityIndicator), new FrameworkPropertyMetadata(typeof(MoreActivityIndicator)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(MoreActivityIndicator), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923

		}

		/// <summary>
		/// Initializes a new <see cref="MoreActivityIndicator"/>
		/// </summary>
		public MoreActivityIndicator()
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
			base.OnApplyTemplate();

			ScheduleControlBase control = this.Control;

			if (control != null)
				control.BindToBrushVersion(this);

			this.ChangeVisualState(false);
		}

		#endregion //OnApplyTemplate

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="ActivityPresenter"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="MoreActivityIndicatorAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new MoreActivityIndicatorAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#region OnMouseLeftButtonDown

		/// <summary>
		/// Invoked when the left mouse button is pressed down on the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse event.</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (!e.Handled)
				e.Handled = this.PerformAction();

			base.OnMouseLeftButtonDown(e);
		}

		#endregion //OnMouseLeftButtonDown

		#region OnMouseEnter
		/// <summary>
		/// Invoked when the mouse enters the bounds of the element
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			_isMouseOver = true;
			this.UpdateVisualState();
		} 
		#endregion // OnMouseEnter

		#region OnMouseLeave
		/// <summary>
		/// Invoked when the mouse leaves the bounds of the element
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			_isMouseOver = false;
			this.UpdateVisualState();
		} 
		#endregion // OnMouseLeave

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region ComputedFill

		private static readonly DependencyPropertyKey ComputedFillPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedFill",
			typeof(Brush), typeof(MoreActivityIndicator), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedFill"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedFillProperty = ComputedFillPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the background based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedFillProperty"/>
		public Brush ComputedFill
		{
			get
			{
				return (Brush)this.GetValue(MoreActivityIndicator.ComputedFillProperty);
			}
			internal set
			{
				this.SetValue(MoreActivityIndicator.ComputedFillPropertyKey, value);
			}
		}

		#endregion //ComputedFill

		#region ComputedStroke

		private static readonly DependencyPropertyKey ComputedStrokePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedStroke",
			typeof(Brush), typeof(MoreActivityIndicator), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedStroke"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedStrokeProperty = ComputedStrokePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the background based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedStrokeProperty"/>
		public Brush ComputedStroke
		{
			get
			{
				return (Brush)this.GetValue(MoreActivityIndicator.ComputedStrokeProperty);
			}
			internal set
			{
				this.SetValue(MoreActivityIndicator.ComputedStrokePropertyKey, value);
			}
		}

		#endregion //ComputedStroke

		#endregion // Public Properties

		#region Internal Properties

		#region Action
		internal Action<MoreActivityIndicator> Action
		{
			get { return _action; }
			set { _action = value; }
		} 
		#endregion // Action

		#region Control

		internal ScheduleControlBase Control
		{
			get
			{
				UIElement parent = VisualTreeHelper.GetParent(this) as UIElement;

				return parent != null ? ScheduleUtilities.GetControl(parent) : null;
			}
		}

		#endregion //Control

		#region Context
		internal object Context
		{
			get { return _context; }
			set { _context = value; }
		} 
		#endregion // Context

		#region Direction
		private MoreActivityIndicatorDirection _direction = MoreActivityIndicatorDirection.Down;

		internal MoreActivityIndicatorDirection Direction
		{
			get { return _direction; }
			set
			{
				if (value != _direction)
				{
					_direction = value;
					this.UpdateVisualState();
				}
			}
		} 
		#endregion // Direction

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region PerformAction
		internal bool PerformAction()
		{
			if (_action == null)
				return false;

			_action(this);
			return true;
		}
		#endregion // PerformAction

		#endregion // Internal Methods

		#region Private Methods

		#region ChangeVisualState
		private void ChangeVisualState(bool useTransitions)
		{
			if (_isMouseOver)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
			else
				VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);

			if (_direction == MoreActivityIndicatorDirection.Up)
				VisualStateManager.GoToState(this, VisualStateUtilities.StateUp, useTransitions);
			else if (_direction == MoreActivityIndicatorDirection.Down)
				VisualStateManager.GoToState(this, VisualStateUtilities.StateDown, useTransitions);

			this.SetProviderProperties();
		}
		#endregion //ChangeVisualState

		#region SetProviderProperties

		private void SetProviderProperties()
		{
			CalendarBrushProvider brushProvider = ScheduleUtilities.GetCalendarGroupBrushProvider(this);

			if (brushProvider == null)
			{
				if (this.Tag == ScheduleControlBase.MeasureOnlyItemId)
				{
					var ctrl = ScheduleUtilities.GetControl(this);

					if (null != ctrl)
						brushProvider = ctrl.DefaultBrushProvider;
				}

				if (brushProvider == null)
					return;
			}

			this.SetValue(CalendarBrushProvider.BrushProviderProperty, brushProvider);

			Brush br;

			if (_isMouseOver)
				br = brushProvider.GetBrush(CalendarBrushId.HotTrackingMoreActivityIndicatorFill);
			else
				br = brushProvider.GetBrush(CalendarBrushId.MoreActivityIndicatorFill);

			this.ComputedFill = br;
			this.ComputedStroke = brushProvider.GetBrush(CalendarBrushId.MoreActivityIndicatorStroke);
		}

		#endregion //SetProviderProperties

		#region UpdateVisualState
		private void UpdateVisualState()
		{
			this.ChangeVisualState(true);
		}
		#endregion // UpdateVisualState

		#endregion // Private Methods

		#endregion // Methods
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