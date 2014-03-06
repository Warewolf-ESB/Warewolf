using System;
using System.Net;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Ink;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Animation;
//using System.Windows.Shapes;

namespace Infragistics.Undo
{
	/// <summary>
	/// Custom <see cref="UndoUnit"/> that takes a delegate that performs the execute operation.
	/// </summary>
	public class CustomUndoUnit : UndoUnit
	{
		#region Member Variables

		private object _target;
		private string _description;
		private string _detailedDescription;
		private Func<UndoExecuteContext, bool> _executeMethod; 

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CustomUndoUnit"/> that invokes the specified method when the operation in the undo/redo history is to be performed.
		/// </summary>
		/// <param name="description">The description for the transaction.</param>
		/// <param name="detailedDescription">The detailed description for the transaction.</param>
		/// <param name="executeMethod">The method to be invoked. The method returns a boolean indicating if the operation was successful.</param>
		/// <param name="target">The object that is the target of the function. This information is exposed via the <see cref="Target"/> property.</param>
		/// <exception cref="ArgumentNullException">The executeMethod parameter cannot be null.</exception>
		public CustomUndoUnit(string description, string detailedDescription, Func<UndoExecuteContext, bool> executeMethod, object target = null)
		{
			CoreUtilities.ValidateNotNull(executeMethod, "executeMethod");

			_executeMethod = executeMethod;
			_description = description;
			_detailedDescription = detailedDescription;
			_target = target;
		}
		#endregion //Constructor

		#region Base class overrides

		#region Execute
		/// <summary>
		/// Used to perform the associated action.
		/// </summary>
		/// <param name="executeInfo">Provides information about the undo/redo operation being executed.</param>
		/// <returns>Returns true if some action was taken. Otherwise false is returned. In either case the object was removed from the undo stack.</returns>
		internal protected override bool Execute(UndoExecuteContext executeInfo)
		{
			return _executeMethod(executeInfo);
		}
		#endregion //Execute

		#region GetDescription
		/// <summary>
		/// Returns a string representation of the action based on whether this is for an undo or redo operation.
		/// </summary>
		/// <param name="itemType">The type of history for which the description is being requested.</param>
		/// <param name="detailed">A boolean indicating if a detailed description should be returned. For example, when false one may return "Typing" but for verbose one may return "Typing 'qwerty'".</param>
		public override string GetDescription(UndoHistoryItemType itemType, bool detailed)
		{
			return detailed ? _detailedDescription : _description;
		}
		#endregion //GetDescription

		#region Target
		/// <summary>
		/// Returns the target object that will be affected by the <see cref="CustomUndoUnit"/>
		/// </summary>
		public override object Target
		{
			get { return _target; }
		}
		#endregion //Target

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