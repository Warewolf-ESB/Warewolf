using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.DataPresenter;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	/// <summary>
	/// Exposes <see cref="RecordPresenter"/> types to UI Automation
	/// </summary>
	public class RecordPresenterAutomationPeer : FrameworkElementAutomationPeer
	{
		#region Member Variables

		// AS 1/21/10 TFS26545
		private List<AutomationPeer> _lastPeers; 

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="RecordPresenterAutomationPeer"/> class
		/// </summary>
		/// <param name="owner">The <see cref="RecordPresenter"/> for which the peer is being created</param>
		public RecordPresenterAutomationPeer(RecordPresenter owner)
			: base(owner)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>DataItem</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.DataItem;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="RecordPresenter"/>
		/// </summary>
		/// <returns>A string that contains 'RecordPresenter'</returns>
		protected override string GetClassNameCore()
		{
			return "RecordPresenter";
		}

		#endregion //GetClassNameCore

		// AS 1/21/10 TFS26545
		#region GetChildrenCore
		/// <summary>
		/// Returns the collection of child elements of the <see cref="RecordPresenter"/> that is associated with this <see cref="RecordPresenterAutomationPeer"/>
		/// </summary>
		/// <returns>The collection of child elements</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			// Cache the actual children.
			_lastPeers = base.GetChildrenCore();

			// if the record is not the underlying peer for the record automation 
			// peer then we need to pretend this element has no children and let 
			// the record peer get the element's children to return as its own
			if (IsWithinRecordContainer)
				return new List<AutomationPeer>();

			return _lastPeers;
		} 
		#endregion //GetChildrenCore

		#region IsControlElementCore
		/// <summary>
		///  Gets or sets a value that indicates whether the UIElement that is associated with this peer 
		///  is understood by the end user as interactive.
		/// </summary>
		/// <returns>Return false if the element is within a container for the associated <see cref="ViewBase"/></returns>
		protected override bool IsControlElementCore()
		{
			// AS 1/21/10 TFS26545
			if (this.IsWithinRecordContainer)
				return false;

			return true;
		} 
		#endregion //IsControlElementCore

		#endregion //Base class overrides	

		#region Properties

		// AS 1/21/10 TFS26545
		#region IsWithinRecordContainer
		internal bool IsWithinRecordContainer
		{
			get
			{
				RecordPresenter rp = this.Owner as RecordPresenter;
				DataPresenterBase dp = rp.DataPresenter;

				return null != dp && dp.CurrentViewInternal.RecordPresenterContainerType != null;
			}
		}
		#endregion //IsWithinRecordContainer

		#endregion //Properties

		#region Methods

		// AS 1/21/10 TFS26545
		#region GetActualChildren
		internal List<AutomationPeer> GetActualChildren()
		{
			this.GetChildren();

			return _lastPeers;
		}
		#endregion //GetActualChildren 

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