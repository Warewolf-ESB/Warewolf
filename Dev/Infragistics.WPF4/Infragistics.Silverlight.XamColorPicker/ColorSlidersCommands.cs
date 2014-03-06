using System.Windows.Input;
using Infragistics.Controls.Editors.Primitives;
using System.Windows.Media;
using System;

namespace Infragistics.Controls.Editors
{
    #region ColorStripsCommand

    /// <summary>
    /// Enum that lists which commands are available for the <see cref="AdvancedColorShadePicker"/>.
    /// </summary>
    public enum ColorStripsCommand
    {
        /// <summary>
        /// Shows the RGB sliders.
        /// </summary>
        ShowRGBSliders,
        /// <summary>
        /// Shows the HSL sliders.
        /// </summary>
        ShowHSLSliders,
        /// <summary>
        /// Shows the CMYK sliders.
        /// </summary>
        ShowCMYKSliders,
        /// <summary>
        /// Accepts the CurrentColor and sets the SelectedColor to the CurrentColor.
        /// </summary>
        Accept,
        /// <summary>
        /// Cancels the CurrentColor and resets it to the SelectedColor
        /// </summary>
        Cancel
    }
    #endregion // ColorStripsCommand

    #region ColorStripsCommandSource

    /// <summary>
    /// An object that describes what kind of Command should be attached to a <see cref="AdvancedColorShadePicker"/> object, and what should trigger the command.
    /// </summary>
    public class ColorStripsCommandSource : CommandSource
    {
        #region Properties

        #region Public

        /// <summary>
        /// The type of command that will be executed.
        /// </summary>
        public ColorStripsCommand CommandType
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
                case (ColorStripsCommand.ShowRGBSliders):
                    command = new RGBCommand();
                    break;
                case (ColorStripsCommand.ShowHSLSliders):
                    command = new HSLCommand();
                    break;
                case (ColorStripsCommand.ShowCMYKSliders):
                    command = new CMYKCommand();
                    break;
                case (ColorStripsCommand.Accept):
                    command = new AcceptColorCommand();
                    break;
                case (ColorStripsCommand.Cancel):
                    command = new CancelColorCommand();
                    break;

            }
            return command;
        }

        #endregion // Protected
        #endregion // Methods
    }

    #endregion // ColorStripsCommandSource
}

namespace Infragistics.Controls.Editors.Primitives
{
    #region ColorSlidersCommandBase

    /// <summary>
    /// An abstract base class for other <see cref="AdvancedColorShadePicker"/> commands.
    /// </summary>
    public abstract class ColorSlidersCommandBase : CommandBase
    {
        #region CanExecute
        /// <summary>
        /// Returns True if the command can run at this time.
        /// </summary>
        /// <param name="parameter">The object where the command originated.</param>
        /// <returns>True if the command is executable.</returns>
        public override bool CanExecute(object parameter)
        {
            return parameter is AdvancedColorShadePicker;
        }
        #endregion // CanExecute
    }

    #endregion // ColorSlidersCommandBase

    #region RGBCommand

    /// <summary>
    /// A <see cref="ColorSlidersCommandBase"/> which will show the RBG sliders.
    /// </summary>
    public class RGBCommand : ColorSlidersCommandBase
    {
        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            AdvancedColorShadePicker picker = parameter as AdvancedColorShadePicker;
            if (picker != null)
            {
                picker.ColorSliderView = ColorSliderView.RGB;
            }
        }
        #endregion // Execute
    }

    #endregion // RGBCommand

    #region HSLCommand

    /// <summary>
    /// A <see cref="ColorSlidersCommandBase"/> which will show the HSL sliders.
    /// </summary>
    public class HSLCommand : ColorSlidersCommandBase
    {
        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            AdvancedColorShadePicker picker = parameter as AdvancedColorShadePicker;
            if (picker != null)
            {
                picker.ColorSliderView = ColorSliderView.HSL;
            }
        }
        #endregion // Execute
    }

    #endregion // HSLCommand

    #region CMYKCommand

    /// <summary>
    /// A <see cref="ColorSlidersCommandBase"/> which will show the CMYK sliders.
    /// </summary>
    public class CMYKCommand : ColorSlidersCommandBase
    {
        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            AdvancedColorShadePicker picker = parameter as AdvancedColorShadePicker;
            if (picker != null)
            {
                picker.ColorSliderView = ColorSliderView.CMYK;
            }
        }
        #endregion // Execute
    }

    #endregion // CMYKCommand

    #region AcceptColorCommand

    /// <summary>
    /// A <see cref="ColorSlidersCommandBase"/> which will set the SelectedColor from the CurrentColor.
    /// </summary>
    public class AcceptColorCommand : ColorSlidersCommandBase
    {
        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            AdvancedColorShadePicker picker = parameter as AdvancedColorShadePicker;
            if (picker != null)
            {
                picker.SelectedColor = picker.CurrentColor;
            }
        }
        #endregion // Execute
    }

    #endregion // AcceptColorCommand

    #region CancelColorCommand

    /// <summary>
    /// A <see cref="ColorSlidersCommandBase"/> which will refresh the SelectedColor.
    /// </summary>
    public class CancelColorCommand : ColorSlidersCommandBase
    {
        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            AdvancedColorShadePicker picker = parameter as AdvancedColorShadePicker;
            if (picker != null)
            {
                if (picker.SelectedColor != null)
                {
                    picker.CurrentColor = (Color)picker.SelectedColor;

                    picker.Dispatcher.Invoke((Action)picker.UpdateDisplayBoxes, null);


                }
                else
                {
                    picker.CurrentColor = Colors.Transparent;
                }

            }
        }
        #endregion // Execute
    }

    #endregion //CancelColorCommand
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