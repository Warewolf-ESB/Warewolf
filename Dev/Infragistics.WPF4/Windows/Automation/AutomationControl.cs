using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Markup;
using Infragistics.Windows.Controls;
using Infragistics.Windows;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Helpers;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers;

namespace Infragistics.Windows.Controls
{
	#region AutomationControl Class

	// SSP 8/25/09 - NAS9.2 IDataErrorInfo support - TFS19773
	// Added AutomationControl class.
	// 

	/// <summary>
	/// Used for displaying drop indicator during a drag-and-drop operation.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>DropIndicator</b> control is used to display drop indicator during a drag-and-drop operation.
	/// For example, when a field in DataGrid is dragged and dropped to rearrange fields, this drop indicator
	/// will be displayed to indicate where the item will be dropped.
	/// </para>
	/// </remarks>
	//	[ToolboxItem(false)]  JM 08-27-09 TFS21522 - Added ToolboxBrowsableAttribute to the design assembly.
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class AutomationControl : Control
	{
		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public AutomationControl( )
		{
		}

		#endregion // Constructor

		#region Base Overrides

		/// <summary>
		/// Overridden. Creates an automation peer for this control.
		/// </summary>
		/// <returns>A new instance of AutomationPeer derived class.</returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer( )
		{
			return new AutomationControlPeer( this );
		}

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#endregion // Public Properties

		#endregion // Properties
	}

	#endregion // AutomationControl Class
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