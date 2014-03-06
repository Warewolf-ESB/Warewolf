using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows;
using Infragistics.Controls.Schedules.Primitives;
using Infragistics.Collections;
using Infragistics.Controls.Editors;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Base class for an object that represents a specific time range.
	/// </summary>
	internal abstract class TimeslotBase : RecyclingContainer<TimeRangePresenterBase>
		, ITimeRange
		, ISparseArrayItem
	{
		#region Member Variables

		private DateTime _start;
		private DateTime _end; 

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TimeslotBase"/>
		/// </summary>
		/// <param name="start">The starting time for the time slot</param>
		/// <param name="end">The ending time for the time slot</param>
		/// <remarks>
		/// <p class="note"><b>Note:</b> If the <paramref name="end"/> is before the <paramref name="start"/>, the <see cref="Start"/> and <see cref="End"/> will represent the normalized time. I.e. The Start will always be before or equal to the <see cref="End"/></p>
		/// </remarks>
		protected TimeslotBase(DateTime start, DateTime end)
		{
			ScheduleUtilities.Normalize(ref start, ref end);

			_start = start;
			_end = end;
		} 
		#endregion //Constructor

		#region Base class overrides

		#region OnElementAttached
		/// <summary>
		/// Invoked when the object is being associated with an element.
		/// </summary>
		/// <param name="element">The element with which the object is being associated</param>
		protected override void OnElementAttached(TimeRangePresenterBase element)
		{
			element.Start = this.Start;
			element.End = this.End;
			element.Timeslot = this;
		}
		#endregion //OnElementAttached

		#region ToString
		/// <summary>
		/// Returns the string representation of the object.
		/// </summary>
		/// <returns>A string containing the <see cref="Start"/> and <see cref="End"/></returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0}-{1}", this.Start, this.End);
		}
		#endregion //ToString

		#endregion //Base class overrides

		#region Properties

		#region End
		/// <summary>
		/// Returns the non-inclusive end time for the time slot.
		/// </summary>
		public DateTime End
		{
			get { return _end; }
		}
		#endregion //End

		#region Start
		/// <summary>
		/// Returns the start time for the time slot.
		/// </summary>
		public DateTime Start
		{
			get { return _start; }
		}
		#endregion //Start

		#endregion //Properties

		#region ISparseArrayItem Members

		private object _sparseArrayData;

		object ISparseArrayItem.GetOwnerData(SparseArray context)
		{
			return _sparseArrayData;
		}

		void ISparseArrayItem.SetOwnerData(object ownerData, SparseArray context)
		{
			_sparseArrayData = ownerData;
		}

		#endregion //ISparseArrayItem Members
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