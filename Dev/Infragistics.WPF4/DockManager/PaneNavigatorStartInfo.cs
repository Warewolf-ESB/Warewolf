using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Class used to define the start up information for the display of a <see cref="PaneNavigator"/>
	/// </summary>
	internal class PaneNavigatorStartInfo
	{
		#region Member Variables

		private bool _closeWhenModifiersReleased;
		private bool? _startWithDocuments = true;
		private int _activationOffset;

		#endregion //Member Variables

		#region PaneNavigatorStartInfo
		/// <summary>
		/// Initializes a new <see cref="PaneNavigatorStartInfo"/>
		/// </summary>
		public PaneNavigatorStartInfo()
		{
		}
		#endregion //PaneNavigatorStartInfo

		#region Properties
		/// <summary>
		/// Indicates if the navigator should be automatically closed when the modifier keys have been released.
		/// </summary>
		public bool CloseWhenModifiersRelease
		{
			get { return this._closeWhenModifiersReleased; }
			set { this._closeWhenModifiersReleased = value; }
		}

		/// <summary>
		/// Indicates if navigation should start with the documents.
		/// </summary>
		public bool? StartWithDocuments
		{
			get { return this._startWithDocuments; }
			set { this._startWithDocuments = value; }
		}

		/// <summary>
		/// Indicates the number of items by which to offset the initially selected item.
		/// </summary>
		public int ActivationOffset
		{
			get { return this._activationOffset; }
			set { this._activationOffset = value; }
		}
		#endregion //Properties
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