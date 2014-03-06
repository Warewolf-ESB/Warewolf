using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading;




namespace Infragistics

{
	/// <summary>
	/// Helper class for invoking an action asynchronously with the ability to cancel the operation or invoke it synchronously at an earlier point.
	/// </summary>
	internal class DeferredOperation
	{
		#region Member Variables

		private DispatcherSynchronizationContext _pendingContext;
		private Action _deferredAction;
		private bool _isPerformingAction;

		#endregion // Member Variables

		#region Constructor
		internal DeferredOperation(Action deferredAction)
		{
			CoreUtilities.ValidateNotNull(deferredAction);
			_deferredAction = deferredAction;
		}
		#endregion // Constructor

		#region Properties

		#region IsOperationPending
		internal bool IsOperationPending
		{
			get { return _pendingContext != null; }
		} 
		#endregion // IsOperationPending 

		#region IsPerformingAction
		internal bool IsPerformingAction
		{
			get { return _isPerformingAction; }
		} 
		#endregion // IsPerformingAction

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region InvokePendingOperation
		internal void InvokePendingOperation()
		{
			if (_pendingContext == null)
				return;

			_pendingContext = null;

			bool wasPerforming = _isPerformingAction;
			_isPerformingAction = true;
			try
			{
				_deferredAction();
			}
			finally
			{
				_isPerformingAction = wasPerforming;
			}
		}
		#endregion // InvokePendingOperation

		#region CancelPendingOperation
		internal void CancelPendingOperation()
		{
			_pendingContext = null;
		}
		#endregion // CancelPendingOperation

		#region StartAsyncOperation
		internal void StartAsyncOperation()
		{
			if (_pendingContext != null)
				return;

			_pendingContext = new DispatcherSynchronizationContext();
			_pendingContext.Post(new SendOrPostCallback(OnAsyncCallback), _pendingContext);
		}
		#endregion // StartAsyncOperation

		#endregion // Internal Methods

		#region Private Methods

		#region OnAsyncCallback
		private void OnAsyncCallback(object state)
		{
			// if the operation was cancelled or performed synchronously then bail out
			if (state != _pendingContext)
				return;

			this.InvokePendingOperation();
		}
		#endregion // OnAsyncCallback

		#endregion // Private Methods

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