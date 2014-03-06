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
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{
	#region XamWebGridPagingCommands
	/// <summary>
	/// Enum describing commands available on the filter control
	/// </summary>
	public enum XamGridFilteringCommand
	{
		/// <summary>
		/// Removes filters for the given column
		/// </summary>
		ClearFilters
	}
	#endregion //XamWebGridColumnCommands

	#region XamGridFilteringCommandSource
	/// <summary>
	/// The command source object for <see cref="FilterControl"/>.
	/// </summary>
	public class XamGridFilteringCommandSource : CommandSource
	{
		#region Properties

		#region Public

		/// <summary>
		/// The type of command that will be executed.
		/// </summary>
		public XamGridFilteringCommand CommandType
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
				case (XamGridFilteringCommand.ClearFilters):
					command = new ClearFilters();
					break;
			}
			return command;
		}

		#endregion // Protected
		#endregion // Methods
	}
	#endregion // XamGridFilteringCommandSource

    /// <summary>
    /// The command source object for <see cref="FilterSelectionControl"/>.
    /// </summary>
    public class XamGridFilterMenuCommandSource : CommandSource
    {
        #region CommandType

        /// <summary>
        /// Identifies the <see cref="CommandType"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CommandTypeProperty = DependencyProperty.Register("CommandType", typeof(List<FilterOperand>), typeof(XamGridFilterMenuCommandSource), null);

        /// <summary>
        /// The type of command that will be executed.
        /// </summary>
        public List<FilterOperand> CommandType
        {
            get { return (List<FilterOperand>)this.GetValue(CommandTypeProperty); }
            set { this.SetValue(CommandTypeProperty, value); }
        }

        #endregion // CommandType

        #region Methods
        /// <summary>
        /// Generates the <see cref="ICommand"/> object that will execute the command.
        /// </summary>
        /// <returns></returns>
        protected override ICommand ResolveCommand()
        {
            return new FilterMenuCommands();
        }
        #endregion // Methods
    }

    /// <summary>
    /// The command source object for <see cref="FilterTextBox"/>.
    /// </summary>
    public class XamGridFilterMenuFilterTextBoxCommandSource : CommandSource
    {
        #region FilterSelectionControl

        /// <summary>
        /// Identifies the <see cref="CommandType"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty FilterSelectionControlProperty = DependencyProperty.Register("FilterSelectionControl", typeof(FilterSelectionControl), typeof(XamGridFilterMenuFilterTextBoxCommandSource), null);

        /// <summary>
        /// The type of command that will be executed.
        /// </summary>
        public FilterSelectionControl FilterSelectionControl
        {
            get { return (FilterSelectionControl)this.GetValue(FilterSelectionControlProperty); }
            set { this.SetValue(FilterSelectionControlProperty, value); }
        }

        #endregion // CommandType

        #region CommandType

        /// <summary>
        /// Identifies the <see cref="CommandType"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CommandTypeProperty = DependencyProperty.Register("CommandType", typeof(FilterTextBoxCommand), typeof(XamGridFilterMenuFilterTextBoxCommandSource), new PropertyMetadata(new PropertyChangedCallback(CommandTypeChanged)));

        /// <summary>
        /// The type of command that will be executed.
        /// </summary>
        public FilterTextBoxCommand CommandType
        {
            get { return (FilterTextBoxCommand)this.GetValue(CommandTypeProperty); }
            set { this.SetValue(CommandTypeProperty, value); }
        }

        private static void CommandTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamGridFilterMenuFilterTextBoxCommandSource ctrl = (XamGridFilterMenuFilterTextBoxCommandSource)obj;
            ctrl.OnPropertyChanged("CommandType");
        }

        #endregion // CommandType 				

        #region Methods
        /// <summary>
        /// Generates the <see cref="ICommand"/> object that will execute the command.
        /// </summary>
        /// <returns></returns>
        protected override ICommand ResolveCommand()
        {
            switch (this.CommandType)
            {
                case( FilterTextBoxCommand.Filter):
                    return new FilterMenuFilterTextBoxFilterTextCommand();
                case (FilterTextBoxCommand.ClearFilterText):
                    return new FilterMenuFilterTextBoxClearFilterTextCommand();
            }

            return null;
        }
        #endregion // Methods
    }
}

namespace Infragistics.Controls.Grids.Primitives
{
    #region FilterTextBoxCommands

    /// <summary>
    /// Enum describing commands available on the filter text box.
    /// </summary>
    public enum FilterTextBoxCommand
    {
        /// <summary>
        /// Filter the data.
        /// </summary>
        Filter,

        /// <summary>
        /// Clear the filter text box.
        /// </summary>
        ClearFilterText
    }

    #endregion // FilterTextBoxCommands

    #region RowFilteringCommandBase
    /// <summary>
    /// Base class for all commands that deal with a <see cref="Column"/>.
    /// </summary>
    public abstract class RowFilteringCommandBase : CommandBase
    {
        #region Overrides

        #region Public

        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Execute

        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param propertyName="parameter">The <see cref="CellBase"/> object that will be executed against.</param>
        public override void Execute(object parameter)
        {
            FilterRowCell filterRowCell = parameter as FilterRowCell;
            if (filterRowCell != null)
            {
                this.ExecuteCommand(filterRowCell);
                this.CommandSource.Handled = true;
            }
            base.Execute(parameter);
        }
        #endregion // Execute

        #endregion // Public

        #region Protected

        /// <summary>
        /// Executes the specific command on the specified <see cref="FilterRowCell"/>
        /// </summary>
        /// <param name="filterRowCell"></param>
        protected abstract void ExecuteCommand(FilterRowCell filterRowCell);

        #endregion // Protected

        #endregion // Overrides
    }
    #endregion // RowFilteringCommandBase

    #region ClearFilters
    /// <summary>
    /// A Command which will clear the filters on a FilterRowCell
    /// </summary>
    public class ClearFilters : RowFilteringCommandBase
    {
        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return parameter is FilterRowCell;
        }
        #endregion // CanExecute

        #region ExecuteCommand
        /// <summary>
        /// Executes the specific command on the specified <see cref="FilterRowCell"/>
        /// </summary>
        /// <param name="filterRowCell"></param>
        protected override void ExecuteCommand(FilterRowCell filterRowCell)
        {
            if (filterRowCell != null)
            {
                FilterRow fr = filterRowCell.Row as FilterRow;
                if (fr != null)
                {
                    fr.RemoveFilters(filterRowCell);
                }
            }
        }
        #endregion // ExecuteCommand
    }
    #endregion // ClearFilters

    #region FilterMenuCommands
    /// <summary>
    /// A Command which allows control over the filter menu.
    /// </summary>
    public class FilterMenuCommands : CommandBase
    {
        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
        #endregion // CanExecute

        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            XamGridFilterMenuCommandSource source = this.CommandSource as XamGridFilterMenuCommandSource;

            if (source.CommandType != null && source.CommandType.Count > 0)
            {
                this.ExcecuteCommand(source.CommandType as List<FilterOperand>, parameter as CellBase);
            }
        }
        #endregion // Execute

        #region ExecuteCommand
        /// <summary>
        /// Executes the specific command on the specified <see cref="CellBase"/>
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="filterOperand"></param>
        protected virtual void ExcecuteCommand(List<FilterOperand> filterOperand, CellBase cell)
        {
            if (filterOperand != null && cell != null && filterOperand.Count == 1 && !filterOperand[0].RequiresFilteringInput)
            {
                RowsManager rm = (RowsManager)cell.Row.Manager;
                cell.Row.ColumnLayout.BuildFilters(rm.RowFiltersCollectionResolved, null, cell.Column, filterOperand[0], false, true);
                cell.Row.ColumnLayout.Grid.OnFiltered(rm.RowFiltersCollectionResolved);
            }
        }
        #endregion //ExecuteCommand
    }
    #endregion // FilterMenuCommands

    #region FilterMenuFilterTextBoxCommand
    /// <summary>
    /// A command to be used on the textbox that will filter the filter menu options
    /// </summary>
    public class FilterMenuFilterTextBoxCommand : CommandBase
    {
        #region Properties

        #region FilterSelectionControl
        /// <summary>
        /// Get / sets the <see cref="FilterSelectionControl"/> which this control is associate with.
        /// </summary>
        protected FilterSelectionControl FilterSelectionControl { get; set; }
        #endregion // FilterSelectionControl

        #endregion // Properties

        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
        #endregion // CanExecute

        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            FilterSelectionControl fsc = ((XamGridFilterMenuFilterTextBoxCommandSource)this.CommandSource).FilterSelectionControl;
            if (fsc != null)
            {
                this.FilterSelectionControl = fsc;

                this.ExecuteCommand(parameter);
            }
        }
        #endregion // Execute

        #region ExcuteCommand

        /// <summary>
        /// Executes the given command.
        /// </summary>
        /// <param name="parameter"></param>
        protected virtual void ExecuteCommand(object parameter)
        {

        }
        #endregion // ExcuteCommand
    }
    #endregion // FilterMenuFilterTextBoxCommand

    #region FilterMenuFilterTextBoxFilterTextCommand

    /// <summary>
    /// A command to be used by the filter text box to filter the text.
    /// </summary>
    public class FilterMenuFilterTextBoxFilterTextCommand : FilterMenuFilterTextBoxCommand
    {
        /// <summary>
        /// Executes the given command.
        /// </summary>
        /// <param name="parameter"></param>
        protected override void ExecuteCommand(object parameter)
        {
            TextBox tb = (TextBox)parameter;

            this.FilterSelectionControl.FilterUniqueValue(tb.Text);
        }
    }
    #endregion // FilterMenuFilterTextBoxFilterTextCommand

    #region FilterMenuFilterTextBoxClearFilterTextCommand
    /// <summary>
    /// A command to be used by the filter text box to set the filter text to string.Empty.
    /// </summary>
    public class FilterMenuFilterTextBoxClearFilterTextCommand : FilterMenuFilterTextBoxCommand
    {
        /// <summary>
        /// Executes the given command.
        /// </summary>
        /// <param name="parameter"></param>
        protected override void ExecuteCommand(object parameter)
        {
            this.FilterSelectionControl.FilterText = string.Empty;

            this.FilterSelectionControl.FilterUniqueValue(string.Empty);
        }
    }
    #endregion // FilterMenuFilterTextBoxClearFilterTextCommand
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