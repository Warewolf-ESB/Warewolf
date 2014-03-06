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
using Infragistics.Controls.Schedules.Primitives;
using System.Diagnostics;
using System.Collections.Generic;

namespace Infragistics.Controls.Schedules
{
	
#region Infragistics Source Cleanup (Region)

































































#endregion // Infragistics Source Cleanup (Region)

	internal class ActivityDragManager
	{
		#region Member Variables

		private XamScheduleDataManager _dm;

		private ActivityBase _activityBeingDragged;
		private ActivityBase _activityBeingDraggedCopy;
		private bool _hideActivityBeingDragged;
		private ScheduleControlBase _controlWithCopy;
		private IScheduleControl _lastValidDropControl;
		private DropAction _dropAction;
		private TimeZoneInfoProvider _tzProvider;
		private bool _activitySupportsEnd;

		#endregion // Member Variables

		#region Constructor
		internal ActivityDragManager(XamScheduleDataManager dm)
		{
			_dm = dm;
		}
		#endregion // Constructor

		#region Properties

		#region ActivityBeingDragged
		/// <summary>
		/// The activity being dragged
		/// </summary>
		public ActivityBase ActivityBeingDragged
		{
			get { return _activityBeingDragged; }
		} 
		#endregion // ActivityBeingDragged

		#region ActivityBeingDraggedCopy
		/// <summary>
		/// The copy of the activity that is being displayed.
		/// </summary>
		public ActivityBase ActivityBeingDraggedCopy
		{
			get { return _activityBeingDraggedCopy; }
		} 
		#endregion // ActivityBeingDraggedCopy

		#region CurrentDropAction
		internal DropAction CurrentDropAction
		{
			get { return _dropAction; }
		}
		#endregion // CurrentDropAction

		#region ControlWithCopy
		/// <summary>
		/// Returns the control that is displaying the activity copy.
		/// </summary>
		public ScheduleControlBase ControlWithCopy
		{
			get { return _controlWithCopy; }
		}

		#endregion // ControlWithCopy

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region BeginDrag
		/// <summary>
		/// Initiates a drag operation for a specific activity
		/// </summary>
		/// <param name="activityToDrag">The activity to be dragged</param>
		/// <param name="isCreatingCopy">True if the initial drag state is to create a copy of the activity; otherwise false to indicate that the source activity will be moved.</param>
		/// <param name="sourceControl">The control for which the activity is being hosted.</param>
		/// <returns>Returns true if the drag was started; otherwise false is returned</returns>
		internal bool BeginDrag(ActivityBase activityToDrag, bool isCreatingCopy, ScheduleControlBase sourceControl)
		{
			this.EndDrag(true);

			if (_activityBeingDragged != null)
				return false;

			// pass in null for the target calendar to test the capabilities
			bool allowMove = _dm.IsActivityDraggingAllowed(activityToDrag, null, false);
			bool allowCopy = _dm.IsActivityDraggingAllowed(activityToDrag, null, true);

			// if we can neither move nor copy the activity then don't start the drag
			if (!allowMove && !allowCopy)
				return false;

			var args = new ActivitiesDraggingEventArgs(new ActivityBase[] { activityToDrag });
			_dm.OnActivitiesDragging(args);

			if (args.Cancel)
				return false;

			if (_activityBeingDragged != null)
				return false;

			// create an internal clone since this may be a move and the connector may not support adding (plus 
			// we don't know the context of the target calendar)
			//
			// SSP 3/22/11 TFS67737
			// Pass in true for 'copyId' parameter. We need to copy the data item so any bindings in the activity
			// presenter continue working for the drag activity presenter.
			// 
			//ActivityBase clone = activityToDrag.Clone(false);
			ActivityBase clone = activityToDrag.Clone( true );

			_activityBeingDragged = activityToDrag;
			_activityBeingDraggedCopy = clone;
			_dropAction = DropAction.Original;
			_hideActivityBeingDragged = false;
			_activitySupportsEnd = _dm.IsEndDateSupportedByActivity(activityToDrag);
			_tzProvider = ScheduleUtilities.GetTimeZoneInfoProvider(sourceControl);

			if ((isCreatingCopy && !allowCopy) || (!isCreatingCopy && !allowMove))
			{
				// the initial operation is not allowed
				this.EnterInvalidDropLocation();
			}
			else if (isCreatingCopy)
			{
				// if we are creating a copy then include it in the control now
				_controlWithCopy = sourceControl;
				_dropAction = DropAction.Copy;

				// assuming we have a source control then show the copy in that
				if (sourceControl != null)
					sourceControl.ExtraActivities.Add(clone);
			}

			return true;
		}
		#endregion // BeginDrag

		#region EndDrag
		/// <summary>
		/// Ends the drag operation.
		/// </summary>
		/// <param name="cancel">True to cancel the drag; otherwise false to perform the current drop action.</param>
		/// <returns>Returns a boolean indicating if the drag was performed</returns>
		internal bool EndDrag(bool cancel)
		{
			ActivitiesDraggedEventArgs draggedArgs;
			DataErrorInfo error;

			bool result = EndDrag(cancel, out draggedArgs, out error);

			if (null != draggedArgs)
				_dm.OnActivitiesDragged(draggedArgs);

			if (null != error)
				_dm.DisplayErrorMessageBox(error, ScheduleUtilities.GetString("MSG_TITLE_DragActivity"));

			return result;
		}

		private bool EndDrag(bool cancel, out ActivitiesDraggedEventArgs draggedArgs, out DataErrorInfo error)
		{
			draggedArgs = null;
			error = null;

			if (_activityBeingDragged == null)
				return false;

			#region Clean up

			ActivityBase activity = _activityBeingDragged;
			ActivityBase copy = _activityBeingDraggedCopy;
			bool hideSource = _hideActivityBeingDragged;
			var targetCtrl = _controlWithCopy;
			var dropAction = _dropAction;
			var dropTargetControl = _lastValidDropControl;

			_lastValidDropControl = null;
			_activityBeingDragged = null;
			_activityBeingDraggedCopy = null;
			_hideActivityBeingDragged = false;
			_controlWithCopy = null;
			_dropAction = DropAction.Invalid;
			_tzProvider = null;

			#endregion // Clean up

			// we will either be updating the original, creating a new actual activity 
			// for the drop or cancelling the operation so always remove the extra activity. 
			if (null != targetCtrl)
				targetCtrl.ExtraActivities.Remove(copy);

			try
			{
				// since we haven't done anything to the source activity we don't need to do anything now
				if (cancel || dropAction == DropAction.Invalid)
					return false;

				if (dropAction == DropAction.Original)
					return true;

				ActivityOperationResult result;
				List<ActivityBase> activitiesDragged = new List<ActivityBase>();

				if (dropAction == DropAction.Move)
				{
					// close dialogs for this activity
					if (copy.OwningCalendar != activity.OwningCalendar)
						_dm.CloseDialog(activity);

					bool copyAndRemove = false;

					// if the activity is to be moved to a different calendar then 
					// we may need to do a remove and add
					if (activity.OwningCalendar != copy.OwningCalendar)
					{
						var connector = _dm.DataConnector;

						if (connector == null)
							return false;

						ActivityFeature feature = activity.OwningResource != copy.OwningResource ? ActivityFeature.CanChangeOwningResource : ActivityFeature.CanChangeOwningCalendar;

						// AS 10/26/10 TFS57182
						// Make sure that the source and target calendars support the change.
						//
						if ( !_dm.IsActivityFeatureSupported(activity.ActivityType, feature, activity.OwningCalendar) ||
							!_dm.IsActivityFeatureSupported(activity.ActivityType, feature, copy.OwningCalendar) )
						{
							copyAndRemove = true;
						}
					}

					if (!copyAndRemove)
					{
						if (!_dm.BeginEdit(activity, out error))
							return false;

						activity.Start = copy.Start;

						if (_activitySupportsEnd)
							activity.End = copy.End;

						activity.OwningCalendar = copy.OwningCalendar;

						// AS 11/1/10 TFS58871
						if (copy.IsTimeZoneNeutral != activity.IsTimeZoneNeutral)
							activity.IsTimeZoneNeutral = copy.IsTimeZoneNeutral;

						result = _dm.EndEdit(activity, true);

						activitiesDragged.Add(activity);
					}
					else
					{
						ActivityBase newActivity;
						
						if (!this.CreateCopy(activity, copy, out result, out error, out newActivity))
							return false;

						Debug.Assert(null != result);
						if (result != null && result.Error == null)
						{
							activitiesDragged.Add(newActivity);

							// if the add was synchronous then just remove the activity now
							if (result.IsComplete)
							{
								result = _dm.Remove(activity);
							}
							else
							{
								// otherwise we have to wait for the result to complete before we remove the source
								var listener = new PropertyChangeListener<Tuple<ActivityOperationResult, ActivityBase, XamScheduleDataManager>>(Tuple.Create(result, activity, _dm), OnAddMoveOperationChanged, false);
								ScheduleUtilities.AddListener(result, listener, false);
							}
						}
					}
				}
				else // copy
				{
					ActivityBase newActivity;

					if (!CreateCopy(activity, copy, out result, out error, out newActivity))
						return false;

					activitiesDragged.Add(newActivity);
				}

				if (result == null || result.Error != null)
					return false;

				// store any pending operations on the data manager
				if (!result.IsComplete && !result.IsCanceled)
				{
					_dm.AddPendingOperation(result);
				}

				// If the drop control is a data navigator then we need to see if it is hooked up
				// to a XamOutlookCalendarView. If so we want to bring the activity that was dropped
				// into view in the CurrentViewControl of the XamOutlookCalendarView.
				IOutlookDateNavigator dn = dropTargetControl as IOutlookDateNavigator;
				if (dn != null)
				{
					foreach (IScheduleControl control in _dm.Controls)
					{
						ScheduleControlBase sc = control as ScheduleControlBase;

						// bypass collapsed or uninitialized controls
						if (sc == null || PresentationUtilities.MayBeVisible(sc) == false)
							continue;

						XamOutlookCalendarView outlookView = PresentationUtilities.GetTemplatedParent( sc ) as XamOutlookCalendarView;

						// bypass controls unless they are within the template of a XamOutlookCalendarView 
						// that is hooked up to the DateNavigator and unless they are the current view control
						if (outlookView == null ||
							outlookView.DateNavigator != dn ||
							outlookView.CurrentViewControl != sc)
							continue;

						// first bring the date into view
						TimeZoneToken token = _dm.TimeZoneInfoProviderResolved.LocalToken;
						sc.BringIntoView(activity.GetEndLocal(token));
						sc.BringIntoView(activity.GetStartLocal(token));

						// finally bring the activity into view
						sc.BringIntoView(activity);

					}
				}

				draggedArgs = new ActivitiesDraggedEventArgs(activitiesDragged.ToArray(), dropAction == DropAction.Copy);
				return true;
			}
			finally
			{
				// if the source was hidden then make sure its visible now
				if (hideSource)
					this.OnDragActivityVisibilityChanged(targetCtrl, activity);
			}
		}
		#endregion // EndDrag

		#region EnterInvalidDropLocation
		/// <summary>
		/// Invoked when the mouse has entered an invalid drop location.
		/// </summary>
		internal void EnterInvalidDropLocation()
		{
			if (_activityBeingDragged == null)
				return;

			var previousTarget = _controlWithCopy;
			var clone = _activityBeingDraggedCopy;

			// show the original and hide the one being dragged
			_dropAction = DropAction.Invalid;

			if (_hideActivityBeingDragged)
			{
				Debug.Assert(previousTarget != null);
				_hideActivityBeingDragged = false;
				this.OnDragActivityVisibilityChanged(previousTarget);

				if (previousTarget != null)
				{
					previousTarget.ExtraActivities.Remove(clone);
					_controlWithCopy = null;
				}
			}
		} 
		#endregion // EnterInvalidDropLocation

		#region IsOperationAllowed
		/// <summary>
		/// Indicates if the activity being dragged can be moved to the specified calendar.
		/// </summary>
		/// <param name="calendar">The target calendar</param>
		/// <param name="copy">True if the activity would be copied otherwise false if it would be moved</param>
		/// <returns></returns>
		internal bool IsOperationAllowed(ResourceCalendar calendar, bool copy)
		{
			if (_activityBeingDragged == null)
				return false;

			if (calendar == null)
				return false;

			return _dm.IsActivityDraggingAllowed(_activityBeingDragged, calendar, copy);
		}
		#endregion // IsOperationAllowed

		#region Move
		/// <summary>
		/// Invoked when the activity enters a new time range.
		/// </summary>
		/// <param name="targetCalendar">The calendar to which the activity should be moved/copied</param>
		/// <param name="copy">True to perform a copy of the original activity; false to perform a move.</param>
		/// <param name="localStart">The new local start of the activity</param>
		/// <param name="localEnd">Optionally the new end of the activity.</param>
		/// <param name="targetControl">The control that the mouse is over and should be displaying the activity being dragged</param>
		/// <param name="isTimeZoneNeutral">Optionally the timezone neutral state for the activity</param>
		/// <returns>Returns true if the move operation was successful</returns>
		// JJD 3/2/11 - Changed targetControl from ScheduleControlBase to IScheduleControl to handle XamDateNavigator
		internal bool Move(IScheduleControl targetControl, bool copy, ResourceCalendar targetCalendar = null, DateTime? localStart = null, DateTime? localEnd = null, bool? isTimeZoneNeutral = null )
		{
			if (_activityBeingDragged == null)
				return false;

			_lastValidDropControl = null;

			var clone = _activityBeingDraggedCopy;
			var previousTarget = _controlWithCopy;

			if (null == clone)
				return false;

			// JJD 3/2/11 
			// Create a stack variable for ScheduleControlBase 
			ScheduleControlBase scb = targetControl as ScheduleControlBase;

			if (scb == null)
				copy = false;

			ResourceCalendar newCalendar = targetCalendar == null ? clone.OwningCalendar : targetCalendar;

			if (!IsOperationAllowed(newCalendar, copy))
			{
				this.EnterInvalidDropLocation();
				return false;
			}

			if (!_activitySupportsEnd)
				localEnd = localStart;

			// update the clone based on the drop target
			var token = _tzProvider.LocalToken;

			// AS 11/1/10 TFS58871
			bool newIsTimeZoneNeutral = isTimeZoneNeutral == null ? clone.IsTimeZoneNeutral : isTimeZoneNeutral.Value;

			if ( newIsTimeZoneNeutral == true && !_activityBeingDragged.IsTimeZoneNeutral )
			{
				// we don't have any property to determine whether we are allowed to change the timezone neutral state 
				// but we should not make something timezone neutral if that fucntionality is disabled or the feature 
				// isn't allowed by the connector.
				if ( (copy && !_dm.IsTimeZoneNeutralActivityAllowed(_activityBeingDragged.ActivityType, clone.OwningCalendar)) ||
					(!copy && !_dm.IsTimeZoneNeutralActivityAllowed(_activityBeingDragged)) )
				{
					isTimeZoneNeutral = false;
				}
			}

			DateTime newStart = localStart == null ? clone.GetStartLocal(token) : localStart.Value;
			DateTime newEnd = localEnd == null ? clone.GetEndLocal(token) : localEnd.Value;

			// the activity should be hidden if we're dragging it within a schedule control and doing a move unless
			// we're back in the original information
			bool shouldHide = false;

			_dropAction = copy ? DropAction.Copy : DropAction.Move;

			if (!copy)
			{
				if (targetControl != null)
				{
					if (newCalendar == _activityBeingDragged.OwningCalendar && 
						newStart == _activityBeingDragged.GetStartLocal(token) && 
						newEnd == _activityBeingDragged.GetEndLocal(token) &&
						newIsTimeZoneNeutral == _activityBeingDragged.IsTimeZoneNeutral )
					{
						// remove the copy when over the original location
						targetControl = null;
						_dropAction = DropAction.Original;
					}
					else
					{	
						shouldHide = true;
					}
				}
			}

			// JJD 3/2/11 
			// Only deal with copies if the target is a ScheduleControlBase
			if (scb != null)
			{
				// if the target control is different then move it
				if (targetControl != previousTarget)
				{
					if (null != previousTarget)
						previousTarget.ExtraActivities.Remove(clone);

					_controlWithCopy = targetControl as ScheduleControlBase;

					if (null != _controlWithCopy)
						_controlWithCopy.ExtraActivities.Add(clone);
				}
			}
			else
			{
				shouldHide = false;
			}

			bool washidden = _hideActivityBeingDragged;
			_hideActivityBeingDragged = shouldHide;
			
			if ( previousTarget != targetControl && washidden )
				this.OnDragActivityVisibilityChanged(previousTarget);

			if (washidden != _hideActivityBeingDragged || previousTarget != targetControl)
				this.OnDragActivityVisibilityChanged(targetControl);

			// just in case the drag is cancelled...
			if (_activityBeingDragged == null)
				return false;

			_lastValidDropControl = targetControl;

			if (null != localStart)
				clone.SetStartLocal(token, localStart.Value);

			if (null != localEnd && _activitySupportsEnd)
				clone.SetEndLocal(token, localEnd.Value);

			if (null != targetCalendar)
				clone.OwningCalendar = targetCalendar;

			// AS 11/1/10 TFS58871
			if ( null != isTimeZoneNeutral )
				clone.IsTimeZoneNeutral = isTimeZoneNeutral.Value;

			return true;
		} 
		#endregion // Move

		#region ShouldHideActivityBeingDragged
		internal bool ShouldHideActivityBeingDragged(IScheduleControl control)
		{
			// if we're over a different control then it should be visible
			if (control != _controlWithCopy)
				return false;

			return _hideActivityBeingDragged;
		}
		#endregion // ShouldHideActivityBeingDragged

		#endregion // Internal Methods

		#region Private Methods

		#region CreateCopy
		private bool CreateCopy(ActivityBase originalActivity, ActivityBase copy, out ActivityOperationResult result, out DataErrorInfo error, out ActivityBase newActivity)
		{
			result = null;

			// now get the datamanager/connector involved in creating an activity
			newActivity = _dm.CreateNew(originalActivity.ActivityType, out error);

			if (newActivity == null || error != null)
				return false;

			copy.CopyToHelper( newActivity, false, true, true );

			result = _dm.EndEdit(newActivity, true);

			// if its still pending then hold it in the extra activities
			if (result.Error == null && !result.IsComplete && !result.IsCanceled && newActivity.IsAddNew)
			{
				_dm.AddExtraActivity(newActivity);
			}

			return true;
		}
		#endregion // CreateCopy

		#region OnAddMoveOperationChanged
		private static void OnAddMoveOperationChanged(Tuple<ActivityOperationResult, ActivityBase, XamScheduleDataManager> owner, object sender, string propertyName, object extraInfo)
		{
			if (string.IsNullOrEmpty(propertyName) || propertyName == "IsComplete")
			{
				if (owner.Item1.IsComplete)
				{
					var result = owner.Item3.Remove(owner.Item2);

					if (result.IsComplete == false)
						owner.Item3.AddPendingOperation(result);
				}
			}
		}
		#endregion // OnAddMoveOperationChanged

		#region OnDragActivityVisibilityChanged
		private void OnDragActivityVisibilityChanged(IScheduleControl control)
		{
			this.OnDragActivityVisibilityChanged(control, _activityBeingDragged);
		}

		private void OnDragActivityVisibilityChanged(IScheduleControl control, ActivityBase activity)
		{
			if (null == control)
				return;

			bool hide = this.ShouldHideActivityBeingDragged(control);

			
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


			ScheduleControlBase sc = control as ScheduleControlBase;

			if (sc != null)
			{
				if (hide)
				{
					int index = sc.FilteredOutActivities.IndexOf(activity);

					if (index < 0)
						sc.FilteredOutActivities.Add(activity);
				}
				else
				{
					sc.FilteredOutActivities.Remove(activity);
				}
			}
		}
		#endregion // OnDragActivityVisibilityChanged

		#endregion // Private Methods

		#endregion // Methods

		#region DropAction enum
		internal enum DropAction
		{
			/// <summary>
			/// The current drop information is not a valid drop point
			/// </summary>
			Invalid,

			/// <summary>
			/// The current drop will result in a move of the activity
			/// </summary>
			Move,

			/// <summary>
			/// The current drop will create a copy of the source activity
			/// </summary>
			Copy,

			/// <summary>
			/// The current drop will result in a no-op
			/// </summary>
			Original,
		} 
		#endregion // DropAction enum
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