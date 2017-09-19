using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;

namespace Warewolf.Studio.CustomControls
{
    public class TextColumnContentProvider : ColumnContentProviderBase
    {
        readonly WatermarkTextBox _textBox;

        public override void AdjustDisplayElement(Cell cell)
        {
            _textBox.Width = cell.Column.ActualWidth - 3;
            _textBox.Height = cell.Row.MinimumRowHeightResolved;
            base.AdjustDisplayElement(cell);
        }

        public TextColumnContentProvider()
        {
            _textBox = new WatermarkTextBox
            {
                Style = Application.Current.TryFindResource("XamGridTextBoxStyle") as Style
            };
        }

        public override bool RemovePaddingDuringEditing => true;

        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            TextBoxColumn column = (TextBoxColumn)cell.Column;

            cell.Control.Padding = new Thickness();
            cell.Control.Cell.Control.Padding = new Thickness();
            _textBox.SetValue(WatermarkTextBox.WatermarkProperty, column.Watermark);
            _textBox.SetBinding(TextBox.TextProperty, cellBinding);
            _textBox.Width = cell.Column.ActualWidth - 3;
            _textBox.Height = cell.Row.MinimumRowHeightResolved;
            return _textBox;
        }

        protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth,
            double availableHeight, Binding editorBinding)
        {
            if (editorValue != null)
            {
                _textBox.Text = editorValue.ToString();
            }

            return _textBox;
        }

        public override object ResolveValueFromEditor(Cell cell)
        {
            return _textBox.Text;
        }
    }
}