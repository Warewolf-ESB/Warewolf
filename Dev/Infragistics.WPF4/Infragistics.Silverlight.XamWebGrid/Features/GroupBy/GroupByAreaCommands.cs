using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Infragistics.Controls.Grids.Primitives
{
	#region GroupByAreaCommandBase

	/// <summary>
	/// A base class used to react to commands that relate to the GroupByArea of the <see cref="XamGrid"/>.
	/// </summary>
	public abstract class GroupByAreaCommandBase: CommandBase
	{
		#region Execute
		/// <summary>
		/// Executes the command with the specified parameter.
		/// </summary>
		/// <param propertyName="parameter"></param>
		public override void Execute(object parameter)
		{
			base.Execute(parameter);

			this.Execute((GroupByAreaCellControl)parameter);
		}
		#endregion // Execute

		#region CanExecute
		/// <summary>
		/// Determines if this command can be run.
		/// </summary>
		/// <param propertyName="parameter"></param>
		/// <returns></returns>
		public override bool CanExecute(object parameter)
		{
			return true;
		}
		#endregion // CanExecute

		#region Execute
		/// <summary>
		/// Executes the command on the specifed <see cref="GroupByAreaCellControl"/>.
		/// </summary>
		/// <param propertyName="groupByArea"></param>
		public abstract void Execute(GroupByAreaCellControl groupByArea);
		#endregion // Execute

	}
	#endregion // GroupByAreaCommandBase

	#region ToggleGroupByAreaCommand

	/// <summary>
	/// A GroupByArea command that toggles the expansion of a <see cref="GroupByAreaCellControl"/>.
	/// </summary>
	public class ToggleGroupByAreaCommand : GroupByAreaCommandBase
	{
		#region Execute
		/// <summary>
		/// Toggles the expansion on the specifed <see cref="GroupByAreaCellControl"/>.
		/// </summary>
		/// <param propertyName="groupByArea"></param>
		public override void Execute(GroupByAreaCellControl groupByArea)
		{
			groupByArea.IsExpanded = !groupByArea.IsExpanded;
		}
		#endregion // Execute
	}

	#endregion // ToggleGroupByAreaCommand

	#region ExpandGroupByAreaCommand

	/// <summary>
	/// A GroupByArea command that expands a <see cref="GroupByAreaCellControl"/>.
	/// </summary>
	public class ExpandGroupByAreaCommand : GroupByAreaCommandBase
	{
		#region Execute
		/// <summary>
		/// Expands the specifed <see cref="GroupByAreaCellControl"/>.
		/// </summary>
		/// <param propertyName="groupByArea"></param>
		public override void Execute(GroupByAreaCellControl groupByArea)
		{
			groupByArea.IsExpanded = true;
		}
		#endregion // Execute
	}

	#endregion // ExpandGroupByAreaCommand

	#region CollapseGroupByAreaCommand

	/// <summary>
	/// A GroupByArea command that collapses a <see cref="GroupByAreaCellControl"/>.
	/// </summary>
	public class CollapseGroupByAreaCommand : GroupByAreaCommandBase
	{
		#region Execute
		/// <summary>
		/// Collapses the specifed <see cref="GroupByAreaCellControl"/>.
		/// </summary>
		/// <param propertyName="groupByArea"></param>
		public override void Execute(GroupByAreaCellControl groupByArea)
		{
			groupByArea.IsExpanded = false;
		}
		#endregion // Execute
	}

	#endregion // CollapseGroupByAreaCommand
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