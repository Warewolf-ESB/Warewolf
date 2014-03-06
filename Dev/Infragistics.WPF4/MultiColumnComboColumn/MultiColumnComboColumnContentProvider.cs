using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.Controls.Editors;
using System.ComponentModel;
using System.Windows.Input;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// An object that provides content to <see cref="Cell"/> objects that belong to a <see cref="MultiColumnComboColumn"/>
    /// </summary>
    public class MultiColumnComboColumnContentProvider : ColumnContentProviderBase
    {
        #region Members

        private readonly string _templateFormat;
        private XamMultiColumnComboEditor _comboEditor;
        private ContentControl _displayElement;
        private ValueHolder _comboValueHolder;

        private Cell _cell;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Instantiates a new instance of the <see cref="MultiColumnComboColumnContentProvider"/>.
        /// </summary>
        public MultiColumnComboColumnContentProvider()
        {
            this._comboEditor = new XamMultiColumnComboEditor();
            this._displayElement = new ContentControl{ VerticalAlignment = VerticalAlignment.Center };
            this._comboValueHolder = new ValueHolder();

            StringBuilder template = new StringBuilder();

            template.Append(@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">");
            template.Append(@"<TextBlock Text=""{{Binding Path={0}}}"" />");
            template.Append(@"</DataTemplate>");
            this._templateFormat = template.ToString();
        }

        #endregion // Constructor

        #region Overrides

        #region ResolveDisplayElement

        /// <summary>
        /// Sets up the element that will be displayed in a <see cref="Cell"/>, when the cell is not in edit mode.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="cellBinding"></param>
        /// <returns>
        /// The element that should be displayed.
        /// </returns>
        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            _cell = cell;
            var col = (MultiColumnComboColumn)cell.Column;

            if (col.EditorDisplayBehavior == EditorDisplayBehaviors.EditMode)
            {
                this.ApplyBindingToDisplayElement(cell, cellBinding);

                // Our wrapping convertor will return an object from the col.ItemsSource, lets use the
                // DisplayMemberPath if we have it.
                if (!string.IsNullOrEmpty(col.DisplayMemberPath))
                {
                    string template = string.Format(this._templateFormat, col.DisplayMemberPath);

                    System.IO.StringReader stringReader = null;

                    try
                    {
                        stringReader = new System.IO.StringReader(template);

                        using (var xmlReader = System.Xml.XmlReader.Create(stringReader))
                        {
                            this._displayElement.ContentTemplate = (DataTemplate)System.Windows.Markup.XamlReader.Load(xmlReader);
                        }
                    }
                    finally
                    {
                        if (stringReader != null)
                        {
                            stringReader.Dispose();
                        }
                    }




                }

                return _displayElement;
            }

            this._comboEditor.AutoGenerateColumns = col.AutoGenerateColumns;

            if (!col.AutoGenerateColumns)
            {
                this._comboEditor.Columns.Clear();

                foreach (var cmbCol in col.Columns)
                {
                    this._comboEditor.Columns.Add(cmbCol);
                }
            }

            this._comboEditor.IsEnabled = cell.IsEditable;
            this.SetupComboBox(this._comboEditor, cell);

            bool isEditable = col.EditorDisplayBehavior == EditorDisplayBehaviors.Always;

            this._comboEditor.IsHitTestVisible = isEditable;

            this._comboEditor.Focusable = isEditable;


            this.ApplyBindingToDisplayElement(cell, cellBinding);

            return this._comboEditor;
        }

        #endregion // ResolveDisplayElement

        #region ResolveValueFromEditor

        /// <summary>
        /// Resolves the value of the editor control, so that the cell's underlying data can be updated.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>
        /// The value that should be displayed in the cell.
        /// </returns>
        public override object ResolveValueFromEditor(Cell cell)
        {
            return this._comboValueHolder.Value;
        }

        #endregion // ResolveValueFromEditor

        #region ResolveEditorControl

        /// <summary>
        /// Sets up the edtior control that will be displayed in a <see cref="Cell"/> when the the cell is in edit mode.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="editorValue"></param>
        /// <param name="availableWidth"></param>
        /// <param name="availableHeight"></param>
        /// <param name="editorBinding"></param>
        /// <returns></returns>
        protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
        {
            MultiColumnComboColumn col = (MultiColumnComboColumn)cell.Column;

            this._comboEditor.IsHitTestVisible = true;

            this._comboEditor.Focusable = true;


            this.SetupComboBox(this._comboEditor, cell);

            if (editorBinding != null)
            {
                var wrappingConverter = new MultiColumnComboEditorValueConverter(col, editorBinding.Converter);
                editorBinding.Converter = wrappingConverter;

                this._comboEditor.SetBinding(XamMultiColumnComboEditor.SelectedItemProperty, editorBinding);
            }
            else
            {
                this._comboEditor.ClearValue(XamMultiColumnComboEditor.SelectedItemProperty);
            }

            this._comboEditor.SelectionChanged += ComboEditor_SelectionChanged;

            if (cell.IsEditing)
            {
                this._comboEditor.Focus();
            }

            return this._comboEditor;
        }

        #endregion // ResolveEditorControl

        #endregion // Overrides

        #region Methods

        #region SetupComboBox

        private void SetupComboBox(XamMultiColumnComboEditor comboBox, Cell cell)
        {
            MultiColumnComboColumn cc = (MultiColumnComboColumn)cell.Column;

            if (comboBox.DisplayMemberPath != cc.DisplayMemberPath)
            {
                if (cc.DisplayMemberPath == null)
                {
                    comboBox.ClearValue(XamMultiColumnComboEditor.DisplayMemberPathProperty);
                }
                else
                {
                    comboBox.DisplayMemberPath = cc.DisplayMemberPath;
                }
            }

            comboBox.ItemsSource = cc.ItemsSource;

            ColumnContentProviderBase.SetControlStyle(comboBox, cell.EditorStyleResolved);
        }

        #endregion // SetupComboBox

        #region ApplyBindingToDisplayElement

        private void ApplyBindingToDisplayElement(Cell cell, Binding binding)
        {
            MultiColumnComboColumn col = (MultiColumnComboColumn)cell.Column;

            if (_comboEditor.DisplayMemberPath != col.DisplayMemberPath)
            {
                if (col.DisplayMemberPath == null)
                    _comboEditor.ClearValue(MultiColumnComboColumn.DisplayMemberPathProperty);
                else
                    _comboEditor.DisplayMemberPath = col.DisplayMemberPath;
            }

            if (binding != null)
            {
                // Wrapping
                var conv = new MultiColumnComboValueConverter(col, binding.Converter);
                binding.Converter = conv;

                if (col.EditorDisplayBehavior == EditorDisplayBehaviors.EditMode)
                {
                    _displayElement.SetBinding(ContentControl.ContentProperty, binding);
                }
                else
                {
                    _comboEditor.SetBinding(XamMultiColumnComboEditor.SelectedItemProperty, binding);
                }

                Binding b = new Binding(col.SelectedValuePath ?? string.Empty);
                b.Mode = BindingMode.OneWay;
                _comboValueHolder.SetBinding(ValueHolder.ValueProperty, b);
            }
            else
            {
                _comboEditor.ClearValue(XamMultiColumnComboEditor.SelectedItemProperty);
            }
        }

        #endregion // ApplyBindingToDisplayElement

        #region EditorRemoved

        /// <summary>
        /// Raised when the editor is removed from the cell.
        /// </summary>
        public override void EditorRemoved()
        {
            base.EditorRemoved();
            this._comboEditor.SelectionChanged -= this.ComboEditor_SelectionChanged;
        }

        #endregion // EditorRemoved

        #endregion // Methods

        #region Event Handlers

        private void ComboEditor_SelectionChanged(object sender, EventArgs e)
        {
            this._comboValueHolder.DataContext = this._comboEditor.SelectedItem;

            object valueChanged = null;

            if (this._comboEditor.SelectedItems.Count > 0)
            {
                valueChanged = this._comboValueHolder.Value;
            }

            this.NotifyEditorValueChanged(valueChanged);

            if ((_cell != null) && (_cell.IsEditing) && (_cell is FilterRowCell))
                _cell.Column.ColumnLayout.Grid.ExitEditMode(false);
        }

        #endregion // Event Handlers

        #region Classes

        #region ValueHolder

        private class ValueHolder : FrameworkElement, INotifyPropertyChanged
        {
            #region Value Property

            public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ValueHolder), new PropertyMetadata(new PropertyChangedCallback(ValueChanged)));

            public object Value
            {
                get { return this.GetValue(ValueProperty); }
                set { this.SetValue(ValueProperty, value); }
            }

            private static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
            {
                var ctrl = obj as ValueHolder;

                if (ctrl != null)
                {
                    ctrl.OnPropertyChanged("Value");
                }
            }

            #endregion

            #region INotifyPropertyChanged

            private void OnPropertyChanged(string prop)
            {
                PropertyChangedEventHandler handler = PropertyChanged;

                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(prop));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion // INotifyPropertyChanged
        }

        #endregion

        #region MultiColumnComboValueConverterBase

        private abstract class MultiColumnComboValueConverterBase : IValueConverter
        {
            #region Members

            private readonly string _selectedValuePath;
            private readonly IValueConverter _wrappedConverter;
            private readonly MultiColumnComboColumn _column;

            #endregion // Members

            #region Constructor

            protected MultiColumnComboValueConverterBase(MultiColumnComboColumn column, IValueConverter wrappedConverter)
            {
                this._column = column;
                this._selectedValuePath = column.SelectedValuePath;
                this._wrappedConverter = wrappedConverter;
            }

            #endregion // Constructor

            #region Methods

            #region ResolveValue

            protected virtual object ResolveValue(object value)
            {
                if (!string.IsNullOrEmpty(_selectedValuePath) && value != null)
                {
                    return DataManagerBase.ResolveValueFromPropertyPath(_selectedValuePath, value);
                }

                return value;
            }

            #endregion // ResolveValue

            #region ResolveValueBack

            protected virtual object ResolveValueBack(object value)
            {
                object resolvedValueBack = value;

                if (!string.IsNullOrEmpty(this._selectedValuePath) && value != null)
                {
                    if (!this._column.DataType.IsAssignableFrom(value.GetType()))
                    {
                        return null;
                    }

                    object resolvedValue = this.ResolveValue(value);

                    DataManagerBase dm = DataManagerBase.CreateDataManager(this._column.ItemsSource);

                    if (dm != null)
                    {
                        ItemsFilter filter = new ItemsFilter
                        {
                            ObjectType = dm.CachedType,
                            FieldName = this._selectedValuePath,
                            ObjectTypedInfo = dm.CachedTypedInfo
                        };

                        ComparisonCondition comparisonCondition = new ComparisonCondition
                        {
                            Operator = ComparisonOperator.Equals,
                            FilterValue = resolvedValue
                        };

                        filter.Conditions.Add(comparisonCondition);

                        RecordFilterCollection recordFilterCollection = new RecordFilterCollection();
                        recordFilterCollection.Add(filter);
                        dm.Filters = recordFilterCollection;

                        if (dm.RecordCount > 0)
                        {
                            resolvedValueBack = dm.GetRecord(0);
                        }
                        else
                        {
                            resolvedValueBack = null;
                        }

                        dm.DataSource = null;
                    }
                }

                return resolvedValueBack;
            }

            #endregion // ResolveValueBack

            #region ApplyConvertion

            protected virtual object ApplyConvertion(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (_wrappedConverter != null)
                {
                    return _wrappedConverter.Convert(value, targetType, parameter, culture);
                }

                return value;
            }

            #endregion // ApplyConvertion

            #region ApplyBackConvertion

            protected virtual object ApplyBackConvertion(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (_wrappedConverter != null)
                {
                    return _wrappedConverter.ConvertBack(value, targetType, parameter, culture);
                }

                return value;
            }

            #endregion // ApplyBackConvertion

            #endregion // Methods

            #region IValueConverter

            public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

            public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

            #endregion // IValueConverter
        }

        #endregion // MultiColumnComboValueConverterBase

        #region MultiColumnComboValueConverter

        private class MultiColumnComboValueConverter : MultiColumnComboValueConverterBase
        {
            #region Constructor

            public MultiColumnComboValueConverter(MultiColumnComboColumn column, IValueConverter wrappedConverter)
                : base(column, wrappedConverter)
            {
            }

            #endregion // Constructor

            #region Overrides

            #region Convert

            public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                object convertedValue = this.ApplyConvertion(value, targetType, parameter, culture);
                object resolveValueBack = this.ResolveValueBack(convertedValue);

                return resolveValueBack;
            }

            #endregion // Convert

            #region ConvertBack

            public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                object convertedValue = this.ApplyBackConvertion(value, targetType, parameter, culture);
                object resolveValueBack = this.ResolveValueBack(convertedValue);

                return resolveValueBack;
            }

            #endregion // ConvertBack

            #endregion // Overrides
        }

        #endregion // MultiColumnComboValueConverter

        #region MultiColumnComboEditorValueConverter

        private class MultiColumnComboEditorValueConverter : MultiColumnComboValueConverterBase
        {
            #region Constructor

            public MultiColumnComboEditorValueConverter(MultiColumnComboColumn column, IValueConverter wrappedConverter)
                : base(column, wrappedConverter)
            {
            }

            #endregion // Constructor

            #region Overrides

            #region Convert

            public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                object convertedValue = this.ApplyConvertion(value, targetType, parameter, culture);
                object resolveValueBack = this.ResolveValueBack(convertedValue);

                return resolveValueBack;
            }

            #endregion // Convert

            #region ConvertBack

            public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                object resolvedValue = this.ResolveValue(value);
                object convertedValue = this.ApplyBackConvertion(resolvedValue, targetType, parameter, culture);

                return convertedValue;
            }

            #endregion // ConvertBack

            #endregion // Overrides
        }

        #endregion // MultiColumnComboEditorValueConverter

        #endregion // Classes
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