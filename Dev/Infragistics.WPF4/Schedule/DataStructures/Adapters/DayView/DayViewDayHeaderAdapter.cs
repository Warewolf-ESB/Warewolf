using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents the header of a specific date in the <see cref="XamDayView"/>
	/// </summary>
	internal class DayViewDayHeaderAdapter : DateItemAdapter<DayViewDayHeader>
	{
		#region Member Variables

		private bool _isToday; 

		#endregion // Member Variables

		#region Constructor
		internal DayViewDayHeaderAdapter(DateTime date) : base(date)
		{
		} 
		#endregion // Constructor

		#region Base class overrides

		#region CreateInstanceOfRecyclingElement
		protected override DayViewDayHeader CreateInstanceOfRecyclingElement()
		{
			return new DayViewDayHeader();
		}
		#endregion // CreateInstanceOfRecyclingElement

		#region OnElementAttached
		protected override void OnElementAttached(DayViewDayHeader element)
		{
			element.DateTime = this.Date;
			element.IsToday = _isToday;
		}
		#endregion // OnElementAttached

		#endregion // Base class overrides

		#region Properties

		#region IsToday
		internal bool IsToday
		{
			get { return _isToday; }
			set
			{
				if (value != _isToday)
				{
					_isToday = value;

					DayViewDayHeader element = this.AttachedElement as DayViewDayHeader;

					if (null != element)
						element.IsToday = value;
				}
			}
		}
		#endregion // IsToday

		#endregion // Properties
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