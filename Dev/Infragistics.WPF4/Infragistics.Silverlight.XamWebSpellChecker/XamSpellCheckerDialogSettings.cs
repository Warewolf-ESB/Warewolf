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

namespace Infragistics.Controls.Interactions
{
    /// <summary>
    /// Contains the properties for the <see cref="XamSpellCheckerDialogWindow"/>.
    /// </summary>
    public class XamSpellCheckerDialogSettings : DependencyObject
    {
        #region Members

        DialogStringResources _dialogStringResources;
        XamSpellChecker _spellChecker;
        #endregion //Members

        #region CurrentWordBrush

        /// <summary>
        /// Identifies the <see cref="CurrentWordBrush"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CurrentWordBrushProperty = DependencyProperty.Register("CurrentWordBrush", typeof(Brush), typeof(XamSpellCheckerDialogSettings), new PropertyMetadata(new SolidColorBrush(new Color() { R = 0xFF, B = 0x00, G = 0x00, A = 0xFF })));

        /// <summary>
        /// Gets or sets the brush that is used to highlight the current word in the context.
        /// </summary>
        public Brush CurrentWordBrush
        {
            get { return (Brush)this.GetValue(CurrentWordBrushProperty); }
            set { this.SetValue(CurrentWordBrushProperty, value); }
        }

        #endregion // CurrentWordBrush

        #region SpellCheckDialogStyle

        /// <summary>
        /// Identifies the <see cref="SpellCheckDialogStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SpellCheckDialogStyleProperty = DependencyProperty.Register("SpellCheckDialogStyle", typeof(Style), typeof(XamSpellCheckerDialogSettings), new PropertyMetadata(new PropertyChangedCallback(SpellCheckDialogStyleChanged)));

        /// <summary>
        /// Gets or sets the style for the <see cref="XamSpellCheckerDialogWindow"/>       
        /// </summary>
        public Style SpellCheckDialogStyle
        {
            get { return (Style)this.GetValue(SpellCheckDialogStyleProperty); }
            set { this.SetValue(SpellCheckDialogStyleProperty, value); }
        }

        private static void SpellCheckDialogStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSpellCheckerDialogSettings settings = (XamSpellCheckerDialogSettings)obj;

            if (settings.SpellChecker != null)
            {
                Style newStyle = (Style)e.NewValue;
                if (newStyle != settings.SpellChecker.SpellCheckDialog.Style)
                    settings.SpellChecker.SpellCheckDialog.Style = newStyle;
            }
        }
        #endregion // SpellCheckDialogStyle
        
        #region Mode

        /// <summary>
        /// Identifies the <see cref="Mode"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(SpellCheckingMode),
            typeof(XamSpellCheckerDialogSettings),
            new PropertyMetadata(SpellCheckingMode.ModalDialog));

        /// <summary>
        /// Gets or sets the type of spell checking that will be performed (basically, will a dialog be displayed or not). 
        /// </summary>
        public SpellCheckingMode Mode
        {
            get { return (SpellCheckingMode)this.GetValue(ModeProperty); }
            set { this.SetValue(ModeProperty, value); }
        }

        #endregion // Mode

        #region DialogStringResources


        /// <summary>
        /// Contains all of the string resources used in the <see cref="XamSpellCheckerDialogWindow"/>.
        /// </summary>
        public DialogStringResources DialogStringResources
        {
            get
            {
                if (this._dialogStringResources == null)
                {
                    this._dialogStringResources = new DialogStringResources();
                }
                return this._dialogStringResources;
            }
        }

        #endregion // DialogStringResources

        internal XamSpellChecker SpellChecker
        {
            get
            {
                return this._spellChecker;
            }
            set
            {
                if (this._spellChecker != value)
                {
                    this._spellChecker = value;
                    if (this._spellChecker != null && this._spellChecker.SpellCheckDialog.Style != this.SpellCheckDialogStyle)
                    {
                        this._spellChecker.SpellCheckDialog.Style = this.SpellCheckDialogStyle;
                    }
                }
            }
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