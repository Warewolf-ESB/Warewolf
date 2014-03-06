using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Infragistics.Collections;
using System.Collections;

namespace Infragistics.Controls.Schedules
{
	// AS 12/8/10 NA 11.1 - XamOutlookCalendarView
	internal interface ISelectedActivityCollectionOwner
	{
		void OnSelectedActivitiesChanged();
	}

	/// <summary>
	/// Represents a collection of <see cref="ActivityBase"/> instances
	/// </summary>
	/// <seealso cref="ScheduleControlBase.SelectedActivities"/>
	public class SelectedActivityCollection : ObservableCollectionExtended<ActivityBase>
	{
		#region Private Members

		// AS 12/8/10 NA 11.1 - XamOutlookCalendarView
		//private ScheduleControlBase _control;
		private ISelectedActivityCollectionOwner _owner;
		private PropertyChangeListener<SelectedActivityCollection> _listener;

		#endregion //Private Members	
    
		#region Constructor

		internal SelectedActivityCollection(ISelectedActivityCollectionOwner owner)
			: base(false, true)
		{
			_owner = owner;
			_listener = new PropertyChangeListener<SelectedActivityCollection>(this, OnSubObjectPropertyChanged);
			this.PropChangeListeners.Add(_listener, false);
		}

		#endregion //Constructor	

		#region Base class overrides

		#region ClearItems
		/// <summary>
		/// Removes all the items from the collection.
		/// </summary>
		protected override void ClearItems()
		{
			// AS 9/20/10 TFS44162
			// ObservableCollections always raise change notifications when you call clear
			// and we are using the collectionchanged event as the trigger for the 
			// selectedactivitieschanged event. So for this collection we'll just suppress
			// the base impl if the count is already 0.
			//
			if (this.Count == 0)
				return;

			base.ClearItems();
		} 
		#endregion // ClearItems

		#region NotifyItemsChanged
		/// <summary>
		/// Returns true to indicate that the collection wants to receive notifications as items are added and removed.
		/// </summary>
		protected override bool NotifyItemsChanged
		{
			get
			{
				return true;
			}
		}
		#endregion // NotifyItemsChanged

		#region OnCollectionChanged

		/// <summary>
		/// Invoked when the collection is sending out its change notifications
		/// </summary>
		/// <param name="e"></param>
		protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			base.OnCollectionChanged(e);

			_owner.OnSelectedActivitiesChanged();
		}

		#endregion //OnCollectionChanged	
    
		#region OnItemAdding
		/// <summary>
		/// Invoked when an item is about to be added to the collection.
		/// </summary>
		/// <param name="itemAdded">The item being added</param>
		protected override void OnItemAdding(ActivityBase itemAdded)
		{
			if (itemAdded == null)
				throw new ArgumentNullException();

			base.OnItemAdding(itemAdded);
		}
		#endregion // OnItemAdding

		#endregion // Base class overrides

		#region Methods

		#region Private Methods

		#region OnSubObjectPropertyChanged

		private static void OnSubObjectPropertyChanged(SelectedActivityCollection collection, object sender, string propName, object extraInfo)
		{
			ActivityBase activity = sender as ActivityBase;

			if (activity != null && propName == "IsDeleted" && collection != null)
			{
				int index = collection.IndexOf(activity);

				if (index >= 0)
					collection.RemoveAt(index);
			}
		}
		#endregion //OnSubObjectPropertyChanged

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