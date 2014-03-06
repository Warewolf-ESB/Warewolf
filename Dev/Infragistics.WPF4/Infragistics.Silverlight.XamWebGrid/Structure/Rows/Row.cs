using System;
using System.Net;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;
using System.Reflection;
using System.Windows.Data;
using Infragistics.AutomationPeers;
using Infragistics.Controls.Grids.Primitives;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that represents a standard row in the <see cref="XamGrid"/>.
	/// </summary>
	public class Row : ExpandableRowBase, IBindableItem, ISelectableObject
	{
		#region Members

		ChildBandRowsManager _childRowsManager;
		bool _isSelected;
		Style _cellStyle;
        string _identifer;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Row"/> class.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="manager">The <see cref="RowsManager"/> that owns the <see cref="Row"/>.</param>		
		/// <param propertyName="data"></param>
		protected internal Row(int index, RowsManager manager, object data)
			: base(manager)
		{
			this.Data = data;
			this.Index = index;

            if (manager != null && manager.ColumnLayout != null)
            {
                this._identifer = manager.ColumnLayout.Key + "_" + manager.ColumnLayout.Level.ToString();
            }
        }

        #endregion // Constructor

        #region Properties

        #region Public

        #region ChildRowsManager

        /// <summary>
		/// Gets the <see cref="ChildBandRowsManager"/> that the <see cref="Row"/> owns.
		/// </summary>
		protected internal override RowsManagerBase ChildRowsManager
		{
			get
			{
				if (this._childRowsManager == null)
				{
					RowsManagerBase manager = this.Manager;
					if (manager.ColumnLayout != null && manager.ColumnLayout.Grid != null && manager.Level < manager.ColumnLayout.Grid.MaxDepth)
					{
						this._childRowsManager = new ChildBandRowsManager(this);
					}
				}
				return this._childRowsManager;
			}
			set
			{
				this._childRowsManager = value as ChildBandRowsManager;
			}
		}

		#endregion // ChildRowsManager

		#region HasChildren

		/// <summary>
		/// Gets whether or not <see cref="Row"/> has any child rows.
		/// </summary>
		public override bool HasChildren
		{
			get
			{
				if (this.ChildRowsManager != null)
				{
					RowBaseCollection rows = this.ChildRowsManager.Rows;
					foreach (ChildBand row in rows)
					{
						if (((RowsManager)row.ChildRowsManager).ChildrenShouldBeDisplayedResolved)
							return true;
					}
				}
				return false;
			}
		}

		#endregion // HasChildren

	    #region IsAlternate

	    /// <summary>
	    /// Determines if the <see cref="Row"/> is an Alternate row.
	    /// </summary>
	    public override bool IsAlternateRow
	    {
		    get
		    {
                if (this.ColumnLayout != null && this.ColumnLayout.IsAlternateRowsEnabledResolved)
				    return ((this.Index % 2) != 0);
			    else
				    return false;
		    }
	    }

	    #endregion // IsAlternate

		#region ChildBands

		/// <summary>
		/// Gets the ChildBands of this particular Row. 
		/// </summary>
		/// <remarks>
		/// You can use a ChildBand to get a references to the child rows of a particular row. 
		/// Row.ChildBands[0].Rows[0] or Row.ChildBands["Orders"].Rows[0]
		/// </remarks>
		public virtual ChildBandCollection ChildBands
		{
			get
			{
				if (this.ChildRowsManager != null)
				{
					return ((ChildBandCollection)this.ChildRowsManager.Rows.ActualCollection);
				}

				return new ChildBandCollection();
			}
		}

		#endregion // ChildBands

		#region IsSelected

		/// <summary>
		/// Gets/Sets whether an item is currently selected. 
		/// </summary>
		public bool IsSelected
		{
			get
			{
				return this._isSelected;
			}
			set
			{
				if (this.ColumnLayout != null)
				{
					if (value)
						this.ColumnLayout.Grid.SelectRow(this, InvokeAction.Code);
					else
						this.ColumnLayout.Grid.UnselectRow(this);
				}
			}
		}
		#endregion // IsSelected

		#region CellStyle

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be used for all <see cref="CellControl"/> objects on this <see cref="Row"/>.
		/// </summary>
		public Style CellStyle
		{
			get { return this._cellStyle; }
			set
			{
				this._cellStyle = value;
                ControlTemplate controlTemplate = null;
                this.StrippedCellStyleForConditionalFormatting = XamGrid.CloneStyleWithoutControlTemplate(value, out controlTemplate);
                this.ControlTemplateForConditionalFormatting = controlTemplate;
                if (this.ColumnLayout != null && this.ColumnLayout.Grid != null && this.ColumnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
                {
                    foreach (CellBase cell in this.Cells)
                    {
                        ConditionalFormattingCell formatCell = cell as ConditionalFormattingCell;
                        if (formatCell != null)
                            formatCell.Refresh();
                    }
                }
				this.ColumnLayout.Grid.InvalidateScrollPanel(false);
			}
		}

		#endregion // CellStyle

		#region ParentRow

		/// <summary>
		/// The <see cref="ParentRow"/> that owns this <see cref="Row"/>.
		/// </summary>
		public RowBase ParentRow
		{
			get
			{
				return this.Manager.ParentRow;
			}
		}

		#endregion // ParentRow

		#region RowType

		/// <summary>
		/// Gets the <see cref="RowType"/> of this <see cref="RowBase"/>
		/// </summary>
		public override RowType RowType
		{
			get { return RowType.DataRow; }
		}

		#endregion // RowType

		#region CellStyle

		/// <summary>
		/// Gets / sets a style set by conditional formatting which will be used to style the row.
		/// </summary>
		protected internal Style ConditionalCellStyle
		{
			get;
			set;
		}

		#endregion // CellStyle

		#endregion // Public

		#region Protected

		#region AllowEditing

		/// <summary>
		/// Gets if editing will be enabled on this <see cref="Row"/>.
		/// </summary>
		protected internal override EditingType AllowEditing
		{
			get
			{
				return this.ColumnLayout.EditingSettings.AllowEditingResolved;
			}
		}
		#endregion // AllowEditing

		#region AllowKeyboardNavigation
		/// <summary>
		/// Gets whether the <see cref="RowBase"/> will allow keyboard navigation.
		/// </summary>
		protected internal override bool AllowKeyboardNavigation
		{
			get
			{
				return true;
			}
		}
		#endregion // AllowKeyboardNavigation

		#region AllowSelection
		/// <summary>
		/// Gets whether selection will be allowed on the <see cref="RowBase"/>.
		/// </summary>
		protected internal override bool AllowSelection
		{
			get
			{
				return true;
			}
		}
		#endregion // AllowSelection

        #region ConditionalStyleDirty

        /// <summary>
        /// Gets/Sets wehther thie ConditionalStyle for this row is dirty.
        /// </summary>
        protected internal bool ConditionalStyleDirty
        {
            get;
            set;
        }

        #endregion // ConditionalStyleDirty

        #region CanBeDeleted

        /// <summary>
        /// Gets whether the <see cref="Row"/> can actually be deleted. 
        /// </summary>
        protected internal virtual bool CanBeDeleted
        {
            get { return true; }
        }

        #endregion // CanBeDeleted

        #endregion // Protected

        #region Internal

        #region StrippedCellStyleForConditionalFormatting

        internal Style StrippedCellStyleForConditionalFormatting
        {
            get;
            set;
        }

        #endregion // StrippedCellStyleForConditionalFormatting

        #region ControlTemplateForConditionalFormatting
        /// <summary>
        /// This property holds the ControlTemplate that would be ripped out of the Row's CellStyle.
        /// </summary>
        internal ControlTemplate ControlTemplateForConditionalFormatting
        {
            get;
            set;
        }
        #endregion //

        #region ConditionalStyleControlTemplate
        /// <summary>
        /// This property holds the ControlTemplate that would have been ripped out by the generated style in conditional formatting.
        /// </summary>
        internal ControlTemplate ConditionalStyleControlTemplate
        {
            get;
            set;
        }
        #endregion // ConditionalStyleControlTemplate

        #region ContainsChildBandRowsManager
        internal bool ContainsChildBandRowsManager
        {
            get
            {
                return this._childRowsManager != null;
            }
        }
        #endregion // ContainsChildBandRowsManager

        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Public

        #region Delete
        /// <summary>
		/// Deletes the row from the <see cref="RowsManager"/> that contains it.
		/// </summary>
		public virtual void Delete()
		{
			RowsManager rm = this.Manager as RowsManager;
			if (rm != null)
			{
				RowCollection rc = rm.Rows.ActualCollection as RowCollection;
				if (rc != null)
				{
					rc.Remove(this);
				}
			}
		}
		#endregion  // Delete

		#endregion

		#region Protected

		#region SetSelected
		/// <summary>
		/// Sets the selected state of an item. 
		/// </summary>
		/// <param propertyName="isSelected"></param>
		protected internal virtual void SetSelected(bool isSelected)
		{
			this._isSelected = isSelected;
			this.OnPropertyChanged("IsSelected");
            if(this.ColumnLayout != null)
			    this.ColumnLayout.Grid.InvalidateScrollPanel(false);
		}
		#endregion // SetSelected

        #region GetCellValue

        /// <summary>
        /// Performs the cell.Value without forcing the cell to be made.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected internal override object GetCellValue(Column column)
        {
            object val = null;
            if (this.Data != null && column.Key != null)
            {
                Infragistics.Controls.Grids.Cell.CellValueObject cellValueObj = new Infragistics.Controls.Grids.Cell.CellValueObject();

                Binding binding = null;
                if (column.Key != null && column.DataType != null)
                {
                    if (column is UnboundColumn)
                        binding = new Binding();
                    else
                        binding = new Binding(column.Key);

                    binding.ConverterCulture = CultureInfo.CurrentCulture;                    
                }

                if (binding != null)
                {
                    binding.Mode = BindingMode.OneTime;

                    binding.Source = this.Data;

                    // Reset the Converters, otherwise, we'll raise the CellControlAttached event.
                    binding.Converter = column.ValueConverter;
                    binding.ConverterParameter = column.ValueConverterParameter;

                    cellValueObj.SetBinding(Infragistics.Controls.Grids.Cell.CellValueObject.ValueProperty, binding);
                    val = cellValueObj.Value;
                }
            }
            return val;
        }

        #endregion // GetCellValue

        #endregion // Protected

        #endregion // Methods

        #region Overrides

        #region OnElementReleasing

        /// <summary>
		/// Invoked when a <see cref="FrameworkElement"/> is being released from an object.
		/// </summary>
		/// <param propertyName="element"></param>
		/// <returns>False, if the element shouldn't be released.</returns>
		protected override bool OnElementReleasing(CellsPanel element)
		{
			if (this.ColumnLayout != null)
			{
				XamGrid grid = this.ColumnLayout.Grid;

                if (grid != null)
                {
                    if (element.Row == grid.CurrentEditRow || (grid.CurrentEditCell != null && grid.CurrentEditCell.Row == this))
                        return false;
                }
			}

			return base.OnElementReleasing(element);
		}
		#endregion // OnElementReleasing

		#region ResolveRowHover

		/// <summary>
		/// Resolves whether the entire row or just the individual cell should be hovered when the 
		/// mouse is over a cell. 
		/// </summary>
		protected internal override RowHoverType ResolveRowHover
		{
			get
			{
			    if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
			    {
                    return this.ColumnLayout.Grid.RowHover;    
			    }

			    return base.ResolveRowHover;
			}
		}
		#endregion // ResolveRowHover

        #region RecyclingElementType

        /// <summary>
        /// Gets the Type of control that should be created for the <see cref="Row"/>.
        /// </summary>
        protected override Type RecyclingElementType
        {
            get
            {
                return null;
            }
        }
        #endregion // RecyclingElementType

        #region RecyclingIdentifier

        /// <summary>
        /// If a <see cref="RecyclingElementType"/> isn't specified, this property can be used to offer another way of identifying 
        /// a reyclable element.
        /// </summary>
		protected override string RecyclingIdentifier
		{
			get
			{
				return this._identifer + "_" + this.RowType.ToString();
			}
		}
        #endregion // RecyclingIdentifier

		#region OnElementAttached

		/// <summary>
		/// Called when the <see cref="CellsPanel"/> is attached to the <see cref="RowBase"/>
		/// </summary>
		/// <param propertyName="element">A <see cref="CellsPanel"/></param>
		protected override void OnElementAttached(CellsPanel element)
		{
			this.Manager.GetRowScopedConditions();

			ReadOnlyCollection<IConditionalFormattingRuleProxy> rules = this.Manager.GetRowScopedConditions();
			List<SetterBase> rowSetters = new List<SetterBase>();

			foreach (IConditionalFormattingRuleProxy rule in rules)
			{
				Style s = rule.EvaluateCondition(this.Data, this.Cells[rule.Parent .Column].Value);
				if (s != null)
				{
					{
						rowSetters.AddRange(s.Setters);
					}

					if (rule.Parent.IsTerminalRule)
						break;
				}
			}
            bool controlTemplateSet;
            bool applyStyle;
            if (rowSetters.Count > 0)
            {
                ControlTemplate controlTemplate = null;
                Style rowBaseStyle = this.CellStyle != null ? this.CellStyle : this.ColumnLayout.CellStyleResolved;
                Style generatedStyle = Infragistics.Controls.Grids.ConditionalFormattingCell.ConditionalFormattingCellBindingConverter.MergeSettersIntoSingleStyle(rowSetters, rowBaseStyle, out controlTemplateSet, out applyStyle, out controlTemplate);
                this.ConditionalStyleControlTemplate = controlTemplate;
                this.ConditionalCellStyle = generatedStyle;
            }
            else
            {
                this.ConditionalCellStyle = null;
            }

            base.OnElementAttached(element);
		}

		#endregion // OnElementAttached

        #region OnElementReleased
        /// <summary>
        /// Called when the <see cref="CellsPanel"/> is removed from the <see cref="RowBase"/>
        /// </summary>
        /// <param propertyName="element">A <see cref="CellsPanel"/></param>
        protected override void OnElementReleased(CellsPanel element)
        {
            base.OnElementReleased(element);
            if (this.ConditionalStyleDirty)
            {
                for (int i = 0; i < this.Cells.Count; i++)
                {
                    ConditionalFormattingCell cfc = this.Cells[i] as ConditionalFormattingCell;
                    if (cfc != null)
                    {
                        cfc.ConditionalStyle = null;
                        cfc.DisplayContent = true;
                    }
                }
            }
            this.ConditionalStyleDirty = false;
        }

        #endregion // OnElementReleased

        #endregion // Overrides

        #region IBindableItem Members

        /// <summary>
		/// Gets/sets whether the <see cref="Row"/> was generated via the datasource or was entered manually.
		/// </summary>
		protected virtual bool IsDataBound
		{
			get;
			set;
		}

		bool IBindableItem.IsDataBound
		{
			get { return this.IsDataBound; }
			set { this.IsDataBound = value; }
		}

		#endregion

		#region ISelectableItem Members

		bool ISelectableObject.IsSelected
		{
			get
			{
				return this.IsSelected;
			}
			set
			{
				this.IsSelected = value;
			}
		}

		void ISelectableObject.SetSelected(bool isSelected)
		{
			this.SetSelected(isSelected);
		}

		#endregion		
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