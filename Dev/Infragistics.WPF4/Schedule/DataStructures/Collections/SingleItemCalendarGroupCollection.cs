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
using Infragistics.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Collection class used by a CalendarGroupCollection to maintain a collection of <see cref="SingleItemCalendarGroup"/> 
	/// instances for each <see cref="ResourceCalendar"/> in the <see cref="CalendarGroupBase.VisibleCalendars"/> of the groups 
	/// in the source collection.
	/// </summary>
	internal class SingleItemCalendarGroupCollection : ObservableCollectionExtended<CalendarGroupBase>
	{
		#region Member Variables

		private CalendarGroupCollection _groups;
		private bool _isDirty = true;

		#endregion // Member Variables

		#region Constructor
		internal SingleItemCalendarGroupCollection(CalendarGroupCollection groups)
		{
			_groups = groups;
			((ISupportPropertyChangeNotifications)groups).AddListener(new PropertyChangeListener<SingleItemCalendarGroupCollection>(this, OnSubObjectPropertyChanged), false);
		}
		#endregion // Constructor

		#region Methods

		#region OnSubObjectPropertyChanged

		private static void OnSubObjectPropertyChanged(SingleItemCalendarGroupCollection collection, object sender, string propName, object extraInfo)
		{
			var calendars = sender as CalendarGroupCalendarCollection;

			if (null != calendars && calendars.IsVisibleCalendars)
			{
				collection._isDirty = true;
			}
			else if (sender == collection._groups)
			{
				collection._isDirty = true;
			}
		}

		#endregion //OnSubObjectPropertyChanged

		#region VerifyState
		internal void VerifyState()
		{
			if (_isDirty)
			{
				_isDirty = false;

				Dictionary<ResourceCalendar, SingleItemCalendarGroup> oldGroups = new Dictionary<ResourceCalendar, SingleItemCalendarGroup>();

				for (int i = this.Count - 1; i >= 0; i--)
				{
					SingleItemCalendarGroup item = this[i] as SingleItemCalendarGroup;
					oldGroups[item.SelectedCalendar] = item;
				}

				this.BeginUpdate();
				this.Clear();

				foreach (CalendarGroup group in _groups)
				{
					foreach (ResourceCalendar calendar in group.VisibleCalendars)
					{
						SingleItemCalendarGroup item;

						// reuse or create an old calendar
						if (oldGroups.TryGetValue(calendar, out item))
							oldGroups.Remove(calendar);
						else
							item = new SingleItemCalendarGroup(calendar);

						this.Add(item);
					}
				}

				this.EndUpdate();
			}
		}
		#endregion // VerifyState

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