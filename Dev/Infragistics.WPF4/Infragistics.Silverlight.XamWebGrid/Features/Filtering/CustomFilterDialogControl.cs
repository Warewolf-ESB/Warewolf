using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// This control is allows the end user to make a simple range compound filter via the UI.  
    /// </summary>
    [TemplatePart(Name = "Operator1", Type = typeof(ComboBox))]
    [TemplatePart(Name = "Operator2", Type = typeof(ComboBox))]
    [TemplatePart(Name = "Operand1", Type = typeof(ContentControl))]
    [TemplatePart(Name = "Operand2", Type = typeof(ContentControl))]
    [TemplatePart(Name = "And", Type = typeof(RadioButton))]
    [TemplatePart(Name = "Or", Type = typeof(RadioButton))]
    public class ColumnFilterDialogControl : Control, ICommandTarget, INotifyPropertyChanged
    {
        #region Members
        Popup _dialogPopup;
        CellBase _cell;
        ComboBox _operator1;
        ComboBox _operator2;
        RadioButton _andButton;
        RadioButton _orButton;
        ContentControl _operand1;
        ContentControl _operand2;
        ProxyValueContainer _proxyValues = new ProxyValueContainer();
        ColumnContentProviderBase _contentProviderForOperand1;
        ColumnContentProviderBase _contentProviderForOperand2;
        Cell _cellForOperand1;
        Cell _cellForOperand2;
        bool _columnChooserWasOpen = false;
        bool _isOpen;
        UIElement _rootVis;
        List<FilterOperand> _defaultFilterOperand;
        #endregion // Members

        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="ColumnFilterDialogControl"/> class.
        /// </summary>
        static ColumnFilterDialogControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColumnFilterDialogControl), new FrameworkPropertyMetadata(typeof(ColumnFilterDialogControl)));
            IsTabStopProperty.OverrideMetadata(typeof(ColumnFilterDialogControl), new FrameworkPropertyMetadata(false));
            FocusableProperty.OverrideMetadata(typeof(ColumnFilterDialogControl), new FrameworkPropertyMetadata(false));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnFilterDialogControl"/> class.
        /// </summary>
        public ColumnFilterDialogControl()
        {




            this._dialogPopup = new Popup();


            this._dialogPopup.Placement = PlacementMode.Relative;
            this._dialogPopup.AllowsTransparency = true;
            this._dialogPopup.StaysOpen = false;
            this._dialogPopup.Closed += new EventHandler(DialogPopup_Closed);
            this._dialogPopup.Opened += new EventHandler(DialogPopup_Opened);


            this._dialogPopup.SizeChanged += new SizeChangedEventHandler(DialogPopup_SizeChanged);

            this.CancelButtonText = SRGrid.GetString("CustomFilterDialog_CancelButtonText");
            this.OKButtonText = SRGrid.GetString("CustomFilterDialog_OKButtonText");
            this.AndRadioButtonText = SRGrid.GetString("CustomFilterDialog_AndRadioButtonText");
            this.OrRadioButtonText = SRGrid.GetString("CustomFilterDialog_OrRadioButtonText");
        }

        #endregion // Constructor

        #region Event Handlers

        #region Grid_SizeChanged

        void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Hide();
        }

        #endregion // Grid_SizeChanged

        #region DialogPopup_SizeChanged

        void DialogPopup_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.AlignPopup();
        }

        #endregion // DialogPopup_SizeChanged

        #region ColumnFilterDialogControl_SizeChanged

        void ColumnFilterDialogControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.AlignPopup();
        }

        #endregion // ColumnFilterDialogControl_SizeChanged

        #region Operator2_SelectionChanged

        void Operator2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                FilterOperand fo = e.AddedItems[0] as FilterOperand;
                if (fo != null)
                {
                    this._operand2.IsEnabled = fo.RequiresFilteringInput;
                }
            }
            else
            {
                this._operand2.IsEnabled = true;
            }
        }

        #endregion // Operator2_SelectionChanged

        #region Operator1_SelectionChanged

        void Operator1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                FilterOperand fo = e.AddedItems[0] as FilterOperand;
                if (fo != null)
                {
                    this._operand1.IsEnabled = fo.RequiresFilteringInput;
                }
            }
            else
            {
                this._operand1.IsEnabled = true;
            }
        }

        #endregion // Operator1_SelectionChanged

        #region MouseLeftButtonDown
        void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {


#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

        }
        #endregion // MouseLeftButtonDown

        #region KeyDown

        /// <summary>
        /// Called before the <see cref="UIElement.KeyDown"/> event occurs.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The data for the event.</param>
        void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.Key == Key.Escape)
                {
                    this.Hide();
                    e.Handled = true;
                    return;
                }
                if (e.Key == Key.Enter)
                {
                    this.Hide();

                    if (this._contentProviderForOperand1 != null)
                        this._proxyValues.ProxyValue1 = _contentProviderForOperand1.ResolveValueFromEditor(this._cellForOperand1 as Cell);
                    if (this._contentProviderForOperand2 != null)
                        this._proxyValues.ProxyValue2 = this._contentProviderForOperand2.ResolveValueFromEditor(this._cellForOperand2 as Cell);

                    this.AcceptChanges();
                    e.Handled = true;
                    return;
                }
            }
        }
        #endregion // KeyDown

        #region DialogPopup_Closed

        void DialogPopup_Closed(object sender, EventArgs e)
        {
            this.Hide();
        }

        #endregion // DialogPopup_Closed

        #region DialogPopup_Opened

        void DialogPopup_Opened(object sender, EventArgs e)
        {
            this.AlignPopup();

            


            this.Dispatcher.BeginInvoke(new Action(
                () =>
                    {
                        if (this._operator1 != null)
                        {
                            this._operator1.Focus();
                        }
                    }));
        }

        #endregion // DialogPopup_Opened

        #endregion // Event Handlers

        #region Methods

        #region Private

        private void ResetControls()
        {
            FilterOperandCollection foc = this.Cell.Column.FilterColumnSettings.RowFilterOperands;

            if (this._operator1 != null)
            {
                this._operator1.ItemsSource = null;
                this._operator1.ItemsSource = foc;
            }

            if (this._operator2 != null)
            {
                this._operator2.ItemsSource = null;
                this._operator2.ItemsSource = foc;
            }

            if (this._operand1 != null)
            {
                this._proxyValues.ProxyValue1 = null;
            }

            if (this._operand2 != null)
            {
                this._proxyValues.ProxyValue2 = null;
            }
        }

        private void AlignPopup()
        {
            if (this._dialogPopup.IsOpen)
            {
                XamGrid grid = this.Cell.Row.ColumnLayout.Grid;

                double halfTheGridWidth = grid.ActualWidth / 2;
                double halfTheGridHeight = grid.ActualHeight / 2;

                double halfThePopupWidth = this._dialogPopup.Child.DesiredSize.Width / 2;
                double halfThePopupHeight = this._dialogPopup.Child.DesiredSize.Height / 2;

                double horizontalOffset = halfTheGridWidth - halfThePopupWidth;
                this._dialogPopup.HorizontalOffset = (horizontalOffset < 0 ? 0 : horizontalOffset);
                double verticalOffset = halfTheGridHeight - halfThePopupHeight;
                this._dialogPopup.VerticalOffset = (verticalOffset < 0 ? 0 : verticalOffset);



#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)

                UIElement root = PlatformProxy.GetRootVisual(this._dialogPopup);
                Rect screen = new Rect(0, 0, root.RenderSize.Width, root.RenderSize.Height);

                



                if (this._dialogPopup.HorizontalOffset > screen.Right)
                {
                    this._dialogPopup.HorizontalOffset = (screen.Right / 2) - halfThePopupWidth;
                }
                if (this._dialogPopup.VerticalOffset > screen.Bottom)
                {
                    this._dialogPopup.VerticalOffset = (screen.Bottom / 2) - halfThePopupHeight;
                }

            }
        }

        private void SetupCurrentView()
        {
            if (this.Cell == null)
                return;
            RowsManager rm = this.Cell.Row.Manager as RowsManager;
            if (rm != null)
            {
                RowsFilter rf = rm.RowFiltersCollectionResolved[this.Cell.Column.Key];

                if (rf == null && rm.DataManager != null)
                {
                    rf = new RowsFilter(rm.DataManager.CachedType, this.Cell.Column);
                    rm.RowFiltersCollectionResolved.Add(rf);
                }

                if (rf == null)
                    return;
                
                bool andCondtion = rf.Conditions.LogicalOperator == LogicalOperator.And;
                if (this._defaultFilterOperand != null && this._defaultFilterOperand.Count > 1)
                {
                    andCondtion = true;
                }

                if (andCondtion)
                {
                    if (this._andButton != null)
                        this._andButton.IsChecked = true;
                }
                else
                {
                    if (this._orButton != null)
                        this._orButton.IsChecked = true;
                }

                


                this.ResetControls();

                



                if (this._defaultFilterOperand != null && this._defaultFilterOperand.Count > 0)
                {
                    if (this._operator1 != null && this._operand1 != null)
                    {
                        foreach (object ccb in this._operator1.ItemsSource)
                        {
                            FilterOperand fo = ccb as FilterOperand;

                            if (fo.GetType() == _defaultFilterOperand[0].GetType())
                            {
                                this._operator1.SelectedItem = ccb;
                                break;
                            }
                        }
                    }
                    if (this._defaultFilterOperand.Count == 2)
                    {
                        if (this._operator2 != null && this._operand2 != null)
                        {
                            foreach (object ccb in this._operator2.ItemsSource)
                            {
                                FilterOperand fo = ccb as FilterOperand;

                                if (fo.GetType() == _defaultFilterOperand[1].GetType())
                                {
                                    this._operator2.SelectedItem = ccb;
                                    break;
                                }
                            }
                        }
                    }
                }

                int rfConditionsCount = rf.Conditions.Count;

                bool prepopulateFilterValueBoxes = rfConditionsCount > 0 && rfConditionsCount < 3;

                if (prepopulateFilterValueBoxes && this._defaultFilterOperand != null && this._defaultFilterOperand.Count != rfConditionsCount)
                {
                    prepopulateFilterValueBoxes = false;
                }

                if (prepopulateFilterValueBoxes)
                {
                    ComparisonCondition cc = rf.Conditions[0] as ComparisonCondition;
                    if (cc != null)
                    {
                        if (this._operator1 != null && this._operand1 != null)
                        {
                            foreach (object ccb in this._operator1.ItemsSource)
                            {
                                FilterOperand fo = ccb as FilterOperand;

                                if (this._defaultFilterOperand != null && this._defaultFilterOperand.Count > 0)
                                {
                                    if (fo.GetType() != this._defaultFilterOperand[0].GetType())
                                    {
                                        continue;
                                    }
                                }

                                if (fo.ComparisonOperatorValue != null)
                                {
                                    if ((ComparisonOperator)fo.ComparisonOperatorValue == cc.Operator)
                                    {
                                        this._operator1.SelectedItem = ccb;
                                        object value = cc.FilterValue;
                                        if (value != null)
                                        {
                                            this._proxyValues.ProxyValue1 = value;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        CustomComparisonCondition ccc = rf.Conditions[0] as CustomComparisonCondition;
                        if (ccc != null)
                        {
                            if (this._operand1 != null && this._operator1 != null)
                            {
                                foreach (object ccb in this._operator1.ItemsSource)
                                {
                                    if (ccb.GetType() == ccc.FilterOperand)
                                    {
                                        this._operator1.SelectedItem = ccb;
                                        object value = ccc.FilterValue;
                                        if (value != null)
                                        {
                                            this._proxyValues.ProxyValue1 = value;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (rfConditionsCount == 2)
                    {
                        ComparisonCondition cccc = rf.Conditions[1] as ComparisonCondition;
                        if (cccc != null)
                        {
                            if (this._operator2 != null && this._operand2 != null)
                            {
                                foreach (object ccb in this._operator2.ItemsSource)
                                {
                                    FilterOperand fo = ccb as FilterOperand;

                                    if (this._defaultFilterOperand != null && this._defaultFilterOperand.Count > 1)
                                    {
                                        if (fo.GetType() != this._defaultFilterOperand[1].GetType())
                                        {
                                            continue;
                                        }
                                    }

                                    if (fo.ComparisonOperatorValue != null)
                                    {
                                        if ((ComparisonOperator)fo.ComparisonOperatorValue == cccc.Operator)
                                        {
                                            this._operator2.SelectedItem = ccb;
                                            object value = cccc.FilterValue;
                                            if (value != null)
                                            {
                                                this._proxyValues.ProxyValue2 = value;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            CustomComparisonCondition ccc = rf.Conditions[1] as CustomComparisonCondition;
                            if (ccc != null)
                            {
                                if (this._operator2 != null && this._operand2 != null)
                                {
                                    foreach (object ccb in this._operator2.ItemsSource)
                                    {
                                        if (ccb.GetType() == ccc.FilterOperand)
                                        {
                                            this._operator2.SelectedItem = ccb;
                                            object value = ccc.FilterValue;
                                            if (value != null)
                                            {
                                                this._proxyValues.ProxyValue2 = value;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }

        #endregion // Private

        #region Public

        #region AcceptChanges

        /// <summary>
        /// Processes the elements in the control.
        /// </summary>
        protected internal virtual void AcceptChanges()
        {
            RowsManager rm = this.Cell.Row.Manager as RowsManager;
            if (rm != null)
            {
                RowsFilter rowsFilter = rm.RowFiltersCollectionResolved[Cell.Column.Key];

                if (rowsFilter == null)
                    return;

                bool needInvalidate = rowsFilter.Conditions.Count > 0;

                rowsFilter.Conditions.ClearSilently();

                

                if (this._cell.Column is TemplateColumn || this._cell.Column is UnboundColumn)
                {
                    List<Infragistics.Controls.Grids.CellControl.BindingDataInfo> editorBindings = CellControl.ResolveBindingsFromChildren(_operand1, true);

                    if (editorBindings != null)
                    {
                        foreach (Infragistics.Controls.Grids.CellControl.BindingDataInfo data in editorBindings)
                        {
                            data.Expression.UpdateSource();
                        }
                    }

                    editorBindings = CellControl.ResolveBindingsFromChildren(_operand2, true);

                    if (editorBindings != null)
                    {
                        foreach (Infragistics.Controls.Grids.CellControl.BindingDataInfo data in editorBindings)
                        {
                            data.Expression.UpdateSource();
                        }
                    }
                }

                needInvalidate |= this.ProcessOperatorOperand(this._operator1, this._operand1, this._proxyValues.ProxyValue1, rm, this._contentProviderForOperand1);

                needInvalidate |= this.ProcessOperatorOperand(this._operator2, this._operand2, this._proxyValues.ProxyValue2, rm, this._contentProviderForOperand2);

                if (this._andButton != null && this._andButton.IsChecked == true)
                {
                    rowsFilter.Conditions.LogicalOperator = LogicalOperator.And;
                }
                else if (this._orButton != null && this._orButton.IsChecked == true)
                {
                    rowsFilter.Conditions.LogicalOperator = LogicalOperator.Or;
                }

                if (needInvalidate)
                    rm.ColumnLayout.InvalidateFiltering();
            }
        }

        #endregion // AcceptChanges

        #region Show

        /// <summary>
        /// Shows the popup
        /// </summary>
        /// <param name="initialOperands">A List of <see cref="FilterOperand"/> objects which this control will attempt to use to prepopulate
        /// the operand dropdowns with.
        /// </param>
        public virtual void Show(List<FilterOperand> initialOperands)
        {
            this._defaultFilterOperand = initialOperands;
            this.Show();
        }

        /// <summary>
        /// Shows the popup
        /// </summary>
        public virtual void Show()
        {
            if (this._dialogPopup != null && !this._isOpen)
            {
                this._isOpen = true;

                this._dialogPopup.Child = this;

                if (!this.Cell.Row.ColumnLayout.Grid.Panel.Children.Contains(this._dialogPopup))
                {
                    this.Cell.Row.ColumnLayout.Grid.Panel.Children.Add(this._dialogPopup);
                }

                FrameworkElement fe = (FrameworkElement)this._dialogPopup.Child;

                if (fe != null)
                    fe.SizeChanged += ColumnFilterDialogControl_SizeChanged;

                this._dialogPopup.IsOpen = true;

                this.SetupCurrentView();

                EditableColumn ec = this.Cell.Column as EditableColumn;

                if (this._operand1 != null)
                {
                    this._operand1.Content = GetEditorForOperand("ProxyValue1", out _contentProviderForOperand1, out this._cellForOperand1);
                    if (this._operand1 != null && ec != null)
                    {
                        this._operand1.HorizontalContentAlignment = ec.EditorHorizontalContentAlignment;
                    }
                }
                if (this._operand2 != null)
                {
                    this._operand2.Content = GetEditorForOperand("ProxyValue2", out _contentProviderForOperand2, out this._cellForOperand2);
                    if (this._operand2 != null && ec != null)
                    {
                        this._operand2.HorizontalContentAlignment = ec.EditorHorizontalContentAlignment;
                    }
                }

                XamGrid grid = this.Cell.Row.ColumnLayout.Grid;

                VisualStateManager.GoToState(grid, "ReadOnly", false);

                UIElement rootVis = PlatformProxy.GetRootVisual(grid);
                this._rootVis = rootVis;

                rootVis.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(this.KeyDownHandler), true);
                rootVis.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.MouseLeftButtonDownHandler), true);
                
                grid.SizeChanged += Grid_SizeChanged;

                if (grid.ColumnChooserDialog.IsOpen)
                {
                    grid.ColumnChooserDialog.IsOpen = false;
                    this._columnChooserWasOpen = true;
                }

                this.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(this.KeyDownHandler), true);






            }
        }

        #endregion // Show

        #region Hide

        /// <summary>
        /// Hides the popup.
        /// </summary>
        public virtual void Hide()
        {
            this._defaultFilterOperand = null;

            if (this._dialogPopup != null && this._isOpen)
            {
                this._dialogPopup.IsOpen = false;
                this._isOpen = false;

                FrameworkElement fe = (FrameworkElement)this._dialogPopup.Child;
                if (fe != null)
                    fe.SizeChanged -= ColumnFilterDialogControl_SizeChanged;

                XamGrid grid = this.Cell.Row.ColumnLayout.Grid;

                if (this._rootVis != null)
                {
                    this._rootVis.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(this.KeyDownHandler));
                    this._rootVis.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.MouseLeftButtonDownHandler));
                }
                
                this.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(this.KeyDownHandler));
                grid.SizeChanged -= Grid_SizeChanged;
                this._dialogPopup.Child = null;

                if (this.Cell.Row.ColumnLayout.Grid.Panel.Children.Contains(this._dialogPopup))
                {
                    this.Cell.Row.ColumnLayout.Grid.Panel.Children.Remove(this._dialogPopup);
                }

                VisualStateManager.GoToState(grid, "Active", false);
            }

            if (this._columnChooserWasOpen)
            {
                if(this.Cell != null && this.Cell.Row != null && this.Cell.Row.ColumnLayout != null)
                    this.Cell.Row.ColumnLayout.Grid.ColumnChooserDialog.IsOpen = true;

                this._columnChooserWasOpen = false;
            }
        }

        #endregion // Hide

        #endregion // Public

        #region Protected

        #region SupportsCommand

        /// <summary>
        /// Returns if the object will support a given command type.
        /// </summary>
        /// <param name="command">The command to be validated.</param>
        /// <returns>True if the object recognizes the command as actionable against it.</returns>
        protected virtual bool SupportsCommand(ICommand command)
        {
            return command is AcceptCustomFilterDialogChangesCommand ||
                    command is CloseCustomFilterDialogCommand;
        }

        #endregion // SupportsCommand

        #region GetParameter

        /// <summary>
        /// Returns the object that defines the parameters necessary to execute the command.
        /// </summary>
        /// <param name="source">The CommandSource object which defines the command to be executed.</param>
        /// <returns>The object necessary for the command to complete.</returns>
        protected virtual object GetParameter(CommandSource source)
        {
            return this;
        }

        #endregion // GetParameter

        #region ProcessOperatorOperand

        /// <summary>
        /// Builds a filter from the operator operand pair
        /// </summary>
        /// <param name="filterOperator"></param>
        /// <param name="operand"></param>
        /// <param name="proxyValue"></param>
        /// <param name="rowsManager" />
        /// <param name="contentProvider"></param>        
        protected virtual bool ProcessOperatorOperand(ComboBox filterOperator, ContentControl operand, object proxyValue, RowsManager rowsManager, ColumnContentProviderBase contentProvider)
        {
            bool needInvalidate = false;
            if (filterOperator != null)
            {
                FilterOperand fo = filterOperator.SelectedItem as FilterOperand;
                Column column = this.Cell.Column;
                if (fo != null)
                {
                    object value = null;

                    bool buildFilter = false;
                    if (fo.RequiresFilteringInput && operand != null)
                    {
                        try
                        {
                            Cell c = this.Cell as Cell;
                            if (c != null)
                                value = contentProvider.ResolveValueFromEditor(c);
                            if (value == null)
                                value = proxyValue;
                            if (column.DataType != typeof(object))
                                value = column.ResolveValue(value);

                            buildFilter = true;
                            needInvalidate = true;
                        }
                        catch (FormatException)
                        {
                            needInvalidate = true;
                        }
                        catch (InvalidCastException)
                        {
                            needInvalidate = true;
                        }
                    }
                    else if (!fo.RequiresFilteringInput)
                    {
                        buildFilter = true;
                        needInvalidate = true;
                    }
                    else
                    {
                        needInvalidate = true;
                    }
                    if (buildFilter)
                    {
                        if (column.DataType == typeof(bool?) && value == null)
                        {
                            rowsManager.ColumnLayout.BuildNullableFilter(rowsManager.RowFiltersCollectionResolved, column, fo, true, false, false);
                        }
                        else
                        {
                            rowsManager.ColumnLayout.BuildFilters(rowsManager.RowFiltersCollectionResolved, value, column, fo, true, false, true);
                            column.ColumnLayout.Grid.OnFiltered(rowsManager.RowFiltersCollectionResolved);
                        }
                    }
                }
            }
            return needInvalidate;
        }
        #endregion // ProcessOperatorOperand

        #endregion // Protected

        #endregion // Methods

        #region Properties

        #region Cell

        /// <summary>
        /// Gets / sets the <see cref="CellBase"/> object which hosts the <see cref="ColumnFilterDialogControl"/>.
        /// </summary>
        public CellBase Cell
        {
            get
            {
                return this._cell;
            }
            set
            {
                if (this._cell != value)
                {
                    this._cell = value;
                    this._defaultFilterOperand = null;
                }

                Style s = null;

                if (this._cell != null)
                {
                    s = this.Cell.Column.ColumnLayout.FilteringSettings.CustomFilterDialogStyleResolved;
                }

                ColumnContentProviderBase.SetControlStyle(this, s);
            }
        }

        #endregion // Cell

        #region CancelButtonText

        /// <summary>
        /// Identifies the <see cref="CancelButtonText"/> dependency property. 
        /// </summary>                
        public static readonly DependencyProperty CancelButtonTextProperty = DependencyProperty.Register("CancelButtonText", typeof(string), typeof(ColumnFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(CancelButtonTextChanged)));


        /// <summary>
        /// Gets / sets the text for the cancel buttom.
        /// </summary>
        public string CancelButtonText
        {
            get { return (string)this.GetValue(CancelButtonTextProperty); }
            set { this.SetValue(CancelButtonTextProperty, value); }
        }

        private static void CancelButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnFilterDialogControl fsc = (ColumnFilterDialogControl)obj;
            fsc.OnPropertyChanged("CancelButtonText");
        }

        #endregion // CancelButtonText

        #region OKButtonText

        /// <summary>
        /// Identifies the <see cref="OKButtonText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty OKButtonTextProperty = DependencyProperty.Register("OKButtonText", typeof(string), typeof(ColumnFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(OKButtonTextChanged)));

        /// <summary>
        /// Gets / sets the text for the OK Button.
        /// </summary>
        public string OKButtonText
        {
            get { return (string)this.GetValue(OKButtonTextProperty); }
            set { this.SetValue(OKButtonTextProperty, value); }
        }

        private static void OKButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnFilterDialogControl fsc = (ColumnFilterDialogControl)obj;
            fsc.OnPropertyChanged("OKButtonText");
        }

        #endregion // OKButtonText

        #region AndRadioButtonText

        /// <summary>
        /// Identifies the <see cref="AndRadioButtonText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AndRadioButtonTextProperty = DependencyProperty.Register("AndRadioButtonText", typeof(string), typeof(ColumnFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(AndRadioButtonTextChanged)));

        /// <summary>
        /// Gets / sets the text for the "And" string.
        /// </summary>
        public string AndRadioButtonText
        {
            get { return (string)this.GetValue(AndRadioButtonTextProperty); }
            set { this.SetValue(AndRadioButtonTextProperty, value); }
        }

        private static void AndRadioButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnFilterDialogControl ctrl = (ColumnFilterDialogControl)obj;
            ctrl.OnPropertyChanged("AndRadioButtonText");
        }

        #endregion // AndRadioButtonText 
				
        #region OrRadioButtonText

        /// <summary>
        /// Identifies the <see cref="OrRadioButtonText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty OrRadioButtonTextProperty = DependencyProperty.Register("OrRadioButtonText", typeof(string), typeof(ColumnFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(OrRadioButtonTextChanged)));
        
        /// <summary>
        /// Gets / sets the text for the "Or" string.
        /// </summary>
        public string OrRadioButtonText
        {
            get { return (string)this.GetValue(OrRadioButtonTextProperty); }
            set { this.SetValue(OrRadioButtonTextProperty, value); }
        }

        private static void OrRadioButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnFilterDialogControl ctrl = (ColumnFilterDialogControl)obj;
            ctrl.OnPropertyChanged("OrRadioButtonText");
        }

        #endregion // OrRadioButtonText 
				

        #endregion // Properties

        #region Overrides

        #region OnApplyTemplate
        /// <summary>
        /// Builds the visual tree for the <see cref="ColumnFilterDialogControl"/>
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this._operator1 != null)
            {
                this._operator1.SelectionChanged -= Operator1_SelectionChanged;
            }
            this._operator1 = base.GetTemplateChild("Operator1") as ComboBox;
            if (this._operator1 != null)
            {
                this._operator1.SelectionChanged += Operator1_SelectionChanged;
            }

            if (this._operator2 != null)
            {
                this._operator2.SelectionChanged -= Operator2_SelectionChanged;
            }
            this._operator2 = base.GetTemplateChild("Operator2") as ComboBox;
            if (this._operator2 != null)
            {
                this._operator2.SelectionChanged += Operator2_SelectionChanged;
            }

            this._andButton = base.GetTemplateChild("And") as RadioButton;
            this._orButton = base.GetTemplateChild("Or") as RadioButton;

            this._operand1 = base.GetTemplateChild("Operand1") as ContentControl;
            this._operand2 = base.GetTemplateChild("Operand2") as ContentControl;

            this.SetupCurrentView();





            if (this.Cell != null)
            {
                EditableColumn ec = this.Cell.Column as EditableColumn;

                if (this._operand1 != null)
                {
                    this._operand1.Content = GetEditorForOperand("ProxyValue1", out _contentProviderForOperand1, out this._cellForOperand1);
                    if (ec != null)
                        this._operand1.HorizontalContentAlignment = ec.EditorHorizontalContentAlignment;
                }

                if (this._operand2 != null)
                {
                    this._operand2.Content = GetEditorForOperand("ProxyValue2", out _contentProviderForOperand2, out this._cellForOperand2);
                    if (ec != null)
                        this._operand2.HorizontalContentAlignment = ec.EditorHorizontalContentAlignment;
                }
            }
        }

        #endregion // OnApplyTemplate

        #region GetEditorForOperand
        /// <summary>
        /// Gets the control that would act as the editor for this cell so that we can use it in the dialog.
        /// </summary>
        /// <param name="operandToLinkTo"></param>
        /// <param name="cellOutput"></param>
        /// <param name="contentProviderControl"></param>
        /// <returns></returns>
        private FrameworkElement GetEditorForOperand(string operandToLinkTo, out ColumnContentProviderBase contentProviderControl, out Cell cellOutput)
        {
            Column column = Cell.Column;

            ColumnLayout columnLayout = column.ColumnLayout;

            TemplateColumn tc = column as TemplateColumn;

            UnboundColumn uc = column as UnboundColumn;

            RowsManager rm = new RowsManager(0, columnLayout, null);

            RowsManager rowsManager = ((RowsManager)Cell.Row.Manager);

            FilterRow fr = new FilterRow(rm);

            FilterRowCell filterRowCell = new FilterRowCell(fr, column);
            Binding binding = this.ResolveEditorBinding(filterRowCell, this._proxyValues, operandToLinkTo);

            CellControl cc = new CellControl();
            filterRowCell.Control = cc;
            cc.OnAttached(filterRowCell);
            filterRowCell.Control.EnsureContent();

            ColumnContentProviderBase contentProvider = Cell.Column.GenerateContentProvider();

            object editorValue = operandToLinkTo == "ProxyValue1" ? this._proxyValues.ProxyValue1 : this._proxyValues.ProxyValue2;

            FrameworkElement editor = contentProvider.ResolveEditor(filterRowCell, null, editorValue, 200, 200, binding);

            if (tc != null)
            {
                if (tc.FilterEditorTemplate != null)
                {
                    fr.SetData(rowsManager.GenerateNewObject(RowType.FilterRow));
                    CustomFilterDialogFilteringDataContext fdc = new CustomFilterDialogFilteringDataContext(filterRowCell, this._proxyValues, operandToLinkTo);
                    editor.DataContext = fdc;
                }
                else
                {
                    throw new ArgumentNullException(SRGrid.GetString("TemplateColumn_RequireFilterEditorTemplate"));
                }
            }
            else if (uc != null)
            {
                if (uc.FilterEditorTemplate != null)
                {
                    fr.SetData(rowsManager.GenerateNewObject(RowType.FilterRow));
                    CustomFilterDialogFilteringDataContext fdc = new CustomFilterDialogFilteringDataContext(filterRowCell, this._proxyValues, operandToLinkTo);
                    editor.DataContext = fdc;
                }
                else
                {
                    throw new ArgumentNullException(SRGrid.GetString("UnboundColumn_RequireFilterEditorTemplate"));
                }
            }

            if (editor != null)
            {
                editor.HorizontalAlignment = HorizontalAlignment.Stretch;
            }        

            contentProviderControl = contentProvider;

            cellOutput = filterRowCell;

            return editor;
        }
        #endregion // GetEditorForOperand

        #region ResolveEditorBinding

        /// <summary>
        /// Get the binding object that should be applied to the editors.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="data"></param>
        /// <param name="objectToLinkTo"></param>
        /// <returns></returns>
        protected internal virtual Binding ResolveEditorBinding(CellBase cell, object data, string objectToLinkTo)
        {
            Binding binding = null;

            if (cell.Control != null)
            {
                binding = cell.Control.ResolveEditorBinding();
            }
            if (binding == null)
            {
                if (cell.Column.Key != null)
                {
                    binding = new Binding(objectToLinkTo);
                    binding.Source = data;
                    binding.ConverterCulture = CultureInfo.CurrentCulture;

                    binding.Mode = BindingMode.TwoWay;
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.Default;

                    binding.ConverterParameter = cell;
                    binding.Converter = new Infragistics.Controls.Grids.Cell.CellEditingBindingConverter();

                    EditableColumn col = cell.Column as EditableColumn;
                    if (col != null)
                    {
                        binding.ValidatesOnDataErrors = binding.ValidatesOnExceptions = binding.NotifyOnValidationError = col.AllowEditingValidation;



                    }
                }
            }

            return binding;
        }

        #endregion // ResolveEditorBinding

        #endregion // Overrides

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return command is AcceptCustomFilterDialogChangesCommand ||
                    command is CloseCustomFilterDialogCommand;
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this.GetParameter(source);
        }

        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Event raised when a property on this object changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="name"></param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }

    #region ProxyValueContainer
    /// <summary>
    /// Class designed to hold two object types for databinding purposes.
    /// </summary>
    public class ProxyValueContainer : INotifyPropertyChanged
    {
        #region Members
        private object _proxyValue1;
        private object _proxyValue2;
        #endregion // Members

        /// <summary>
        /// Get / set the first object
        /// </summary>
        public object ProxyValue1
        {
            get
            {
                return this._proxyValue1;
            }
            set
            {
                if (this._proxyValue1 != value)
                {
                    this._proxyValue1 = value;
                    this.OnPropertyChanged("ProxyValue1");
                }
            }
        }

        /// <summary>
        /// Get / set the second object
        /// </summary>
        public object ProxyValue2
        {
            get
            {
                return this._proxyValue2;
            }
            set
            {
                if (this._proxyValue2 != value)
                {
                    this._proxyValue2 = value;
                    this.OnPropertyChanged("ProxyValue2");
                }
            }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fired when a property changes on the <see cref="ProxyValueContainer"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a property changes on the <see cref="ProxyValueContainer"/> object.
        /// </summary>
        /// <param name="name">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
    #endregion // ProxyValueContainer

    #region CustomFilterDialogFilteringDataContext
    /// <summary>
    /// Classed derived from <see cref="FilteringDataContext"/> used by <see cref="ColumnFilterDialogControl"/> for filtering.
    /// </summary>
    public class CustomFilterDialogFilteringDataContext : FilteringDataContext
    {
        #region Properties

        #region Private

        private ProxyValueContainer Container { get; set; }

        private string PropertyName { get; set; }

        #endregion // Private

        #endregion // Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomFilterDialogFilteringDataContext"/> class.
        /// </summary>
        public CustomFilterDialogFilteringDataContext(FilterRowCell frc, ProxyValueContainer proxyValueContainer, string propName)
            : base(frc)
        {
            this.PropertyName = propName;
            this.Container = proxyValueContainer;

            if (this.PropertyName == "ProxyValue1")
            {
                this.SetValueSilent(this.Container.ProxyValue1);
            }
            else if (this.PropertyName == "ProxyValue2")
            {
                this.SetValueSilent(this.Container.ProxyValue2);
            }
        }
        #endregion // Constructor

        #region OnValueChanged

        /// <summary>
        /// Method called when the value is modified so that the underlying object can be updated as well.
        /// </summary>
        protected override void OnValueChanged()
        {
            if (this.PropertyName == "ProxyValue1")
            {
                this.Container.ProxyValue1 = this.Value;
            }
            else if (this.PropertyName == "ProxyValue2")
            {
                this.Container.ProxyValue2 = this.Value;
            }
        }

        #endregion // OnValueChanged
    }
    #endregion // CustomFilterDialogFilteringDataContext
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