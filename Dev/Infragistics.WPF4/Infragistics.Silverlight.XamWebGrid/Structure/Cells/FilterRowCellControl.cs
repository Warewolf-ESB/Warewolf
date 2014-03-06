using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Collections.Generic;
using System.Globalization;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// Visual object for the <see cref="FilterRowCell"/> object.
	/// </summary>
	[TemplatePart(Name = "OperatorControl", Type = typeof(FilterControlBase))]
	[TemplateVisualState(GroupName = "AllowFiltering", Name = "ShowFilterUI")]
	[TemplateVisualState(GroupName = "AllowFiltering", Name = "HideFilterUI")]
	[TemplateVisualState(GroupName = "AllowFiltering", Name = "ShowFilterUIWithCancelButton")]

	[TemplateVisualState(GroupName = "FilterCellValueVisibility", Name = "ShowFilterCellValue")]
	[TemplateVisualState(GroupName = "FilterCellValueVisibility", Name = "HideFilterCellValue")]
	public class FilterRowCellControl : CellControl, ICommandTarget
	{
		#region Members
		FilterControlBase _filterControl;
		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterRowCellControl"/> class.
		/// </summary>
		public FilterRowCellControl()
		{
			base.DefaultStyleKey = typeof(FilterRowCellControl);
		}

		#endregion // Constructor

		#region Overrides

		#region OnApplyTemplate
		/// <summary>
		/// Builds the visual tree for the <see cref="FilterRowCellControl"/>
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			this._filterControl = base.GetTemplateChild("OperatorControl") as FilterControlBase;
			if (this._filterControl != null)
			{
				this._filterControl.Cell = this.Cell as FilterRowCell;
			}
		}
		#endregion // OnApplyTemplate

		#region ResolveBinding
		/// <summary>
		/// In a derived class this method should be implemented to resolve the binding that should be used for a bound <see cref="Column"/>
		/// </summary>
		/// <remarks>This will be called during the ContentProvider ResolveBinding and is not to be called directly otherwise.</remarks>
		/// <returns></returns>
		protected internal override Binding ResolveBinding()
		{
			if (!(this.Cell.Column is TemplateColumn))
			{
				Binding binding = CreateBinding();
				binding.ConverterParameter = this;
				binding.Converter = new Infragistics.Controls.Grids.Cell.CellBindingConverter();
				return binding;
			}
			return base.ResolveBinding();
		}
		#endregion // ResolveBinding

		#region ResolveEditorBinding

		/// <summary>
		/// Creates a <see cref="Binding"/> that can be applied to an editor.
		/// </summary>
		/// <remarks>This will be called during the ContentProvider ResolveBinding and is not to be called directly otherwise.</remarks>
		/// <returns></returns>
		protected internal override Binding ResolveEditorBinding()
		{
			if (!(this.Cell.Column is TemplateColumn))
			{
				return CreateBinding();
			}
			return base.ResolveEditorBinding();
		}

		#endregion // ResolveEditorBinding

		#region ResolveEditorCellValue
		/// <summary>
		/// Determines the value that will be used as the text for the editor control.
		/// </summary>
		/// <param propertyName="dataValue"></param>
		/// <returns></returns>
		protected override object ResolveEditorCellValue(object dataValue)
		{
			if (!(this.Cell.Column is TemplateColumn))
			{
				return ((FilterRowCell)this.Cell).FilterCellValueResolved;
			}
			return base.ResolveEditorCellValue(dataValue);
		}

		#endregion // ResolveEditorCellValue

		#region AddEditorToControl

		/// <summary>
		/// Used during inline editing, sets up the cell control with the child editor needed to update this cell.
		/// </summary>
		protected internal override void AddEditorToControl()
		{
			FilterRowCell fc = this.Cell as FilterRowCell;
			if (fc != null && fc.FilteringOperandResolved != null && !fc.FilteringOperandResolved.RequiresFilteringInput)
				return;
			base.AddEditorToControl();
		}

		#endregion // AddEditorToControl

        #region RemoveEditorFromControl
        /// <summary>
        /// Used during inline editing, cleans up the cell control restoring it to display the data of the cell.
        /// </summary>
        protected internal override void RemoveEditorFromControl()
        {
            FilterRowCell fc = this.Cell as FilterRowCell;
            if (fc != null && fc.FilteringOperandResolved != null && !fc.FilteringOperandResolved.RequiresFilteringInput)
                return;
            base.RemoveEditorFromControl();
        }
        #endregion // RemoveEditorFromControl

        #endregion // Overrides

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
		{
			return this.SupportsCommand(command);
		}

		object ICommandTarget.GetParameter(CommandSource source)
		{
			return this.GetParameter(source);
		}

		#endregion

		#region Methods

		#region Protected

		#region SupportsCommand


		/// <summary>
		/// Returns if the object will support a given command type.
		/// </summary>
		/// <param propertyName="command">The command to be validated.</param>
		/// <returns>True if the object recognizes the command as actionable against it.</returns>
		protected virtual bool SupportsCommand(ICommand command)
		{
			return command is ClearFilters;
		}

		#endregion // SupportsCommand

		#region GetParameter
		/// <summary>
		/// Returns the object that defines the parameters necessary to execute the command.
		/// </summary>
		/// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
		/// <returns>The object necessary for the command to complete.</returns>
		protected virtual object GetParameter(CommandSource source)
		{
			return this.Cell;
		}
		#endregion // GetParameter

		#region EnsureContent

		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>		
		protected internal override void EnsureContent()
		{
			if (this._filterControl != null)
			{
				this._filterControl.EnsureContent();
			}

			base.EnsureContent();
		}

		#endregion // EnsureContent

		#endregion // Protected

		#region Private
		private Binding CreateBinding()
		{
			Binding binding = new Binding("FilterCellValue");

			if (this.Cell.Column == null || this.Cell.Column.ColumnLayout.FilteringSettings.FilteringScopeResolved == FilteringScope.ChildBand)
				binding.Source = this.Cell;
			else
				binding.Source = this.Cell.Column.FilterColumnSettings;

			binding.Mode = BindingMode.TwoWay;
			binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;

			binding.ValidatesOnExceptions = true;
			binding.NotifyOnValidationError = true;
            binding.ValidatesOnDataErrors = true;





			binding.ConverterCulture = CultureInfo.CurrentCulture;
			binding.ConverterParameter = this.Cell;
            binding.Converter = new Infragistics.Controls.Grids.Cell.CellEditingBindingConverter();

			return binding;

		}
		#endregion // Private

		#region OnAttached

		/// <summary>
		/// Called when the <see cref="CellBase"/> is attached to the <see cref="CellControlBase"/>.
		/// </summary>
		/// <param propertyName="cell">The <see cref="CellBase"/> that is being attached to the <see cref="CellControlBase"/></param>
		protected internal override void OnAttached(CellBase cell)
		{
			base.OnAttached(cell);
			if (this._filterControl != null)
			{
				this._filterControl.Cell = this.Cell as FilterRowCell;
			}
		}

		#endregion // OnAttached

		#region OnReleased
		/// <summary>
		/// Called when the <see cref="FilterRowCell"/> releases the <see cref="FilterRowCellControl"/>.
		/// </summary>
		protected internal override void OnReleased(CellBase cell)
		{
			if (this._filterControl != null)
			{
				this._filterControl.Cell = null;
			}
			base.OnReleased(cell);
		}

		#endregion // OnReleased

		#endregion // Methods
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