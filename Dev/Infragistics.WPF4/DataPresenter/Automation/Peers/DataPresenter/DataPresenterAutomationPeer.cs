using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.DataPresenter;
using Infragistics.Collections;
using Infragistics.Windows.Internal;
using System.Diagnostics;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	/// <summary>
	/// Exposes <see cref="DataPresenterBase"/> types to UI Automation
	/// </summary>
	public class DataPresenterBaseAutomationPeer : FrameworkElementAutomationPeer
	{
		#region Member Variables

		// AS 7/26/11 TFS80926
		private static readonly SimpleValueHolder<bool> _processVisualTreeOnlyState = new SimpleValueHolder<bool>(false);
		private WeakList<AutomationPeer> _proxyPeerHostList;
		private PropertyValueTracker _processVisualTreeTracker;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="DataPresenterBaseAutomationPeer"/> class
		/// </summary>
		/// <param name="owner">The <see cref="DataPresenterBase"/> for which the peer is being created</param>
		public DataPresenterBaseAutomationPeer(DataPresenterBase owner)
			: base(owner)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Custom</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Custom;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="DataPresenterBase"/>
		/// </summary>
		/// <returns>A string that contains 'DataPresenterBase'</returns>
		protected override string GetClassNameCore()
		{
			return "DataPresenterBase";
		}

		#endregion //GetClassNameCore

		#endregion //Base class overrides	

		#region Properties

		// AS 7/26/11 TFS80926
		#region ProcessVisualTreeOnly
		/// <summary>
		/// Determines if the automation peers for the records and cells should be limited to the peers for the elements that are part of the visual tree.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default the automation peers for list type controls will return peers that represent the items 
		/// in the collection which then delegate to the peer of the associated container element (when available). In this way 
		/// objects for which elements have not been created may be accessed and interacted with via the automation peers. In the 
		/// case of DataPresenter this includes objects such as the Records and Cells. However creating and returning these proxy 
		/// peers has some performance implications as the UI Automation Tree is larger and automation clients that enumerate the 
		/// available peers could take longer. Setting this property to true will avoid the creation of these proxy peers. Note 
		/// that could cause problems with the automation clients so you would need to test with the various automation clients 
		/// to ensure they are still able to perform their function.</p>
		/// </remarks>
		public static bool ProcessVisualTreeOnly
		{
			get { return _processVisualTreeOnlyState.Value; }
			set { _processVisualTreeOnlyState.Value = value; }
		}
		#endregion //ProcessVisualTreeOnly

		#endregion //Properties

		#region Methods

		#region Internal Methods

		// AS 7/26/11 TFS80926
		#region AddProxyPeerHost
		internal void AddProxyPeerHost(AutomationPeer peer)
		{
			if (_proxyPeerHostList == null)
			{
				_proxyPeerHostList = new WeakList<AutomationPeer>();
				_processVisualTreeTracker = new PropertyValueTracker(_processVisualTreeOnlyState, "Value", OnProcessVisualTreeOnlyChanged);
			}

			_proxyPeerHostList.Add(peer);
		}

		internal static void AddProxyPeerHost(AutomationPeer peer, DataPresenterBase dp)
		{
			if (null != dp)
			{
				DataPresenterBaseAutomationPeer dpPeer = UIElementAutomationPeer.CreatePeerForElement(dp) as DataPresenterBaseAutomationPeer;

				if (null != dpPeer)
					dpPeer.AddProxyPeerHost(peer);
			}
		}
		#endregion //AddProxyPeerHost

		#endregion //Internal Methods

		#region Private Methods

		// AS 7/26/11 TFS80926
		#region OnProcessVisualTreeOnlyChanged
		private void OnProcessVisualTreeOnlyChanged()
		{
			Debug.Assert(this.CheckAccess(), "Property changed on other thread?");

			foreach (AutomationPeer peer in _proxyPeerHostList)
			{
				peer.ResetChildrenCache();
			}
		}
		#endregion //OnProcessVisualTreeOnlyChanged

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