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

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// Visual object for the <see cref="GroupByAreaCell"/> object.
	/// </summary>
	[TemplatePart(Name="Panel", Type=typeof(Panel))]

	[TemplateVisualState(GroupName = "DisplayStates", Name = "Empty")]
	[TemplateVisualState(GroupName = "DisplayStates", Name = "NonEmpty")]

	[TemplateVisualState(GroupName = "DraggingStates", Name = "NotDragging")]
	[TemplateVisualState(GroupName = "DraggingStates", Name = "Dragging")]
	[TemplateVisualState(GroupName = "DraggingStates", Name = "DraggingOver")]

	[TemplateVisualState(GroupName = "ExpansionStates", Name = "Expanded")]
	[TemplateVisualState(GroupName = "ExpansionStates", Name = "Collapsed")]

	[TemplateVisualState(GroupName = "GroupByAreaLocation", Name = "Top")]
	[TemplateVisualState(GroupName = "GroupByAreaLocation", Name = "Bottom")]
	public class GroupByAreaCellControl : CellControlBase, ICommandTarget
	{
		#region Members
		Panel _panel;
		Collection<Column> _groupedColumns;
		int _gbcDirtyFlag;
        bool _isDesignTime;
		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="GroupByAreaCellControl"/> class.
		/// </summary>
		public GroupByAreaCellControl()
		{
			base.DefaultStyleKey = typeof(GroupByAreaCellControl);
			this._groupedColumns = new Collection<Column>();

            this._isDesignTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(this);
		}

		#endregion // Constructor

		#region Properties 

		#region EmptyContent

		/// <summary>
		/// Identifies the <see cref="EmptyContent"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty EmptyContentProperty = DependencyProperty.Register("EmptyContent", typeof(object), typeof(GroupByAreaCellControl), new PropertyMetadata(new PropertyChangedCallback(EmptyContentChanged)));

		/// <summary>
		/// Gets/sets the content that will be displayed when there are no UIElements in the panel.
		/// </summary>
		public object EmptyContent
		{
			get { return (object)this.GetValue(EmptyContentProperty); }
			set { this.SetValue(EmptyContentProperty, value); }
		}

		private static void EmptyContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
		
		}

		#endregion // EmptyContent 
				
         
		#region IsExpanded

		/// <summary>
		/// Identifies the <see cref="IsExpanded"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(GroupByAreaCellControl), new PropertyMetadata(true, new PropertyChangedCallback(IsExpandedChanged)));

		/// <summary>
		/// Gets/Sets whether the <see cref="GroupByAreaCellControl"/> is expanded or collapsed.
		/// </summary>
		public bool IsExpanded
		{
			get { return (bool)this.GetValue(IsExpandedProperty); }
			set { this.SetValue(IsExpandedProperty, value); }
		}

		private static void IsExpandedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupByAreaCellControl ctrl = (GroupByAreaCellControl)obj;
            CellBase cell = ctrl.Cell;
            if (cell != null)
            {
                ctrl.Cell.Row.ColumnLayout.Grid.GroupBySettings.IsGroupByAreaExpanded = (bool)e.NewValue;
                ctrl.Cell.Row.ColumnLayout.Grid.InvalidateScrollPanel(false);
            }
		}

		#endregion // IsExpanded 
				
		#endregion // Properties

		#region Overrides

		#region OnApplyTemplate

		/// <summary>
		/// Builds the visual tree for the <see cref="GroupByAreaCellControl"/> when a new template is applied. 
		/// </summary>
		public override void OnApplyTemplate()
		{
			this._panel = base.GetTemplateChild("Panel") as Panel;
			this.EnsureContent();
		}

		#endregion // OnApplyTemplate

		#region OnAttached

		/// <summary>
		/// Called when the <see cref="GroupByAreaCell"/> is attached to the <see cref="GroupByAreaCellControl"/>.
		/// </summary>
		/// <param propertyName="cell">The <see cref="GroupByAreaCell"/> that is being attached to the <see cref="GroupByAreaCellControl"/></param>
		protected internal override void OnAttached(CellBase cell)
		{
			base.OnAttached(cell);

			this.IsExpanded = cell.Row.ColumnLayout.Grid.GroupBySettings.IsGroupByAreaExpanded;
		}

		#endregion // OnAttached

		#region EnsureContent

		/// <summary>
		/// Ensures that the underlying panel is populated with all <see cref="Column"/> objects that the <see cref="XamGrid"/>
		/// is grouped by.
		/// </summary>
		protected internal override void EnsureContent()
		{
			base.EnsureContent();
			
			if (this._panel != null && this.Cell != null)
			{
				XamGrid grid = this.Cell.Row.ColumnLayout.Grid;
				GroupByColumnsCollection groupByColumns = grid.GroupBySettings.GroupByColumns;

				if (grid.GroupBySettings.AllowGroupByArea == GroupByAreaLocation.Bottom)
					this.GoToState("Bottom", false);
				else
					this.GoToState("Top", true);

                bool isExpanded = grid.GroupBySettings.IsGroupByAreaExpanded;
                // Ugh... so toggling the IsGroupByAreaExpanded property at DesignTime causes VS to crash. 
                // And it appears to happen b/c of this one line, right here...
                // So if we're in DesignTime, don't set it.  
                // And Yes, i did try removing the code from the PropertyChangedEventHandler for the IsExpanded property
                // Still crashes....
                // the reason why its actually crashing is that in the ControlTemplate we have a TemplateBinding to that property.
                if (!this._isDesignTime)
                    this.IsExpanded = isExpanded;

				if (grid.GroupBySettings.ExpansionIndicatorVisibility == Visibility.Visible)
				{
					this.GoToState("ExpansionVisible", false);
					
					if (isExpanded)
					{
						this.GoToState("Expanded", true);
					}
					else
					{
						this.GoToState("Collapsed", true);
						this.Cell.Row.ActualHeight = 0;
					}
				}
				else
				{
					this.GoToState("Expanded", true);
					this.GoToState("ExpansionHidden", false);
				}

				this.EmptyContent = grid.GroupBySettings.EmptyGroupByAreaContent;

				if (groupByColumns.Count == 0)
					this.GoToState("Empty", false);
				else
					this.GoToState("NonEmpty", false);

				// If something has changed, then lets update the panel.
				if (this._gbcDirtyFlag != groupByColumns.DirtyFlag)
				{
					this._gbcDirtyFlag = groupByColumns.DirtyFlag;
					// Reset the actual height, so that it doesn'type get stuck at it's largest height. 
					this.Cell.Row.ActualHeight = 0; 

					// Clear out the cells we used previously
					this._panel.Children.Clear();
					this._groupedColumns.Clear();

					// Lets loop through all the group by columns
					foreach (Column column in groupByColumns)
					{
						ReadOnlyCollection<Column> cols = groupByColumns[column.ColumnLayout];
						
						// We need to bulid ourselves a Cell to put into the panel.
						GroupByHeaderCellControl hcc = new GroupByHeaderCellControl();
						RowsManager rmb = new RowsManager(0, column.ColumnLayout, null) { GroupedColumn = column };
						rmb.UnregisterRowsManager(false, false, false);
						hcc.OnAttached(new GroupByHeaderCell(new Row(0, rmb, null), rmb.GroupByColumn));
						hcc.Cell.Control = hcc;
						
						// Lets determine the Index, Level, and Key of the particular column we're adding to the panel
						ColumnLayout layout = column.ColumnLayout;
						int index = cols.IndexOf(column);
						string key = layout.Key;
						int level = layout.Level;

						// If we're not on Level 0, it means we need to display an additional header, for the column layout header.
						if (level > 0 )
						{
							if (index == 0)
							{
								GroupByColumnLayoutHeaderCellControl layoutHcc = new GroupByColumnLayoutHeaderCellControl();
								layoutHcc.OnAttached(new GroupByColumnLayoutHeaderCell(new Row(0, rmb, null), new GroupByColumn() { IsMovable = false }));
								layoutHcc.Cell.Control = layoutHcc;
								GroupByAreaPanel.SetIndex(layoutHcc, 0);
								GroupByAreaPanel.SetLevel(layoutHcc, level);
								GroupByAreaPanel.SetLevelKey(layoutHcc, key);

								this._panel.Children.Add(layoutHcc);
							}

							index++;
						}

						// Set the attached properties, so the panel knows what it's working with. 
						GroupByAreaPanel.SetIndex(hcc, index);
						GroupByAreaPanel.SetLevel(hcc, level);
						GroupByAreaPanel.SetLevelKey(hcc, key);

						this._panel.Children.Add(hcc);

						// Store off the column, for future verification, so that we don't have to repopulate the panel every time this method is called.
						this._groupedColumns.Add(column);
					}
					
				}

				// This is neccessary, to ensure that the panel always measures, especially, when it has no children.
				this._panel.InvalidateMeasure();
			}
		}

		#endregion // EnsureContent

		#endregion // Overrides

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
			return (command is GroupByAreaCommandBase);
		}
		#endregion // SupportsCommand

		#region  GetParameter
		/// <summary>
		/// Returns the object that defines the parameters necessary to execute the command.
		/// </summary>
		/// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
		/// <returns>The object necessary for the command to complete.</returns>
		protected virtual object GetParameter(CommandSource source)
		{
			return this;
		}
		#endregion // GetParameter

		#endregion // Protected

		#endregion // Methods

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