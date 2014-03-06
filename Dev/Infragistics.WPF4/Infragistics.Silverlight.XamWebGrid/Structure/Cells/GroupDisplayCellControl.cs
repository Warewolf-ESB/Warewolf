using System.Windows;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// Visual object for the <see cref="GroupDisplayCell"/> object.
    /// </summary>
    [TemplateVisualState(GroupName = "ActiveStates", Name = "Active")]
    [TemplateVisualState(GroupName = "ActiveStates", Name = "InActive")]
    [TemplateVisualState(GroupName = "SelectedStates", Name = "NotSelected")]
    [TemplateVisualState(GroupName = "SelectedStates", Name = "Selected")]
    [TemplateVisualState(GroupName = "FixedStates", Name = "Fixed")]
    [TemplateVisualState(GroupName = "FixedStates", Name = "UnFixed")]
    [TemplateVisualState(GroupName = "EditingStates", Name = "Editing")]
    [TemplateVisualState(GroupName = "EditingStates", Name = "NotEditing")]
    [TemplateVisualState(GroupName = "LogicalOperators", Name = "NoLogicalOp")]
    [TemplateVisualState(GroupName = "LogicalOperators", Name = "And")]
    [TemplateVisualState(GroupName = "LogicalOperators", Name = "Or")]
    public class GroupDisplayCellControl : CellControl
    {
        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="GroupDisplayCellControl"/> class.
        /// </summary>
        static GroupDisplayCellControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupDisplayCellControl), new FrameworkPropertyMetadata(typeof(GroupDisplayCellControl)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="GroupDisplayCellControl"/> class.
        /// </summary>
        public GroupDisplayCellControl()
        {



        }

        #endregion // Constructor

        #region InnerControlMargin

        /// <summary>
        /// Identifies the <see cref="InnerControlMargin"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty InnerControlMarginProperty = DependencyProperty.Register("InnerControlMargin", typeof(Thickness), typeof(GroupDisplayCellControl), new PropertyMetadata(new PropertyChangedCallback(InnerControlMarginChanged)));

        /// <summary>
        /// This control is designed with a border in center so lower groups can be denoted with a slight indentation.
        /// </summary>
        public Thickness InnerControlMargin
        {
            get { return (Thickness)this.GetValue(InnerControlMarginProperty); }
            set { this.SetValue(InnerControlMarginProperty, value); }
        }

        private static void InnerControlMarginChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            GroupDisplayCellControl ctrl = (GroupDisplayCellControl)obj;
            ctrl.OnPropertyChanged("InnerControlMargin");
        }

        #endregion // InnerControlMargin

        #region EnsureCurrentState
        /// <summary>
        /// Ensures that <see cref="CellControlBase"/> is in the correct state.
        /// </summary>
        protected internal override void EnsureCurrentState()
        {
            if (this.Cell != null && this.Cell.Row != null)
            {
                XamGridConditionInfo info = this.Cell.Row.Data as XamGridConditionInfo;

                if (info != null)
                {
                    LogicalOperator? op = ResolveOperator();

                    if (op == LogicalOperator.Or)
                    {
                        VisualStateManager.GoToState(this, "Or", false);
                    }
                    else if (op == LogicalOperator.And)
                    {
                        VisualStateManager.GoToState(this, "And", false);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, "NoLogicalOp", false);
                    }
                }
            }

            base.EnsureCurrentState();
        }
        #endregion // EnsureCurrentState

        #region ArrangeOverride
        
        /// <summary>
        /// Validates the Arranging of the cell, and makes sure that the width it will arrange at, matches the column's ActualWidth
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            this.EnsureCurrentState();

            return base.ArrangeOverride(finalSize);
        }

        #endregion // ArrangeOverride

        #region Methods
        internal LogicalOperator? ResolveOperator()
        {
            int groupDisplayColumnCount = 0;

            ReadOnlyKeyedColumnBaseCollection<Column> dataColumns = this.Column.ColumnLayout.Columns.DataColumns;

            for (int i = 0; i < dataColumns.Count; i++)
            {
                if (dataColumns[i] is GroupDisplayColumn)
                    groupDisplayColumnCount++;
            }

            int myIndex = dataColumns.IndexOf(this.Column);

            // ok so at this point I know what index I am at, so I need to go up to the level that I am 

            XamGridConditionInfoGroup currentGroup = ((XamGridConditionInfo)this.Cell.Row.Data).Group;

            while (currentGroup != null)
            {
                if (currentGroup.Level == myIndex)
                {
                    return currentGroup.Operator;
                }
                currentGroup = currentGroup.ParentGroup;
            }

            return null;
        }
        #endregion // Mehtods        
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