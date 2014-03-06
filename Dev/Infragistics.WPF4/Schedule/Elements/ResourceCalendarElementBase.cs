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
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Base class for an element whose DataContext is a <see cref="ResourceCalendar"/>
	/// </summary>
	[DesignTimeVisible(false)]
	public abstract class ResourceCalendarElementBase : Control
		, ICalendarBrushClient
		, IReceivePropertyChange<object>
	{
		#region Member Variables

		private bool _isBrushVersionBindingInitialized;

		#endregion //Member Variables

		#region Constructor
		static ResourceCalendarElementBase()
		{

			DataContextHelper.RegisterType(typeof(ResourceCalendarElementBase));
			UIElement.FocusableProperty.OverrideMetadata(typeof(ResourceCalendarElementBase), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923

		}

		/// <summary>
		/// Initializes a new <see cref="ResourceCalendarElementBase"/>
		/// </summary>
		protected ResourceCalendarElementBase()
		{



		} 
		#endregion // Constructor

		#region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			this.VerifyBrushVersionBinding();

			this._isBrushVersionBindingInitialized = true;

			base.OnApplyTemplate();

			this.ChangeVisualState(false);
		}
		#endregion //OnApplyTemplate

		#region OnMouseLeftButtonDown

		/// <summary>
		/// Invoked when the left mouse button is pressed down on the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse event.</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			ResourceCalendar calendar = this.DataContext as ResourceCalendar;

			if (calendar != null)
			{
				ScheduleControlBase control = ScheduleUtilities.GetControl(this);

				if (control != null)
				{
					control.SelectCalendar(calendar, true, this);
				}
			}

			base.OnMouseLeftButtonDown(e);
		}

		#endregion //OnMouseLeftButtonDown

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region ComputedBackground

		internal static readonly DependencyPropertyKey ComputedBackgroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBackground",
			typeof(Brush), typeof(ResourceCalendarElementBase), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBackground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBackgroundProperty = ComputedBackgroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the background based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedBackgroundProperty"/>
		public Brush ComputedBackground
		{
			get
			{
				return (Brush)this.GetValue(ResourceCalendarElementBase.ComputedBackgroundProperty);
			}
			internal set
			{
				this.SetValue(ResourceCalendarElementBase.ComputedBackgroundPropertyKey, value);
			}
		}

		#endregion //ComputedBackground

		#region ComputedBorderBrush

		private static readonly DependencyPropertyKey ComputedBorderBrushPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBorderBrush",
			typeof(Brush), typeof(ResourceCalendarElementBase), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBorderBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBorderBrushProperty = ComputedBorderBrushPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the BorderBrush based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedBorderBrushProperty"/>
		public Brush ComputedBorderBrush
		{
			get
			{
				return (Brush)this.GetValue(ResourceCalendarElementBase.ComputedBorderBrushProperty);
			}
			internal set
			{
				this.SetValue(ResourceCalendarElementBase.ComputedBorderBrushPropertyKey, value);
			}
		}

		#endregion //ComputedBorderBrush

		#endregion // Public Properties

		#region Internal Properties

		#region Calendar
		// note I'm not exposing this because then someone might bind to it in which case we have to make it a dp which would just add overhead when its already the datacontext
		internal ResourceCalendar ResourceCalendar
		{
			get
			{
				return this.DataContext as ResourceCalendar;
			}
		}
		#endregion // Calendar

		#region IsBrushVersionBindingInitialized
		internal bool IsBrushVersionBindingInitialized
		{
			get { return _isBrushVersionBindingInitialized; }
		}
		#endregion // IsBrushVersionBindingInitialized

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region ChangeVisualState
		internal virtual void ChangeVisualState(bool useTransitions)
		{
			this.SetProviderBrushes();
		}
		#endregion //ChangeVisualState

		#region GoToState

		internal void GoToState(string stateName, bool useTransitions)
		{
			VisualStateManager.GoToState(this, stateName, useTransitions);
		}

		#endregion // GoToState

		#region SetProviderBrushes

		internal abstract void SetProviderBrushes();

		#endregion //SetProviderBrushes

		#region UpdateVisualState
		internal void UpdateVisualState()
		{
			
			this.ChangeVisualState(true);
		}
		#endregion // UpdateVisualState

		#region VerifyBrushVersionBinding
		internal virtual void VerifyBrushVersionBinding()
		{
			if (!this._isBrushVersionBindingInitialized)
			{
				ScheduleControlBase.BindToResourceCalendarBrushVersion(this);
			}
		}
		#endregion // VerifyBrushVersionBinding

		#endregion // Internal Methods

		#endregion // Methods

		#region ICalendarBrushClient Members

		void ICalendarBrushClient.OnBrushVersionChanged()
		{
			if (this._isBrushVersionBindingInitialized)
				this.SetProviderBrushes();
		}

		#endregion

		#region IReceivePropertyChange<object> Members

		void IReceivePropertyChange<object>.OnPropertyChanged(DependencyProperty property, object oldValue, object newValue)
		{
			if (property == FrameworkElement.DataContextProperty)
				this.UpdateVisualState();
		}

		#endregion //IReceivePropertyChange<object> Members
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