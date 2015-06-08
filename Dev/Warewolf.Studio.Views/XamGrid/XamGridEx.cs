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

namespace Warewolf.Studio.Views.XamGridEx
{
    public class XamGridEx : XamGrid
    {
        private ContextMenuSettings _contextMenuSettings;

        protected override bool OnColumnResizing(Collection<Column> columns, double newWidth)
        {
            return base.OnColumnResizing(columns, newWidth);
        }


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

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(string),
                typeof(TextBoxColumn),
                new PropertyMetadata(
                    new PropertyChangedCallback(ContentChanged)));

        public string Content
        {
            get { return (string)this.GetValue(ContentProperty); }
            set { this.SetValue(ContentProperty, value); }
        }

        private static void ContentChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            TextBoxColumn col = (TextBoxColumn)obj;
            col.OnPropertyChanged("Content");
        }

        public static readonly DependencyProperty ContentBindingProperty =
            DependencyProperty.Register("ContentBinding",
                typeof(Binding), typeof(TextBoxColumn),
                new PropertyMetadata(
                    new PropertyChangedCallback(ContentBindingChanged)));

        public Binding ContentBinding
        {
            get { return (Binding)this.GetValue(ContentBindingProperty); }
            set { this.SetValue(ContentBindingProperty, value); }
        }

        private static void ContentBindingChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            TextBoxColumn col = (TextBoxColumn)obj;
            col.OnPropertyChanged("ContentBinding");
        }

        protected override ColumnContentProviderBase GenerateContentProvider()
        {
            return new TextColumnContentProvider();
        }
    }

    public class TextColumnContentProvider : ColumnContentProviderBase
    {
        TextBox _textBox;
        public override void AdjustDisplayElement(Cell cell)
        {
            if (cell.Column.ActualWidth < 230)
            {
                _textBox.Width = cell.Column.ActualWidth - 3;
            }
            else
            {
                _textBox.Width = cell.Column.ActualWidth - 4;   
            }
            _textBox.Height = cell.Row.MinimumRowHeightResolved;
            base.AdjustDisplayElement(cell);
        }

        public TextColumnContentProvider()
        {
            _textBox = new TextBox();
            _textBox.Style = Application.Current.TryFindResource("XamGridTextBoxStyle") as Style;
        }

        public override bool RemovePaddingDuringEditing {
            get { return true; }
        }

        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            TextBoxColumn column = (TextBoxColumn)cell.Column;
            
            cell.Control.Padding = new Thickness();
            cell.Control.Cell.Control.Padding = new Thickness();
            
            Binding textBinding = new Binding();
            textBinding.Source = cell.Row.Data;

            if (column.Content != null)
                textBinding.Path =
                    new PropertyPath(string.Format("{0}.{1}", column.Key,
                                     column.Content));
            else
                textBinding.Path = new PropertyPath(column.Key);

            textBinding.Mode = BindingMode.TwoWay;
            this._textBox.SetBinding(TextBox.TextProperty, textBinding);
            _textBox.Width = cell.Column.ActualWidth - 4;
            _textBox.Height = cell.Row.MinimumRowHeightResolved;
            return _textBox; 
        }

        protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth,
             double availableHeight, Binding editorBinding)
        {
            TextBoxColumn column = (TextBoxColumn)cell.Column;

            this._textBox.SetValue(TextBox.TextProperty, column.Content);

            Binding selectedItemBinding = new Binding();
            selectedItemBinding.Source = cell.Row.Data;
            selectedItemBinding.Path = new PropertyPath(column.Key);
            selectedItemBinding.Mode = BindingMode.TwoWay;


            return this._textBox;
        }

        public override object ResolveValueFromEditor(Cell cell)
        {
            return _textBox.Text;
        }
    }
}


