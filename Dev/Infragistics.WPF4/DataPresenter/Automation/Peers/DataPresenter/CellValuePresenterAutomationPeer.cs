using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.DataPresenter;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	/// <summary>
	/// Exposes <see cref="CellValuePresenter"/> types to UI Automation
	/// </summary>
	public class CellValuePresenterAutomationPeer : FrameworkElementAutomationPeer
	{
		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="CellValuePresenterAutomationPeer"/> class
		/// </summary>
		/// <param name="owner">The <see cref="CellValuePresenter"/> for which the peer is being created</param>
		public CellValuePresenterAutomationPeer(CellValuePresenter owner)
			: base(owner)
		{
			// when the automation peer is created, associate it with the cell
			// AS 6/19/12 TFS112736
			// It is possible that the Cell could be null. Also, we should only do this if the cell is within a record 
			// since we wouldn't want to hold the CVP for a tooltip, etc.
			//
			//((CellValuePresenter)owner).Cell.AssociatedCellValuePresenterInternal = owner;
			var cell = owner.Cell;

			if (null != cell && owner.IsWithinRecord)
				cell.AssociatedCellValuePresenterInternal = owner;
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
		/// Returns the name of the <see cref="CellValuePresenter"/>
		/// </summary>
		/// <returns>A string that contains 'CellValuePresenter'</returns>
		protected override string GetClassNameCore()
		{
			return "CellValuePresenter";
		}

		#endregion //GetClassNameCore

		#endregion //Base class overrides	
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