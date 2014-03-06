using System.Windows.Input;
using Infragistics.Controls.Editors.Primitives;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A <see cref="CommandBase"/> object that will decrease decrease its 
    /// ActiveThumb.Value property with LargeChange value. 
    /// </summary>
    public class LargeDecreaseCommand : XamSliderBaseCommandBase
    {
        /// <summary>
        /// Applies the decrease of the <see cref="XamSliderBase"/>  ActiveThumb.Value property with 
        /// LargeChange value
        /// </summary>
        /// <param name="slider">The <see cref="XamSliderBase"/> object.</param>
        protected override void ExecuteCommand(XamSliderBase slider)
        {
            slider.ProcessChanges(false, true, true);
        }
    }

    /// <summary>
    /// A <see cref="CommandBase"/> object that will increase decrease its 
    /// ActiveThumb.Value property  with LargeCjhange value. 
    /// </summary> 
    public class LargeIncreaseCommand : XamSliderBaseCommandBase
    {
        /// <summary>
        /// Applies the increase of the <see cref="XamSliderBase"/>  ActiveThumb.Value property with 
        ///  LargeChange value
        /// </summary>
        /// <param name="slider">The <see cref="XamSliderBase"/> object.</param>
        protected override void ExecuteCommand(XamSliderBase slider)
        {
            slider.ProcessChanges(true, true, true);
        }
    }

    /// <summary>
    /// A <see cref="CommandBase"/> object that will decrease its 
    /// ActiveThumb.Value property with SmallChange value. 
    /// </summary>
    public class SmallDecreaseCommand : XamSliderBaseCommandBase
    {
        /// <summary>
        /// Applies the decrease of the <see cref="XamSliderBase"/>  ActiveThumb.Value property with 
        /// SmallChange value
        /// </summary>
        /// <param name="slider">The <see cref="XamSliderBase"/> object.</param>
        protected override void ExecuteCommand(XamSliderBase slider)
        {
            slider.ProcessChanges(false, false, true);
        }
    }

    /// <summary>
    /// A <see cref="CommandBase"/> object that will increase decrease its 
    /// ActiveThumb.Value property with SmallChange value. 
    /// </summary> 
    public class SmallIncreaseCommand : XamSliderBaseCommandBase
    {
        /// <summary>
        /// Applies the increase of the <see cref="XamSliderBase"/>  ActiveThumb.Value property with 
        ///  SmallChange value
        /// </summary>
        /// <param name="slider">The <see cref="XamSliderBase"/> object.</param>
        protected override void ExecuteCommand(XamSliderBase slider)
        {
            slider.ProcessChanges(true, false, true);
        }
    }

    /// <summary>
    /// Base class for all commands that deal with a <see cref=" XamSliderBase"/>.
    /// </summary>
    public abstract class XamSliderBaseCommandBase : CommandBase
    {
        #region Overrides

        #region Public

        #region CanExecute

        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the command is executable.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Execute
        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">The <see cref="XamSliderBase"/> object that will be executed against.</param>
        public override void Execute(object parameter)
        {
            XamSliderBase slider = parameter as XamSliderBase;
            if (slider != null)
            {
                this.ExecuteCommand(slider);
                this.CommandSource.Handled = true;
            }

            base.Execute(parameter);
        }
        #endregion // Execute
        #endregion // Public

        #region Protected
        /// <summary>
        /// Executes the specific command on the specified <see cref="XamSliderBase"/>
        /// </summary>
        /// <param name="slider">The slider for which the command will be executed.</param>
        protected abstract void ExecuteCommand(XamSliderBase slider);
        #endregion // Protected

        #endregion // Overrides
    }
}

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// The command source object for <see cref="XamSliderBase"/> object.
    /// </summary>
    public class XamSliderBaseCommandSource : CommandSource
    {
        /// <summary>
        /// Gets or sets the XamSliderBaseCommand which is to be executed by the command.
        /// </summary>
        public XamSliderBaseCommand CommandType
        {
            get;
            set;
        }

        /// <summary>
        /// Resolves the <see cref="ICommand"/> that this <see cref="CommandSource"/> represents.
        /// </summary>
        /// <returns>The new instance of the resolved command</returns>
        protected override ICommand ResolveCommand()
        {
            switch (this.CommandType)
            {
                case XamSliderBaseCommand.LargeDecrease:
                    return new LargeDecreaseCommand();

                case XamSliderBaseCommand.LargeIncrease:
                    return new LargeIncreaseCommand();

                case XamSliderBaseCommand.SmallDecrease:
                    return new SmallDecreaseCommand();

                case XamSliderBaseCommand.SmallIncrease:
                    return new SmallIncreaseCommand();
            }

            return null;
        }
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