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
	#region XamWebGridPagingCommands
	/// <summary>
	/// An enumeration of available commands for paging.
	/// </summary>
	public enum XamGridPagingCommand
	{
		/// <summary>
		/// Goes to the first page of data.		
		/// </summary>
		FirstPage,
		/// <summary>
		/// Goes to the previous page of data.		
		/// </summary>
		PreviousPage,
		/// <summary>
		/// Goes to the next page of data.
		/// </summary>
		NextPage,
		/// <summary>
		/// Goes to the last page of data.
		/// </summary>
		LastPage,
		/// <summary>
		/// Goes to a expressed page of data.
		/// </summary>
		GoToPage
	}
	#endregion //XamWebGridColumnCommands

	#region XamGridPagingCommandSource
	/// <summary>
	/// The command source object for <see cref="PagerCell"/> object.
	/// </summary>
	public class XamGridPagingCommandSource : CommandSource
	{
		#region Properties
		#region Public
		/// <summary>
		/// Gets / sets the <see cref="XamGridPagingCommand"/> which is to be executed by the command.
		/// </summary>
		public XamGridPagingCommand CommandType
		{
			get;
			set;
		}
		#endregion // Public
		#endregion // Properties

		#region Methods
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
				case (XamGridPagingCommand.FirstPage):
					{
						command = new FirstPageCommand();
						break;
					}
				case (XamGridPagingCommand.PreviousPage):
					{
						command = new PreviousPageCommand();
						break;
					}
				case (XamGridPagingCommand.NextPage):
					{
						command = new NextPageCommand();
						break;
					}

				case (XamGridPagingCommand.LastPage):
					{
						command = new LastPageCommand();
						break;
					}

				case (XamGridPagingCommand.GoToPage):
					{
						command = new GoToPageCommand();
						break;
					}

			}
			return command;
		}
		#endregion // Protected
		#endregion // Methods
	}
	#endregion // XamGridPagingCommandSource

	#region XamGridPagingControlsCommandSource
	/// <summary>
	/// A <see cref="CommandSource"/> for the controls which control the paging action.
	/// </summary>
	public class XamGridPagingControlsCommandSource : CommandSource
	{
		#region Properties
		#region Public
		/// <summary>
		/// Gets / sets the <see cref="XamGridPagingCommand"/> which is to be executed by the command.
		/// </summary>
		public XamGridPagingCommand CommandType
		{
			get;
			set;
		}
		#endregion // Public
		#endregion // Properties

		#region Methods
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
				case (XamGridPagingCommand.FirstPage):
					{
						command = new FirstPageControlCommand();
						break;
					}
				case (XamGridPagingCommand.PreviousPage):
					{
						command = new PreviousPageControlCommand();
						break;
					}

				case (XamGridPagingCommand.NextPage):
					{
						command = new NextPageControlCommand();
						break;
					}

				case (XamGridPagingCommand.LastPage):
					{
						command = new LastPageControlCommand();
						break;
					}
				case (XamGridPagingCommand.GoToPage):
					{
						command = new GoToPageControlCommand();
						break;
					}
			}
			return command;
		}
		#endregion // Protected
		#endregion // Methods
	}
	#endregion // XamGridPagingControlsCommandSource

}

namespace Infragistics.Controls.Grids.Primitives
{
	#region PagingBaseCommand
	/// <summary>
	/// Base class for PagingLocation commands which encapsulates shared logic.
	/// </summary>
	public abstract class PagingBaseCommand : CommandBase
	{
		#region Methods

		#region Public

		#region CanExecute
		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param propertyName="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			RowsManager rowsBase = parameter as RowsManager;
			if (rowsBase != null)
				return rowsBase != null && rowsBase.ColumnLayout.PagerSettings.AllowPagingResolved!=PagingLocation.None  && this.CanExecutePaging(rowsBase);

			Object[] paramArray = parameter as object[];
			if (paramArray != null && paramArray.Length > 1)
			{
				rowsBase = paramArray[0] as RowsManager;
				int rowIndex = int.Parse(paramArray[1].ToString(), System.Globalization.CultureInfo.InvariantCulture);
				return rowsBase != null && rowsBase.ColumnLayout.PagerSettings.AllowPagingResolved != PagingLocation.None && this.CanExecutePaging(rowsBase, rowIndex);
			}
			return false;
		}
		#endregion

		#region Execute
		/// <summary>
		/// Execute the command 
		/// </summary>
		/// <param propertyName="parameter">The <see cref="CellBase"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			RowsManager rowsBase = parameter as RowsManager;
			if (rowsBase != null)
			{
				this.ExecutePaging(rowsBase);
				this.CommandSource.Handled = true;
			}

			object[] paramArray = parameter as object[];
			if (paramArray != null)
			{
				this.ExecutePaging((RowsManager)paramArray[0], int.Parse(paramArray[1].ToString(), System.Globalization.CultureInfo.InvariantCulture));
				this.CommandSource.Handled = true;
			}
			base.Execute(parameter);
		}
		#endregion // Execute
		#endregion // Public

		#region Protected

		#region ExecutePaging
		/// <summary>
		/// Applies a paging action to the inputted <see cref="RowsManager"/>
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> which will be paged.</param>
		protected virtual void ExecutePaging(RowsManager rowsBase)
		{
		}
		/// <summary>
		/// Applies a paging action to the inputted <see cref="RowsManager"/>
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> which will be paged.</param>
		/// <param propertyName="pageIndex">The page index that the pager should navigate to.</param>
		protected virtual void ExecutePaging(RowsManager rowsBase, int pageIndex)
		{
		}
		#endregion //ExecutePaging

		#region CanExecutePaging
		/// <summary>
		/// Determines if the command can execute it's paging action
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> that contains the rows to be paged.</param>
		/// <returns>True if paging can can be executed.</returns>
		protected virtual bool CanExecutePaging(RowsManager rowsBase) { return false; }

		/// <summary>
		/// Determines if the command can execute it's paging action
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> that contains the rows to be paged.</param>
		/// <param propertyName="rowIndex">The page to navigate to.</param>
		/// <returns>True if paging can can be executed.</returns>
		protected virtual bool CanExecutePaging(RowsManager rowsBase, int rowIndex) { return false; }
		#endregion // CanExecutePaging

		#endregion // Protected

		#endregion // Methods
	}
	#endregion // PagingBaseCommand

	#region FirstPageCommand
	/// <summary>
	/// A <see cref="CommandBase"/> object which will move to the first page.
	/// </summary>
	public class FirstPageCommand : PagingBaseCommand
	{
		#region Methods
		#region Protected

		#region ExecutePaging
		/// <summary>
		/// Applies a paging action to the inputted <see cref="RowsManager"/> 
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> which will be paged.</param>
		protected override void ExecutePaging(RowsManager rowsBase)
		{
			if (rowsBase.CurrentPageIndex > 1)
			{
				rowsBase.CurrentPageIndex = 1;
			}
		}
		#endregion // ExecutePaging

		#region CanExecutePaging
		/// <summary>
		/// Determines if the command can execute it's paging action
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> that contains the rows to be paged.</param>
		/// <returns>True if paging can can be executed.</returns>
		protected override bool CanExecutePaging(RowsManager rowsBase)
		{
			return rowsBase.CurrentPageIndex != 1;
		}
		#endregion // CanExecutePaging

		#endregion // Protected
		#endregion // Methods
	}
	#endregion // FirstPageCommand

	#region PreviousPageCommand
	/// <summary>
	/// A <see cref="CommandBase"/> object which will move to the previous page.
	/// </summary>
	public class PreviousPageCommand : PagingBaseCommand
	{
		#region Methods
		#region Protected

		#region ExecutePaging
		/// <summary>
		/// Applies a paging action to the inputted <see cref="RowsManager"/>
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> which will be paged.</param>
		protected override void ExecutePaging(RowsManager rowsBase)
		{
			if (rowsBase.CurrentPageIndex > 1)
			{
				rowsBase.CurrentPageIndex--;
			}
		}
		#endregion // ExecutePaging

		#region CanExecutePaging
		/// <summary>
		/// Determines if the command can execute it's paging action
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> that contains the rows to be paged.</param>
		/// <returns>True if paging can can be executed.</returns>
		protected override bool CanExecutePaging(RowsManager rowsBase)
		{
			return rowsBase.CurrentPageIndex != 1;
		}
		#endregion // CanExecutePaging

		#endregion // Protected
		#endregion // Methods
	}
	#endregion // PreviousPageCommand

	#region LastPageCommand
	/// <summary>
	/// A <see cref="CommandBase"/> object which will move to the last page.
	/// </summary>
	public class LastPageCommand : PagingBaseCommand
	{
		#region Methods
		#region Protected

		#region ExecutePaging
		/// <summary>
		/// Applies a paging action to the inputted <see cref="RowsManager"/>
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> which will be paged.</param>
		protected override void ExecutePaging(RowsManager rowsBase)
		{
			if (rowsBase.CurrentPageIndex < rowsBase.PageCount)
			{
				rowsBase.CurrentPageIndex = rowsBase.PageCount;
			}
		}
		#endregion // ExecutePaging

		#region CanExecutePaging
		/// <summary>
		/// Determines if the command can execute it's paging action
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> that contains the rows to be paged.</param>
		/// <returns>True if paging can can be executed.</returns>
		protected override bool CanExecutePaging(RowsManager rowsBase)
		{
			return rowsBase.CurrentPageIndex < rowsBase.PageCount;
		}
		#endregion // CanExecutePaging

		#endregion // Protected
		#endregion // Methods
	}
	#endregion // LastPageCommand

	#region NextPageCommand
	/// <summary>
	/// A <see cref="CommandBase"/> object which will move to the next page.
	/// </summary>
	public class NextPageCommand : PagingBaseCommand
	{
		#region Methods
		#region Protected

		#region ExecutePaging
		/// <summary>
		/// Applies a paging action to the inputted <see cref="RowsManager"/>
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> which will be paged.</param>
		protected override void ExecutePaging(RowsManager rowsBase)
		{
			if (rowsBase.CurrentPageIndex < rowsBase.PageCount)
			{
				rowsBase.CurrentPageIndex++;
			}
		}
		#endregion // ExecutePaging

		#region CanExecutePaging
		/// <summary>
		/// Determines if the command can execute it's paging action
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> that contains the rows to be paged.</param>
		/// <returns>True if paging can can be executed.</returns>
		protected override bool CanExecutePaging(RowsManager rowsBase)
		{
			return rowsBase.CurrentPageIndex < rowsBase.PageCount;
		}
		#endregion // CanExecutePaging

		#endregion // Protected
		#endregion // Methods
	}
	#endregion // NextPageCommand

	#region GoToPageCommand
	/// <summary>
	/// A <see cref="CommandBase"/> object which will move to a page determined by the parameter.
	/// </summary>
	public class GoToPageCommand : PagingBaseCommand
	{
		#region Methods
		#region Protected
		#region ExecutePaging
		/// <summary>
		/// Applies a paging action to the inputted <see cref="RowsManager"/>
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> which will be paged.</param>
		/// <param propertyName="pageIndex">The page index that the pager should navigate to.</param>
		protected override void ExecutePaging(RowsManager rowsBase, int pageIndex)
		{
			if (rowsBase.PageCount >= pageIndex && rowsBase.CurrentPageIndex != pageIndex)
			{
				rowsBase.CurrentPageIndex = pageIndex;
			}
		}
		#endregion // ExecutePaging

		#region CanExecutePaging
		/// <summary>
		/// Determines if the command can execute it's paging action
		/// </summary>
		/// <param propertyName="rowsBase">The <see cref="RowsManager"/> that contains the rows to be paged.</param>
		/// <param propertyName="rowIndex">The page to navigate to.</param>
		/// <returns>True if paging can can be executed.</returns>
		protected override bool CanExecutePaging(RowsManager rowsBase, int rowIndex)
		{
			return rowsBase.CurrentPageIndex != rowIndex && rowIndex > 0 && rowsBase.PageCount >= rowIndex;
		}
		#endregion // CanExecutePaging
		#endregion // Protected
		#endregion // Methods
	}
	#endregion // GoToPageCommand

	#region PagingControlCommandBase
	/// <summary>
	/// Base class for PagingControls 
	/// </summary>
	public class PagingControlCommandBase : CommandBase
	{
		#region Methods

		#region CanExecute
		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param propertyName="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			PagerControlBase pager = parameter as PagerControlBase;
			if (pager != null)
				return this.CanExecutePagingControl(pager);

			object[] paramArray = parameter as object[];
			if (paramArray != null)
			{
				this.CanExecutePagingControl((PagerControlBase)paramArray[0], int.Parse(paramArray[1].ToString(), System.Globalization.CultureInfo.InvariantCulture));
			}

			return true;
		}
		#endregion // CanExecute

		#region Execute
		/// <summary>
		/// Execute the command 
		/// </summary>
		/// <param propertyName="parameter">The <see cref="CellBase"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			PagerControlBase pcb = parameter as PagerControlBase;

			if (pcb != null)
			{
				this.ExecutePagingCommand(pcb);
				this.CommandSource.Handled = true;
			}

			object[] paramArray = parameter as object[];
			if (paramArray != null)
			{
				this.ExecutePagingCommand((PagerControlBase)paramArray[0], int.Parse(paramArray[1].ToString(), System.Globalization.CultureInfo.InvariantCulture));
				this.CommandSource.Handled = true;
			}
		}
		#endregion // Execute

		#region ExecutePagingCommand
		/// <summary>
		/// Applies the paging command to the <see cref="PagerControlBase"/>
		/// </summary>
		/// <param propertyName="pager"></param>
		protected virtual void ExecutePagingCommand(PagerControlBase pager)
		{
		}

		/// <summary>
		/// Applies the paging command to the <see cref="PagerControlBase"/>
		/// </summary>
		/// <param propertyName="pager"></param>
		/// <param propertyName="rowIndex"></param>
		protected virtual void ExecutePagingCommand(PagerControlBase pager, int rowIndex)
		{
		}
		#endregion // ExecutePagingCommand

		#region CanExecutePagingCommand
		/// <summary>
		/// Determines if the <see cref="PagerControlBase"/> object would allow this operation to execute.
		/// </summary>
		/// <param propertyName="pager"></param>
		/// <returns></returns>
		protected virtual bool CanExecutePagingControl(PagerControlBase pager)
		{
			return true;
		}

		/// <summary>
		/// Determines if the <see cref="PagerControlBase"/> object would allow this operation to execute.
		/// </summary>
		/// <param propertyName="pager"></param>
		/// <param propertyName="nextPage"></param>
		/// <returns></returns>
		protected virtual bool CanExecutePagingControl(PagerControlBase pager, int nextPage)
		{
			return true;
		}
		#endregion // CanExecutePagingCommand

		#endregion // Methods
	}
	#endregion // PagingControlCommandBase

	#region FirstPageControlCommand
	/// <summary>
	/// A <see cref="PagingControlCommandBase"/> which signals the <see cref="PagerControl"/> to navigate to the first page.
	/// </summary>
	public class FirstPageControlCommand : PagingControlCommandBase
	{
		#region CanExecutePagingCommand
		/// <summary>
		/// Determines if the <see cref="PagerControlBase"/> object would allow this operation to execute.
		/// </summary>
		/// <param propertyName="pager"></param>
		/// <returns></returns>
		protected override bool CanExecutePagingControl(PagerControlBase pager)
		{
			return pager.CurrentPageIndex > 1;
		}
		#endregion // CanExecutePagingCommand

		#region ExecutePagingCommand
		/// <summary>
		/// Applies the paging command to the <see cref="PagerControlBase"/>
		/// </summary>
		/// <param propertyName="pager"></param>
		protected override void ExecutePagingCommand(PagerControlBase pager)
		{
			pager.OnFirstPage();
		}
		#endregion // ExecutePagingCommand
	}
	#endregion // FirstPageControlCommand

	#region PreviousPageControlCommand
	/// <summary>
	/// A <see cref="PagingControlCommandBase"/> which signals the <see cref="PagerControl"/> to navigate to the previous page.
	/// </summary>
	public class PreviousPageControlCommand : PagingControlCommandBase
	{
		#region ExecutePagingCommand
		/// <summary>
		/// Applies the paging command to the <see cref="PagerControlBase"/>
		/// </summary>
		/// <param propertyName="pager"></param>
		protected override void ExecutePagingCommand(PagerControlBase pager)
		{
			pager.OnPreviousPage();
		}
		#endregion // ExecutePagingCommand

		#region CanExecutePagingCommand
		/// <summary>
		/// Determines if the <see cref="PagerControlBase"/> object would allow this operation to execute.
		/// </summary>
		/// <param propertyName="pager"></param>
		/// <returns></returns>
		protected override bool CanExecutePagingControl(PagerControlBase pager)
		{
			return pager.CurrentPageIndex > 1;
		}
		#endregion // CanExecutePagingCommand
	}
	#endregion // PreviousPageControlCommand

	#region NextPageControlCommand
	/// <summary>
	/// A <see cref="PagingControlCommandBase"/> which signals the <see cref="PagerControl"/> to navigate to the next page.
	/// </summary>
	public class NextPageControlCommand : PagingControlCommandBase
	{
		#region ExecutePagingCommand
		/// <summary>
		/// Applies the paging command to the <see cref="PagerControlBase"/>
		/// </summary>
		/// <param propertyName="pager"></param>
		protected override void ExecutePagingCommand(PagerControlBase pager)
		{
			pager.OnNextPage();
		}
		#endregion // ExecutePagingCommand

		#region CanExecutePagingCommand
		/// <summary>
		/// Determines if the <see cref="PagerControlBase"/> object would allow this operation to execute.
		/// </summary>
		/// <param propertyName="pager"></param>
		/// <returns></returns>
		protected override bool CanExecutePagingControl(PagerControlBase pager)
		{
			return pager.CurrentPageIndex < pager.TotalPages;
		}
		#endregion // CanExecutePagingCommand
	}
	#endregion // NextPageControlCommand

	#region LastPageControlCommand
	/// <summary>
	/// A <see cref="PagingControlCommandBase"/> which signals the <see cref="PagerControl"/> to navigate to the last page.
	/// </summary>
	public class LastPageControlCommand : PagingControlCommandBase
	{
		#region CanExecutePagingCommand
		/// <summary>
		/// Determines if the <see cref="PagerControlBase"/> object would allow this operation to execute.
		/// </summary>
		/// <param propertyName="pager"></param>
		/// <returns></returns>
		protected override bool CanExecutePagingControl(PagerControlBase pager)
		{
			return pager.CurrentPageIndex < pager.TotalPages;
		}
		#endregion // CanExecutePagingCommand

		#region ExecutePagingCommand
		/// <summary>
		/// Applies the paging command to the <see cref="PagerControlBase"/>
		/// </summary>
		/// <param propertyName="pager"></param>
		protected override void ExecutePagingCommand(PagerControlBase pager)
		{
			pager.OnLastPage();
		}
		#endregion // ExecutePagingCommand
	}
	#endregion // LastPageControlCommand

	#region GoToPageControlCommand
	/// <summary>
	/// A <see cref="PagingControlCommandBase"/> which signals the <see cref="PagerControl"/> to navigate to a particular.
	/// </summary>
	public class GoToPageControlCommand : PagingControlCommandBase
	{
		#region ExecutePagingCommand
		/// <summary>
		/// Applies the paging command to the <see cref="PagerControlBase"/>
		/// </summary>
		/// <param propertyName="pager"></param>
		/// <param propertyName="rowIndex"></param>
		protected override void ExecutePagingCommand(PagerControlBase pager, int rowIndex)
		{
			pager.OnGoToPage(rowIndex);
		}
		#endregion // ExecutePagingCommand
	}
	#endregion // GoToPageControlCommand
	
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