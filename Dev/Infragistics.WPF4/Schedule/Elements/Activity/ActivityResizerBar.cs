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
using System.Diagnostics;
using Infragistics.AutomationPeers;
using System.ComponentModel;
using Infragistics.Controls.Primitives;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// An element used to resize either the leading or trailing (start or end time) of an activity
	/// </summary>
	[DesignTimeVisible(false)]
	public class ActivityResizerBar : Control, IPropertyChangeListener
	{
		#region Member Variables

		ActivityPresenter _activityPresenter;
		private bool _cachedIsLeading;

		#endregion //Member Variables

		#region Constructor
		static ActivityResizerBar()
		{

			ActivityResizerBar.DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityResizerBar), new FrameworkPropertyMetadata(typeof(ActivityResizerBar)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(ActivityResizerBar), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923

		}

		/// <summary>
		/// Initializes a new <see cref="ActivityResizerBar"/>
		/// </summary>
		public ActivityResizerBar()
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

			this._activityPresenter = PresentationUtilities.GetVisualAncestor<ActivityPresenter>(this, null);

			Debug.Assert(this._activityPresenter != null || System.ComponentModel.DesignerProperties.GetIsInDesignMode(this));

			this.Initialize();

			if (this._activityPresenter != null)
				_activityPresenter.AddListener(this, false);
		}

		#endregion //OnApplyTemplate	

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="ActivityPresenter"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="ActivityPresenterAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new ActivityResizerBarAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#region OnMouseEnter

		/// <summary>
		/// Called when the mouse enters this element
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			this.VerifyState();
		}

		#endregion //OnMouseEnter	
	
		#region OnMouseLeftButtonDown

		/// <summary>
		/// Invoked when the left mouse button is pressed down on the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse operation</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			this.VerifyState();

			if (this._activityPresenter != null && this._activityPresenter.CanResizeEdge(this.IsLeading))
			{
				ScheduleControlBase control = this._activityPresenter.Control;

				if (control != null)
				{
					// AS 3/7/12 TFS102945
					// If this is happening during a touch operation then wait to see if this is a 
					// drag since the user may want to perform a scroll operation.
					//
					//control.EditHelper.BeginResize(this._activityPresenter.Activity, this, false, 0, 0);
					if (control.ShouldQueueTouchActions)
					{
						control.EnqueueTouchAction((a) =>
						{
							var c = _activityPresenter != null ? _activityPresenter.Control : null;
							if (c != null && a == ScrollInfoTouchAction.Drag)
							{
								control.EditHelper.BeginResize(_activityPresenter.Activity, this, false, 0, 0);
							}
						});
					}
					else
					{
						control.EditHelper.BeginResize(this._activityPresenter.Activity, this, false, 0, 0);
					}
				}

				e.Handled = true;
			}
		}

		#endregion //OnMouseLeftButtonDown	
	
		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region IsLeading

		/// <summary>
		/// Identifies the <see cref="IsLeading"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsLeadingProperty = DependencyPropertyUtilities.Register("IsLeading",
			typeof(bool), typeof(ActivityResizerBar),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsLeadingChanged))
			);

		private static void OnIsLeadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ActivityResizerBar)d)._cachedIsLeading = (bool)e.NewValue;
		}

		/// <summary>
		///  Returns or sets whether this is the Leading or trailing resizer bar
		/// </summary>
		/// <seealso cref="IsLeadingProperty"/>
		public bool IsLeading
		{
			get
			{
				return (bool)this.GetValue(ActivityResizerBar.IsLeadingProperty);
			}
			set
			{
				this.SetValue(ActivityResizerBar.IsLeadingProperty, value);
			}
		}

		#endregion //IsLeading

		#region ResizeGripVisibility

		private static readonly DependencyPropertyKey ResizeGripVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ResizeGripVisibility",
			typeof(Visibility), typeof(ActivityResizerBar), KnownBoxes.VisibilityCollapsedBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="ResizeGripVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResizeGripVisibilityProperty = ResizeGripVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the resize grip (read-only)
		/// </summary>
		/// <seealso cref="ResizeGripVisibilityProperty"/>
		public Visibility ResizeGripVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityResizerBar.ResizeGripVisibilityProperty);
			}
			internal set
			{
				this.SetValue(ActivityResizerBar.ResizeGripVisibilityPropertyKey, value);
			}
		}

		#endregion //ResizeGripVisibility

		#endregion //Public Properties

		#region Internal Properties

		#region ActivityPresenter

		internal ActivityPresenter ActivityPresenter { get { return this._activityPresenter; } }

		#endregion //ActivityPresenter

		#endregion //Internal Properties	
		
		#endregion //Properties	

		#region Methods

		#region Initialize
		private void Initialize()
		{
			if (_activityPresenter == null)
				return;

			ScheduleActivityPanel panel = VisualTreeHelper.GetParent(_activityPresenter) as ScheduleActivityPanel;

			// we won't have a panel for the template item but since the dayview all day event area, schedule view
			// and monthview all use horizontal arrangement of the activity we'll use that as the default as well
			Orientation orientation = panel != null ? panel.TimeslotOrientation : Orientation.Horizontal;

			if (orientation == Orientation.Vertical)
			{
				this.SetValue(VerticalAlignmentProperty, KnownBoxes.FromValue(_cachedIsLeading ? VerticalAlignment.Top : VerticalAlignment.Bottom));
				this.SetValue(HorizontalAlignmentProperty, KnownBoxes.HorizontalAlignmentStretchBox);
				this.Cursor = Cursors.SizeNS;
				this.Height = 4;
			}
			else
			{
				this.SetValue(HorizontalAlignmentProperty, KnownBoxes.FromValue(_cachedIsLeading ? HorizontalAlignment.Left : HorizontalAlignment.Right));
				this.SetValue(VerticalAlignmentProperty, KnownBoxes.VerticalAlignmentStretchBox);
				this.Cursor = Cursors.SizeWE;
				this.Width = 4;
			}

			this.VerifyState();
		} 
		#endregion // Initialize
    
		#region VerifyState

		private void VerifyState()
		{
			if (_activityPresenter == null)
				return;

			if (this._activityPresenter.CanResizeEdge(_cachedIsLeading) == false)
			{
				this.Opacity = 0;
				this.ClearValue(CursorProperty);
				this.ClearValue(ResizeGripVisibilityPropertyKey);
				this.ClearValue(MarginProperty); // AS 12/13/10 TFS61522
				return;
			}

			this.ClearValue(OpacityProperty);

			if (_activityPresenter.IsSelected)
				this.SetValue(ResizeGripVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
			else
				this.ClearValue(ResizeGripVisibilityPropertyKey);

			ScheduleActivityPanel panel = VisualTreeHelper.GetParent(this._activityPresenter) as ScheduleActivityPanel;

			// we won't have a panel for the template item but since the dayview all day event area, schedule view
			// and monthview all use horizontal arrangement of the activity we'll use that as the default as well
			Orientation orientation = panel != null ? panel.TimeslotOrientation : Orientation.Horizontal;

			if (orientation == Orientation.Vertical)
			{
				this.Cursor = Cursors.SizeNS;
			}
			else
			{
				this.Cursor = Cursors.SizeWE;
			}

			// AS 12/13/10 TFS61522
			Thickness? margin = null;

			if ( orientation == Orientation.Horizontal )
			{
				// AS 12/17/10 TFS62030
				var ctrl = _activityPresenter.Control;

				if ( null != ctrl )
				{
					bool indentLeading, indentTrailing;
					ctrl.ShouldIndentActivityEdge(_activityPresenter, out indentLeading, out indentTrailing);

					if ( (_cachedIsLeading && indentLeading) || (!_cachedIsLeading && indentTrailing) )
					{
						Thickness m = _activityPresenter.ComputedContentMargin;
						double offset = _cachedIsLeading ? m.Left : m.Right;

						if ( offset != 0 )
							margin = _cachedIsLeading ? new Thickness(offset, 0, 0, 0) : new Thickness(0, 0, offset, 0);
					}
				}
			}

			if ( margin == null )
				this.ClearValue(MarginProperty);
			else
				this.Margin = margin.Value;
		}

		#endregion //VerifyState

		#endregion //Methods

		#region ITypedPropertyChangeListener<object,string> Members

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
		{
			switch (property)
			{
				case "Activity":
				case "IsSelected":
				case "HasPendingOperation":
				case ActivityPresenter.CanResizeCriteria: // AS 10/1/10 TFS50023
					if ( _activityPresenter.Activity != null )
						this.VerifyState();
					break;
			}
		}

		#endregion
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