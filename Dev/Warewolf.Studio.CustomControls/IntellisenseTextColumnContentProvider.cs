using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Studio.InterfaceImplementors;
using Dev2.UI;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;

namespace Warewolf.Studio.CustomControls
{
    public class IntellisenseTextColumnContentProvider : ColumnContentProviderBase
    {
        readonly IntellisenseTextBox _textBox;

        public override void AdjustDisplayElement(Cell cell)
        {
            _textBox.Width = cell.Column.ActualWidth - 3;
            _textBox.Height = cell.Row.MinimumRowHeightResolved;
            base.AdjustDisplayElement(cell);
        }

        public IntellisenseTextColumnContentProvider()
        {
            if(_textBox == null)
            {
                _textBox = new IntellisenseTextBox
                {
                    Style = Application.Current.TryFindResource("DatagridIntellisenseTextBoxStyle") as Style,
                    IntellisenseProvider = new DefaultIntellisenseProvider(),
                    WrapInBrackets = false,
                    AcceptsReturn = false,
                    IsOpen = true
                };
            }
        }

        public override bool RemovePaddingDuringEditing
        {
            get { return true; }
        }

        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            IntellisenseTextBoxColumn column = (IntellisenseTextBoxColumn)cell.Column;

            cell.Control.Padding = new Thickness();
            cell.Control.Cell.Control.Padding = new Thickness();
            _textBox.SetValue(IntellisenseTextBox.DefaultTextProperty, column.Watermark);
            _textBox.SetBinding(TextBox.TextProperty, cellBinding);
            _textBox.Width = cell.Column.ActualWidth - 3;
            _textBox.Height = cell.Row.MinimumRowHeightResolved;
            return _textBox;
        }

        protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth,
            double availableHeight, Binding editorBinding)
        {
            if (editorValue != null) _textBox.Text = editorValue.ToString();
            return _textBox;
        }

        public override object ResolveValueFromEditor(Cell cell)
        {
            return _textBox.Text;
        }

        #region Overrides of ColumnContentProviderBase

        public override void EditorRemoved()
        {
            BindingOperations.ClearBinding(_textBox, TextBox.TextProperty);
            base.EditorRemoved();
        }

        #endregion
    }
}