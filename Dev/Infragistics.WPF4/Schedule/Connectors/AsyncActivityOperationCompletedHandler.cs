using System;
using System.ComponentModel;
using System.Windows.Threading;
using System.Diagnostics;








using Infragistics.Collections;

namespace Infragistics.Controls.Schedules

{
	#region PropertyChangeTrigger

	internal class PropertyChangeTrigger<TOwner, TActionData> : PropertyChangeListener<TOwner>
		where TOwner : class
	{
		private ISupportPropertyChangeNotifications _item;
		private string _property;
		protected Action<TOwner, TActionData> _action;
		protected TActionData _actionData;

		protected PropertyChangeTrigger( ISupportPropertyChangeNotifications item, string property, Action<TOwner, TActionData> action, TOwner owner, TActionData actionData )
			: base( owner, null )
		{
			_item = item;
			_property = property;
			_action = action;
			_actionData = actionData;

			item.AddListener( this, false );
		}

		internal static void ExecuteWhenPropertyChanges( ISupportPropertyChangeNotifications item, string property, TOwner owner, TActionData actionData, Action<TOwner, TActionData> action )
		{
			var handler = new PropertyChangeTrigger<TOwner, TActionData>( item, property, action, owner, actionData );
		}

		protected void RemoveListener( )
		{
			_item.RemoveListener( this );
		}

		public override void OnPropertyValueChanged( object dataItem, string property, object extraInfo )
		{
			TOwner owner = this.Owner;

			if ( null != owner && _property == property )
				this.OnPropertyChangedOverride( owner );
		}

		protected virtual void OnPropertyChangedOverride( TOwner owner )
		{
			if ( null != _action )
				_action( owner, _actionData );
		}
	} 

	#endregion // PropertyChangeTrigger

	#region AsyncActivityOperationCompletedHandler Class

	/// <summary>
	/// When an async operation is being performed that requires raising of an event when the operation is 
	/// complete, for example the add appointment operation, this class is used to raise the event when the 
	/// operation completes.
	/// </summary>
	internal class AsyncActivityOperationCompletedHandler<TOwner, TActionData> : PropertyChangeTrigger<TOwner, TActionData>
		where TOwner : class
	{
		protected OperationResult _result;

		protected AsyncActivityOperationCompletedHandler( OperationResult result, Action<TOwner, TActionData> action, TOwner owner, TActionData actionData )
			: base( result, "IsComplete", action, owner, actionData )
		{
			_result = result;
		}

		internal static void ExecuteOnComplete( OperationResult result, Action<TOwner, TActionData> action, TOwner owner, TActionData data )
		{
			if ( result.IsComplete )
			{
				action( owner, data );
			}
			else
			{
				var handler = new AsyncActivityOperationCompletedHandler<TOwner, TActionData>( result, action, owner, data );
			}
		}

		protected override void OnPropertyChangedOverride( TOwner owner )
		{
			if ( _result.IsComplete )
				base.OnPropertyChangedOverride( owner );
		}
	}

	#endregion // AsyncActivityOperationCompletedHandler Class


	#region DataManagerActivityOperationCompletedHandler Class

	/// <summary>
	/// When an async operation is being performed that requires raising of an event when the operation is 
	/// complete, for example the add appointment operation, this class is used to raise the event when the 
	/// operation completes.
	/// </summary>
	internal class DataManagerActivityOperationCompletedHandler : AsyncActivityOperationCompletedHandler<XamScheduleDataManager, ActivityOperation?>
	{
		private DataManagerActivityOperationCompletedHandler( OperationResult result, XamScheduleDataManager owner, ActivityOperation? activityOperation = null )
			: base( result, null, owner, activityOperation )
		{
		}

		protected override void OnPropertyChangedOverride( XamScheduleDataManager dm )
		{
			if ( _result.IsComplete )
			{
				ActivityOperationResult result = (ActivityOperationResult)_result;
				ActivityBase activity = null != result ? result.Activity : null;

				Debug.Assert( null != activity );

				// Null out the pending operation on the activity since the operation is complete.
				// 
				OnActivityPendingOperationCompleted( dm, result );

				// If one of the Add, Edit or Remove operation was performed on the activity, raise
				// the corresponding ActivityAdded/Changed/Removed event.
				// 
				ActivityOperation? activityOperation = _actionData;
				if ( activityOperation.HasValue )
					RaiseEventHelperHelper( dm, result, activityOperation.Value );
			}
		}
		
		internal static void OnCancelOperationBeingPerformed( XamScheduleDataManager dm, CancelOperationResult cancelOperationResult )
		{
			Debug.Assert( null != cancelOperationResult );

			// If an operation being canceled is an activity operation.
			// 
			ActivityOperationResult activityOperation = null != cancelOperationResult ? cancelOperationResult.Operation as ActivityOperationResult : null;
			ActivityBase activity = null != activityOperation ? activityOperation.Activity : null;
			if ( null != activity )
			{
				if ( cancelOperationResult.IsComplete )
					OnActivityPendingOperationCompleted( dm, activityOperation );
				else
					ManagePendingOperationHelper( dm, activity, cancelOperationResult, true );
			}
		}

		private static void OnActivityPendingOperationCompleted( XamScheduleDataManager dm, ActivityOperationResult operation )
		{
			ActivityBase activity = null != operation ? operation.Activity : null;
			Debug.Assert( null != activity );
			if ( null != activity )
			{
				// If the pending operation completed then reset the activity's PendingOperation to null.
				// 
				OperationResult pendingOp = activity.PendingOperation;
				if ( pendingOp == operation )
				{
					// If a cancel operation gets canceled or there's an error with cancelation, and 
					// the original operation that was to be canceled is still pending then restore
					// the activity's PendingOperation to the original operation since that's still
					// pending.
					// 
					CancelOperationResult cancelOp = pendingOp as CancelOperationResult;
					if ( null != cancelOp && !cancelOp.Operation.IsComplete )
					{
						activity.PendingOperation = cancelOp.Operation;
					}
					else
					{
						activity.PendingOperation = null;

						// Since the opreation is complete, set the activity's Error if there's an error. If 
						// there's no error then clear any previous error.
						// 
						ManageActivityErrorHelper( dm, operation, true );
					}
				}
				else
					// If a cancel operation is pending and the associated operation that's being canceled actually completes,
					// then we could get into this situation where the activity's PendingOperation is not the operation that
					// completed. In which case assert to make sure the PendingOperation on the activity is actually a cancel
					// operation and the associated operation being canceled is the one that just completed. The cancel operation
					// itself should become completed.
					// 
					Debug.Assert( pendingOp is CancelOperationResult && ( (CancelOperationResult)pendingOp ).Operation == operation );
			}
		}

		private static void ManagePendingOperationHelper( XamScheduleDataManager dm, ActivityBase activity, OperationResult operation, bool hook )
		{
			Debug.Assert( null != operation && null != activity );
			if ( null != operation && null != activity )
			{
				// Set the pending operation on the activity. This can be used to show a busy indicator in the UI
				// to convey to the end user that an operation on the activity is being performed.
				// 
				Debug.Assert( operation is CancelOperationResult ? ( (CancelOperationResult)operation ).Operation == activity.PendingOperation : null == activity.PendingOperation );
				activity.PendingOperation = operation;

				if ( hook )
				{
					// Simply create a new instance of DataManagerActivityOperationCompletedHandler which will hook
					// into the operation and call virtual OnPropertyChangedOverride method when the operation is complete.
					// 
					var handler = new DataManagerActivityOperationCompletedHandler( operation, dm );
				}
			}
		}

		internal static void RaiseEventHelper( XamScheduleDataManager dm, ActivityOperationResult result, ActivityOperation activityOperation )
		{
			// If the activity was an add-new activity and we raised ActivityAdding above then also
			// raise the corresponding ActivityAdded event.
			// 
			ActivityBase activity = null != result ? result.Activity : null;
			if ( null != activity )
			{
				if ( result.IsComplete )
				{
					RaiseEventHelperHelper( dm, result, activityOperation );
				}
				else
				{
					var handler = new DataManagerActivityOperationCompletedHandler( result, dm, activityOperation );

					// Manage setting and resetting of the activity's PendingOperation property. Pass in false for
					// the 'hook' parameter since we are hooking right above.
					// 
					ManagePendingOperationHelper( dm, activity, result, false );
				}
			}
		}

		private static void RaiseEventHelperHelper( XamScheduleDataManager dm, ActivityOperationResult result, ActivityOperation activityOperation )
		{
			// If the activity was an add-new activity and we raised ActivityAdding above then also
			// raise the corresponding ActivityAdded event.
			// 
			ActivityBase activity = null != result ? result.Activity : null;
			if ( null != dm && null != activity )
			{
				Debug.Assert( result.IsComplete, "This should be called when the result is complete!" );

				bool operationCompletedSuccessfully = !result.IsCanceled && null == result.Error;

				switch ( activityOperation )
				{
					case ActivityOperation.Add:
						// Reset IsAddNew flag when the add-new activity gets comitted.
						// 
						activity.IsAddNew = false;

						if ( operationCompletedSuccessfully )
							dm.RaiseActivityAdded( new ActivityAddedEventArgs( activity ) );
						break;
					case ActivityOperation.Edit:
						if ( operationCompletedSuccessfully )
							dm.RaiseActivityChanged( new ActivityChangedEventArgs( activity ) );
						break;
					case ActivityOperation.Remove:
						if ( operationCompletedSuccessfully )
							dm.RaiseActivityRemoved( new ActivityRemovedEventArgs( activity ) );
						break;
					default:
						Debug.Assert( false, "Unknown activity operation." );
						break;
				}
			}
		}

		internal static void ManageActivityErrorHelper( XamScheduleDataManager dm, ActivityBase activity, DataErrorInfo error, bool clearPreviousError )
		{
			if ( null != error )
			{
				// Raise the Error event.
				// 
				dm.ProcessError( error );

				// Raise the ErrorDisplaying event.
				// 
				ErrorDisplayingEventArgs args = new ErrorDisplayingEventArgs( error, ScheduleErrorDisplayType.ActivityErrorIcon );
				dm.OnErrorDisplaying( args );
				if ( ! args.Cancel )
					activity.Error = error;
			}
			else if ( clearPreviousError )
				activity.Error = null;
		}

		internal static void ManageActivityErrorHelper( XamScheduleDataManager dm, ActivityOperationResult result, bool clearPreviousError )
		{
			if ( result.IsComplete )
				ManageActivityErrorHelper( dm, result.Activity, result.Error, clearPreviousError );
		}
	}

	#endregion // DataManagerActivityOperationCompletedHandler Class 

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