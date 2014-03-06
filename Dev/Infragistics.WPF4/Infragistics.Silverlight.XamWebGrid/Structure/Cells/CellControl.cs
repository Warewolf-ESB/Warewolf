using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using Infragistics.Controls.Grids.Primitives;
using System.Collections;
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// Visual object for the <see cref="Cell"/> object.
	/// </summary>
	[TemplateVisualState(GroupName = "ActiveStates", Name = "Active")]
	[TemplateVisualState(GroupName = "ActiveStates", Name = "InActive")]

	[TemplateVisualState(GroupName = "SelectedStates", Name = "NotSelected")]
	[TemplateVisualState(GroupName = "SelectedStates", Name = "Selected")]

	[TemplateVisualState(GroupName = "FixedStates", Name = "Fixed")]
	[TemplateVisualState(GroupName = "FixedStates", Name = "UnFixed")]

	[TemplateVisualState(GroupName = "EditingStates", Name = "Editing")]
	[TemplateVisualState(GroupName = "EditingStates", Name = "NotEditing")]
	public class CellControl : CellControlBase
	{
		#region Members

		ColumnContentProviderBase _content;
		bool _bindingError;
		double _columnActualWidth, _rowActualHeight;
		Thickness _padding;
		List<BindingDataInfo> _editorBindings;
		int _colReadOnlyIsDirtyFlag;
        IValueConverter _currentConveter;

        ToolTip _tooltip;
        ColumnContentProviderBase _tooltipContentProvider;
        bool _wasColumnWidthSetAtEditTime;
		#endregion // Members

		#region Constructor


        /// <summary>
        /// Static constructor for the <see cref="CellControl"/> class.
        /// </summary>
        static CellControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CellControl), new FrameworkPropertyMetadata(typeof(CellControl)));
        }


		/// <summary>
		/// Initializes a new instance of the <see cref="CellControl"/> class.
		/// </summary>
		public CellControl()
		{



		}

		#endregion // Constructor

		#region Overrides

		#region AttachContent

		/// <summary>
		/// Invoked when content is attached to the Control.
		/// </summary>
		protected override void AttachContent()
		{
            

            Cell c = (Cell)this.Cell;

            if (this._content == null)
            {
                this._content = c.Column.GenerateContentProvider();

                if (this._content != null)
                {
                    this._currentConveter = c.Column.ValueConverter;
                    Binding b = this._content.ResolveBinding(c);
                    this.Content = this._content.ResolveDisplayElementInternal(c, b);
                }
            }

            if (this._content != null)
            {
                if (c.IsEditing || (c.IsEditable && c.Row == c.Row.ColumnLayout.Grid.CurrentEditRow && !c.Row.ColumnLayout.Grid.CellsThatCancelledEditMode.Contains(c)))
                {
                    this.AddEditorToControl();
                }

                if (this._currentConveter != c.Column.ValueConverter)
                {
                    this._currentConveter = c.Column.ValueConverter;
                    Binding b = this._content.ResolveBinding(c);
                    this._content.ResolveDisplayElementInternal(c, b);
                }
            }

            if (c != null && c.IsActive)
            {
                Control content = this.Content as Control;
                if (content != null && content.IsHitTestVisible)
                    content.Focus();
            }

            EditableColumn col = this.Cell.Column as EditableColumn;
            if (col != null)
                this._colReadOnlyIsDirtyFlag = col.IsReadOnlyDirtyFlag;

		}
		#endregion // AttachContent

		#region EnsureContent

		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>
		internal protected override void EnsureContent()
		{
		    CellBase cell = this.Cell;
		    Column column = cell.Column;

            EditableColumn col = column as EditableColumn;

            if (col != null && this._content != null && this._colReadOnlyIsDirtyFlag != col.IsReadOnlyDirtyFlag)
            {
                Cell c = (Cell)cell;
                Binding b = this._content.ResolveBinding(c);
                this._content.ResolveDisplayElementInternal(c, b);
                this._colReadOnlyIsDirtyFlag = col.IsReadOnlyDirtyFlag;
            }

		    RowBase row = cell.Row;
		    XamGrid grid = row.ColumnLayout.Grid;
			if (col != null && cell.IsEditable && (grid.CurrentEditCell == cell || grid.CurrentEditRow == row))
			{
                if (row is AddNewRow && col.AddNewRowEditorTemplate != null)
                {
                    this.HorizontalContentAlignment = col.AddNewRowEditorTemplateHorizontalContentAlignment;
                    this.VerticalContentAlignment = col.AddNewRowEditorTemplateVerticalContentAlignment;
                }
                else
                {
                    this.HorizontalContentAlignment = col.EditorHorizontalContentAlignment;
                    this.VerticalContentAlignment = col.EditorVerticalContentAlignment;
                }
			}
            else if (column != null && (grid.CurrentEditCell == cell || grid.CurrentEditRow == row) && row.RowType == RowType.AddNewRow && cell.Column.AddNewRowEditorTemplate != null)
            {
                this.HorizontalContentAlignment = column.AddNewRowEditorTemplateHorizontalContentAlignment;
                this.VerticalContentAlignment = column.AddNewRowEditorTemplateVerticalContentAlignment;
            }
            else if (column != null)
            {
                if (row is AddNewRow && column.AddNewRowItemTemplate != null)
                {
                    this.HorizontalContentAlignment = column.AddNewRowItemTemplateHorizontalContentAlignment;
                    this.VerticalContentAlignment = column.AddNewRowItemTemplateVerticalContentAlignment;
                }
                else if (row is SummaryRow)
                {
                    // Do not overwrite the Horizontal and VerticalContentAlignment for SummaryRowCellControls
                }
                else
                {
                    this.HorizontalContentAlignment = column.HorizontalContentAlignment;
                    this.VerticalContentAlignment = column.VerticalContentAlignment;
                }
            }

			if (this._content != null)
			{
				this._content.AdjustDisplayElement((Cell)cell);
			}

            if (column != null && column.CachedAllowToolTips == AllowToolTips.Always)
            {
                this.ShowToolTip();
            }
            else
            {
                this.HideToolTip();
            }
		}

		#endregion // EnsureContent

		#region OnLoaded

		/// <summary>
		/// Raised when the <see cref="CellControl"/> is Loaded. 
		/// </summary>
		protected override void OnLoaded()
		{
			base.OnLoaded();

			if (this.Cell != null)
				this.Cell.EnsureCurrentState();
		}

		#endregion OnLoaded

		#region ShowResizingArrow
		/// <summary>
		/// Gets if a resizing arrow should be shown.
		/// </summary>
		protected override bool ShowResizingArrow
		{
			get
			{
				return this.Cell.Column.ColumnLayout.ColumnResizingSettings.AllowCellAreaResizingResolved;
			}
		}
		#endregion // ShowResizingArrow

        #region ReleaseContent

        /// <summary>
        /// Invoked before content is released from the control.
        /// </summary>
        protected override void ReleaseContent()
        {
            base.ReleaseContent();

            if (this._content != null)
            {
                this._content.ResetContent();
            }
        }
        #endregion // ReleaseContent

        #endregion // Overrides

        #region Properties

        #region ContentProvider
        /// <summary>
		/// Resolves the <see cref="ColumnContentProviderBase"/> for this <see cref="CellControl"/>.
		/// </summary>
		public override ColumnContentProviderBase ContentProvider
		{
			get { return this._content; }
		}
		#endregion // ContentProvider

		#region HasEditingBindings
		/// <summary>
		/// Resolves whether the <see cref="FrameworkElement"/> that is being used for editing has any <see cref="Binding"/> objects
		/// associated with it.
		/// </summary>
		/// <remarks>
		/// Note: this property is only valid when the <see cref="Cell"/> is currently in edit mode.
		/// </remarks>
		protected internal bool HasEditingBindings
		{
			get
			{
				return (this._editorBindings != null && this._editorBindings.Count > 0);
			}
		}
		#endregion // HasEditingBindings

		#region EditorAlreadyLoaded

		/// <summary>
		/// Gets / sets if the editor control for this cell control is loaded.
		/// </summary>
		protected virtual bool EditorAlreadyLoaded
		{
			get;
			set;
		}

		#endregion // EditorAlreadyLoaded

        #region ToolTipContent

        /// <summary>
        ///  Allows a <see cref="CellControl"/> to provide different content for it's Tooltip.
        /// </summary>
        protected virtual object ToolTipContent
        {
            get
            {
                return null;
            }
        }
        #endregion // ToolTipContent

        #region IsEditorTemplateAddNewRowCell
        private bool IsEditorTemplateAddNewRowCell
        {
            get
            {
                return this.Cell != null && this.Cell is AddNewRowCell && this.Column.AddNewRowEditorTemplate != null;
            }
        }
        #endregion // IsEditorTemplateAddNewRowCell

        #endregion // Properties

        #region Methods

        #region Protected

        #region ResolveEditorCellValue

        /// <summary>
		/// Determines the value that will be used as the text for the editor control.
		/// </summary>
		/// <param propertyName="dataValue"></param>
		/// <returns></returns>
		protected virtual object ResolveEditorCellValue(object dataValue)
		{
			Cell c = this.Cell as Cell;
			Dictionary<string, object> values = c.Row.ColumnLayout.Grid.EditCellValues;

			object obj = null;
			if (values != null && values.ContainsKey(c.Column.Key))
			{
				obj = values[c.Column.Key];
			}
			else
				obj = dataValue;

			return obj;
		}

		#endregion // ResolveEditorCellValue

		#region AddEditorToControl

		/// <summary>
		/// Used during inline editing, sets up the cell control with the child editor needed to update this cell.
		/// </summary>
		protected internal virtual void AddEditorToControl()
		{
            if (this.Cell.Column.WidthResolved.WidthType == ColumnWidthType.InitialAuto && !this.Cell.Column.IsInitialAutoSet)
            {
                this._wasColumnWidthSetAtEditTime = false;
            }
            else
            {
                this._wasColumnWidthSetAtEditTime = true;
            }
			this._columnActualWidth = this.Cell.Column.ActualWidth;
			this._rowActualHeight = this.Cell.Row.ActualHeight;

			this.EditorAlreadyLoaded = false;
			Cell c = this.Cell as Cell;
			if (c != null)
			{
				object dataValue = this.Cell.Value;
				object obj = ResolveEditorCellValue(dataValue);

				if (obj != null && !obj.Equals(dataValue))
					this.EditorAlreadyLoaded = true;

                if (this.ContentProvider.RemovePaddingDuringEditing || IsEditorTemplateAddNewRowCell)
				{
					this._padding = this.Padding;
					this.Padding = new Thickness();
				}

				double availWidth = c.Column.ActualWidth - (this.BorderThickness.Right + this.BorderThickness.Left + this.Padding.Left + this.Padding.Right);
				double availHeight = c.Row.ActualHeight - (this.BorderThickness.Top + this.BorderThickness.Bottom + this.Padding.Top + this.Padding.Bottom);

				if (c.Row.ActualHeight == 0)
					availHeight += this.DesiredSize.Height;

				if (availHeight <= 0)
					availHeight = double.NaN;

				FrameworkElement editor = this._content.ResolveEditor(c, this.NotifyEditorValueChanged, obj, availWidth, availHeight, this._content.ResolveEditorBinding(c));

				if (editor != null)
				{
					// Check to see if the focused element is a control, if so, then call ReleaseMouseCapture, 
					// Just in case it has captured the mouse, otherwise, when we remove it from the visual tree, it could throw an exception.
                    Control ctrl = PlatformProxy.GetFocusedElement(this) as Control;
					if (ctrl != null)
						ctrl.ReleaseMouseCapture();

                    if (c.Column is TemplateColumn && editor is TemplateColumnEditorContentPresenter)
                    {
                        ((TemplateColumnEditorContentPresenter)editor).Control = this;
                    }
                    else
                    {
                        this._editorBindings = CellControl.ResolveBindingsFromChildren(editor, true);
                    }

                    if (this.Content != editor)
                    {
                        this.Content = editor;
                        editor.Loaded += new RoutedEventHandler(Editor_Loaded);
                    }
				}
			}
		}

		#endregion // AddEditorToControl

		#region RemoveEditorFromControl

		/// <summary>
		/// Used during inline editing, cleans up the cell control restoring it to display the data of the cell.
		/// </summary>
        protected internal virtual void RemoveEditorFromControl()
        {
            if (this._content != null)
            {
                Cell cell = (Cell)this.Cell;
                if (cell != null)
                {
                    FrameworkElement editor = this.Content as FrameworkElement;

					if (editor != null)
					{
						editor.Loaded -= Editor_Loaded;

                        if (editor is TemplateColumnEditorContentPresenter)
                        {
                            ((TemplateColumnEditorContentPresenter)editor).Control = null;
                        }
					}

                    this._content.EditorRemoved();

                    Binding b = this._content.ResolveBinding(cell);

                    bool editorHadFocus = CellControl.IsFocusedControlInsideEditor(editor);

                    this.Content = this._content.ResolveDisplayElementInternal(cell, b);

                    // Make sure that the editor no longer has focus, speficially, if the editor is always visible
                    // such as a checkbox. 
                    if (this.Cell == this.Cell.Row.ColumnLayout.Grid.ActiveCell)
                    {
                        bool isMouseCaptured = false;

                        // NZ 11 June 2012 - TFS108885, TFS108652
                        IInputElement target = Mouse.DirectlyOver;

                        if (target != null && target != editor)
                        {
                            isMouseCaptured = target.IsMouseCaptured || target.IsKeyboardFocused;
                        }

                        if (CellControl.IsFocusedControlInsideEditor(editor) && !isMouseCaptured)
                        {
                            this.Focus();
                        }

                        else if (editorHadFocus && !this.IsFocused && !isMouseCaptured)
                        {
                            this.Focus();
                        }

                    }

                    if (this.ContentProvider.RemovePaddingDuringEditing || this.IsEditorTemplateAddNewRowCell)
                    {
                        this.Padding = this._padding;
                    }
                }
            }

            this.Cell.Row.ActualHeight = this._rowActualHeight;

            




            if (this._wasColumnWidthSetAtEditTime)
                this.Cell.Column.ActualWidth = this._columnActualWidth;
        }

		#endregion // RemoveEditorFromControl        

        #region OnEditorLoaded

        /// <summary>
		/// This method is raised when an editor has fired it's Loaded event.
		/// And should be used to set focus and intialize anything in the editor that needs to be initialized.
		/// </summary>
		/// <param propertyName="editor"></param>
		protected virtual void OnEditorLoaded(FrameworkElement editor)
		{
            this.FocusOnEditorElement(editor);

			// This line of code ensures that the editor will actually be rendered inside of the cell
			// If you enter and exit edit mode a few times, the editor's ActualHeight and ActualWidth may resolve to 0.0
			// I'm not exactly sure why this occurs, but it appears to be a timing issue. 
			// The following line code solves that problem, by ensuring that everything is rendered properly for this particular cell. 
			this.Cell.Row.Control.RenderCell(this.Cell);

			// To Continue on the same issue, sometimes RenderCell doesn't event work, in which case we need a way to triggering 
			// The height and width of the editor to be recalculated.  This code appears to do the trick.  Merely calling UpdateLayout
			// has no effect, however, once we touch the Height and Width, it seems to work. 
			if ((editor.ActualHeight == 0 || editor.ActualWidth == 0) && double.IsNaN(editor.Height) && double.IsNaN(editor.Width))
			{
				editor.Height = 1;
				editor.Width = 1;
				editor.UpdateLayout();
				editor.Height = double.NaN;
				editor.Width = double.NaN;
			}

			if (!this.EditorAlreadyLoaded)
				this._content.EditorLoaded();
		}

		#endregion // OnEditorLoaded

		#region EvaluateEditingBindings
		/// <summary>
		/// Loops through all the <see cref="Binding"/> objects that are associated with an editor, and determines
		/// if there is a binding error. 
		/// </summary>
		/// <returns>False if a binding error is found.</returns>
		protected internal bool EvaluateEditingBindings()
		{
            if (this._editorBindings != null)
            {
                foreach (BindingDataInfo data in this._editorBindings)
                {

                    Validation.AddErrorHandler(data.Element, Element_BindingValidationError);
                    data.Expression.UpdateSource();
                    Validation.RemoveErrorHandler(data.Element, Element_BindingValidationError);
                    





                }
                if (this.Cell.Row.RowType != RowType.FilterRow)
                {
                    if (this._bindingError)
                    {
                        this._bindingError = false;
                        return false;
                    }
                    else
                    {


#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

                    }
                }
            }
			return true;
		}
		#endregion // EvaluateEditingBindings

		#endregion // Protected

		#region Private

		#region NotifyEditorValueChanged

		private void NotifyEditorValueChanged(object value)
		{
			if (this.Cell != null)
			{
				this.Cell.EditorValueChanged(value);
			}
		}

		#endregion // NotifyEditorValueChanged

		#region ResolveBindingsFromChildren
		/// <summary>
		/// Loops through a <see cref="FrameworkElement"/>'s children, and find all <see cref="Binding"/>s that are assoicated with them.
		/// </summary>
		/// <param propertyName="element">The element to recurse through</param>
		/// <param propertyName="forEditing">Whether certain element's should be traversed.</param>
		/// <returns></returns>
		internal static List<BindingDataInfo> ResolveBindingsFromChildren(FrameworkElement element, bool forEditing)
		{
			List<BindingDataInfo> bindings = new List<BindingDataInfo>();
			bindings.AddRange(CellControl.ResovingBindingData(element, forEditing));

			int children = VisualTreeHelper.GetChildrenCount(element);
			for (int i = 0; i < children; i++)
			{
				FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
				if (child != null)
				{
					bindings.AddRange(CellControl.ResolveBindingsFromChildren(child, forEditing));
				}
			}

			return bindings;
		}
		#endregion // ResolveBindingsFromChildren

		#region IsFocusedControlInsideEditor

		private static bool IsFocusedControlInsideEditor(FrameworkElement ctrl)
		{
			if (ctrl != null)
			{
                DependencyObject dp = PlatformProxy.GetFocusedElement(ctrl) as DependencyObject;
				FrameworkElement oldParent;
				while (dp != null)
				{
					if (dp == ctrl)
					{
						return true;
					}

					oldParent = dp as FrameworkElement;

					dp = PlatformProxy.GetParent(dp);

					if (dp == null && oldParent != null)
					{
						dp = oldParent.Parent as DependencyObject;
					}
				}
			}
			return false;
		}

		#endregion // IsFocusedControlInsideEditor

        #region FocusOnEditorElement

        private bool FocusOnEditorElement(FrameworkElement editor)
        {
            Control ctrl = editor as Control;

            if (ctrl != null)
            {
                if (this.Cell != null && ((Cell)this.Cell).IsEditing && editor.IsHitTestVisible)
                {
                    if (!CellControl.IsFocusedControlInsideEditor(ctrl))
                    {
                        ctrl.Focus();
                        return true;
                    }
                }
            }
            else
            {
                Panel panel = editor as Panel;

                if (panel != null)
                {
                    foreach (UIElement child in panel.Children)
                    {
                        if (FocusOnEditorElement(child as FrameworkElement))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        #endregion // FocusOnEditorElement

		#endregion // Private

        #region Internal

        internal void ShowToolTip()
        {
            Cell c = (Cell)this.Cell;
            Column column = c.Column;

            if (this._tooltip == null)
            {
                this._tooltip = new ToolTip();
            }

            this._tooltip.Style = column.ToolTipStyle;

            object content = this.ToolTipContent;
            if (content != null)
            {
                this._tooltip.Content = content;
            }
            else
            {
                if (column.ToolTipContentTemplate == null)
                {
                    if (this._tooltipContentProvider == null)
                    {
                        this._tooltipContentProvider = column.GenerateContentProvider();
                        this._tooltipContentProvider.IsToolTip = true;
                    }

                    TemplateColumn tc = column as TemplateColumn;
                    object val = c.Value;
                    string str = val as string;
                    if (c.Row.Data == null || val == null || (str != null && str.Length == 0))
                    {
                        UnboundColumn uc = column as UnboundColumn;

                        if (uc != null && uc.ItemTemplate != null)
                        {
                            Binding b = this._tooltipContentProvider.ResolveBinding(c);
                            this._tooltip.Content = this._tooltipContentProvider.ResolveDisplayElement(c, b);
                            this._tooltip.DataContext = c.Row.Data;
                        }
                        else
                        {
                            this._tooltip.Content = null;
                            this._tooltip = null;
                        }
                    }
                    else if (tc != null && tc.ItemTemplate == null)
                    {
                        this._tooltip.Content = c.Value;
                    }
                    else
                    {
                        Binding b = this._tooltipContentProvider.ResolveBinding(c);
                        this._tooltip.Content = this._tooltipContentProvider.ResolveDisplayElement(c, b);
                        this._tooltip.DataContext = c.Row.Data;
                    }
                }
                else
                {
                    this._tooltip.Content = c.Row.Data;
                    this._tooltip.ContentTemplate = column.ToolTipContentTemplate;
                } 
            }

            ToolTipService.SetToolTip(this, this._tooltip);
        }

        internal void HideToolTip()
        {
            if (this._tooltip != null)
            {
                ToolTipService.SetToolTip(this, null);
                this._tooltip.Content = null;
                this._tooltip = null;
            }
        }

        #region RefreshEditorBindings

        /// <summary>
        /// Refreshes the bindings of the editor that is used for <see cref="ContentControl.Content"/> of the <see cref="CellControl"/>.
        /// </summary>
        /// <remarks>
        /// This method is used by the <see cref="TemplateColumnContentProvider"/>, to refresh the list of bindings,
        /// when we have implicit data templates and the the content of the editor (<see cref="TemplateColumnEditorContentPresenter"/>)
        /// is changed after it's added to the VisualTree.
        /// </remarks>
        internal void RefreshEditorBindings()
        {
            FrameworkElement editor = this.Content as FrameworkElement;

            if (editor != null)
            {
                this._editorBindings = CellControl.ResolveBindingsFromChildren(editor, true);
            }
        }

        #endregion // RefreshEditorBindings

        #endregion // Internal

        #region Static

        private static List<DependencyProperty> GetDependencyProperties(FrameworkElement element, bool forEditing)
		{
			List<DependencyProperty> list = new List<DependencyProperty>();
			if (!forEditing || ((((!(element is Panel) && !(element is Button)) && (!(element is Image) && !(element is ScrollViewer))) && ((!(element is TextBlock) && !(element is Border)) && !(element is Shape))) && !(element is ContentPresenter)))
			{
                FieldInfo[] fields = element.GetType().GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);
				foreach (FieldInfo info in fields)
				{
					if (info.FieldType == typeof(DependencyProperty))
					{
						list.Add((DependencyProperty)info.GetValue(null));
					}
				}
			}
			return list;
		}

		private static List<BindingDataInfo> ResovingBindingData(FrameworkElement element, bool forEditing)
		{
			List<BindingDataInfo> data = new List<BindingDataInfo>();
			List<DependencyProperty> properties = CellControl.GetDependencyProperties(element, forEditing);

			if (properties.Count > 0)
			{
				foreach (DependencyProperty property in properties)
				{
					BindingExpression expression = element.GetBindingExpression(property);
					if (expression != null)
					{
                        if (expression.ParentBinding != null)
                        {
                            if (expression.ParentBinding.Mode == BindingMode.TwoWay)
                            {
                                data.Add(new BindingDataInfo() { Element = element, Expression = expression });
                            }
                        }
                        else
                        {
                            data.Add(new BindingDataInfo() { Element = element, Expression = expression });
                        }
					}
				}
			}

			return data;
		}

		#endregion // Static

		#endregion // Methods

		#region EventHandlers

		void Editor_Loaded(object sender, RoutedEventArgs e)
		{
			this.OnEditorLoaded(sender as FrameworkElement);
		}

		void Element_BindingValidationError(object sender, ValidationErrorEventArgs e)
		{
			if (!this.Cell.Row.ColumnLayout.Grid.OnCellEditingValidationFailed((Cell)this.Cell, e))
			{
				if (!e.Handled && e.Action == ValidationErrorEventAction.Added)
					this._bindingError = true;
			}
		}


		#endregion // EventHandlers

		#region BindingDataInfo Class

		internal class BindingDataInfo
		{
			public FrameworkElement Element
			{
				get;
				set;
			}

			public BindingExpression Expression
			{
				get;
				set;
			}
		}

		#endregion // BindingDataInfo
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