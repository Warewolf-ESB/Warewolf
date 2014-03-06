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
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// The base class for all cell objects in the <see cref="XamGrid"/>.
	/// </summary>
	public abstract class CellBase : RecyclingContainer<CellControlBase>, ISelectableObject
	{
		#region Members

		CellControlBase _control;
		Style _style;
		Size _measuringSize = Size.Empty;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="CellBase"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="RowBase"/> object that owns the <see cref="CellBase"/></param>
		/// <param propertyName="column">The <see cref="Column"/> object that the <see cref="CellBase"/> represents.</param>
		protected CellBase(RowBase row, Column column)
		{
			this.Row = row;
			this.Column = column;

		}

		#endregion // Constructor

		#region Properties

		#region Public

		#region Content

		/// <summary>
		/// The Content of the <see cref="CellBase"/>.
		/// </summary>
		public object Content
		{
			get;
			set;
		}
		#endregion // Content

		#region Row
		/// <summary>
		/// The <see cref="RowBase"/> that owns the <see cref="CellBase"/>
		/// </summary>
		public RowBase Row
		{
			get;
			internal set;
		}
		#endregion // Row

		#region Column

		/// <summary>
		/// The <see cref="Column"/> that the <see cref="CellBase"/> represents.
		/// </summary>
		public Column Column
		{
			get;
			private set;
		}
		#endregion // Column

		#region Control

		/// <summary>
		/// Gets the <see cref="CellControlBase"/> that is attached to the <see cref="CellBase"/>
		/// </summary>
		/// <remarks>A Control is only assoicated with a Cell when it's in the viewport of the <see cref="RowsPanel"/></remarks>
		public CellControlBase Control
		{
			get { return this._control; }
			protected internal set { this._control = value; }
		}
		#endregion // Control

		#region Value

		/// <summary>
		/// Gets the the underlying value that the cell represents. 
		/// Note: in order to retrieve the cell's value we use a binding since we don't know about the underlying object. 
		/// The most performant way to retrieve the cell's value is to grab the row's Data (this.Cell.Row.Data), 
		/// cast it as your object and grab the actual value manually. 
		/// </summary>
		public virtual object Value
		{
			get
			{
				return null;
			}
		}

		#endregion // Value

		#region IsActive

		/// <summary>
		/// Gets/Sets whether a cell is the ActiveCell in the <see cref="XamGrid"/>
		/// </summary>
		public bool IsActive
		{
			get
			{
				return (this.Row.ColumnLayout.Grid.ActiveCell == this);
			}
			set
			{
				if (value)
					this.Row.ColumnLayout.Grid.ActiveCell = this;
				else
					this.Row.ColumnLayout.Grid.ActiveCell = null;
			}
		}

		#endregion // IsActive

		#region IsSelected

		/// <summary>
		/// Gets/Sets whether an item is currently selected. 
		/// </summary>
		public virtual bool IsSelected
		{
			get
			{
				return false;
			}
			set
			{
			}

		}
		#endregion // IsSelected

		#region Style

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be used for all <see cref="CellControlBase"/> objects.
		/// </summary>
		public virtual Style Style
		{
			get
			{
				return this._style;
			}
			set
			{
				if (this._style != value)
				{
					this._style = value;

					if (this.Column.ColumnLayout != null && this.Column.ColumnLayout.Grid != null)
					{
						this.Column.ColumnLayout.Grid.InvalidateScrollPanel(false);
					}

					this.ApplyStyle();
				}
			}
		}

		#endregion // Style

		#region IsEditable
		/// <summary>
		/// Determines if this <see cref="CellBase"/> can be edited via the UI.
		/// </summary>
		public virtual bool IsEditable
		{
			get
			{
				return false;
			}
		}
		#endregion // IsEditable

		#region Tag

		/// <summary>
		/// Allows a user to store additional information about a <see cref="CellBase"/>
		/// </summary>
		public object Tag
		{
			get;
			set;
		}

		#endregion // Tag

		#endregion // Public

		#region Protected

		#region ResolveStyle

		/// <summary>
		/// Gets the Style that should be applied to the <see cref="CellControlBase"/> when it's attached.
		/// </summary>
		protected virtual Style ResolveStyle
		{
			get
			{
				return this.Style;
			}
		}

		#endregion // ResolveStyle

		#region BindingMode

		/// <summary>
		/// Gets the <see cref="BindingMode"/> that will be applied when binding a <see cref="CellBase"/> to data.
		/// </summary>
		protected internal virtual BindingMode BindingMode
		{
			get
			{
				if (this.IsEditable)
					return BindingMode.TwoWay;
				else
					return BindingMode.OneWay;
			}
		}

		#endregion // BindingMode

		#region EditingSettings
		/// <summary>
		/// Gets the <see cref="EditingSettingsBaseOverride"/> object that controls the settings for this object.
		/// </summary>
		protected internal virtual EditingSettingsBaseOverride EditingSettings
		{
			get
			{
				return this.Row.ColumnLayout.EditingSettings;
			}
		}
		#endregion // EditingSettings

        #region ShouldClearDataContext

        /// <summary>
        /// Gets whether the control attached to the cell should reset it DataContext when a new Cell is attached to it.
        /// </summary>
        protected internal virtual bool ShouldClearDataContext
        {
            get { return true; }
        }

        #endregion // ShouldClearDataContext

 
        #region SupportsActivation
        /// <summary>
        /// Gets whether this particular <see cref="CellBase"/> can be made Active
        /// </summary>
        protected internal virtual bool SupportsActivation
        {
            get
            {
                return true;
            }
        }
        #endregion // SupportsActivation

        #region EnableCustomEditorBehaviors

        /// <summary>
        /// Allows a Cell to disable Editor Behavior Support if they choose to. For example: Filter and Add New Row Cells disable Editor Behavior support.
        /// </summary>
        protected internal virtual bool EnableCustomEditorBehaviors
        {
            get { return true; }
        }

        #endregion

        #endregion // Protected

        #region Internal

        /// <summary>
		/// Used for storing the size of a cell, if it needs to be re-measured.
		/// </summary>
		internal Size MeasuringSize
		{
			get { return this._measuringSize; }
			set { this._measuringSize = value; }
		}

        #region ActualMergedHeight

        /// <summary>
        /// Gets/Sets the total height that the merged cell should be.
        /// </summary>
        internal double ActualMergedHeight
        {
            get;
            set;
        }

        #endregion // ActualMergedHeight

        #region RawValue

        /// <summary>
        /// Gets the the underlying value that the cell represents. 
        /// Note: in order to retrieve the cell's value we use a binding since we don't know about the underlying object. 
        /// The most performant way to retrieve the cell's value is to grab the row's Data (this.Cell.Row.Data), 
        /// cast it as your object and grab the actual value manually. 
        /// </summary>
        /// <remarks>
        /// Unlike Value this does not apply the ValueConverter to the value before returning it.
        /// </remarks>
        internal virtual object RawValue
        {
            get
            {
                return null;
            }
        }

        #endregion // RawValue

        #region SuppressCellControlAttached

        internal bool SuppressCellControlAttached { get; set; }

        #endregion // SuppressCellControlAttached

        #endregion // Internal

        #endregion // Properties

        #region Overrides

        #region OnElementAttached
        /// <summary>
		/// Called when the <see cref="CellControlBase"/> is attached to the <see cref="CellBase"/>
		/// </summary>
		/// <param propertyName="element">A <see cref="CellControlBase"/></param>
		protected override void OnElementAttached(CellControlBase element)
		{
			this._control = element;
			this.ApplyStyle();
			element.OnAttached(this);
		}
		#endregion // OnElementAttached

        #region OnElementReleasing

        /// <summary>
        /// Invoked when a <see cref="CellControlBase"/> is being released from an object.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>False, if the element shouldn't be released.</returns>
        protected override bool OnElementReleasing(CellControlBase element)
        {
            return element.OnReleasing(this);
        }

        #endregion // OnElementReleasing

        #region OnElementReleased
        /// <summary>
		/// Called when the <see cref="CellControlBase"/> is removed from the <see cref="CellBase"/>
		/// </summary>
		/// <param propertyName="element">A <see cref="CellControlBase"/></param>
		protected override void OnElementReleased(CellControlBase element)
		{
			this._control = null;
			element.OnReleased(this);
		}
		#endregion // OnElementReleased

		#region ToString


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		#endregion // ToString

        #region RecyclingElementType

        /// <summary>
        /// Gets the Type of control that should be created for the <see cref="Cell"/>.
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
                return this.Row.RowType.ToString() + "_" + this.Column.Key + "_" + this.Column.ColumnLayout.Key;
            }
        }
        #endregion // RecyclingIdentifier

		#endregion // Overrides

		#region Methods

		#region Protected

		#region EnsureCurrentState

		/// <summary>
		/// Ensures that <see cref="CellBase"/> is in the correct state.
		/// </summary>
		protected internal virtual void EnsureCurrentState()
		{
			if (this.Control != null)
			{
				// Common States				
				if (this.Row.IsMouseOver && this.Row.ResolveRowHover != RowHoverType.None && (this.Row.ResolveRowHover == RowHoverType.Row || this == this.Row.ColumnLayout.Grid.MouseOverCell))
					this.Control.GoToState(this.MouseOverState, true);
				else
					this.Control.GoToState(this.NormalState, false);

				// Active States
				if (this == this.Row.ColumnLayout.Grid.ActiveCell)
					this.Control.GoToState("Active", false);
				else
					this.Control.GoToState("InActive", false);
			}
		}

		#endregion // EnsureCurrentState

		#region HandleKeyDown

		/// <summary>
		/// Should be handled by a derived class so that a cell can determine what to do with the given keyboard action.
		/// </summary>
		/// <param propertyName="key">The <see cref="Key"/> that was pressed.</param>
		/// <param propertyName="platformKey">The integer that represents the key pressed</param>
		/// <returns>True if the key is handled.</returns>
		protected internal virtual bool HandleKeyDown(Key key, int platformKey)
		{
			return false;
		}

		#endregion // HandleKeyDown

		#region OnCellMouseDown

		/// <summary>
		/// Invoked when a cell is clicked.
		/// </summary>
		/// <returns>Whether or not the method was handled.</returns>
		protected internal virtual DragSelectType OnCellMouseDown(MouseEventArgs e)
		{
			return DragSelectType.None;
		}

		#endregion // OnCellMouseDown

		#region OnCellClick

        /// <summary>
        /// Invoked when a cell is clicked.
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void OnCellClick(MouseButtonEventArgs e)
		{

		}

		#endregion // OnCellClick

		#region OnCellDoubleClick

		/// <summary>
		/// Invoked when a cell is double clicked.
		/// </summary>
		protected internal virtual void OnCellDoubleClick()
		{

		}

		#endregion // OnCellDoubleClick

		#region OnCellDragging

		/// <summary>
		/// Invoked when dragging the mouse over a cell. 
		/// </summary>
		protected internal virtual void OnCellDragging(DragSelectType type)
		{

		}
		#endregion // OnCellDragging

		#region OnCellMouseOver

		/// <summary>
		/// Executed when the mouse moves over a cell.
		/// </summary>
		/// <param propertyName="e"></param>
		protected internal virtual void OnCellMouseMove(MouseEventArgs e)
		{

		}

		#endregion //  OnCellMouseOver

		#region OnCellMouseUp

		/// <summary>
		/// Executed when the mouse up happens on a cell.
		/// </summary>
		/// <param propertyName="e"></param>
		protected internal virtual void OnCellMouseUp(MouseEventArgs e)
		{

		}

		#endregion //  OnCellMouseUp

		#region SetSelected
		/// <summary>
		/// Sets the selected state of an item. 
		/// </summary>
		/// <param propertyName="isSelected"></param>
		protected internal virtual void SetSelected(bool isSelected)
		{
		}
		#endregion // SetSelected

		#region ApplyStyle
		/// <summary>
		/// Applies the resolved style of a Cell to it's <see cref="CellControlBase"/>
		/// </summary>
		protected internal virtual void ApplyStyle()
		{
			if (this.Control != null)
			{
				Style s = this.ResolveStyle;
				if (this.Control.Style != s)
				{
                    this.EnsureCurrentState();

                    if (s != null)
                        this.Control.Style = s;
                    else
                        this.Control.ClearValue(CellControlBase.StyleProperty);
				}
			}
		}
		#endregion // ApplyStyle

		#region EditorValueChanged

		/// <summary>
		/// Method called when the editor of the column registers a value changed.
		/// </summary>
		/// <param propertyName="value"></param>
		protected internal virtual void EditorValueChanged(object value)
		{
			this.Row.CellEditorValueChanged(this, value);
		}

		#endregion // EditorValueChanged

		#endregion // Protected

		#region Public

		#region Refresh

		/// <summary>
		/// Refreshes the content of the cell.
		/// </summary>
		public virtual void Refresh()
		{
			if (this.Control != null)
			{
				this.Control.Refresh();
			}
		}

		#endregion // Refresh

		#endregion //Public 

		#endregion // Methods

		#region States

		/// <summary>
		/// Determines the string that should be used for the "Normal" Visual State of the <see cref="CellControlBase"/>.
		/// </summary>
		protected internal virtual string NormalState
		{
			get
			{
                if (this.Row.IsAlternateRow)
                    return "Alternate";

				return "Normal";
			}
		}

		/// <summary>
		/// Determines the string that should be used for the "MouseOver" Visual State of the <see cref="CellControlBase"/>.
		/// </summary>
		protected internal virtual string MouseOverState
		{
			get
			{
				return "MouseOver";
			}
		}

		#endregion // States

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