using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;

namespace Warewolf.Studio.CustomControls
{
    public class CheckBoxColumnContentProvider : ColumnContentProviderBase
    {
        CheckBox _checkBox;

        public override void AdjustDisplayElement(Cell cell)
        {
            _checkBox.Height = cell.Row.MinimumRowHeightResolved;
            cell.Column.HorizontalContentAlignment = HorizontalAlignment.Center;
            base.AdjustDisplayElement(cell);
        }

        public CheckBoxColumnContentProvider()
        {
            _checkBox = new CheckBox();
            _checkBox.Style = Application.Current.TryFindResource("CheckBoxXamGridStyle") as Style;
        }

        public override bool RemovePaddingDuringEditing => true;

        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            CheckBoxColumn column = (CheckBoxColumn)cell.Column;

            cell.Control.Padding = new Thickness();
            cell.Control.Cell.Control.Padding = new Thickness();
            _checkBox.SetValue(WatermarkTextBox.WatermarkProperty, column.CheckBox);
            _checkBox.SetBinding(TextBox.TextProperty, cellBinding);
            _checkBox.Height = cell.Row.MinimumRowHeightResolved;
            cell.Column.HorizontalContentAlignment = HorizontalAlignment.Center;
            return _checkBox;
        }

        protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth,
            double availableHeight, Binding editorBinding)
        {
            if (editorValue != null)
            {
                _checkBox.IsChecked = (bool)editorValue;
            }

            return _checkBox;
        }

        public override object ResolveValueFromEditor(Cell cell)
        {
            return _checkBox.IsChecked;
        }
    }
}