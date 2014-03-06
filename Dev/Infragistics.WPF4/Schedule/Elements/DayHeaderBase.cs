using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Base class for an element that represents a header for a specific date
	/// </summary>
    [TemplateVisualState(Name = VisualStateUtilities.StateRegularDay,	GroupName = VisualStateUtilities.GroupDay)]
    [TemplateVisualState(Name = VisualStateUtilities.StateToday,		GroupName = VisualStateUtilities.GroupDay)]
	public class DayHeaderBase : ResourceCalendarElementBase
	{
		#region Constructor

		static DayHeaderBase()
		{
		}

		/// <summary>
		/// Initializes a new <see cref="DayHeaderBase"/>
		/// </summary>
		protected DayHeaderBase()
		{
        } 
		#endregion // Constructor

        #region Base class overrides

		#region OnMouseLeftButtonDown
		/// <summary>
		/// Invoked when the left mouse button is pressed down on the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse operation</param>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			// AS 3/7/12 TFS102945
			//ClickHelper.OnMouseLeftButtonDown(this, e, this.RaiseClick, true);
			ScheduleUtilities.OnTouchAwareClickHelperDown(this, e, this.RaiseClick, true);

			base.OnMouseLeftButtonDown(e);
		}
		#endregion // OnMouseLeftButtonDown

        #endregion //Base class overrides

		#region Properties

		#region ComputedBorderThickness

		private static readonly DependencyPropertyKey ComputedBorderThicknessPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBorderThickness",
			typeof(Thickness), typeof(DayHeaderBase), new Thickness(), null);

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
				return (Thickness)this.GetValue(DayHeaderBase.ComputedBorderThicknessProperty);
			}
			internal set
			{
				this.SetValue(DayHeaderBase.ComputedBorderThicknessPropertyKey, value);
			}
		}

		#endregion //ComputedBorderThickness
		
		#region ComputedForeground

		private static readonly DependencyPropertyKey ComputedForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedForeground",
			typeof(Brush), typeof(DayHeaderBase), ScheduleUtilities.GetBrush(Colors.Black), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedForeground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedForegroundProperty = ComputedForegroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the Foreground based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedForegroundProperty"/>
		public Brush ComputedForeground
		{
			get
			{
				return (Brush)this.GetValue(DayHeaderBase.ComputedForegroundProperty);
			}
			internal set
			{
				this.SetValue(DayHeaderBase.ComputedForegroundPropertyKey, value);
			}
		}

		#endregion //ComputedForeground

		#region DateTime

		/// <summary>
		/// Identifies the <see cref="DateTime"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DateTimeProperty = DependencyProperty.Register("DateTime",
			typeof(DateTime), typeof(DayHeaderBase),
			DependencyPropertyUtilities.CreateMetadata(DateTime.Today)
			);

		/// <summary>
		/// Returns or sets the Date of the logical day that the element represents
		/// </summary>
		/// <seealso cref="DateTimeProperty"/>
		public DateTime DateTime
		{
			get
			{
				return (DateTime)this.GetValue(DayHeaderBase.DateTimeProperty);
			}
			
			internal set
			{
				this.SetValue(DayHeaderBase.DateTimeProperty, value);
			}
		}

		#endregion //DateTime

		#region IsToday

		private bool _isToday = false;

		private static readonly DependencyPropertyKey IsTodayPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsToday",
			typeof(bool), typeof(DayHeaderBase),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback(OnIsTodayChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="IsToday"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsTodayProperty = IsTodayPropertyKey.DependencyProperty;

		private static void OnIsTodayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DayHeaderBase instance = (DayHeaderBase)d;

			instance._isToday = true.Equals(e.NewValue);

			// bring the today item to the front so its border shows
			if (instance._isToday)
				Canvas.SetZIndex(instance, 1);
			else
				instance.ClearValue(Canvas.ZIndexProperty);

			instance.UpdateVisualState();
		}

		/// <summary>
		/// Returns a boolean indicating if the header represents the current logical day.
		/// </summary>
		/// <seealso cref="IsTodayProperty"/>
		public bool IsToday
		{
			get
			{
				return _isToday;
			}
			internal set
			{
				this.SetValue(DayHeaderBase.IsTodayPropertyKey, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsToday

		#endregion // Properties

        #region Methods

        #region Private Methods

        #region ChangeVisualState
        internal override void ChangeVisualState(bool useTransitions)
        {
			if (this.IsToday)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateToday, VisualStateUtilities.StateRegularDay);
			else
				this.GoToState(VisualStateUtilities.StateRegularDay, useTransitions);

			base.ChangeVisualState(useTransitions);
        }

        #endregion //ChangeVisualState

		#region RaiseClick
		private void RaiseClick()
		{
			var ctrl = ScheduleUtilities.GetControl(this);

			if (null != ctrl)
				ctrl.OnDayHeaderClicked(this);
		} 
		#endregion // RaiseClick

        #region SetProviderBrushes

        internal override void SetProviderBrushes()
        {
            if (!this.IsBrushVersionBindingInitialized)
                return;

			var owingControl = ScheduleUtilities.GetControl(this);
			CalendarBrushProvider brushProvider = ScheduleUtilities.GetCalendarGroupBrushProvider(this, owingControl);

			if (brushProvider == null)
				return;

			bool isToday = this.IsToday;

            #region Set backround

            Brush br = null;

            CalendarBrushId brushId;

			if (isToday)
				brushId = CalendarBrushId.CurrentDayHeaderBackground;
			else
	            brushId = CalendarBrushId.DayHeaderBackground;

            br = brushProvider.GetBrush(brushId);
 
            if (br != null)
            {
                this.ComputedBackground = br;
            }

            #endregion //Set background

            #region Set Foreground

			if (isToday)
				brushId = CalendarBrushId.CurrentDayHeaderForeground;
			else
				brushId = CalendarBrushId.DayHeaderForground;

            br = brushProvider.GetBrush(brushId);
 
            if (br != null)
                this.ComputedForeground = br;

            #endregion //Set border brush

            #region Set border brush

            br = null;

			if (isToday)
				brushId = CalendarBrushId.CurrentDayBorder;
			else
				brushId = CalendarBrushId.DayBorder;

            br = brushProvider.GetBrush(brushId);
 
            if (br != null)
            {
                this.ComputedBorderBrush = br;

                this.ComputedBorderThickness = new Thickness(1,1,1,0);
            }

            #endregion //Set border brush
        }

        #endregion //SetProviderBrushes

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