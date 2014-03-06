using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.Controls.Grids.Primitives;
using Expression = System.Linq.Expressions.Expression;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// An object that provides content to <see cref="Cell"/> objects that belong to a <see cref="ComboBoxColumn"/>
    /// </summary>
    public class ComboBoxColumnContentProvider : ColumnContentProviderBase
    {
        #region Members
        private readonly string _templateFormat;
        ComboBox _comboEditor;
        ComboBox _comboDisplay;
        Binding _cellBinding;
        private ContentControl _displayElement;
        private ValueHolder _comboValueHolder;
        #endregion // Members

        #region Constructor

        /// <summary>
        /// Instantiates a new instance of the <see cref="ComboBoxColumnContentProvider"/>.
        /// </summary>
        public ComboBoxColumnContentProvider()
        {
            this._comboEditor = new ComboBox();
            this._comboDisplay = new ComboBox();
            this._displayElement = new ContentControl { VerticalAlignment = VerticalAlignment.Center };
            this._comboValueHolder = new ValueHolder();

            StringBuilder template = new StringBuilder();

            template.Append(@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">");
            template.Append(@"<TextBlock Text=""{{Binding Path={0}}}"" />");
            template.Append(@"</DataTemplate>");
            this._templateFormat = template.ToString();
        }

        #endregion // Constructor

        #region Overrides

        private void SetupComboBox(ComboBox comboBox, Cell cell, Binding cellBinding)
        {
            ComboBoxColumn cc = (ComboBoxColumn)cell.Column;

            if (comboBox.ItemTemplate != cc.ItemTemplate)
            {
                if (cc.ItemTemplate == null)
                    comboBox.ClearValue(ComboBox.ItemTemplateProperty);
                else
                    comboBox.ItemTemplate = cc.ItemTemplate;
            }

            if (comboBox.SelectedValuePath != cc.SelectedValuePath)
            {
                if (cc.SelectedValuePath == null)
                    comboBox.ClearValue(ComboBox.SelectedValuePathProperty);
                else
                    comboBox.SelectedValuePath = cc.SelectedValuePath;
            }

            if (comboBox.DisplayMemberPath != cc.DisplayMemberPath)
            {
                if (cc.DisplayMemberPath == null)
                    comboBox.ClearValue(ComboBox.DisplayMemberPathProperty);
                else
                    comboBox.DisplayMemberPath = cc.DisplayMemberPath;
            }

            comboBox.ItemsSource = cc.ItemsSource;

            Style comboStyle = cell.EditorStyleResolved;
            if (comboStyle != comboBox.Style)
            {
                if (comboStyle == null)
                    comboBox.ClearValue(Control.StyleProperty);
                else
                    comboBox.Style = comboStyle;
            }
        }

        #region ResolveDisplayElement

        /// <summary>
        /// Sets up the element that will be displayed in a <see cref="Cell"/>, when the cell is not in edit mode. 
        /// </summary>
        /// <param propertyName="cell">The cell that the display element will be displayed in.</param>
        /// <param propertyName="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
        /// <returns>The element that should be displayed.</returns>
        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            var col = (ComboBoxColumn)cell.Column;

            if (cell.EnableCustomEditorBehaviors && (col.EditorDisplayBehavior == EditorDisplayBehaviors.EditMode))
            {
                this.ApplyBindingToDisplayElement(cell, cellBinding);

                // Our wrapping convertor will return an object from the col.ItemsSource, lets use the
                // DisplayMemberPath if we have it.
                if (!string.IsNullOrEmpty(col.DisplayMemberPath))
                {
                    string template = string.Format(CultureInfo.InvariantCulture, this._templateFormat, col.DisplayMemberPath);

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
            
            this._comboDisplay.IsEnabled = cell.IsEditable;
            this.SetupComboBox(this._comboDisplay, cell, cellBinding);

            bool isEditable = false;

            if (cell.EnableCustomEditorBehaviors)
            {
                isEditable = col.EditorDisplayBehavior == EditorDisplayBehaviors.Always;
            }

            this._comboDisplay.IsHitTestVisible = isEditable;

            this._comboDisplay.Focusable = isEditable;


            this._cellBinding = cellBinding;
            
            this.ApplyBindingToDisplayElement(cell, cellBinding);

            return this._comboDisplay;   
        }

        #endregion ResolveDisplayElement

        #region ApplyBindingToDisplayElement

        /// <summary>
        /// This is where a ColumnContentProvider should apply the Binding to their Display element.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="binding"></param>
        private void ApplyBindingToDisplayElement(Cell cell, Binding binding)
        {
            ComboBoxColumn col = (ComboBoxColumn)cell.Column;

            if (_comboDisplay.DisplayMemberPath != col.DisplayMemberPath)
            {
                if (col.DisplayMemberPath == null)
                    _comboDisplay.ClearValue(ComboBox.DisplayMemberPathProperty);
                else
                    _comboDisplay.DisplayMemberPath = col.DisplayMemberPath;
            }

            if (binding != null)
            {
                if (col.EditorDisplayBehavior == EditorDisplayBehaviors.EditMode)
                {
                    // Wrapping
                    var conv = new ComboBoxColumnValueConverter(col, binding.Converter);
                    binding.Converter = conv;

                    _displayElement.SetBinding(ContentControl.ContentProperty, binding);
                }
                else
                {
                    _comboEditor.SetBinding(ComboBox.SelectedValueProperty, binding);
                }

                Binding b = new Binding(col.SelectedValuePath ?? string.Empty);
                b.Mode = BindingMode.OneWay;
                _comboValueHolder.SetBinding(ValueHolder.ValueProperty, b);
            }
            else
            {
                _comboDisplay.ClearValue(ComboBox.SelectedValueProperty);
            }
        }

        #endregion // ApplyBindingToDisplayElement

        #region ResolveEditorControl

        /// <summary>
        /// Sets up the editor control that will be displayed in a <see cref="Cell"/> when the the cell is in edit mode.
        /// </summary>
        /// <param propertyName="cell">The <see cref="Cell"/> entering edit mode.</param>
        /// <param propertyName="editorValue">The value that should be put in the editor.</param>
        /// <param propertyName="availableWidth">The amount of horizontal space available.</param>
        /// <param propertyName="availableHeight">The amound of vertical space available.</param>
        /// <param propertyName="editorBinding">Provides a <see cref="Binding"/> that can be used for setting up the editor.</param>
        /// <returns></returns>
        protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
        {
            this._comboEditor.IsHitTestVisible = true;

            this._comboEditor.Focusable = true;


            this.SetupComboBox(this._comboEditor, cell, editorBinding);

            if (editorBinding != null)
            {
                this._comboEditor.SetBinding(ComboBox.SelectedValueProperty, editorBinding);
            }
            else
            {
                this._comboEditor.ClearValue(ComboBox.SelectedValueProperty);
            }

            this._comboEditor.SelectionChanged += ComboEditor_SelectionChanged;

            if (cell.IsEditing)
                this._comboEditor.Focus();

            return this._comboEditor;
        }

        #endregion // ResolveEditorControl

        #region ResolveValueFromEditor

        /// <summary>
        /// Resolves the value of the editor control, so that the cell's underlying data can be updated. 
        /// </summary>
        /// <param propertyName="cell">The <see cref="Cell"/> that the editor id being displayed in.</param>
        /// <returns>The value that should be displayed in the cell.</returns>
        public override object ResolveValueFromEditor(Cell cell)
        {
            return this._comboValueHolder.Value;
        }

        #endregion // ResolveValueFromEditor

        #region EditorRemoved

        /// <summary>
        /// Raised when the editor is removed from the cell.
        /// </summary>
        public override void EditorRemoved()
        {
            base.EditorRemoved();
            this._comboEditor.SelectionChanged -= ComboEditor_SelectionChanged;
        }

        #endregion // EditorRemoved

        #region AdjustDisplayElement
        /// <summary>
        /// Called during EnsureContent to allow the provider a chance to modify it's display based on the current conditions.
        /// </summary>
        /// <param name="cell"></param>
        public override void AdjustDisplayElement(Cell cell)
        {
            ComboBoxColumn col = (ComboBoxColumn)cell.Column;

            this._comboDisplay.ItemsSource = col.ItemsSource;

            Style comboStyle = cell.EditorStyleResolved;
            if (comboStyle != this._comboDisplay.Style)
            {
                if (comboStyle == null)
                    this._comboDisplay.ClearValue(Control.StyleProperty);
                else
                    this._comboDisplay.Style = comboStyle;
            }

            BindingExpression be = this._comboDisplay.GetBindingExpression(ComboBox.SelectedValueProperty);
            if (be == null)
            {
                if (this._cellBinding != null)
                {
                    if (cell.EnableCustomEditorBehaviors && (col.EditorDisplayBehavior != EditorDisplayBehaviors.EditMode))
                        _comboDisplay.SetBinding(ComboBox.SelectedValueProperty, _cellBinding);
                }
            }

            base.AdjustDisplayElement(cell);
        }
        #endregion // AdjustDisplayElement

        #region FocusEditor

        /// <summary>
        /// Calls the ContentProvider to Attempt to Focus the underlying editor control
        /// </summary>
        protected internal override void FocusEditor()
        {
            if (this._comboEditor != null)
                this._comboEditor.Focus();
        }

        #endregion

        #region ResolveBinding
        /// <summary>
        /// Builds the binding that will be used for a <see cref="Cell"/>
        /// </summary>
        /// <returns>If a binding cannot be created, null will be returned.</returns>
        protected internal override Binding ResolveBinding(Cell cell)
        {

            Binding b =  base.ResolveBinding(cell);





            return b;
        }
        #endregion // ResolveBinding

        #endregion // Overrides

        #region Event Handlers

        void ComboEditor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._comboValueHolder.DataContext = this._comboEditor.SelectedItem;

            object valueChanged = null;

            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                if (!string.IsNullOrEmpty(this._comboEditor.SelectedValuePath))
                {
                    valueChanged = this._comboEditor.SelectedValue;
                }
                else
                {
                    valueChanged = e.AddedItems[0];
                }
            }

            this.NotifyEditorValueChanged(valueChanged);
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

        #endregion // ValueHolder

        #region ComboBoxColumnValueConverter

        private class ComboBoxColumnValueConverter : IValueConverter
        {
            #region Members

            private readonly string _selectedValuePath;
            private readonly IValueConverter _wrappedConverter;
            private readonly ComboBoxColumn _column;

            #endregion // Members

            #region Constructor

            public ComboBoxColumnValueConverter(ComboBoxColumn column, IValueConverter wrappedConverter)
            {
                this._column = column;
                this._selectedValuePath = column.SelectedValuePath;
                this._wrappedConverter = wrappedConverter;
            }

            #endregion // Constructor

            #region Methods

            #region Convert

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                object convertedValue = this.ApplyConvertion(value, targetType, parameter, culture);
                object resolveValueBack = this.ResolveValueBack(convertedValue);

                return resolveValueBack;
            }

            #endregion // Convert

            #region ConvertBack

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                object convertedValue = this.ApplyBackConvertion(value, targetType, parameter, culture);
                object resolveValueBack = this.ResolveValueBack(convertedValue);

                return resolveValueBack;
            }

            #endregion // ConvertBack

            #region ResolveValue

            private object ResolveValue(object value)
            {
                if (!string.IsNullOrEmpty(_selectedValuePath) && value != null)
                {
                    return DataManagerBase.ResolveValueFromPropertyPath(_selectedValuePath, value);
                }

                return value;
            }

            #endregion // ResolveValue

            #region ResolveValueBack

            private object ResolveValueBack(object value)
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
                        Type selectedValuePathPropertyType = null;

                        try
                        {
                            selectedValuePathPropertyType = DataManagerBase.ResolvePropertyTypeFromPropertyName(this._selectedValuePath, dm.CachedTypedInfo);
                        }
                        catch
                        {
                            // probably an illegal type, or the indexer is of type object and they want to access a property off of it.
                            return null;
                        }

                        ComboBoxItemsFilter filter = new ComboBoxItemsFilter(dm.CachedType, dm.CachedTypedInfo, this._selectedValuePath, selectedValuePathPropertyType);

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

            private object ApplyConvertion(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (_wrappedConverter != null)
                {
                    return _wrappedConverter.Convert(value, targetType, parameter, culture);
                }

                return value;
            }

            #endregion // ApplyConvertion

            #region ApplyBackConvertion

            private object ApplyBackConvertion(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (_wrappedConverter != null)
                {
                    return _wrappedConverter.ConvertBack(value, targetType, parameter, culture);
                }

                return value;
            }

            #endregion // ApplyBackConvertion

            #endregion // Methods
        }

        #endregion // ComboBoxColumnValueConverter

        #region ComboBoxItemsFilter

        private class ComboBoxItemsFilter : RowsFilter
        {
            #region Members

            private readonly string _fieldName;
            private readonly Type _fieldType;

            #endregion // Members

            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="ComboBoxItemsFilter"/> class.
            /// </summary>
            /// <param name="objectType"></param>
            /// <param name="typeInfo"></param>
            /// <param name="fieldName"></param>
            /// <param name="fieldType"></param>
            public ComboBoxItemsFilter(Type objectType, CachedTypedInfo typeInfo, string fieldName, Type fieldType)
            {
                this._fieldName = fieldName;
                this._fieldType = fieldType;
                this.ObjectType = objectType;
                this.ObjectTypedInfo = typeInfo;
            }

            #endregion // Constructor

            #region Overrides

            /// <summary>
            /// Generates the current expression for this <see cref="ComboBoxItemsFilter"/>.
            /// </summary>
            protected override Expression GetCurrentExpression()
            {
                Expression exp = null;

                if (this.ObjectTypedInfo != null && this.FieldType != null)
                {
                    FilterContext context = FilterContext.CreateGenericFilter(this.ObjectTypedInfo, this.FieldType, false, false);

                    exp = this.GetCurrentExpression(context);
                }

                return exp;
            }

            #endregion // Overrides

            #region Properties

            #region FieldName

            /// <summary>
            /// Gets the field name of the property that is being filtered on.
            /// </summary>
            public override string FieldName
            {
                get
                {
                    return this._fieldName;
                }
            }

            #endregion // FieldName

            #region FieldType

            /// <summary>
            /// Gets the Type of the FieldName property.
            /// </summary>
            public override Type FieldType
            {
                get
                {
                    return this._fieldType;
                }
            }

            #endregion // FieldType

            #endregion // Properties
        }

        #endregion // ComboBoxItemsFilter

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