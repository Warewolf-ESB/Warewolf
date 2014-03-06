using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Windows.Data;
using System.Linq;
using Infragistics.Controls.Schedules;
using Infragistics.Controls.Primitives;


using Infragistics.Services;
using Infragistics.Controls.Schedules.Services;

namespace Infragistics.Collections.Services



{
	#region ItemNotificationCollection

	
	
	
	internal class ItemNotificationCollection<T> : ReadOnlyNotifyCollection<T>
	{
		#region Member Vars

		private IEnumerable<T> _source;
		private ObservableCollectionExtended<T> _dest;
		private DataListEventListener _listEventListener;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source"></param>
		internal ItemNotificationCollection( IEnumerable<T> source )
			: base( source is IList<T> && source is ISupportPropertyChangeNotifications
					? (IList<T>)source : new ObservableCollectionExtended<T>( true, true ) )
		{
			CoreUtilities.ValidateNotNull( source );

			this.SetSourceCollectionHelper( source );
		}

		#endregion // Constructor

		#region Base Overrides

		#region SetSourceCollection

		internal override void SetSourceCollection( IList<T> source )
		{
			this.SetSourceCollectionHelper( source );
		}

		#endregion // SetSourceCollection 

		#endregion // Base Overrides

		#region Methods

		#region Internal Methods

		#region SetSourceCollection

		internal void SetSourceCollection(IEnumerable<T> source)
		{
			this.SetSourceCollectionHelper(source);
		}

		#endregion  // SetSourceCollection 

		#endregion  // Internal Methods

		#region Private Methods

		#region HasMultipleItems

		private static bool HasMultipleItems( IList list )
		{
			return null != list && list.Count > 1;
		}

		#endregion // HasMultipleItems

		#region OnSourceListChanged

		private void OnSourceListChanged( object owner, DataListEventListener listener, DataListChangeInfo changeInfo )
		{
			var dest = _dest;
			if ( null == _dest )
				return;		

			bool processAsReset = false;

			switch ( changeInfo._changeType )
			{
				case DataListChangeInfo.ChangeType.Add:
					{
						int index = changeInfo._newIndex;
						if ( index < 0 )
							index = dest.Count;

						foreach ( T ii in changeInfo.NewItems )
							dest.Insert( index++, ii );
					}
					break;
				case DataListChangeInfo.ChangeType.Move:
					{
						int oldIndex = changeInfo._oldIndex;
						int newIndex = changeInfo._newIndex;

						if ( !HasMultipleItems( changeInfo._oldItems ) )
						{
							T item = dest[oldIndex];
							dest.RemoveAt( oldIndex );
							dest.Insert( newIndex, item );
						}
						else
						{
							foreach ( T ii in changeInfo._oldItems )
								dest.RemoveAt( oldIndex );

							foreach ( T ii in changeInfo._newItems )
								dest.Insert( newIndex++, ii );
						}
					}
					break;
				case DataListChangeInfo.ChangeType.Remove:
					{
						int index = changeInfo._oldIndex;

						if ( !HasMultipleItems( changeInfo._oldItems ) )
						{
							dest.RemoveAt( index );
						}
						else
						{
							foreach ( T ii in changeInfo._oldItems )
								dest.RemoveAt( index );
						}
					}
					break;
				case DataListChangeInfo.ChangeType.Replace:
					{
						int index = changeInfo._newIndex;

						if ( index >= 0 )
						{
							if ( !HasMultipleItems( changeInfo._newItems ) )
							{
								dest[index] = (T)changeInfo.FirstAndOnlyNewItem;
							}
							else
							{
								foreach ( T ii in changeInfo._newItems )
									dest[index++] = ii;
							}
						}
					}
					break;
				case DataListChangeInfo.ChangeType.Reset:
					processAsReset = true;
					break;
				default:
					processAsReset = true;
					break;
			}

			if ( processAsReset )
				this.Verify( );
		}

		#endregion // OnSourceListChanged

		#region SetSourceCollectionHelper

		private void SetSourceCollectionHelper( IEnumerable<T> source )
		{
			IList<T> resolvedSource = source is IList<T> && source is ISupportPropertyChangeNotifications
					? (IList<T>)source 
					: ( _dest ?? new ObservableCollectionExtended<T>( true, true ) );

			base.SetSourceCollection( resolvedSource );

			_source = source;
			_dest = _source != this.Source ? this.Source as ObservableCollectionExtended<T> : null;

			if ( _source != _dest && null != _dest )
			{
				this.Verify( );

				if ( null == _listEventListener )
					_listEventListener = new DataListEventListener( this, OnSourceListChanged, null, false );

				_listEventListener.List = source;
			}
		}

		#endregion // SetSourceCollectionHelper

		#region Verify

		private void Verify( )
		{
			ScheduleUtilities.ReplaceCollectionContents( _dest, _source );
		}

		#endregion // Verify

		#endregion // Private Methods

		#endregion // Methods
	}

	#endregion // ItemNotificationCollection
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