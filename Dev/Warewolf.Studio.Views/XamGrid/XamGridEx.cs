using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Infragistics;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;
using Warewolf.Studio.CustomControls;

namespace Warewolf.Studio.Views.XamGridEx
{
    public class XamGridEx : XamGrid
    {
        private ContextMenuSettings _contextMenuSettings;

        /// <summary>
        ///     Gets a reference to the <see cref="ContextMenuSettings" /> object that
        ///     controls all the properties concerning the display of a context menu
        ///     in this <see cref="XamGrid" />.
        /// </summary>
        public ContextMenuSettings ContextMenuSettings
        {
            get
            {
                if (_contextMenuSettings == null)
                {
                    _contextMenuSettings = new ContextMenuSettings();
                    _contextMenuSettings.Grid = this;
                }

                return _contextMenuSettings;
            }
            set
            {
                // ReSharper disable PossibleUnintendedReferenceComparison
                if (value != _contextMenuSettings)
                    // ReSharper restore PossibleUnintendedReferenceComparison
                {
                    _contextMenuSettings = value;
                    _contextMenuSettings.Grid = this;

                    OnPropertyChanged("ContextMenuSettings");
                }
            }
        }

        internal void OnContextMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            if (ContextMenuOpening != null)
            {
                ContextMenuOpening(sender, e);
            }
            ActiveItem = e.Cell.Row.Data;
        }

        public delegate void OpeningEventHandler(object sender, ContextMenuOpeningEventArgs e);

        public event OpeningEventHandler ContextMenuOpening;
    }

    public class TextBoxColumn : EditableColumn
    {
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark",
                                        typeof(string),
                                        typeof(TextBoxColumn), new PropertyMetadata(null));

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set
            {
                SetValue(WatermarkProperty, value);
            }
        }

        protected override ColumnContentProviderBase GenerateContentProvider()
        {
            return new TextColumnContentProvider();
        }
    }

    public class TextColumnContentProvider : ColumnContentProviderBase
    {
        WatermarkTextBox _textBox;

        public override void AdjustDisplayElement(Cell cell)
        {
            _textBox.Width = cell.Column.ActualWidth - 3;
            _textBox.Height = cell.Row.MinimumRowHeightResolved;
            base.AdjustDisplayElement(cell);
        }

        public TextColumnContentProvider()
        {
            _textBox = new WatermarkTextBox();
            _textBox.Style = Application.Current.TryFindResource("XamGridTextBoxStyle") as Style;
        }

        public override bool RemovePaddingDuringEditing {
            get { return true; }
        }

        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            TextBoxColumn column = (TextBoxColumn) cell.Column;

            cell.Control.Padding = new Thickness();
            cell.Control.Cell.Control.Padding = new Thickness();
            this._textBox.SetValue(WatermarkTextBox.WatermarkProperty, column.Watermark);
            this._textBox.SetBinding(TextBox.TextProperty, cellBinding);
            _textBox.Width = cell.Column.ActualWidth - 3;
            _textBox.Height = cell.Row.MinimumRowHeightResolved;
            return _textBox;
        }

        protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth,
             double availableHeight, Binding editorBinding)
        {
            if (editorValue != null) this._textBox.Text = editorValue.ToString();
            return this._textBox;
        }

        public override object ResolveValueFromEditor(Cell cell)
        {
            return _textBox.Text;
        }
    }

    public class CheckBoxColumn : EditableColumn
    {
        public static readonly DependencyProperty CheckBoxProperty =
            DependencyProperty.Register("CheckBox",
                                        typeof(string),
                                        typeof(CheckBoxColumn), new PropertyMetadata(null));

        public string CheckBox
        {
            get { return (string)GetValue(CheckBoxProperty); }
            set
            {
                SetValue(CheckBoxProperty, value);
            }
        }

        protected override ColumnContentProviderBase GenerateContentProvider()
        {
            return new CheckBoxColumnContentProvider();
        }
    }

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

        public override bool RemovePaddingDuringEditing
        {
            get { return true; }
        }

        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            CheckBoxColumn column = (CheckBoxColumn)cell.Column;

            cell.Control.Padding = new Thickness();
            cell.Control.Cell.Control.Padding = new Thickness();
            this._checkBox.SetValue(WatermarkTextBox.WatermarkProperty, column.CheckBox);
            this._checkBox.SetBinding(TextBox.TextProperty, cellBinding);
            _checkBox.Height = cell.Row.MinimumRowHeightResolved;
            cell.Column.HorizontalContentAlignment = HorizontalAlignment.Center;
            return _checkBox;
        }

        protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth,
             double availableHeight, Binding editorBinding)
        {
            if (editorValue != null) this._checkBox.IsChecked = (bool)editorValue;
            return this._checkBox;
        }

        public override object ResolveValueFromEditor(Cell cell)
        {
            return _checkBox.IsChecked;
        }
    }
}


