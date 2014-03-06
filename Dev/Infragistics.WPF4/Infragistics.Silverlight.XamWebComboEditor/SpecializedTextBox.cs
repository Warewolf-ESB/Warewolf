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
using System.ComponentModel;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// For Internal Use Only: TextBox wrapper class that allows the consumer to pre-validate text before allowing the underlying TextBox to consume it.
    /// We use this to prevent Japanese text from causing catastrophic failures.
    /// </summary>
    [DesignTimeVisible(false)]
    public class SpecializedTextBox : TextBox
    {
        bool _keyPressed;
        bool _allowTextChanges;

        /// <summary>
        /// Constructor for SpecializedTextBox, just used to set our default value;
        /// </summary>
        public SpecializedTextBox()
        {
            this._keyPressed = false;
            this._allowTextChanges = false;


            this.Focusable = true;


            
        }

        #region Overrides

        ///<summary>
        /// Called before the System.Windows.UIElement.KeyDown event occurs.
        ///</summary>
        /// <param name="e" >Provides data about the event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {




            this._allowTextChanges = true;
            base.OnKeyDown(e);
        }

        #region OnTextInput
        ///<summary>
        /// Called before the System.Windows.UIElement.TextInput event occurs.
        ///</summary>
        /// <param name="e" >Provides data about the event.</param>
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            this._allowTextChanges = true;

            this._keyPressed = false;
        }
        #endregion // OnTextInput



#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Called before the System.Windows.UIElement.PreviewKeyDown event occurs.
        /// </summary>
        /// <param name="e">Provides data about the event.</param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.ImeProcessed)
                _keyPressed = true;
        }

        /// <summary>
        /// Called when content in this editing control changes.
        /// </summary>
        /// <param name="e">The arguments associated with the content change.</param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            this._allowTextChanges = false;
            base.OnTextChanged(e);
        }



        #endregion // Overrides

        #region Properties

        /// <summary>
        /// Specifies whether or not the text should be changed at any given point.  This is to fix the catastrophic failures for IME.
        /// </summary>
        internal bool AllowTextChanges
        {
            get
            {
                if (_keyPressed)
                    return _allowTextChanges;
                else
                    return true;
            }
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