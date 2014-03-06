using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Infragistics.Controls.Charts;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// An object that provides content to <see cref="Cell"/> objects that belong to a <see cref="SparklineColumnContentProvider"/>
    /// </summary>
    public class SparklineColumnContentProvider : ColumnContentProviderBase
    {
        #region Members

        private readonly XamSparkline _chart;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SparklineColumnContentProvider"/> class.
        /// </summary>
        public SparklineColumnContentProvider()
        {
            this._chart = new XamSparkline();
        }

        #endregion // Constructor

        #region Methods

        #region Overrides

        #region ResolveDisplayElement

        /// <summary>
        /// Sets up the element that will be displayed in a <see cref="Cell"/>, when the cell is not in edit mode. 
        /// </summary>
        /// <param name="cell">The cell that the display element will be displayed in.</param>
        /// <param name="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
        /// <returns>The element that should be displayed.</returns>
        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            if ((cell is AddNewRowCell) || (cell is FilterRowCell) || (cell is SummaryRowCell))
                return null;
            
            SparklineColumn column = (SparklineColumn)cell.Column;

            CopyProperties(column, _chart);

            Binding binding = cellBinding;

            if (!string.IsNullOrEmpty(column.ItemsSourcePath))
            {
                binding = new Binding(column.ItemsSourcePath);
                binding.Mode = BindingMode.OneWay;

                

                binding.ConverterParameter = cell.Control;
                binding.Converter = cell.CreateCellBindingConverter();
                binding.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture;
            }

            this.ApplyBindingToDisplayElement(binding);

            return this._chart;
        }

        #endregion // ResolveDisplayElement

        #region ResolveBinding

        /// <summary>
        /// Builds the binding that will be used for a <see cref="Cell"/>
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>
        /// If a binding cannot be created, null will be returned.
        /// </returns>
        protected internal override Binding ResolveBinding(Cell cell)
        {
            Binding binding = new Binding();

            binding.ConverterCulture = CultureInfo.CurrentCulture;
            binding.Mode = cell.BindingMode;

            

            binding.ConverterParameter = cell.Control;
            binding.Converter = cell.CreateCellBindingConverter();

            return binding;
        }

        #endregion // ResolveBinding

        #region ResolveEditorControl

        /// <summary>
        /// Sets up the editor control that will be displayed in a <see cref="Cell"/> when the the cell is in edit mode.
        /// </summary>
        /// <param name="cell">The <see cref="Cell"/> entering edit mode.</param>
        /// <param name="editorValue">The value that should be put in the editor.</param>
        /// <param name="availableWidth">The amount of horizontal space available.</param>
        /// <param name="availableHeight">The amount of vertical space available.</param>
        /// <param name="editorBinding">Provides a <see cref="Binding"/> that can be used for setting up the editor.</param>
        /// <returns></returns>
        protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
        {
            return null;
        }

        #endregion // ResolveEditorControl

        #region ResolveValueFromEditor

        /// <summary>
        /// Resolves the value of the editor control, so that the cell's underlying data can be updated. 
        /// </summary>
        /// <param name="cell">The <see cref="Cell"/> that the editor id being displayed in.</param>
        /// <returns>The value that should be displayed in the cell.</returns>
        public override object ResolveValueFromEditor(Cell cell)
        {
            return null;    
        }

        #endregion // ResolveValueFromEditor

        #endregion // Overrides

        #region Private

        #region ApplyBindingToDisplayElement

        /// <summary>
        /// This is where a ColumnContentProvider should apply the Binding to their Display element.
        /// </summary>
        /// <param name="cellBinding"></param>
        private void ApplyBindingToDisplayElement(Binding cellBinding)
        {
            if (cellBinding != null)
            {
                this._chart.SetBinding(XamSparkline.ItemsSourceProperty, cellBinding);
            }
        }

        #endregion // ApplyBindingToDisplayElement

        #region CopyProperties

        private void CopyProperties(SparklineColumn column, XamSparkline chart)
        {
            if (column.ValueMemberPath != null)
                chart.ValueMemberPath = column.ValueMemberPath;
            
            if (column.LabelMemberPath != null)
                chart.LabelMemberPath = column.LabelMemberPath;

            if (column.Style != null)
                chart.Style = column.Style;

            chart.DisplayType = column.DisplayType;
        }

        #endregion // CopyProperties

        #endregion // Private

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