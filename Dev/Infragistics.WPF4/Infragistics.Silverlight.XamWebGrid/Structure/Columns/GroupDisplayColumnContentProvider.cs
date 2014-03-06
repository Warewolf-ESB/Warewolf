using System.Windows;
using System.Windows.Data;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A custom <see cref="ColumnContentProviderBase"/> which is used by the <see cref="GroupDisplayColumn"/> on the <see cref="CompoundFilterDialogControl"/>.
    /// </summary>
    /// <remarks>Not for general use.</remarks>
    public class GroupDisplayColumnContentProvider : ColumnContentProviderBase
    {
        #region Overrides

        #region ResolveDisplayElement

        /// <summary>
        /// Sets up the element that will be displayed in a <see cref="Cell"/>, when the cell is not in edit mode. 
        /// </summary>
        /// <param propertyName="cell">The cell that the display element will be displayed in.</param>
        /// <param propertyName="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
        /// <returns>The element that should be displayed.</returns>
        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            XamGridConditionInfo ci = cell.Row.Data as XamGridConditionInfo;

            int groupDisplayColumnCount = 0;

            ReadOnlyKeyedColumnBaseCollection<Column> dataColumns = cell.Column.ColumnLayout.Columns.DataColumns;

            GroupDisplayCellControl displayCellControl = (GroupDisplayCellControl)cell.Control;
            for (int i = 0; i < dataColumns.Count; i++)
            {
                if (dataColumns[i] is GroupDisplayColumn)
                    groupDisplayColumnCount++;
            }

            int myIndex = dataColumns.IndexOf(cell.Column);

            if (myIndex == 0)
            {
                displayCellControl.InnerControlMargin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                if (ci.Group.ParentGroup != null)
                {
                    int currentIndex = ci.Group.ParentGroup.ChildGroups.IndexOf(ci.Group);
                    if (currentIndex == 0)
                        displayCellControl.InnerControlMargin = new Thickness(0, myIndex * 3, 0, 0);
                    else if (currentIndex == ci.Group.ParentGroup.ChildGroups.Count - 1)
                        displayCellControl.InnerControlMargin = new Thickness(0, 0, 0, myIndex * 3);
                    else
                        displayCellControl.InnerControlMargin = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    displayCellControl.InnerControlMargin = new Thickness(0, 0, 0, 0);
                }
            }

            return null;
        }

        #endregion // ResolveDisplayElement

        #region AdjustDisplayElement
        /// <summary>
        /// Called during EnsureContent to allow the provider a chance to modify it's display based on the current conditions.
        /// </summary>
        /// <param name="cell"></param>
        public override void AdjustDisplayElement(Cell cell)
        {
            XamGridConditionInfo ci = cell.Row.Data as XamGridConditionInfo;

            int groupDisplayColumnCount = 0;

            ReadOnlyKeyedColumnBaseCollection<Column> dataColumns = cell.Column.ColumnLayout.Columns.DataColumns;

            GroupDisplayCellControl displayCellControl = (GroupDisplayCellControl)cell.Control;
            for (int i = 0; i < dataColumns.Count; i++)
            {
                if (dataColumns[i] is GroupDisplayColumn)
                    groupDisplayColumnCount++;
            }

            int myIndex = dataColumns.IndexOf(cell.Column);

            if (myIndex == 0)
            {
                displayCellControl.InnerControlMargin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                
                if (ci.Group.ParentGroup != null)
                {
                    
                    //int currentIndex = ci.Group.ParentGroup.ChildGroups.IndexOf(ci.Group);

                    //if (currentIndex == 0 && ci.Group.InfoObjects.IndexOf(ci) == 0 && ci.Group.Level == myIndex)
                    //{
                    //    displayCellControl.InnerControlMargin = new Thickness(0, myIndex * 3, 0, 0);
                    //}
                    //else if ((currentIndex == ci.Group.ParentGroup.ChildGroups.Count - 1) && 
                    //    (ci.Group.InfoObjects.IndexOf(ci) == ci.Group.InfoObjects.Count - 1) && ci.Group.Level == myIndex)
                    //{
                    //    displayCellControl.InnerControlMargin = new Thickness(0, 0, 0, myIndex * 3);
                    //}
                    //else
                    //    displayCellControl.InnerControlMargin = new Thickness(0, 0, 0, 0);

                    // get the index of the item inside the group.
                    int currentIndex = ci.Group.InfoObjects.IndexOf(ci);

                    if (currentIndex == 0 && ci.Group.Level == myIndex)
                    {
                        displayCellControl.InnerControlMargin = new Thickness(0, myIndex * 3, 0, 0);
                    }
                    else if (currentIndex == ci.Group.InfoObjects.Count - 1 && ci.Group.Level == myIndex)
                    {
                        displayCellControl.InnerControlMargin = new Thickness(0, 0, 0, myIndex * 3);
                    }
                    else
                        displayCellControl.InnerControlMargin = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    displayCellControl.InnerControlMargin = new Thickness(0, 0, 0, 0);

                }
            }

            if (cell.Control != null)
                cell.Control.EnsureCurrentState();

            base.AdjustDisplayElement(cell);
        }
        #endregion // AdjustDisplayElement

        #region ResolveValueFromEditor
        /// <summary>
        /// Resolves the value of the editor control, so that the cell's underlying data can be updated. 
        /// </summary>
        /// <param propertyName="cell">The <see cref="Cell"/> that the editor id being displayed in.</param>
        /// <returns>The value that should be displayed in the cell.</returns>
        public override object ResolveValueFromEditor(Cell cell)
        {
            return null;
        }
        #endregion // ResolveValueFromEditor

        #region ResolveEditorControl

        /// <summary>
        /// Sets up the edtior control that will be displayed in a <see cref="Cell"/> when the the cell is in edit mode.
        /// </summary>
        /// <param propertyName="cell">The <see cref="Cell"/> entering edit mode.</param>
        /// <param propertyName="editorValue">The value that should be put in the editor.</param>
        /// <param propertyName="availableWidth">The amount of horizontal space available.</param>
        /// <param propertyName="availableHeight">The amound of vertical space available.</param>
        /// <param propertyName="editorBinding">Provides a <see cref="Binding"/> that can be used for setting up the editor.</param>
        /// <returns></returns>
        protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
        {
            return null;
        }

        #endregion // #region ResolveEditorControl

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