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

namespace Infragistics.Controls.Editors
{
    /// <summary>
	/// A standard Cell object in the <see cref="XamMultiColumnComboEditor"/>.
    /// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class ComboCell : ComboCellBase
    {
        #region Constructor

        /// <summary>
		/// Initializes a new instance of the <see cref="ComboCell"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="ComboRowBase"/> object that owns the <see cref="ComboCell"/></param>
		/// <param propertyName="column">The <see cref="ComboColumn"/> object that the <see cref="ComboCell"/> represents.</param>
        public ComboCell(ComboRowBase row, ComboColumn column)
			: base(row, column)
		{
		}

		#endregion // Constructor

        #region Properties

        #region Protected

        #region ResolveStyle

        /// <summary>
        /// Gets the Style that should be applied to the <see cref="ComboCellControl"/> when it's attached.
        /// </summary>
        protected override Style ResolveStyle
        {
            get
            {
                if (this.Style != null)
                    return this.Style;
                else
                {
                    if (this.Row != null)
                    {
                        if (this.Row.CellStyle != null)
                            return this.Row.CellStyle;
                    }
                    return this.Column.CellStyle;
                }
            }
        }

        #endregion // ResolveStyle

        #endregion // Protected

        #endregion // Properties

        #region Overrides

        #region CreateInstanceOfRecyclingElement

        /// <summary>
        /// Creates a new instance of the <see cref="ComboCellControl"/> that represents the object.
        /// </summary>
        /// <returns></returns>
        protected override ComboCellControlBase CreateInstanceOfRecyclingElement()
        {
            return new ComboCellControl();
        }
        #endregion // CreateInstanceOfRecyclingElement

		#region CreateCellBindingConverter

		/// <summary>
		/// Creates the <see cref="IValueConverter"/> which will be attached to this <see cref="ComboCell"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override IValueConverter CreateCellBindingConverter()
		{
			return new ComboCellBindingConverter();
		}

		#endregion // CreateCellBindingConverte

        #endregion // Overrides

        #region CellBindingConverter Nested Class

        internal class ComboCellBindingConverter : IValueConverter
        {
            #region IValueConverter Members

            public virtual object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                ComboCellControl ctrl = (ComboCellControl)parameter;
                ComboCell cell = (ComboCell)ctrl.Cell;
                if (cell != null)
                {
                    bool noCntrl = (cell.Control == null);
                    if (noCntrl)
                        cell.Control = ctrl;

                    ComboColumn column = cell.Column;

                    if (column.ValueConverter != null)
                        value = column.ValueConverter.Convert(value, targetType, column.ValueConverterParameter, culture);
                    else if (ctrl.ContentProvider != null)
                        value = ctrl.ContentProvider.ApplyFormatting(value, column, culture);

                    if (value == null && !string.IsNullOrEmpty(column.DataField.NullDisplayText))
                        value = column.DataField.NullDisplayText;

                    if (noCntrl)
                        cell.Control = null;
                }

                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                ComboCellControl ctrl = (ComboCellControl)parameter;
                ComboCell cell = (ComboCell)ctrl.Cell;
                if (cell != null)
                {
                    ComboColumn column = cell.Column;

                    if (column.ValueConverter != null)
                        value = column.ValueConverter.ConvertBack(value, targetType, column.ValueConverterParameter, culture);

                    if (column.DataField.ConvertEmptyStringToNull)
                    {
                        string val = value as string;
                        if (val != null && val.Length == 0)
                            value = null;
                    }
                }

                return value;
            }

            #endregion
        }
        #endregion // CellBindingConverter Nested Class      
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