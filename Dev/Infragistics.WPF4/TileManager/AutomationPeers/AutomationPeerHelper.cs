using System;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Collections;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using System.Collections.Generic;

namespace Infragistics.AutomationPeers
{
	// AS 4/13/11 TFS72669
	// Some changes (probably for a bug fix) cause the InvalidatePeer method to 
	// not necessarily re-request the Children of the peer so now we need to explicitly 
	// indicate that the children have been changed. The only public method for 
	// invalidating the children is a synchronous one - ResetChildrenCache - 
	// so this class will provide the ability to asynchronously invoke that method. 
	// Also to avoid resetting the children when the children have been updated 
	// in the interim there is a method that the peer can call (usually from the 
	// GetChildrenCore) to remove the peer from the pending list.
	//
	internal static class AutomationPeerHelper
	{
		#region Member Variables

		[ThreadStatic]
		private static DispatcherOperation _pendingOperation;

		[ThreadStatic]
		private static WeakDictionary<AutomationPeer, bool> _pendingInvalidationTable;  

		#endregion //Member Variables

		#region Internal Methods

		#region InvalidateChildren
		internal static void InvalidateChildren(AutomationPeer peer)
		{
			if (_pendingInvalidationTable == null)
				_pendingInvalidationTable = new WeakDictionary<AutomationPeer, bool>(true, false);

			_pendingInvalidationTable[peer] = true;

			if (null == _pendingOperation)
			{

				_pendingOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new SendOrPostCallback(OnProcessPendingInvalidations), new object[] { null });



			}
		}
		#endregion //InvalidateChildren

		#region RemovePendingChildrenInvalidation
		internal static void RemovePendingChildrenInvalidation(AutomationPeer peer)
		{
			if (_pendingInvalidationTable != null)
				_pendingInvalidationTable.Remove(peer);
		}
		#endregion //RemovePendingChildrenInvalidation

		#endregion //Internal Methods

		#region Private Methods

		#region OnProcessPendingInvalidations
		private static void OnProcessPendingInvalidations(object param)
		{
			_pendingOperation = null;
			List<AutomationPeer> peers = new List<AutomationPeer>(_pendingInvalidationTable.Keys);
			_pendingInvalidationTable.Clear();

			foreach (AutomationPeer peer in peers)
			{

				peer.ResetChildrenCache();



			}
		}
		#endregion //OnProcessPendingInvalidations

		#endregion //Private Methods
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