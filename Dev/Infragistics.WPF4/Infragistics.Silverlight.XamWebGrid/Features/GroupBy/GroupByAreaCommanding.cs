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
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	#region XamGroupByAreaCommand

	/// <summary>
	/// An enumeration of available commands for the GroupByArea of the <see cref="XamGrid"/>.
	/// </summary>
	public enum XamGroupByAreaCommand
	{
		/// <summary>
		/// Toggles the expansion of the GroupByArea of the <see cref="XamGrid"/>
		/// </summary>
		ToggleExpansion,

		/// <summary>
		/// Expands the GroupByArea of the <see cref="XamGrid"/>
		/// </summary>
		Expand,

		/// <summary>
		/// Collapses the GroupByArea of the <see cref="XamGrid"/>
		/// </summary>
		Collapse
	}
	#endregion //XamGroupByAreaCommand

	#region XamGroupByAreaCommandSource
	/// <summary>
	/// The command source object for <see cref="GroupByAreaCellControl"/>.
	/// </summary>
	public class XamGroupByAreaCommandSource : CommandSource
	{
		#region Properties
		#region Public
		/// <summary>
		/// Gets / sets the <see cref="XamGroupByAreaCommand"/> which is to be executed by the command.
		/// </summary>
		public XamGroupByAreaCommand CommandType
		{
			get;
			set;
		}
		#endregion // Public
		#endregion // Properties

		#region Overrides

		#region Protected
		/// <summary>
		/// Generates the <see cref="ICommand"/> object that will execute the command.
		/// </summary>
		/// <returns></returns>
		protected override ICommand ResolveCommand()
		{
			ICommand command = null;

			switch (this.CommandType)
			{
				case XamGroupByAreaCommand.ToggleExpansion:
					{
						command = new ToggleGroupByAreaCommand();
						break;
					}
				case XamGroupByAreaCommand.Expand:
					{
						command = new ExpandGroupByAreaCommand();
						break;
					}
				case XamGroupByAreaCommand.Collapse:
					{
						command = new CollapseGroupByAreaCommand();
						break;
					}
			}
			return command;
		}
		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // XamGroupByAreaCommandSource
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