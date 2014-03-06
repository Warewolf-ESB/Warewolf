using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infragistics.Undo
{
	/// <summary>
	/// Used to provide information about a potential merge operation to the <see cref="UndoUnit.Merge(UndoMergeContext)"/> method.
	/// </summary>
	public class UndoMergeContext
	{
		#region Member Variables

		private UndoUnit _unitBeingAdded;
		private UndoManager _undoManager; 

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UndoManager"/>
		/// </summary>
		/// <param name="undoManager">The UndoManager for which the unit is being added</param>
		/// <param name="unitBeingAdded">The unit being added</param>
		internal UndoMergeContext(UndoManager undoManager, UndoUnit unitBeingAdded)
		{
			CoreUtilities.ValidateNotNull(undoManager, "undoManager");
			CoreUtilities.ValidateNotNull(unitBeingAdded, "unitBeingAdded");

			_undoManager = undoManager;
			_unitBeingAdded = unitBeingAdded;
		} 
		#endregion //Constructor

		#region Properties

		#region UndoManager
		/// <summary>
		/// Returns the <see cref="UndoManager"/> for which the UndoUnit is being added.
		/// </summary>
		/// <seealso cref="UnitBeingAdded"/>
		public UndoManager UndoManager
		{
			get { return _undoManager; }
		}
		#endregion //UndoManager

		#region UnitBeingAdded
		/// <summary>
		/// Returns the <see cref="UndoUnit"/> that is being added.
		/// </summary>
		public UndoUnit UnitBeingAdded
		{
			get { return _unitBeingAdded; }
		}
		#endregion //UnitBeingAdded

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