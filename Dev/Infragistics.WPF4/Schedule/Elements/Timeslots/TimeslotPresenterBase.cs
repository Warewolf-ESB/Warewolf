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
using System.Diagnostics;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents a specific range of time.
	/// </summary>
	[TemplateVisualState(Name = VisualStateUtilities.StateIsFirstInDay,             GroupName = VisualStateUtilities.GroupDay)]
	[TemplateVisualState(Name = VisualStateUtilities.StateIsFirstAndLastInDay,      GroupName = VisualStateUtilities.GroupDay)]
	[TemplateVisualState(Name = VisualStateUtilities.StateIsLastInDay,              GroupName = VisualStateUtilities.GroupDay)]
	[TemplateVisualState(Name = VisualStateUtilities.StateIsNotFirstOrLastInDay,    GroupName = VisualStateUtilities.GroupDay)]
	
	[TemplateVisualState(Name = VisualStateUtilities.StateIsFirstInMajor,           GroupName = VisualStateUtilities.GroupMajor)]
	[TemplateVisualState(Name = VisualStateUtilities.StateIsFirstAndLastInMajor,    GroupName = VisualStateUtilities.GroupMajor)]
	[TemplateVisualState(Name = VisualStateUtilities.StateIsLastInMajor,            GroupName = VisualStateUtilities.GroupMajor)]
	[TemplateVisualState(Name = VisualStateUtilities.StateIsNotFirstOrLastInMajor,  GroupName = VisualStateUtilities.GroupMajor)]
	public abstract class TimeslotPresenterBase : TimeRangePresenterBase
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TimeslotPresenterBase"/>
		/// </summary>
		protected TimeslotPresenterBase()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region ChangeVisualState
		internal override void ChangeVisualState(bool useTransitions)
		{
			if (this.IsFirstInDay)
			{
				if (this.IsLastInDay)
					VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateIsFirstAndLastInDay, VisualStateUtilities.StateIsLastInDay, VisualStateUtilities.StateIsFirstInDay);
				else
					this.GoToState(VisualStateUtilities.StateIsFirstInDay, useTransitions);
			}
			else
			{
				if (this.IsLastInDay)
					this.GoToState(VisualStateUtilities.StateIsLastInDay, useTransitions);
				else
					this.GoToState(VisualStateUtilities.StateIsNotFirstOrLastInDay, useTransitions);
			}


			if (this.IsFirstInMajor)
			{
				if (this.IsLastInMajor)
					VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateIsFirstAndLastInMajor, VisualStateUtilities.StateIsLastInMajor, VisualStateUtilities.StateIsFirstInMajor);
				else
					this.GoToState(VisualStateUtilities.StateIsFirstInMajor, useTransitions);
			}
			else
			{
				if (this.IsLastInMajor)
					this.GoToState(VisualStateUtilities.StateIsLastInMajor, useTransitions);
				else
					this.GoToState(VisualStateUtilities.StateIsNotFirstOrLastInMajor, useTransitions);
			}

			base.ChangeVisualState(useTransitions);
		}
		#endregion //ChangeVisualState

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region IsFirstInDay

		/// <summary>
		/// Identifies the <see cref="IsFirstInDay"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsFirstInDayProperty = DependencyProperty.Register("IsFirstInDay",
			typeof(bool), typeof(TimeslotPresenterBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnVisualStatePropertyChanged))
			);

		/// <summary>
		/// Returns a boolean indicating if this is the first timeslot for the containing date.
		/// </summary>
		/// <seealso cref="IsFirstInDayProperty"/>
		public bool IsFirstInDay
		{
			get
			{
				return (bool)this.GetValue(TimeslotPresenterBase.IsFirstInDayProperty);
			}
			internal set
			{
				this.SetValue(TimeslotPresenterBase.IsFirstInDayProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsFirstInDay

		#region IsFirstInMajor

		private bool _isFirstInMajor; // AS 4/1/11 TFS60630 - Optimization

		/// <summary>
		/// Identifies the <see cref="IsFirstInMajor"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsFirstInMajorProperty = DependencyProperty.Register("IsFirstInMajor",
			typeof(bool), typeof(TimeslotPresenterBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsFirstInMajorChanged))
			);

		private static void OnIsFirstInMajorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeslotPresenterBase item = (TimeslotPresenterBase)d;
			item._isFirstInMajor = (bool)e.NewValue; // AS 4/1/11 TFS60630
			item.UpdateVisualState();
			item.OnIsFirstInMajorChanged((bool)e.OldValue, (bool)e.NewValue);
		}

		internal virtual void OnIsFirstInMajorChanged(bool oldValue, bool newValue)
		{
		}

		/// <summary>
		/// Returns a boolean indicating if this is element is considered a major change in the timeslot interval such as a change in hour.
		/// </summary>
		/// <seealso cref="IsFirstInMajorProperty"/>
		public bool IsFirstInMajor
		{
			get
			{
				// AS 4/1/11 TFS60630
				//return (bool)this.GetValue(TimeslotPresenterBase.IsFirstInMajorProperty);
				return _isFirstInMajor;
			}
			internal set
			{
				this.SetValue(TimeslotPresenterBase.IsFirstInMajorProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsFirstInMajor

		#region IsLastInMajor

		/// <summary>
		/// Identifies the <see cref="IsLastInMajor"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsLastInMajorProperty = DependencyProperty.Register("IsLastInMajor",
			typeof(bool), typeof(TimeslotPresenterBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnVisualStatePropertyChanged))
			);

		/// <summary>
		/// Returns a boolean indicating if this is element is considered a major change in the timeslot interval such as a change in hour.
		/// </summary>
		/// <seealso cref="IsLastInMajorProperty"/>
		public bool IsLastInMajor
		{
			get
			{
				return (bool)this.GetValue(TimeslotPresenterBase.IsLastInMajorProperty);
			}
			internal set
			{
				this.SetValue(TimeslotPresenterBase.IsLastInMajorProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsLastInMajor

		#region IsLastInDay

		/// <summary>
		/// Identifies the <see cref="IsLastInDay"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsLastInDayProperty = DependencyProperty.Register("IsLastInDay",
			typeof(bool), typeof(TimeslotPresenterBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnVisualStatePropertyChanged))
			);

		/// <summary>
		/// Returns a boolean indicating if this is the last timeslot for the containing date.
		/// </summary>
		/// <seealso cref="IsLastInDayProperty"/>
		public bool IsLastInDay
		{
			get
			{
				return (bool)this.GetValue(TimeslotPresenterBase.IsLastInDayProperty);
			}
			internal set
			{
				this.SetValue(TimeslotPresenterBase.IsLastInDayProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsLastInDay

		#endregion //Public Properties

		#endregion //Properties
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