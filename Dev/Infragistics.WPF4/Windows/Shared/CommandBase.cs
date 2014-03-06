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
using Infragistics.Controls;

namespace Infragistics
{
	/// <summary>
	/// The base class for commands.
	/// </summary>
	public class CommandBase : ICommand
	{
		#region OnCanExecuteChanged
		/// <summary>
		/// Raises the CanExecuteChanged event.
		/// </summary>
		protected virtual void OnCanExecuteChanged()
		{
			if (this.CanExecuteChanged != null)
				this.CanExecuteChanged(this, EventArgs.Empty);
		}
		#endregion

		#region CommandSource
		/// <summary>
		/// The CommandSource object that originated the command.
		/// </summary>
		public CommandSource CommandSource { get; set; }
		#endregion

		#region ICommand Members
		/// <summary>
		/// Returns True if the command can run at this time.
		/// </summary>
		/// <param name="parameter">The object where the command originated.</param>
		/// <returns>True if the command is executable.</returns>
		public virtual bool CanExecute(object parameter)
		{
			return false;
		}

		/// <summary>
		/// Event handler for the CanExecuteChanged event.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Runs the command with the given parameter.
		/// </summary>
		/// <param name="parameter">An object containing any parameters for the command.</param>
		public virtual void Execute(object parameter)
		{
			
		}
		/// <summary>
		/// Runs the command with the given parameter.
		/// </summary>		
		/// <param name="source">The CommandSource that initiated the Command. </param>
		/// <param name="target">The object that is the target of the command.</param>
		public virtual void Execute(CommandSource source, ICommandTarget target)
		{
			
		}

		#endregion

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