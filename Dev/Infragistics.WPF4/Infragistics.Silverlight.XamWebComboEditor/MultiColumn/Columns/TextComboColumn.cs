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
using Infragistics.Controls.Editors.Primitives;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A <see cref="ComboColumn"/> that uses <see cref="TextBlock"/> elements to represent data.
    /// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class TextComboColumn : ComboColumn
    {
        #region Members
        Style _textBlockStyle;
        #endregion
        #region Properties

        #region TextBlockStyle

        /// <summary>
        /// Gets/Sets the <see cref="Style"/> that will be applied to the <see cref="TextBlock"/> that displays data in a <see cref="ComboCellBase"/>
        /// when it is not in edit mode.
        /// </summary>
        public Style TextBlockStyle
        {
            get
            {
                return this._textBlockStyle;
            }
            set
            {
                if (this._textBlockStyle != value)
                {
                    this._textBlockStyle = value;
                    this.OnPropertyChanged("TextBlockStyle");
                }
            }
        }

        #endregion // TextBlockStyle

        #region TextWrapping

        /// <summary>
        /// Identifies the <see cref="TextWrapping"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(TextComboColumn), new PropertyMetadata(TextWrapping.NoWrap, new PropertyChangedCallback(TextWrappingChanged)));

        /// <summary>
        /// Gets/Sets whether <see cref="TextWrapping"/> should be applied to the  <see cref="TextBlock"/> and <see cref="TextBox"/> of a <see cref="TextComboColumn"/>
        /// </summary>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)this.GetValue(TextWrappingProperty); }
            set { this.SetValue(TextWrappingProperty, value); }
        }

        private static void TextWrappingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TextComboColumn col = (TextComboColumn)obj;
            col.OnPropertyChanged("TextWrapping");
        }

        #endregion // TextWrapping

        #region FormatString

        /// <summary>
        /// Identifies the <see cref="FormatString"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(TextComboColumn), new PropertyMetadata(new PropertyChangedCallback(FormatStringChanged)));

        /// <summary>
        /// Gets/sets the format string that will be applied to all cells in the column, if applicable. 
        /// </summary>
        /// <remarks>
        /// Note: The <see cref="ComboColumn.ValueConverter"/> property has higher precedence. 
        /// <para>In order to set this property in xaml, the value must begin with {}. For example: FormatString="{}{0:C}"</para>
        /// </remarks>
        public string FormatString
        {
            get { return (string)this.GetValue(FormatStringProperty); }
            set { this.SetValue(FormatStringProperty, value); }
        }

        private static void FormatStringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TextComboColumn col = (TextComboColumn)obj;
            col.OnPropertyChanged("FormatString");
        }

        #endregion // FormatString

        #endregion // Properties    

        #region Overrides

        #region GenerateContentProvider

        /// <summary>
        /// Generates a new <see cref="Primitives.TextComboColumnContentProvider"/> that will be used to generate content for <see cref="ComboCellBase"/> objects for this <see cref="TextComboColumn"/>.
        /// </summary>
        /// <returns></returns>
        protected internal override Primitives.ComboColumnContentProviderBase GenerateContentProvider()
        {
            return new TextComboColumnContentProvider();
        }
        #endregion // GenerateContentProvider

        #endregion // Overrides
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