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
using Infragistics.Controls.Interactions.Primitives;
using System.ComponentModel;

namespace Infragistics.Controls.Interactions
{
    /// <summary>
    /// The control shows the progress while the dictionary is downloaded from uri location. 
    /// </summary>

    [DesignTimeVisible(false)]

    public class DictionaryLoadProgressDialog : ContentControl, ICommandTarget
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="XamSpellChecker"/> that is associated with this <see cref="DictionaryLoadProgressDialog"/>.
        /// </summary>
        public XamSpellChecker SpellChecker
        {
            get;
            protected internal set;
        }

        #endregion // Properties

        #region Constructors
        /// <summary>
        /// Initialize new instance from the class.
        /// </summary>
        public DictionaryLoadProgressDialog()
        {
            this.DefaultStyleKey = typeof(DictionaryLoadProgressDialog);
        }
        #endregion //Constructors

        #region ProgressValue

        /// <summary>
        /// Identifies the <see cref="ProgressValue"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register("ProgressValue", typeof(double),
            typeof(DictionaryLoadProgressDialog), new PropertyMetadata(0.0));

        /// <summary>
        /// Returns a value representing the percentage of the downloding process that has been completed
        /// </summary>
        public double ProgressValue
        {
            get { return (double)this.GetValue(ProgressValueProperty); }
            internal set { this.SetValue(ProgressValueProperty, value); }
        }
        #endregion // ProgressValue

        #region Methods

        #region Protected

        #region CancelAsyncDictionaryLoad
        /// <summary>
        /// If a <see cref="XamSpellChecker"/> is associated, will call to the <see cref="XamSpellChecker"/> to cancel it's dictionary load.
        /// </summary>
        protected internal void CancelAsyncDictionaryLoad()
        {
            if (this.SpellChecker != null)
            {
                this.SpellChecker.CancelAsyncDictionaryLoad();
            }
        }

        #endregion // CancelAsyncDictionaryLoad

        #endregion // Protected

        #endregion //Methods

        #region  GetParameter
        /// <summary>
        /// Returns the object that defines the parameters necessary to execute the command.
        /// </summary>
        /// <param name="source">The CommandSource object which defines the command to be executed.</param>
        /// <returns>The object necessary for the command to complete.</returns>
        protected virtual object GetParameter(CommandSource source)
        {

            if (source.Command is CloseDialogCommand)
                return this;

            return null;
        }
        #endregion // GetParameter

        #region SupportsCommand

        /// <summary>
        /// Returns if the object will support a given command type.
        /// </summary>
        /// <param name="command">The command to be validated.</param>
        /// <returns>True if the object recognizes the command as actionable against it.</returns>
        protected virtual bool SupportsCommand(ICommand command)
        {
            return (command is CloseDialogCommand);
        }
        #endregion // SupportsCommand

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return this.SupportsCommand(command);
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this.GetParameter(source);
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