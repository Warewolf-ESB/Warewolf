using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.DataPresenter;
using System.Windows.Automation.Provider;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	// JM 08-20-09 NA 9.2 EnhancedGridView
	/// <summary>
	/// Interface implemented by AutomationPeers that support a list of <see cref="Record"/>s.
	/// </summary>
	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_EnhancedGridView, Version = FeatureInfo.Version_9_2)]
	public interface IRecordListAutomationPeer
	{
		//Properties
		//
		/// <summary>
		/// Returns true if the AutomationPeer represents an object that is using horizontal row layout.
		/// </summary>
		bool			IsHorizontalRowLayout	{ get; }
		
		/// <summary>
		/// Returns true if the AutomationPeer represents a root level list of <see cref="Record"/>s.
		/// </summary>
		bool			IsRootLevel { get; }

		// Methods
		//
		/// <summary>
		/// Creates and returns a <see cref="RecordAutomationPeer"/> for the specified <see cref="Record"/>.
		/// </summary>
		/// <param name="record">The <see cref="Record"/> for which to create the <see cref="RecordAutomationPeer"/></param>
		/// <returns>A <see cref="RecordAutomationPeer"/> for the specified <see cref="Record"/>.</returns>
		RecordAutomationPeer CreateAutomationPeer(Record record);

		/// <summary>
		/// Creates and returns a <see cref="HeaderAutomationPeer"/> for the specified <see cref="Record"/>.
		/// </summary>
		/// <param name="record">The <see cref="Record"/> for which to create the <see cref="HeaderAutomationPeer"/></param>
		/// <returns>A <see cref="HeaderAutomationPeer"/> for the specified <see cref="Record"/>.</returns>
		HeaderAutomationPeer GetHeaderPeer(Record record);

		/// <summary>
		/// Returns whether the AutomationPeer represents an object that supports the Grid pattern.
		/// </summary>
		/// <returns>True if the object represented by the AutomationPeer supports the Grid pattern, otherwise false.</returns>
		bool				 CouldSupportGridPattern();

		/// <summary>
		/// Returns the <see cref="RecordCollectionBase"/> associated with the AutomationPeer
		/// </summary>
		/// <returns></returns>
		RecordCollectionBase GetRecordCollection();

		/// <summary>
		/// Returns an IList of <see cref="Record"/> objects contained in the object represented by the AutomationPeer.
		/// </summary>
		IList<Record>		GetRecordList();

		/// <summary>
		/// Returns the index of the table row for the specified <see cref="Cell"/>.
		/// </summary>
		/// <param name="cell">The <see cref="Cell"/> for which to retrieve the table row index.</param>
		/// <returns>The index of the row containing the specified <see cref="Cell"/>.</returns>
		int					GetTableRowIndex(Cell cell);

		/// <summary>
		/// Returns an AutomationPeer for the grid that contains the specified <see cref="Cell"/>.
		/// </summary>
		/// <param name="cell">The <see cref="Cell"/> for which to return the containing grid AutomationPeer.</param>
		/// <returns>An AutomationPeer for the grid that contains the specified <see cref="Cell"/>.</returns>
		AutomationPeer		GetContainingGrid(Cell cell);

		/// <summary>
		/// Returns provider for the specified peer.
		/// </summary>
		/// <param name="peer">The peer for which to return the provider.</param>
		/// <returns>The provider for the specified peer.</returns>
		IRawElementProviderSimple ProviderFromPeer(AutomationPeer peer);
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