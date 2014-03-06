using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Windows.Data;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Collections;
using Infragistics.Controls.Schedules.Primitives;

namespace Infragistics.Controls.Schedules

{
	#region ListScheduleDataConnectorUtilities class

	
	
	

	internal static class ListScheduleDataConnectorUtilities<T>
	{
		#region Internal Methods

		#region BeginEdit

		/// <summary>
		/// Begins modifications to an item.
		/// </summary>
		/// <param name="connector">Schedule data connector.</param>
		/// <param name="item">Item that is to be modified.</param>
		/// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <param name="getEditListCallback">The callback to get the edit list associated with the item being modified.</param>
		/// <returns>A value indicating whether the operation succeeded.</returns>
		internal static bool BeginEdit( IScheduleDataConnector connector, T item, Func<T, IEditList<T>> getEditListCallback, out DataErrorInfo errorInfo )
		{
			if ( !CheckItemOperationAllowed( connector, item, ActivityOperation.Edit, out errorInfo ) )
				return false;

			IEditList<T> editList = getEditListCallback( item );
			if ( null != editList && editList.IsEditTransactionSupported( item ) )
				return editList.BeginEdit( item, out errorInfo );

			return ScheduleDataConnectorUtilities<T>.DefaultBeginEditImplementation( item, out errorInfo );
		}

		#endregion // BeginEdit

		#region CheckItemOperationAllowed

		/// <summary>
		/// Checks to see if the specified operation is allowed on the item. If the operation is not allowed, 
		/// initializes the specified result with an error.
		/// </summary>
		/// <param name="connector">Schedule data connector.</param>
		/// <param name="result">Item result object.</param>
		/// <param name="operation">Operation being performed.</param>
		/// <returns>True if the operation is allowed. False otherwise.</returns>
		internal static bool CheckItemOperationAllowed( IScheduleDataConnector connector, ItemOperationResult<T> result, ActivityOperation operation )
		{
			DataErrorInfo error;
			if ( !CheckItemOperationAllowed( connector, result.Item, operation, out error ) )
			{
				result.InitializeResult( error, true );
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks to see if the specified operation is allowed for the activity using connector's IsActivityOperationSupported
		/// method. If the operation is not allowed, initializes the specified result with an error.
		/// </summary>
		/// <param name="connector">Schedule data connector.</param>
		/// <param name="item">Item object.</param>
		/// <param name="operation">Operation being performed.</param>
		/// <param name="error">This will be set to an error object if the operation is not allowed.</param>
		/// <returns>True if the operation is allowed. False otherwise.</returns>
		/// <returns>True if the operation is allowed. False otherwise.</returns>
		internal static bool CheckItemOperationAllowed( IScheduleDataConnector connector, T item, ActivityOperation operation, out DataErrorInfo error )
		{
			error = null;
			ActivityBase activity = item as ActivityBase;
			if ( null != activity && !connector.IsActivityOperationSupported( activity, operation ) )
			{
				error = new DataErrorInfo( "Activity can not be edited. Underlying data source does not allow modifications to the activity." )
				{
					Context = activity
				};

				Debug.WriteLine( "We should not get in here. The UI should check IsActivityOperationSupported and disallow edits through the UI to begin with and therefore the BeginEdit or EndEdit should not be called." );
				return false;
			}

			return true;
		}

		#endregion // CheckItemOperationAllowed

		#region DefaultCancelEditImplementation

		/// <summary>
		/// Cancels a new activity that was created by the ScheduleDataConnectorBase's CreateNew call however one that 
		/// hasn't been commited yet.
		/// </summary>
		/// <param name="item">ActivityBase derived object that was created using ScheduleDataConnectorBase's CreateNew method however
		/// one that hasn't been committed yet.</param>
		/// <param name="getEditListCallback">The callback to get the edit list associated with a specific activity.</param>
		/// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <returns>True to indicate that the operation was successfull, False otherwise.</returns>
		internal static bool DefaultCancelEditImplementation( T item, Func<T, IEditList<T>> getEditListCallback, out DataErrorInfo errorInfo )
		{
			IEditList<T> editList = getEditListCallback( item );
			if ( null != editList && editList.IsEditTransactionSupported( item ) )
				return editList.CancelEdit( item, out errorInfo );

			return ScheduleDataConnectorUtilities<T>.DefaultCancelEditImplementation( item, out errorInfo );
		}

		#endregion // DefaultCancelEditImplementation

		#region DefaultEndEditOverrideImplementation

		/// <summary>
		/// This is called to commit changes to an activity to the underlying data source.
		/// </summary>
		/// <param name="editList">This is the list manager.</param>
		/// <param name="result">Result's Activity is being updated. This result should be initialized with the result of the opreation.</param>
		/// <param name="force">True if the UI cannot remain in edit mode and therefore the operation must be ended,
		/// either with success or with an error. It cannot be canceled.</param>
		internal static void DefaultEndEditOverrideImplementation( IEditList<T> editList, ItemOperationResult<T> result, bool force )
		{
			T item = result.Item;

			// If the activity is an add-new activity then add it to the underlying data list.
			// If the activity is not add-new and the list object implements IEditableObject,
			// then call IEditableObject.EndEdit on the list object to commit the changes.
			// 
			bool isAddNew = editList.IsAddNew( item );

			// Also consider an activity add-new if it's an occurrence. Note that we only commit variances
			// which we check for in EndEditHelper method.
			// 
			if ( !isAddNew )
			{
				ActivityBase activity = item as ActivityBase;
				isAddNew = null != activity && activity.IsOccurrence && activity.DataItem is OccurrenceId;
			}

			DataErrorInfo errorInfo = null;

			if ( isAddNew || editList.IsEditTransactionSupported( item ) )
			{
				bool success = editList.EndEdit( item, out errorInfo );
				Debug.Assert( success ^ null != errorInfo );
			}
			else
			{
				// Let the list manager perform any additional processing, like letting the underlying
				// ITable submit changes.
				// 
				editList.OnEditEnded( item, out errorInfo );
			}

			// Call the base class implementation to commit changes to an existing activity.
			// 
			ItemOperationResult<T> baseResult = ScheduleDataConnectorUtilities<T>.DefaultEndEditImplementation( item );

			// Base implementation is always synchronous.
			// 
			if ( null == errorInfo )
				errorInfo = baseResult.Error;

			result.InitializeResult( errorInfo, true );
		}

		#endregion // DefaultEndEditOverrideImplementation

		#region EndEdit

		/// <summary>
		/// Commits a new or modified activity.
		/// </summary>
		/// <param name="connector">Schedule data connector.</param>
		/// <param name="item">A new or modified ActivityBase derived instance.</param>
		/// <param name="force">True to force the edit operation to end. Used when user interface
		/// being used to perform the edit operation cannot remain in edit mode and therefore the
		/// edit operation must be ended. If the specified activity is deemed invalid to be committed
		/// then an error result should be returned.
		/// </param>
		/// <param name="endEditOverride">The callback used for each activity actually changed in the end edit operation.</param>
		/// <param name="getEditListCallback">The callback to get the edit list associated with a specific activity.</param>
		/// <returns><see cref="ActivityOperationResult"/> instance which may be initialized with the result
		/// asynchronously.</returns>
		internal static ItemOperationResult<T> EndEdit(
			IScheduleDataConnector connector,
			T item,
			bool force,
			Action<IEditList<T>, ItemOperationResult<T>, bool> endEditOverride,
			Func<T, IEditList<T>> getEditListCallback )
		{
			ItemOperationResult<T> result = ScheduleDataConnectorUtilities<T>.Instance.CreateOperationResult( item );

			if ( CheckItemOperationAllowed( connector, result, ScheduleDataConnectorUtilities<T>.Instance.IsAddNew( item ) ? ActivityOperation.Add : ActivityOperation.Edit ) )
				EndEditHelper( connector, result, force, endEditOverride, getEditListCallback );

			// If EndEdit fails then restore the data to the original data. Otherwise the activity will be left
			// with the modified data when that data has not been comitted to the data source.
			// 
			if ( null != result.Error )
			{
				DataErrorInfo errorInfo;
				DefaultCancelEditImplementation( item, getEditListCallback, out errorInfo );
			}

			return result;
		}

		#endregion // EndEdit

		#region Remove

		/// <summary>
		/// Removes an activity.
		/// </summary>
		/// <param name="connector">Schedule data connector.</param>
		/// <param name="activity">ActivityBase derived instance to remove.</param>
		/// <param name="endEditOverride">The callback used for each activity actually changed in the end edit operation.</param>
		/// <param name="getEditListCallback">The callback to get the edit list associated with a specific activity.</param>
		/// <returns><see cref="ActivityOperationResult"/> instance.</returns>
		/// <seealso cref="EndEdit"/>
		internal static ActivityOperationResult Remove(
			IScheduleDataConnector connector,
			ActivityBase activity,
			Action<IEditList<ActivityBase>, ItemOperationResult<ActivityBase>, bool> endEditOverride,
			Func<ActivityBase, IEditList<ActivityBase>> getEditListCallback )
		{
			DataErrorInfo errorInfo;

			if ( ListScheduleDataConnectorUtilities<ActivityBase>.CheckItemOperationAllowed( connector, activity, ActivityOperation.Remove, out errorInfo ) )
			{
				if ( activity.IsOccurrence )
				{
					if ( activity.IsOccurrenceDeleted )
					{
						errorInfo = ScheduleUtilities.CreateErrorFromId( activity, "LE_AlreadyDeleted" );//"This occurrence has already been deleted."
					}
					else
					{
						if ( ListScheduleDataConnectorUtilities<ActivityBase>.BeginEdit( connector, activity, getEditListCallback, out errorInfo ) )
						{
							activity.IsOccurrenceDeleted = true;

							return ListScheduleDataConnectorUtilities<ActivityBase>.EndEdit( connector, activity, false, endEditOverride, getEditListCallback ) as ActivityOperationResult;
						}

						errorInfo = ScheduleUtilities.CreateErrorFromId( activity, "LE_CanNotDelete" );//"Cannot delete occurrence because either it's already in edit mode or cannot be modified."
					}
				}
				else
				{
					IEditList<ActivityBase> editList = getEditListCallback( activity );

					errorInfo = null;
					bool success = null != editList && editList.Remove( activity, out errorInfo );
					Debug.Assert( success ^ null != errorInfo );
				}
			}

			return new ActivityOperationResult( activity, errorInfo, true );
		}

		#endregion // Remove

		#endregion // Internal Methods

		#region Private Methods

		#region CommitVarianceHelper

		/// <summary>
		/// Committing a variance involves potentially updating the associated root activity's MaxOccurrenceDateTime
		/// field if the new end time of the occurrence is greater than the MaxOccurrenceDateTime because the 
		/// MaxOccurrenceDateTime must always be greater than or equal to the maximum end time of all the occurrences
		/// of the recurring activity. This is relied upon by our querying logic.
		/// </summary>
		/// <param name="connector">Schedule data connector.</param>
		/// <param name="result">This is the result object. Result's Activity is the variance that's being updated.</param>
		/// <param name="force">True if the UI cannot remain in edit mode and therefore the operation must be ended,
		/// either with success or with an error. It cannot be canceled.</param>
		/// <param name="attempt">0 if this is the first attempt at updating the root activity. 1 if we had previously attempted
		/// to update the root activity and we shouldn't try again.</param>
		/// <param name="endEditOverride">The callback used for each activity actually changed in the end edit operation.</param>
		/// <param name="getEditListCallback">The callback to get the edit list associated with a specific activity.</param>
		private static void CommitVarianceHelper(
			IScheduleDataConnector connector,
			ItemOperationResult<ActivityBase> result,
			bool force,
			int attempt,
			Action<IEditList<ActivityBase>, ItemOperationResult<ActivityBase>, bool> endEditOverride,
			Func<ActivityBase, IEditList<ActivityBase>> getEditListCallback )
		{
			DataErrorInfo errorInfo = null;
			ActivityBase activity = result.Item;
			ActivityBase rootActivity = activity.RootActivity;
			Debug.Assert( null != rootActivity );
			if ( null == rootActivity )
			{
				errorInfo = ScheduleUtilities.CreateErrorFromId( activity, "LE_RootActivityNotFound" ); //"The activity's series is not found."
				errorInfo.DiagnosticText = ScheduleUtilities.GetString( "LE_RootActivityNotFound_Full" );
				//"The variant activity's root activity reference is not initialized."
				//    + " This can typically occur because the MaxOccurrenceDataTime of the root activity"
				//    + " was not updated to reflect change in the End time of the variant activity to"
				//    + " a value greater than the MaxOccurrenceDateTime. Also this error can occur if"
				//    + " Start of a variant activity was changed to a value before the Start of the"
				//    + " root activity.";
			}

			if ( null == errorInfo )
			{
				// Set the flags that maintain which properties are variant properties based on the current values of properties
				// compared to the values of those properties in the root activity.
				// 
				activity.InitializeVariantPropertiesFlags( );

				// We do not allow moving an activity to a time before root activity's start because
				// of our activity querying logic that relies on all occurrences, including variances,
				// to be between the root activity's Start and MaxOccurrenceDateTime values. If we
				// want to remove this restriction than we have to modify the querying logic - 
				// potentially add a MinOccurrenceDateTime field that contains the time of the 
				// earliest variance of the recurrence.
				// 
				if ( activity.Start < rootActivity.Start )
					errorInfo = ScheduleUtilities.CreateErrorFromId( activity, "LE_MustBeAfterSeries" );//"An occurrence' Start time cannot be changed to be earlier than the series start time."

				if ( null == errorInfo && activity.End > rootActivity.MaxOccurrenceDateTime )
				{
					if ( 0 == attempt && ListScheduleDataConnectorUtilities<ActivityBase>.BeginEdit( connector, rootActivity, getEditListCallback, out errorInfo ) )
					{
						rootActivity.MaxOccurrenceDateTime = activity.End;

						ActivityOperationResult rootCommitResult = ListScheduleDataConnectorUtilities<ActivityBase>.EndEdit( connector, rootActivity, true, endEditOverride, getEditListCallback ) as ActivityOperationResult;

						AsyncActivityOperationCompletedHandler<object, Tuple<ItemOperationResult<ActivityBase>, ItemOperationResult<ActivityBase>, bool, Action<IEditList<ActivityBase>, ItemOperationResult<ActivityBase>, bool>, Func<ActivityBase, IEditList<ActivityBase>>>>.ExecuteOnComplete(
							rootCommitResult, OnRootActivityUpdated_CommitVariance, connector,
							new Tuple<ItemOperationResult<ActivityBase>, ItemOperationResult<ActivityBase>, bool, Action<IEditList<ActivityBase>, ItemOperationResult<ActivityBase>, bool>, Func<ActivityBase, IEditList<ActivityBase>>>( rootCommitResult, result, force, endEditOverride, getEditListCallback ) );

						return;
					}

					errorInfo = ScheduleUtilities.CreateErrorFromId( activity, "LE_CanNotUpdateSeries" ); //"Unabled to update the series."
					errorInfo.DiagnosticText = ScheduleUtilities.GetString( "LE_CanNotUpdateSeries_Full" );
					//"Unable to update the variant activity's root activity.
					//        + " MaxOccurrenceDateTime property needs to be updated to reflect the new time"
					//        + " of the variant activity. MaxOccurrenceDateTime must be a value that's greater"
					//        + " than or equal to the greatest end time of any of the occurrences of the"
					//        + " series.";
				}
			}

			if ( null == errorInfo )
				EndEditActivityHelper( connector, result, force, endEditOverride, getEditListCallback, false );
			else
				result.InitializeResult( errorInfo, true );
		}

		#endregion // CommitVarianceHelper

		#region EndEditHelper

		/// <summary>
		/// A helper method that's called by the EndEdit and CommitVarianceHelper methods.
		/// </summary>
		/// <param name="connector">Schedule data connector.</param>
		/// <param name="result">Activity result object.</param>
		/// <param name="force">Whether to force end.</param>
		/// <param name="endEditOverride">The callback used for each activity actually changed in the end edit operation.</param>
		/// <param name="getEditListCallback">The callback to get the edit list associated with a specific activity.</param>
		/// <param name="updateVarianceRootQueryTimes"></param>
		private static void EndEditActivityHelper(
			IScheduleDataConnector connector,
			ItemOperationResult<ActivityBase> result,
			bool force,
			Action<IEditList<ActivityBase>, ItemOperationResult<ActivityBase>, bool> endEditOverride,
			Func<ActivityBase, IEditList<ActivityBase>> getEditListCallback,
			bool? updateVarianceRootQueryTimes = null )
		{
			ActivityBase activity = result.Item;

			if ( activity.IsOccurrence && ( updateVarianceRootQueryTimes ?? true ) )
			{
				// Only commit the occurrence if it's data has changed. Otherwise if all its data is 
				// the default data as it would be generated from the root activity don't commit it
				// to the data source. Fall through further below and call the base EndEdit which
				// clears the begin edit data.
				// 
				if ( activity.HasVariantDataFromBeginEditData( ) )
				{
					CommitVarianceHelper( connector, result, force, 0, endEditOverride, getEditListCallback );
					return;
				}
			}
			else
			{
				// When a recurrence rule is modified, or the activity's start, end, start/end time zones
				// are changed, we need to bump the recurrence sequence so any old variances get dropped.
				// 
				if ( activity.IsRecurrenceRoot )
					activity.BumpRecurrenceSequenceIfNecessary( );

				IEditList<ActivityBase> editList = getEditListCallback( activity );
				if ( null != editList )
				{
					endEditOverride( editList, result, force );
					return;
				}
			}

			// Call the base to clear the begin edit data.
			// 
			ScheduleDataConnectorUtilities<ActivityBase>.DefaultEndEditImplementation( activity );
			result.InitializeResult( null, true );
		}

		private static void EndEditHelper(
			IScheduleDataConnector connector,
			ItemOperationResult<T> result,
			bool force,
			Action<IEditList<T>, ItemOperationResult<T>, bool> endEditOverride,
			Func<T, IEditList<T>> getEditListCallback )
		{
			if ( typeof( T ) == typeof( ActivityBase ) )
			{
				EndEditActivityHelper( connector,
					result as ItemOperationResult<ActivityBase>,
					force,
					endEditOverride as Action<IEditList<ActivityBase>, ItemOperationResult<ActivityBase>, bool>,
					getEditListCallback as Func<ActivityBase, IEditList<ActivityBase>>,
					null );

				return;
			}

			T item = result.Item;

			IEditList<T> editList = getEditListCallback( item );
			if ( null != editList )
			{
				endEditOverride( editList, result, force );
				return;
			}

			// Call the base to clear the begin edit data.
			// 
			ScheduleDataConnectorUtilities<T>.DefaultEndEditImplementation( item );
			result.InitializeResult( null, true );
		}

		#endregion // EndEditHelper

		#region OnRootActivityUpdated_CommitVariance

		/// <summary>
		/// When a variance' root activity is updated, in order to update the variance we may need to wait for
		/// the root activity to complete updating. This is called when the root activity end edit operation
		/// compltes.
		/// </summary>
		/// <param name="notUsed"></param>
		/// <param name="data"></param>
		private static void OnRootActivityUpdated_CommitVariance(
			object notUsed,
			Tuple<ItemOperationResult<ActivityBase>, ItemOperationResult<ActivityBase>, bool, Action<IEditList<ActivityBase>, ItemOperationResult<ActivityBase>, bool>, Func<ActivityBase, IEditList<ActivityBase>>> data )
		{
			ItemOperationResult<ActivityBase> rootCommitResult = data.Item1;
			ItemOperationResult<ActivityBase> varianceCommitResult = data.Item2;
			bool force = data.Item3;

			if ( null != rootCommitResult.Error )
				varianceCommitResult.InitializeResult( rootCommitResult.Error, true );
			else if ( rootCommitResult.IsCanceled )
				varianceCommitResult.OnCanceled( );
			else
				CommitVarianceHelper( null, varianceCommitResult, force, 1, data.Item4, data.Item5 );
		}

		#endregion // OnRootActivityUpdated_CommitVariance

		#endregion // Private Methods
	}

	#endregion // ListScheduleDataConnectorUtilities class
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