using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Controls.Primitives;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Infragistics.Controls.Editors.Primitives
{
	/// <summary>
	/// A specialized panel that that arranges <see cref="CalendarDayOfWeek"/> elements for a <see cref="CalendarItemArea"/> 
	/// </summary>

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

	public class CalendarDayOfWeekPanel : UniformGrid, ICalendarElement
	{
		private int _lastColumnCount;

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="CalendarDayOfWeekPanel"/>
		/// </summary>
		public CalendarDayOfWeekPanel()
		{
			this.Rows = 1;
		}

		#endregion //Constructor

		#region Base class overrides

		#endregion //Base class overrides	
        
		#region Methods

		#region InitializeDaysOfWeek

		internal void InitializeDaysOfWeek()
		{
			CalendarBase cal = CalendarUtilities.GetCalendar(this);

			if ( cal == null )
				return;
			
			UIElementCollection children = this.Children;
			int oldChildCount = children.Count;
			ReadOnlyObservableCollection<DayOfWeek> daysOfWeekColl = cal.DaysOfWeek;
			int oldCount = this._lastColumnCount;
			int newCount = daysOfWeekColl.Count;
			this.Columns = newCount;

			int childIndex = 0;

			for (int i = 0; i < newCount; i++)
			{
				CalendarDayOfWeek dowElement = null;
				while (dowElement == null && childIndex < oldChildCount)
				{
					dowElement = children[childIndex] as CalendarDayOfWeek;
					childIndex++;
				}

				if (dowElement == null)
				{
					dowElement = new CalendarDayOfWeek();
					children.Add(dowElement);
				}

				dowElement.Initialize(daysOfWeekColl[i], cal);
			}

			// remove any extra off the end
			for (int i = oldChildCount - 1; i >= childIndex; i--)
			{
				CalendarDayOfWeek dowElement = children[i] as CalendarDayOfWeek;

				if (dowElement != null)
					children.RemoveAt(i);
			}

			this._lastColumnCount = this.Columns;
		}

		#endregion //InitializeDaysOfWeek

		#region Private

		#region OnDaysOfWeekChanged

		private void OnDaysOfWeekChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.InitializeDaysOfWeek();
		}

		#endregion //OnDaysOfWeekChanged	
    
		#endregion //Private	
    
		#endregion //Methods	
    
		#region ICalendarElement Members

		void ICalendarElement.OnCalendarChanged(CalendarBase newValue, CalendarBase oldValue)
		{

			if (oldValue != null)
			{
				((INotifyCollectionChanged)(oldValue.DaysOfWeek)).CollectionChanged -= new NotifyCollectionChangedEventHandler(OnDaysOfWeekChanged);
			}

			if (newValue != null)
			{
				((INotifyCollectionChanged)(newValue.DaysOfWeek)).CollectionChanged += new NotifyCollectionChangedEventHandler(OnDaysOfWeekChanged);
				this.InitializeDaysOfWeek();
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