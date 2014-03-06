using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A cell that represents a <see cref="Cell"/> in a <see cref="FilterRow"/>
	/// </summary>
	public class FilterRowCell : Cell
	{
		#region Members
		object _filterCellValue;
		FilterOperand _filteringOperand;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="FilterRowCell"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="FilterRow"/> object that owns the <see cref="FilterRowCell"/></param>
		/// <param propertyName="column">The <see cref="Column"/> object that the <see cref="FilterRowCell"/> represents.</param>
		protected internal FilterRowCell(RowBase row, Column column)
			: base(row, column)
		{
		}
		
		#endregion // Constructor

		#region Overrides

		#region Properties

		#region Protected

		#region ResolveStyle

		/// <summary>
		/// Gets the Style that should be applied to the <see cref="FilterRowCellControl"/> when it's attached.
		/// </summary>
		protected override System.Windows.Style ResolveStyle
		{
			get
			{
				if (this.Style != null)
					return this.Style;

                if (this.Column != null && this.Column.FilterColumnSettings.FilterRowCellStyle != null)
                    return this.Column.FilterColumnSettings.FilterRowCellStyle;

				return this.Row.ColumnLayout.FilteringSettings.StyleResolved;
			}
		}

		#endregion // ResolveStyle

		#region EditingSettings
		/// <summary>
		/// Gets the <see cref="EditingSettingsBaseOverride"/> object that controls the settings for this object.
		/// </summary>
		protected internal override EditingSettingsBaseOverride EditingSettings
		{
			get
			{
				return this.Row.ColumnLayout.FilteringSettings;
			}
		}
		#endregion // EditingSettings

        #region EnableCustomEditorBehaviors

        /// <summary>
        /// Allows a Cell to disable Editor Behavior Support if they choose to.
        /// </summary>
        /// <remarks>
        /// See <see cref="CellBase.EnableCustomEditorBehaviors"/> for more information.
        /// </remarks>
        protected internal override bool EnableCustomEditorBehaviors
        {
            get { return false; }
        }

        #endregion

		#endregion // Protected

		#region Public

		#region IsEditable
		/// <summary>
		/// Gets whether a particular <see cref="Cell"/> can enter edit mode.
		/// </summary>
		public override bool IsEditable
		{
			get
			{
				return this.Column.IsFilterable;
			}
		}
		#endregion // IsEditable

		#region Value
		/// <summary>
		/// Gets the the underlying value that the cell represents. 		
		/// </summary>
		public override object Value
		{
			get
			{
				return this.FilterCellValueResolved;
			}
		}
		#endregion // Value

		#endregion // Public

		#endregion // Properties

		#region Methods

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of a <see cref="FilterRowCellControl"/> for the <see cref="FilterRowCell"/>.
		/// </summary>
		/// <returns>A new <see cref="FilterRowCellControl"/></returns>
		/// <remarks>This method should only be used by the <see cref="Infragistics.RecyclingManager"/></remarks>
		protected override CellControlBase CreateInstanceOfRecyclingElement()
		{
			return new FilterRowCellControl();
		}

		#endregion // CreateInstanceOfRecyclingElement

		#region EnsureCurrentState

		/// <summary>
		/// Ensures that <see cref="Cell"/> is in the correct state.
		/// </summary>
		protected internal override void EnsureCurrentState()
		{
			base.EnsureCurrentState();
			if (this.Control != null)
			{
				if (!this.Column.IsFilterable)
				{
					VisualStateManager.GoToState(this.Control, "HideFilterUI", true);
				}
				else
				{
					object filterCellResolved = this.FilterCellValueResolved;

					VisualStateManager.GoToState(this.Control, "ShowFilterCellValue", true);

					if (this.FilteringOperandResolved == null)
					{
						VisualStateManager.GoToState(this.Control, "ShowFilterUI", true);						
					}
					else
					{
						if (this.FilteringOperandResolved.RequiresFilteringInput)
						{
							if (filterCellResolved == null)
							{
								VisualStateManager.GoToState(this.Control, "ShowFilterUI", true);
							}
							else
							{
								string convertedString = filterCellResolved.ToString();
								if (string.IsNullOrEmpty(convertedString))
								{
									VisualStateManager.GoToState(this.Control, "ShowFilterUI", true);
								}
								else
								{
									VisualStateManager.GoToState(this.Control, "ShowFilterUIWithCancelButton", true);
								}
							}
						}
						else
						{
							VisualStateManager.GoToState(this.Control, "ShowFilterUI", true);
							VisualStateManager.GoToState(this.Control, "HideFilterCellValue", true);
						}
					}
				}
			}
		}

		#endregion // EnsureCurrentState

		#region CreateCellValueBinding

		/// <summary>
		/// Creates the binding used by the CellValueObject for updating
		/// </summary>
		/// <returns></returns>
        protected override Binding CreateCellValueBinding(bool addValidation)
		{
			if (this.Column != null && this.Column is TemplateColumn)
			{
                return base.CreateCellValueBinding(addValidation);
			}
			if (this.Column != null && this.Column is UnboundColumn)
			{
                return base.CreateCellValueBinding(addValidation);
			}

			Binding binding = null;
			binding = new Binding("FilterCellValue");

			if (this.Column == null || this.Column.ColumnLayout.FilteringSettings.FilteringScopeResolved == FilteringScope.ChildBand)
				binding.Source = this;
			else
				binding.Source = this.Column.FilterColumnSettings;

			binding.Mode = BindingMode.TwoWay;

			binding.ConverterCulture = CultureInfo.CurrentCulture;
			return binding;
		}

		#endregion // CreateCellValueBinding

		#region OnElementAttached
		/// <summary>
		/// Called when the <see cref="CellControlBase"/> is attached to the <see cref="CellBase"/>
		/// </summary>
		/// <param propertyName="element">A <see cref="CellControlBase"/></param>
		protected override void OnElementAttached(CellControlBase element)
		{
			base.OnElementAttached(element);

			if (this.Column != null)
			{
				this.Column.FilterColumnSettings.PropertyChanged += FilterColumnSettings_PropertyChanged;
			}
		}
		#endregion // OnElementAttached

		#region OnElementReleased
		/// <summary>
		/// Called when the <see cref="CellControlBase"/> is removed from the <see cref="CellBase"/>
		/// </summary>
		/// <param propertyName="element">A <see cref="CellControlBase"/></param>
		protected override void OnElementReleased(CellControlBase element)
		{
			base.OnElementReleased(element);
			if (this.Column != null)
			{
				this.Column.FilterColumnSettings.PropertyChanged -= FilterColumnSettings_PropertyChanged;
			}
		}
		#endregion // OnElementReleased

        #region OnCellMouseDown

        /// <summary>
        /// Invoked when a cell is clicked.
        /// </summary>
        /// <returns>Whether or not the method was handled.</returns>
        protected internal override DragSelectType OnCellMouseDown(System.Windows.Input.MouseEventArgs e)
        {
            if (((System.Windows.Input.MouseButtonEventArgs)e).Handled)
                return DragSelectType.None;
            else
                return base.OnCellMouseDown(e);
        }

        #endregion // OnCellMouseDown

        #region OnCellClick

        /// <summary>
        /// Invoked when a cell is clicked.
        /// </summary>
        /// <param name="e"></param>
        protected internal override void OnCellClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!e.Handled)
                base.OnCellClick(e);
        }

        #endregion // OnCellClick

		#endregion // Methods

		#endregion // Overrides

		#region Properties

		#region FilteringOperand

		/// <summary>
		/// Gets / set the operand which will be used for filtering.
		/// </summary>
		public FilterOperand FilteringOperand
		{
			get
			{
				return this._filteringOperand;
			}
			set
			{
				if (this._filteringOperand != value)
				{
					this._filteringOperand = value;

					FilterRow fr = this.Row as FilterRow;
					if (fr != null)
					{
						fr.BuildFilters(this, this.FilterCellValueResolved, true);
                        fr.RaiseFilteredEvent();   
					}

					this.OnPropertyChanged("FilteringOperand");
                    this.OnPropertyChanged("FilteringOperandResolved");
					if (this._filteringOperand != null)
					{
						if (!this._filteringOperand.RequiresFilteringInput)
						{
							fr.ColumnLayout.Grid.ExitEditModeInternal(true);
							this._filterCellValue = null;
						}
					}

					this.Row.ColumnLayout.Grid.InvalidateScrollPanel(false);
				}
			}
		}

		#endregion // FilteringOperand

		#region FilteringOperandResolved

		/// <summary>
		/// Gets the <see cref="FilterOperand"/> which will be used for filtering, taking into account the settings of the <see cref="XamGrid"/>.
		/// </summary>
		public FilterOperand FilteringOperandResolved
		{
			get
			{
				if (this.Column == null)
					return this.FilteringOperand;

				if (this.FilteringOperand == null)
					return this.Column.FilterColumnSettings.FilteringOperandResolved;

				if (this.Column.ColumnLayout.FilteringSettings.FilteringScopeResolved == FilteringScope.ChildBand)
					return this.FilteringOperand;

				return this.Column.FilterColumnSettings.FilteringOperandResolved;
			}
		}

		#endregion // FilteringOperandResolved

		#region FilterCellValue

		/// <summary>
		/// Gets / sets the value which will be filtered on.
		/// </summary>
		public virtual object FilterCellValue
		{
			get
			{
				return _filterCellValue;
			}
			set
			{
				if (this.Column == null || this.Column.DataType == null)
				{
					this._filterCellValue = value;
					this.OnPropertyChanged("FilterCellValue");
					this.OnPropertyChanged("FilterCellValueResolved");
					return;
				}

				if (this._filterCellValue != value)
				{
					string convertedValue = value as string;

					// Empty string is the same as null, in the case of dealing with strings. 
					// So if this happens, consider them to be equal values and don't do anything.
					if (convertedValue != null && convertedValue.Length == 0 && this._filterCellValue == null)
						return;

					if (!string.IsNullOrEmpty(convertedValue))
					{
						try
						{
							object objectValue = FilterRow.ChangeType(value, this.Column.DataType);

							if (this._filterCellValue != objectValue)
							{
								this._filterCellValue = FilterRow.ChangeType(value, this.Column.DataType);
								this.OnPropertyChanged("FilterCellValue");
								this.OnPropertyChanged("FilterCellValueResolved");
								this.InvalidateFiltering();
							}
						}
						catch (FormatException)
						{
							EditableColumn col = this.Column as EditableColumn;
							if (col != null && col.AllowEditingValidation)
							{
								throw;
							}
						}
					}
					else
					{
						if (this._filterCellValue != null)
						{
							if (this.Column.DataType == typeof(string))
							{
								if (!string.IsNullOrEmpty(this._filterCellValue as string))
								{
									this._filterCellValue = value;
									this.OnPropertyChanged("FilterCellValue");
									this.OnPropertyChanged("FilterCellValueResolved");
									this.InvalidateFiltering();
								}
							}
							else
							{
								this._filterCellValue = value;
								this.OnPropertyChanged("FilterCellValue");
								this.OnPropertyChanged("FilterCellValueResolved");
								this.InvalidateFiltering(true);
							}
						}
						else
						{
							this._filterCellValue = value;
							this.OnPropertyChanged("FilterCellValue");
							this.OnPropertyChanged("FilterCellValueResolved");
							this.InvalidateFiltering();
						}
					}
				}
			}
		}

		#endregion // FilterCellValue

		#region FilterCellValueResolved

		/// <summary>
		/// Gets the value with will be filtered on, factoring in the <see cref="FilteringScope"/>.
		/// </summary>
		public object FilterCellValueResolved
		{
			get
			{
				UnboundColumn uc = this.Column as UnboundColumn;
								
				TemplateColumn tc = this.Column as TemplateColumn;

				if ((uc != null && uc.FilterEditorTemplate==null) ||( tc != null && tc.FilterEditorTemplate == null))
				{
					return base.Value;
				}
				if (this.Column == null || this.Column.ColumnLayout.FilteringSettings.FilteringScopeResolved == FilteringScope.ChildBand)
				{
					return this.FilterCellValue;
				}
				return this.Column.FilterColumnSettings.FilterCellValue;
			}
		}

		#endregion // FilterCellValueResolved

		#region SilentUpdate
		
		/// <summary>
		/// Gets / sets if the filters being built by modifing this object should call for a rebind of data.
		/// </summary>
		protected internal bool SilentUpdate { get; set; }
		
		#endregion // SilentUpdate

		#endregion // Properties

		#region Methods

		#region Private

		#region InvalidateFiltering

        private void InvalidateFiltering(bool forceFilteredEvent)
        {
            

            FilterRow fr = this.Row as FilterRow;
            if (fr != null && fr.ColumnLayout != null)
            {
                bool raiseFilteredEvent = fr.BuildFilters(this, this.FilterCellValueResolved, true);

                if (raiseFilteredEvent)
                {
                    fr.RaiseFilteredEvent();
                }
                //else 
                //if (forceFilteredEvent || raiseFilteredEvent)
                //    fr.RaiseFilteredEvent();
            }
        }

        private void InvalidateFiltering()
        {
            this.InvalidateFiltering(false);
        }

		#endregion // InvalidateFiltering	

		#endregion // Private

		#endregion // Methods

		#region EventHandlers

		void FilterColumnSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "FilterCellValue")
			{
				if (this.Column.ColumnLayout.FilteringSettings.FilteringScopeResolved == FilteringScope.ColumnLayout)
					this.OnPropertyChanged("FilterCellValueResolved");
			}
            else if(e.PropertyName == "FilteringOperand")
			{
                if (this.Column.ColumnLayout.FilteringSettings.FilteringScopeResolved == FilteringScope.ColumnLayout)
                    this.OnPropertyChanged("FilteringOperandResolved");
			}
		}

		#endregion // EventHandlers
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