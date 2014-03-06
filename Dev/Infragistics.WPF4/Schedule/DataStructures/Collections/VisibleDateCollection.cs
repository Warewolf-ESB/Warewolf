using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Infragistics.Collections;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Custom collection of DateTime that represent the dates that should be in view in the associated XamSchedule control
	/// </summary>
	internal class VisibleDateCollection : DateCollection
	{
		#region Member Variables

		private ScheduleControlBase _owner; 
		
		#endregion //Member Variables

		#region Constructor
		internal VisibleDateCollection(ScheduleControlBase owner)
		{
			ScheduleUtilities.ValidateNotNull(owner, "owner");
			_owner = owner;
		} 
		#endregion //Constructor

		#region Base class overrides

		#region OnDatesChanged
		/// <summary>
		/// Invoked when items have been added and/or removed from the collection
		/// </summary>
		/// <param name="added">The items that were added</param>
		/// <param name="removed">The items that were removed</param>
		protected override void OnDatesChanged(IList<DateTime> added, IList<DateTime> removed)
		{
			_owner.OnVisibleDatesChanged(added, removed);
		}
		#endregion //OnDatesChanged 

		#endregion //Base class overrides

		#region Methods

		#region Internal Methods

		#region GetAllowedDatesInternal
		internal IList<DateTime> GetAllowedDatesInternal( DateRange range )
		{
			return this.GetAllowedDates(range);
		}
		#endregion // GetAllowedDatesInternal

		#endregion // Internal Methods 

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