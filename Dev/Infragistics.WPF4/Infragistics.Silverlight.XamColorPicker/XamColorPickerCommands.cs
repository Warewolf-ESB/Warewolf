using System.Windows.Input;
using Infragistics.Controls.Editors.Primitives;
using System;

namespace Infragistics.Controls.Editors
{
    #region XamPickerCommand
    /// <summary>
    /// Enum that lists which commands are available for the <see cref="XamColorPicker"/>.
    /// </summary>
    public enum XamPickerCommand
    {
        /// <summary>
        /// Opens the <see cref="XamColorPicker"/> dropdown if closed, closes if open.
        /// </summary>
        Toggle,

        /// <summary>
        /// Closes the <see cref="XamColorPicker"/> dropdown.
        /// </summary>
        Close,

        /// <summary>
        /// Opens the advanced color editor.
        /// </summary>
        OpenAdvanceEditor,

        /// <summary>
        /// Moves to the next color palette if available.
        /// </summary>
        NextPalette,

        /// <summary>
        /// Moves to the previous color palette if available.
        /// </summary>
        PreviousPalette
    }
    #endregion // XamPickerCommand

    #region XamPickerCommandSource
    /// <summary>
    /// An object that describes what kind of Command should be attached to a <see cref="XamColorPicker"/> object, and what should trigger the command.
    /// </summary>
    public class XamPickerCommandSource : CommandSource
    {
        #region Properties

        #region Public

        /// <summary>
        /// The type of command that will be executed.
        /// </summary>
        public XamPickerCommand CommandType
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
                case (XamPickerCommand.Toggle):
                    command = new PickerToggleCommand();
                    break;
                case (XamPickerCommand.NextPalette):
                    command = new NextPaletteCommand();
                    break;
                case (XamPickerCommand.PreviousPalette):
                    command = new PreviousPaletteCommand();
                    break;
                case (XamPickerCommand.Close):
                    command = new PickerCloseCommand();
                    break;
                case (XamPickerCommand.OpenAdvanceEditor):
                    command = new OpenAdvanceEditorCommand();
                    break;
            }
            return command;
        }

        #endregion // Protected

        #endregion // Methods
    }
    #endregion // XamPickerCommandSource
}

namespace Infragistics.Controls.Editors.Primitives
{
    #region PickerCommandBase

    /// <summary>
    /// An abstract base class for other commands used by the <see cref="XamColorPicker"/>.
    /// </summary>
    public abstract class PickerCommandBase : CommandBase
    {
        #region CanExecute
        /// <summary>
        /// Returns True if the command can run at this time.
        /// </summary>
        /// <param name="parameter">The object where the command originated.</param>
        /// <returns>True if the command is executable.</returns>
        public override bool CanExecute(object parameter)
        {
            return parameter is XamColorPicker;
        }
        #endregion // CanExecute
    }

    #endregion // PickerCommandBase

    #region PickerToggleCommand

    /// <summary>
    /// A <see cref="PickerCommandBase"/> which will open the <see cref="XamColorPicker"/> dropdown if closed and close it if open.
    /// </summary>
    public class PickerToggleCommand : PickerCommandBase
    {
        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            XamColorPicker picker = parameter as XamColorPicker;
            if (picker != null)
            {
                picker.IsDropDownOpen = !picker.IsDropDownOpen;
            }
        }
        #endregion // Execute
    }

    #endregion // PickerToggleCommand

    #region PickerCloseCommand

    /// <summary>
    /// A <see cref="PickerCommandBase"/> which will close the <see cref="XamColorPicker"/> dropdown.
    /// </summary>
    public class PickerCloseCommand : PickerCommandBase
    {
        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            XamColorPicker picker = parameter as XamColorPicker;
            if (picker != null)
            {
                picker.IsDropDownOpen = false;
            }
        }
        #endregion // Execute
    }

    #endregion // PickerCloseCommand

    #region PreviousPaletteCommand

    /// <summary>
    /// A <see cref="PickerCommandBase"/> which will move to the previous <see cref="ColorPalette"/>.
    /// </summary>
    public class PreviousPaletteCommand : PickerCommandBase
    {      
        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            XamColorPicker picker = parameter as XamColorPicker;
            if (picker != null)
            {
                if (picker.CurrentPalette == null)
                {
                    if (picker.ColorPalettes.Count > 0)
                        picker.CurrentPalette = picker.ColorPalettes[0];
                }

                else
                {
                    int index = picker.ColorPalettes.IndexOf(picker.CurrentPalette);
                    if (index > 0)
                    {
                        picker.CurrentPalette = picker.ColorPalettes[--index];
                    }
                }
            }
        }
        #endregion // Execute
    }

    #endregion // PreviousPaletteCommand

    #region NextPaletteCommand

    /// <summary>
    /// A <see cref="PickerCommandBase"/> which will move to the next <see cref="ColorPalette"/>.
    /// </summary>
    public class NextPaletteCommand : PickerCommandBase
    {    
        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            XamColorPicker picker = parameter as XamColorPicker;
            if (picker != null)
            {
                if (picker.CurrentPalette == null)
                {
                    if (picker.ColorPalettes.Count > 0)
                        picker.CurrentPalette = picker.ColorPalettes[0];
                }

                else
                {
                    int index = picker.ColorPalettes.IndexOf(picker.CurrentPalette);
                    if (index < picker.ColorPalettes.Count - 1)
                    {
                        picker.CurrentPalette = picker.ColorPalettes[++index];
                    }
                }
            }
        }
        #endregion // Execute
    }

    #endregion // NextPaletteCommand

    #region OpenAdvanceEditorCommand

    /// <summary>
    /// A <see cref="PickerCommandBase"/> which will open the advanced editor.
    /// </summary>
    public class OpenAdvanceEditorCommand : PickerCommandBase
    {
        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            XamColorPicker picker = parameter as XamColorPicker;
            if (picker != null)
            {
                picker.SelectedColor = picker.ColorForAdvancedEditor;
                picker.ColorPickerDialog.IsOpen = true;
            }
        }
        #endregion // Execute
    }

    #endregion // OpenAdvanceEditorCommand
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