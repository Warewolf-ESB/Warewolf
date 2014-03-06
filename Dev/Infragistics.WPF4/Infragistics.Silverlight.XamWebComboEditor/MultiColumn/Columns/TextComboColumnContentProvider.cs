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
using System.Windows.Data;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A class used to provide content to a <see cref="ComboCellBase"/> object for a particular <see cref="TextComboColumn"/>.
    /// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class TextComboColumnContentProvider : ComboColumnContentProviderBase
    {
        #region Members

		HighlightingTextBlock _tb;

		#endregion // Members

		#region Constructor

		/// <summary>
        /// Instantiates a new instance of the <see cref="TextComboColumnContentProvider"/>.
		/// </summary>
        public TextComboColumnContentProvider()
		{
			this._tb = new HighlightingTextBlock();
		}

		#endregion // Constructor

		#region Methods

		#region ResolveDisplayElement

		/// <summary>
		/// Sets up the element that will be displayed in a <see cref="ComboCellBase"/>, when the cell is not in edit mode. 
		/// </summary>
		/// <param propertyName="cell">The cell that the display element will be displayed in.</param>
		/// <param propertyName="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
		/// <returns>The element that should be displayed.</returns>
        public override FrameworkElement ResolveDisplayElement(ComboCellBase cell, Binding cellBinding)
		{
            TextComboColumn column = (TextComboColumn)cell.Column;

			this._tb.TextBlockTextWrapping = column.TextWrapping;

            this._tb.TextBlockStyle = column.TextBlockStyle;

            this._tb.SetBinding(HighlightingTextBlock.TextProperty, cellBinding);

			if (cell.Column.DataType == typeof(string) && cell.Row.RowType == RowType.DataRow)
				cell.Row.ComboEditor.RegisterCellTextBlock(this._tb);

			return this._tb;
		}
		#endregion // ResolveDisplayElement

		#region ApplyFormatting

		/// <summary>
		/// Allows the <see cref="ComboColumnContentProviderBase"/> to update the value being set for the display element.
		/// </summary>
		/// <param propertyName="value">The original data value from the underlying data.</param>
        /// <param propertyName="column">The <see cref="ComboColumn"/> whose properties should be used to determine the formatting.</param>
		/// <param propertyName="culture"></param>
		/// <returns>The value that should be displayed in the <see cref="ComboCellBase"/></returns>
        public override object ApplyFormatting(object value, ComboColumn column, System.Globalization.CultureInfo culture)
		{
            TextComboColumn col = (TextComboColumn)column;
			
            if (!string.IsNullOrEmpty(col.FormatString))
				return String.Format(culture, col.FormatString, value);

            if (!string.IsNullOrEmpty(col.DataField.FormatString))
                return String.Format(culture, col.DataField.FormatString, value);

			this._tb.HighlightText();

			return base.ApplyFormatting(value, column, culture);
		}

		#endregion // ApplyFormatting

        #endregion // Methods
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