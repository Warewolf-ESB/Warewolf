using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Layouts;
using System.Windows;
using System.Windows.Input;

namespace Infragistics.Controls.Layouts.Primitives
{
	#region TileCommand Class
	/// <summary>
	/// Class for all commands that deal with a <see cref="XamTile"/>.
	/// </summary>
	public class TileCommand : CommandBase
	{
		#region Private Members

		private TileCommandType _commandType;

		#endregion //Private Members

		#region Constructor

		internal TileCommand(TileCommandType commandTyoe)
		{
			_commandType = commandTyoe;
		}

		#endregion //Constructor

		#region Overrides

		#region Public

		#region CanExecute
		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			XamTile tile = GetTile(parameter);
			if (tile != null)
			{
				CommandSource source = this.CommandSource;

				return tile.CanExecuteCommand(_commandType, source.SourceElement);
			}

			return base.CanExecute(parameter);
		}
		#endregion

		#region Execute
		/// <summary>
		/// Executes the command 
		/// </summary>
		/// <param name="parameter">The <see cref="XamTile"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			XamTile tile = GetTile(parameter);
			if (tile != null)
			{
				CommandSource source = this.CommandSource;

				tile.ExecuteCommand(_commandType, source.ParameterResolved, source.SourceElement);
				this.CommandSource.Handled = true;

				return;
			}

			base.Execute(parameter);
		}

		private static XamTile GetTile(object parameter)
		{
			XamTile tile = parameter as XamTile;
			if (tile == null)
			{
				DependencyObject target = parameter as DependencyObject;

				if (target != null)
					tile = PresentationUtilities.GetVisualAncestor<XamTile>(target, null, null);

			}
			return tile;
		}

		#endregion Execute

		#endregion // Public

		#endregion // Overrides
	}
	#endregion // TileCommandBase Class

	#region TileCommandSource Class
	/// <summary>
	/// The command source object for <see cref="TileCommand"/> object.
	/// </summary>
	public class TileCommandSource : CommandSource
	{
		/// <summary>
		/// Gets or sets the TileCommand which is to be executed by the command.
		/// </summary>
		public TileCommandType CommandType
		{
			get;
			set;
		}

		/// <summary>
		/// Generates the <see cref="ICommand"/> object that will execute the command.
		/// </summary>
		/// <returns></returns>
		protected override ICommand ResolveCommand()
		{
			return new TileCommand(this.CommandType);
		}
	}

	#endregion //TileCommandSource Class
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