using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Windows.Automation.Peers
{
	/// <summary>
	/// Interface implemented by AutomationPeers that support a list of items.
	/// </summary>
	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_EnhancedGridView, Version = FeatureInfo.Version_9_2)]
	public interface IListAutomationPeer
	{
		// Properties
		//
		/// <summary>
		/// Returns the <see cref="UIElement"/> that is the owner of the peer implementing this interface.
		/// </summary>
		UIElement			Owner { get; }

		/// <summary>
		/// Returns the <see cref="Panel"/> this is associated with the Owner of the peer implementating this interface, or null if there is no panel associated with the owner.
		/// </summary>
		Panel				ItemsControlPanel { get; }

		// Methods
		//
		/// <summary>
		/// Returns the underlying AutomationPeer for the specified item.
		/// </summary>
		/// <param name="item">The item for which to retrieve the underlying AutomationPeer.</param>
		/// <returns>The underlying AutomationPeer</returns>
		AutomationPeer		GetUnderlyingPeer(object item);

		/// <summary>
		/// Returns the container element for the specified item in the list associated with the peer implementing this interface.
		/// </summary>
		/// <param name="item">The item to return the container element for.</param>
		/// <returns>The container element for the specified item.</returns>
		DependencyObject	ContainerFromItem(object item);

		/// <summary>
		/// Returns the Pattern implementation for the specified PatternInterface as supplied by the peer implementing this interface.
		/// </summary>
		/// <param name="patternInterface">The PatternInterface for which the Pattern implementation is being requested.</param>
		/// <returns>The requested Pattern implementation or null if the peer does not implement the pattern.</returns>
		object				GetPattern(PatternInterface patternInterface);
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