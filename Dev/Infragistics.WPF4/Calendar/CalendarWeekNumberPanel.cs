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
using System.Diagnostics;

namespace Infragistics.Controls.Editors.Primitives
{
	/// <summary>
	/// A specialized panel that that arranges <see cref="CalendarWeekNumber"/> elements for a <see cref="CalendarItemArea"/> 
	/// </summary>

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

	public class CalendarWeekNumberPanel : UniformGrid
	{
		#region Private Members

		private CalendarItemArea _itemArea;

		#endregion //Private Members	
    
		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="CalendarWeekNumberPanel"/>
		/// </summary>
		public CalendarWeekNumberPanel()
		{
			this.Columns = 1;
			this.Rows = 6;
		}

		#endregion //Constructor

		#region Base class overrides

		#endregion //Base class overrides	
        
		#region Methods

		#region Internal

		#region GetWeek

		internal CalendarWeekNumber GetWeekNumber(int weekNumber)
		{
			UIElementCollection children = this.Children;
			int count = children.Count;

			for (int i = 0; i < count; i++)
			{
				CalendarWeekNumber wkElement = children[i] as CalendarWeekNumber;

				if (wkElement != null && wkElement.Week == weekNumber)
					return wkElement;
			}

			return null;
		}
		#endregion //GetWeek

		#region InitializeItemsArea

		internal void InitializeItemsArea(CalendarItemArea itemsArea)
		{
			if (itemsArea == _itemArea)
				return;

			if (_itemArea != null)
				((INotifyCollectionChanged)(_itemArea.WeekNumbers)).CollectionChanged -= new NotifyCollectionChangedEventHandler(OnWeekNumbersChanged);

			_itemArea = itemsArea;

			if (_itemArea != null)
			{
				this.InitializeWeekNumbers();
				((INotifyCollectionChanged)(_itemArea.WeekNumbers)).CollectionChanged += new NotifyCollectionChangedEventHandler(OnWeekNumbersChanged);
			}
		}

		#endregion //InitializeItemsArea

		#endregion //Internal	
 
		#region Private

		#region InitializeWeekNumbers

		private void InitializeWeekNumbers()
		{
			if ( _itemArea == null )
				return;

			CalendarBase cal = CalendarBase.GetCalendar(this);

			//Debug.Assert(cal != null, "CalendarBase.GetCalendar() should not return null");
			if ( cal == null )
				return;

			// JJD 5/2/11 - TFS74024
			// Use the cached overall first date from the item area
			DateTime? firstDate = _itemArea.FirstDate;

			if (firstDate == null)
				return;

			

			
			

			Visibility visibility = cal.WeekNumberVisibility;

			UIElementCollection children = this.Children;
			int oldChildCount = children.Count;
			ReadOnlyObservableCollection<int> weekNumbers = _itemArea.WeekNumbers;
			int oldCount = children.Count;
			int newCount = weekNumbers.Count;

			CalendarManager calMgr = cal.CalendarManager;
			
			int offset;

			DateTime startdate = calMgr.GetFirstDayOfWeekForDate( firstDate.Value, cal.FirstDayOfWeekInternal, out offset);
			DateTime enddate;

			int childIndex = 0;

			for (int i = 0; i < newCount; i++)
			{
				CalendarWeekNumber weekNumElement = null;
				while (weekNumElement == null && childIndex < oldChildCount)
				{
					weekNumElement = children[childIndex] as CalendarWeekNumber;
					childIndex++;
				}

				if (weekNumElement == null)
				{
					weekNumElement = new CalendarWeekNumber();
					children.Add(weekNumElement);
				}

				enddate = calMgr.AddDays(startdate, 6 + offset);
				
				offset = 0;
				
				weekNumElement.Initialize(cal, _itemArea, weekNumbers[i], visibility, startdate, enddate);

				startdate = calMgr.AddDays(enddate, 1);
			}

			// remove any extra off the end
			for (int i = oldChildCount - 1; i >= childIndex; i--)
			{
				CalendarWeekNumber weekNumElement = children[i] as CalendarWeekNumber;

				if (weekNumElement != null)
					children.RemoveAt(i);
			}
		}

		#endregion //InitializeWeekNumbers

		#region OnWeekNumbersChanged

		private void OnWeekNumbersChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.InitializeWeekNumbers();
		}

		#endregion //OnWeekNumbersChanged	
    
		#endregion //Private	
    
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