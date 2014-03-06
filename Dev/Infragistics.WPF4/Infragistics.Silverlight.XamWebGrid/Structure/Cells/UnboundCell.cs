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

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A cell that represents a <see cref="Cell"/> in a <see cref="UnboundColumn"/>
	/// </summary>
	public class UnboundCell : Cell
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="UnboundCell"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="RowBase"/> object that owns the <see cref="Cell"/></param>
		/// <param propertyName="column">The <see cref="Column"/> object that the <see cref="Cell"/> represents.</param>
		public UnboundCell(RowBase row, Column column)
			: base(row, column)
		{
		}

		#endregion // Constructor

        #region Overrides

        #region Value
        /// <summary>
		/// Gets the the underlying value that the cell represents. 
		/// Note: in order to retrieve the cell's value we use a binding since we don't know about the underlying object. 
		/// The most performant way to retrieve the cell's value is to grab the row's Data (this.Cell.Row.Data), 
		/// cast it as your object and grab the actual value manually. 
		/// </summary>
		public override object Value
		{
			get
			{
				UnboundColumn uc = this.Column as UnboundColumn;

				if (uc != null && uc.ValueConverter != null)
				{
					Binding binding;
					binding = new Binding();
					binding.Mode = BindingMode.OneWay;
					binding.ConverterParameter = uc.ValueConverterParameter;
					binding.Converter = uc.ValueConverter;
					binding.Source = this.Row.Data;

					CellValueObject cellValueObj = new CellValueObject();
					cellValueObj.SetBinding(CellValueObject.ValueProperty, binding);
					return cellValueObj.Value;
				}
                
				return null;
			}
		}
		#endregion // Value

        #region CreateCellBindingConverter

        /// <summary>
        /// Creates the <see cref="IValueConverter"/> which will be attached to this <see cref="Cell"/>.
        /// </summary>
        /// <returns></returns>
        protected internal override IValueConverter CreateCellBindingConverter()
        {
            return new UnboundCellBindingConverter();
        }

        #endregion // CreateCellBindingConverter   

        #region ShouldClearDataContext

        /// <summary>
        /// Gets whether the control attached to the cell should reset it DataContext when a new Cell is attached to it.
        /// </summary>
        protected internal override bool ShouldClearDataContext
        {
            get { return false; }
        }

        #endregion // ShouldClearDataContext

        #region Refresh
        /// <summary>
        /// Refreshes the content of the cell.
        /// </summary>
        public override void Refresh()
        {
            if (this.Control != null)
            {
                object obj = this.Control.DataContext;
                this.Control.DataContext = null;

                // Refresh the Value property of the UnboundColumnDataContext with a re-evaluated cell's Value.
                if (obj is UnboundColumnDataContext)
                {
                    ((UnboundColumnDataContext)obj).Value = this.Value;
                }

                this.Control.DataContext = obj;
            }            
        }
        #endregion // Refresh

        #endregion //  Overrides
    }

    #region CellBindingConverter

    internal class UnboundCellBindingConverter : IValueConverter
    {
        #region IValueConverter Members

        public virtual object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {           
            CellControlBase ctrl = (CellControlBase)parameter;
            Cell cell = (Cell)ctrl.Cell;
            if (cell != null)
            {
                bool noCntrl = (cell.Control == null);
                if (noCntrl)
                    cell.Control = ctrl;

                cell.RaiseCellControlAttachedEvent();

                Column column = cell.ResolveColumn;

                if (column.ValueConverter != null)
                    value = column.ValueConverter.Convert(value, targetType, column.ValueConverterParameter, culture);
                else if (ctrl.ContentProvider != null)
                    value = ctrl.ContentProvider.ApplyFormatting(value, column, culture);

                if (noCntrl)
                    cell.Control = null;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CellControlBase ctrl = (CellControlBase)parameter;
            Cell cell = (Cell)ctrl.Cell;
            if (cell != null)
            {
                Column column = cell.ResolveColumn;

                if (column.ValueConverter != null)
                    return column.ValueConverter.ConvertBack(value, targetType, column.ValueConverterParameter, culture);
            }

            return value;
        }

        #endregion
    }
    #endregion // CellBindingConverter
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