using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Interface implemented by objects that contain <see cref="ContentPane"/> instances
	/// </summary>
	internal interface IContentPaneContainer
	{
		/// <summary>
		/// Used to obtain a list of all the panes that should be affected within the container when the 
		/// pane action behavior is AllPanes and we are acting upon the specified pane.
		/// </summary>
		/// <param name="pane">The pane for which the AllPanes action is being applied.</param>
		/// <returns>A list of all the panes (including the specified pane) that should be affected</returns>
		IList<ContentPane> GetAllPanesForPaneAction(ContentPane pane);

		/// <summary>
		/// Removes a pane from the visual tree.
		/// </summary>
		/// <param name="pane">The pane to remove</param>
		/// <param name="replaceWithPlaceholder">True if the pane is only temporarily being removed</param>
		void RemoveContentPane(ContentPane pane, bool replaceWithPlaceholder);

		/// <summary>
		/// Used to add a pane to the specified container
		/// </summary>
		/// <param name="index">Index at which to insert the pane or null to just add it to the end or wherever the placeholder may be</param>
		/// <param name="pane">The pane to add</param>
		void InsertContentPane(int? index, ContentPane pane);

		/// <summary>
		/// Returns the location of the container
		/// </summary>
		PaneLocation PaneLocation { get; }

		/// <summary>
		/// Returns the element that represents the container or null if it doesn't have one
		/// </summary>
		FrameworkElement ContainerElement { get; }

		/// <summary>
		/// Returns a list of the currently visible <see cref="ContentPane"/> instances
		/// </summary>
		IList<ContentPane> GetVisiblePanes();
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