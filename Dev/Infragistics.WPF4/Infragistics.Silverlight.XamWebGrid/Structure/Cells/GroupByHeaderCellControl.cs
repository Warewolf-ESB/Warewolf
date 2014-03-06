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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// Visual object for the <see cref="GroupByHeaderCell"/> object.
	/// </summary>
	[TemplateVisualState(GroupName = "FirstStates", Name = "NotFirst")]
	[TemplateVisualState(GroupName = "FirstStates", Name = "First")]

	[TemplateVisualState(GroupName = "LastStates", Name = "NotLast")]
	[TemplateVisualState(GroupName = "LastStates", Name = "Last")]
	public class GroupByHeaderCellControl : HeaderCellControl
	{
		#region Members

		bool _isFirst, _isLast;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="GroupByHeaderCellControl"/> object.
		/// </summary>
		public GroupByHeaderCellControl()
		{
			base.DefaultStyleKey = typeof(GroupByHeaderCellControl);
		}

		#endregion // Constructor

		#region Properties
		
		#region Public
		
		#region IsFirst

		/// <summary>
		/// Gets/Sets whether this <see cref="GroupByHeaderCellControl"/> is the first <see cref="Column"/> in its grouping.
		/// </summary>
		/// <remarks>
		/// Note: this is entirely for styling purposes
		/// </remarks>
		public bool IsFirst
		{
			get
			{
				return this._isFirst;
			}
			set
			{
				this._isFirst = value;

				if (this._isFirst)
					VisualStateManager.GoToState(this, "First", false);
				else
					VisualStateManager.GoToState(this, "NotFirst", false);
			}
		}
		#endregion // IsFirst

		#region IsLast

		/// <summary>
		/// Gets/Sets whether this <see cref="GroupByHeaderCellControl"/> is the last <see cref="Column"/> in it's grouping.
		/// </summary>
		/// <remarks>
		/// Note: this is entirely for styling purposes
		/// </remarks>
		public bool IsLast
		{
			get
			{
				return this._isLast;
			}
			set
			{
				this._isLast = value;

				if (this._isLast)
					VisualStateManager.GoToState(this, "Last", false);
				else
					VisualStateManager.GoToState(this, "NotLast", false);
			}
		}
		#endregion // IsLast

		#endregion // Public

		#endregion // Properties

		#region Methods

		#region Public

		/// <summary>
		/// Has the <see cref="GroupByHeaderCellControl"/> validate it's current state.
		/// </summary>
		public void UpdateState()
		{
			Column col = this.ResolveColumn();

			if (col != null)
			{
				if (col.IsSorted == SortDirection.Ascending)
					this.GoToState("Ascending", false);
				else if (col.IsSorted == SortDirection.Descending)
					this.GoToState("Descending", false);
				else
					this.GoToState("NotSorted", false);
			}
		}

		#endregion // Public

		#endregion // Methods

		#region Overrides

		#region OnMouseEnter
		/// <summary>
		/// Called before the MouseEnter event is fired.
		/// </summary>
		/// <param propertyName="e"></param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			if(this.Cell != null)
				this.GoToState(this.Cell.MouseOverState, true);

			base.OnMouseEnter(e);
		}
		#endregion // OnMouseEnter

		#region OnMouseLeave
		/// <summary>
		/// Called before the MouseLeave event is fired.
		/// </summary>
		/// <param propertyName="e"></param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			if (this.Cell != null)
				this.GoToState(this.Cell.NormalState, false);
			base.OnMouseLeave(e);
		}
		#endregion // OnMouseLeave

		#region OnLoaded

		/// <summary>
		/// Raised when the <see cref="CellControlBase"/> is Loaded. 
		/// </summary>
		protected override void OnLoaded()
		{
			this.UpdateState();
			base.OnLoaded();
		}
		#endregion // OnLoaded
		
		#region OnMouseLeftButtonUpColumnMoving
		/// <summary>
		/// Called before the <see cref="UIElement.MouseLeftButtonDown"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event</param>
		protected override void OnMouseLeftButtonUpColumnMoving(MouseButtonEventArgs e)
		{
			if (this.IsDragging)
			{
				// If we're not over the GroupByArea, then it's time to remove the column from the grouping.
				GroupByAreaCellControl gbc = this.GetCellControlFromPoint(e.GetPosition(null), typeof(GroupByAreaCellControl), false) as GroupByAreaCellControl;
				if (gbc == null)
				{
					this.ResolveColumn().IsGroupBy = false;
					this.Cell.Row.ColumnLayout.Grid.OnColumnDragCanceled(this.ResolveColumn(), DragCancelType.ColumnUngrouped);
					this.EndDrag();
				}
				else
				{
					base.OnMouseLeftButtonUpColumnMoving(e);
				}
			}
			else
				base.OnMouseLeftButtonUpColumnMoving(e);
		}
		#endregion // OnMouseLeftButtonUpColumnMoving

		#region ResolveDropHeader

		/// <summary>
		/// Looks for a <see cref="CellControlBase"/> under the current mouse position.
		/// </summary>
		/// <param propertyName="rootPoint"></param>
		/// <param propertyName="draggingElementBounds"></param>
		/// <returns></returns>
		protected override CellControlBase ResolveDropHeader(Point rootPoint, Rect draggingElementBounds)
		{
			return this.GetCellControlFromPoint(rootPoint, typeof(GroupByHeaderCellControl), true);
		}
		#endregion // ResolveDropHeader
		
		#region ReactToMouseOverDroppableHeader
		/// <summary>
		/// Allows the GroupByHeader to react to other GroupByHeaders in it's Panel.
		/// </summary>
		/// <param propertyName="e"></param>
		/// <param propertyName="dropHeaderBounds"></param>
		/// <param propertyName="currentHeaderBounds"></param>
		protected override void ReactToMouseOverDroppableHeader(MouseEventArgs e, Rect dropHeaderBounds, Rect currentHeaderBounds)
		{
			
		}

		#endregion // ReactToMouseOverDroppableHeader

		#region ResolveColumn
		/// <summary>
		/// Resolves the underlying <see cref="Column"/> that should be references when checking properties such as 
		/// IsMoveable, IsGroupable, etc...
		/// </summary>
		/// <returns></returns>
		protected override Column ResolveColumn()
		{
			if(this.Cell != null)
				return ((RowsManager)this.Cell.Row.Manager).GroupedColumn;
			return null;
		}
		#endregion // ResolveColumn

		#region GenerateDragHeader
		/// <summary>
		/// Generates a new <see cref="HeaderCellControl"/> that will be dragged around for moving operations.
		/// </summary>
		/// <returns></returns>
		protected override HeaderCellControl GenerateDragHeader()
		{
			GroupByHeaderCellControl gbhc = new GroupByHeaderCellControl();
			gbhc.OnAttached(this.Cell);
			gbhc.IsFirst = this.IsFirst;
			gbhc.IsLast = this.IsLast;
			return gbhc;
		}
		#endregion // GenerateDragHeader

		#region OnApplyTemplate

		/// <summary>
		/// Builds the visual tree for the <see cref="GroupByHeaderCellControl"/> when a new template is applied. 
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.IsFirst = this.IsFirst;
			this.IsLast = this.IsLast;
		}

		#endregion // OnApplyTemplate

		#region MarkColumnAsMoving
		/// <summary>
		/// Allows the <see cref="HeaderCellControl"/> to react to apply settings regarding marking the column has moving.
		/// </summary>
		protected override void MarkColumnAsMoving()
		{

		}
		#endregion // MarkColumnAsMoving

		#region Indicator
		/// <summary>
		///  Gets the drop indciator <see cref="Popup"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Popup Indicator
		{
			get
			{
				if (this.Cell != null)
					return this.Cell.Row.ColumnLayout.GroupBySettings.IndicatorContainer;
				return null;
			}
		}
		#endregion // Indicator

		#region ValidateWidthInArrange
		/// <summary>
		/// Gets whether the Control should validate it's width in ArrangeOverride. 
		/// </summary>
		protected override bool ValidateWidthInArrange
		{
			get { return false; }
		}
		#endregion // ValidateWidthInArrange

        #region EnsureContent

        /// <summary>
        /// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
        /// </summary>
        internal protected override void EnsureContent()
        {
            Column column = this.ResolveColumn();
            if (column != null)
            {
                base.EnsureContent();

                if (column.DataField.GroupName != null)
                    this.Content = column.DataField.GroupName;
            }
        }

        #endregion // EnsureContent

		#endregion // Overrides
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