using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;
using System.Threading;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// An editor designed to limit entries to a specified data.
    /// </summary>
    /// <remarks>This editor is designed for the <see cref="XamColorPicker"/> and not intended for external use.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class SpecializedEditorsBase : TextBox
    {
        #region Members

        private string _oldText = "";

        #endregion // Members

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteTextBox"/> class.
        /// </summary>
        public SpecializedEditorsBase()
        {



        }
       
        #endregion // Constructor

        #region OnLostFocus
        /// <summary>
        /// Called before System.Windows.UIElement.LostFocus event occurs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.Text))
            {



                this.Dispatcher.BeginInvoke((Action)this.FocusHelper);

                return;
            }

            if (!this.ValidateInputText(this.Text))
            {



                this.Dispatcher.BeginInvoke((Action)this.FocusHelper);

                return;
            }

            this._oldText = "";

            base.OnLostFocus(e);
        }
        #endregion // OnLostFocus

        private void FocusHelper()
        {
            this.Focus();
        }



#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)



        /// <summary>
        /// Is called when content in this editing control changes.
        /// </summary>
        /// <param name="e">The arguments that are associated with the System.Windows.Controls.Primitives.TextBoxBase.TextChanged event.</param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            bool valid = this.ValidateInputText(this.Text);
            if (!valid)
            {                
                e.Handled = true;
                return;
            }
            base.OnTextChanged(e);
        }


        #region OnTextInput
        ///<summary>
        /// Called before the System.Windows.UIElement.TextInput event occurs.
        ///</summary>
        /// <param name="e" >Provides data about the event.</param>
        protected override void OnTextInput(System.Windows.Input.TextCompositionEventArgs e)
        {
            this._oldText = this.Text;

            bool valid = this.ValidateInputText(e.Text);
            if (!valid)
            {
                e.Handled = true;
                return;
            }
            base.OnTextInput(e);
        }
        #endregion // OnTextInput

        #region ValidateInputText
        /// <summary>
        /// Validated that the input text meets the criteria of the editor.
        /// </summary>
        /// <param name="compositionText"></param>
        /// <returns>Returns false if validation failed</returns>
        protected virtual bool ValidateInputText(string compositionText)
        {
            return true;
        }
        #endregion // ValidateInputText

        #region EventHandlers



#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)


        #endregion // EventHandlers
    }

    /// <summary>
    /// An editor designed to limit entries to byte data.
    /// </summary>
    /// <remarks>This editor is designed for the <see cref="XamColorPicker"/> and not intended for external use.</remarks>    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ByteTextBox : SpecializedEditorsBase
    {

    }

    /// <summary>
    /// An editor designed to limit entries to degree data.
    /// </summary>
    /// <remarks>This editor is designed for the <see cref="XamColorPicker"/> and not intended for external use.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DegreeTextBox : SpecializedEditorsBase
    {
        
    }

    /// <summary>
    /// An editor designed to limit entries to percent data.
    /// </summary>
    /// <remarks>This editor is designed for the <see cref="XamColorPicker"/> and not intended for external use.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PercentTextBox : SpecializedEditorsBase
    {
        
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