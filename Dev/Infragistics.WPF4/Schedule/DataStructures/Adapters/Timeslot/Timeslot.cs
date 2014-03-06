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

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom object used to represent a specific time range in a <see cref="ScheduleTimeControlBase"/>
	/// </summary>
	internal class Timeslot : TimeslotBase
	{
		#region Member Variables

		private bool _isSelected;
		private bool _isWorkingHour;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="Timeslot"/>
		/// </summary>
		/// <param name="start">The starting time for the time slot</param>
		/// <param name="end">The ending time for the time slot</param>
		/// <remarks>
		/// <p class="note"><b>Note:</b> If the <paramref name="end"/> is before the <paramref name="start"/>, the <see cref="TimeslotBase.Start"/> and <see cref="TimeslotBase.End"/> will represent the normalized time. I.e. The Start will always be before or equal to the End.</p>
		/// </remarks>
		public Timeslot(DateTime start, DateTime end)
			: base(start, end)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region CreateInstanceOfRecyclingElement
		/// <summary>
		/// Used to provide an instance of the containing element.
		/// </summary>
		/// <returns></returns>
		protected override TimeRangePresenterBase CreateInstanceOfRecyclingElement()
		{
			return new TimeslotPresenter();
		}
		#endregion //CreateInstanceOfRecyclingElement

		#region OnElementAttached
		/// <summary>
		/// Invoked when the object is being associated with an element.
		/// </summary>
		/// <param name="element">The element with which the object is being associated</param>
		protected override void OnElementAttached(TimeRangePresenterBase element)
		{
			TimeslotPresenter tsp = element as TimeslotPresenter;

			if (null != tsp)
			{
				tsp.IsWorkingHour = this.IsWorkingHour;
				tsp.IsSelected = this.IsSelected;
			}

			base.OnElementAttached(element);
		}
		#endregion //OnElementAttached

		#region RecyclingElementType

		/// <summary>
		/// Gets the Type of control that should be created for the object.
		/// </summary>
		protected override Type RecyclingElementType
		{
			get
			{
				return typeof(TimeslotPresenter);
			}
		}
		#endregion // RecyclingElementType

		#endregion //Base class overrides

		#region Properties

		#region IsSelected
		/// <summary>
		/// Returns a boolean indicating if the time slot is selected.
		/// </summary>
		public bool IsSelected
		{
			get { return _isSelected; }
			internal set
			{
				if (value != _isSelected)
				{
					_isSelected = value;

					TimeslotPresenter tsp = this.AttachedElement as TimeslotPresenter;

					if (null != tsp)
						tsp.IsSelected = value;

					this.OnPropertyChanged("IsSelected");
				}
			}
		}
		#endregion //IsSelected

		#region IsWorkingHour
		/// <summary>
		/// Returns a boolean indicating if the time slot represents a working hour.
		/// </summary>
		public bool IsWorkingHour
		{
			get { return _isWorkingHour; }
			internal set
			{
				if (value != _isWorkingHour)
				{
					_isWorkingHour = value;

					TimeslotPresenter tsp = this.AttachedElement as TimeslotPresenter;

					if (null != tsp)
						tsp.IsWorkingHour = value;

					this.OnPropertyChanged("IsWorkingHour");
				}
			}
		}
		#endregion //IsWorkingHour

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