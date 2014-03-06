using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Themes;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.OutlookBar;
using System.ComponentModel;

namespace Infragistics.Windows.OutlookBar
{
    /// <summary>
    /// A Thumb derived element used as splitter between the navigation area and selected group content area in the <see cref="XamOutlookBar"/>.
    /// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GroupAreaSplitter : Thumb
    {
        #region Member Variables

        // GroupAreaSplitter has two increment step in contrast to GridSplitter
        // This is because the height of groups in XamOutlookBar is not constant value for all groups
        // GroupAreaSplitter has <!--ResizeBehavior="PreviousAndNext"--> always
        
        private double _dragIngrementUp;    // this is a height of 1st group in the overflow area    
        private double _dragIngrementDn;    // this is a height of last group in the navigation area

        // Put the GroupAreaSplitter in middle row of the grid: 
        //         selected group content-splitter-navigation area
        
        private RowDefinition _contentRow;  // previous row - selected group content
        private RowDefinition _groupsRow;   // next row - navigation area

        GroupsPresenter _navigationArea;    // navigation area items control

        #endregion //Member Variables	
    
        #region Constructors
        static GroupAreaSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupAreaSplitter), new FrameworkPropertyMetadata(typeof(GroupAreaSplitter)));

        }

        /// <summary>
        /// Initializes a new <see cref="GroupAreaSplitter"/>
        /// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamOutlookBar"/> in the control's default Templates. You do not normally need to create an instance of this class.</p>
		/// </remarks>
		public GroupAreaSplitter()
        {
            this.DragDelta += new DragDeltaEventHandler(GroupAreaSplitter_DragDelta);
        }

        #endregion //Constructors	

        #region Base Class Overrides

        #region Automation
        /// <summary>
        /// Returns <see cref="GroupAreaSplitter"/> Automation Peer Class <see cref="GroupAreaSplitterAutomationPeer"/>
        /// </summary>
        /// <returns>AutomationPeer</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GroupAreaSplitterAutomationPeer(this);
        }
        #endregion

        #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            XamOutlookBar xob = this.GetValue(XamOutlookBar.OutlookBarProperty) as XamOutlookBar;

            if (xob == null)
                return;

            Grid grid = VisualTreeHelper.GetParent(this) as Grid;
            if (grid != null)
            {
                int r = (int)this.GetValue(Grid.RowProperty);

                if (grid.RowDefinitions.Count <= r + 1 || r < 1)
                    return;

                _contentRow = grid.RowDefinitions[r - 1];   // r-1: selected group content
                                                            //  r : this splitter
                _groupsRow = grid.RowDefinitions[r + 1];    // r+1: navigation area groups

                foreach (FrameworkElement f in grid.Children)
                {
                    _navigationArea = f as GroupsPresenter;
                    if (_navigationArea != null)
                        break;
                }// end for - search for the navigation Area
            }// end if- this is a grid splitter
        }

        #endregion //OnApplyTemplate	
            
        #endregion //Base Class Overrides

        #region Properties

        #region Internal Properties

        #region DragIngrementDn






        internal double DragIngrementDn
        {
            get { return _dragIngrementDn; }
            set { _dragIngrementDn = value; }
        }

        #endregion //DragIngrementDn	

        #region DragIngrementUp






        internal double DragIngrementUp
        {
            get { return _dragIngrementUp; }
            set { _dragIngrementUp = value; }
        }

        #endregion //DragIngrementUp	
    
    
        #endregion //Internal Properties	
    
        #endregion //Properties	

        #region Methods

        #region Internal Methods







        internal bool ShrinkNavigationArea()
        {
            if (_groupsRow != null)
            {
                double hNavigationArea = _groupsRow.ActualHeight - DragIngrementDn;
                if (hNavigationArea < 0)
                    hNavigationArea = 0;
                GridLength l = new GridLength(hNavigationArea);
                _groupsRow.Height = l;
                (_groupsRow.Parent as Grid).UpdateLayout();
                if (_navigationArea != null)
                {
                    _navigationArea.Height = l.Value;
                }
                return true;
            }
            return false;
        }






        internal bool ExtendNavigationArea()
        {
            if (_contentRow != null)
            {
                if (DragIngrementUp < Double.Epsilon)
                    return false;

                double maxIncrementUp = _contentRow.ActualHeight - _contentRow.MinHeight;

                GridLength l = new GridLength(_groupsRow.ActualHeight + DragIngrementUp);
                double hCurArea = _contentRow.ActualHeight - DragIngrementUp;
                if (hCurArea < 0)
                    hCurArea = 0;
                GridLength lcurArea = new GridLength(hCurArea);

                if (lcurArea.Value < _contentRow.MinHeight)
                    return false;
                
                _groupsRow.Height = l;
                Grid parentGrid = _groupsRow.Parent as Grid;

                if (_navigationArea != null)
                {
                    _navigationArea.Height = l.Value;
                    _navigationArea.UpdateLayout();
                    if (Math.Abs(_groupsRow.ActualHeight - l.Value) > 0.1)
                    {
                        _navigationArea.Height = l.Value;
                        _navigationArea.UpdateLayout();
                    }
                }

                return true;
            }
            return false;
        }

        internal bool CanSetNavigationAreaSize(double newHeight)
        {
            if (_contentRow != null)
            {
                double deltaH = newHeight - _groupsRow.ActualHeight;
                double newSelectedAreaHeight = _contentRow.ActualHeight - deltaH;

                if (newSelectedAreaHeight < 0)
                    newSelectedAreaHeight = 0;

                if (newSelectedAreaHeight < _contentRow.MinHeight)
                    return false;
                
                return true;
            }
            return false;
        }

        internal bool SetNavigationAreaSize(double newHeight)
        {
            if (_contentRow == null)
                this.OnApplyTemplate();
            if (_contentRow != null)
            {
                if (!CanSetNavigationAreaSize(newHeight))
                    return false;

                _groupsRow.Height = new GridLength(newHeight);

                Grid parentGrid = _groupsRow.Parent as Grid;

                parentGrid.InvalidateMeasure();
                parentGrid.UpdateLayout();

                if (_navigationArea != null)
                {
                    _navigationArea.Height = newHeight;
                    _navigationArea.UpdateLayout();
                    if (Math.Abs(_groupsRow.ActualHeight - newHeight) > 0.1)
                    {
                        _navigationArea.Height = newHeight;
                        _navigationArea.UpdateLayout();
                    }
                }
                return true;
            }
            return false;
        }

        #region PerformMove
        internal void PerformMove(double x, double y)
        {
            XamOutlookBar xob = this.GetValue(XamOutlookBar.OutlookBarProperty) as XamOutlookBar;

            if (xob == null)
                return;

            int allGroups = xob.VisibleGroupsCount;

            if (allGroups == 0)
                return;
            
            Point topLeft= Utilities.PointToScreenSafe(this, new Point(0, 0));

            double dy = y - topLeft.Y;
            
            double groupHeight = 24;

            if (xob.NavigationAreaGroups.Count > 0)
                groupHeight = xob.NavigationAreaGroups[0].ActualHeight;
            else if (xob.OverflowAreaGroups.Count > 0)
                groupHeight = xob.OverflowAreaGroups[0].ActualHeight;

            int deltaGroups = (int)(dy / groupHeight);
            int navigationAreaGroups= xob.NavigationAreaGroups.Count - deltaGroups;
            
            if(navigationAreaGroups<0)
                navigationAreaGroups=0;

            if (navigationAreaGroups > allGroups)
                navigationAreaGroups = allGroups;

            xob.NavigationAreaMaxGroups = navigationAreaGroups;
            xob.NavigationAreaMaxGroups = xob.NavigationAreaGroups.Count;

        }
        #endregion //PerformMove

        #endregion //Internal Methods

        #region Private methods

        #region Event Handlers

        void GroupAreaSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            XamOutlookBar xob = this.GetValue(XamOutlookBar.OutlookBarProperty) as XamOutlookBar;

            if (xob == null)
                return;

            if (e.VerticalChange >= 0)
            {
                //  set drag increment to height of first group in the overflow area
                if (DragIngrementDn < 0.01)
                    return;
                if (e.VerticalChange < DragIngrementDn)
                    return;
                if (xob.NavigationAreaGroups.Count > 0)
                    xob.NavigationAreaMaxGroups = xob.NavigationAreaGroups.Count - 1;
            }// end if- shrink groups, splitter moves down
            else
            {
                //  set drag increment to height of first group in the overflow area
                if (DragIngrementUp < 0.01)
                    return;
                if (Math.Abs(e.VerticalChange) < DragIngrementUp)
                    return;
                xob.NavigationAreaMaxGroups = xob.NavigationAreaGroups.Count + 1;
                xob.NavigationAreaMaxGroups = xob.NavigationAreaGroups.Count;
            }// end else - expnad groups, splitter moves up
        }

        private bool CanExtendNavigationArea()
        {
            XamOutlookBar xob = this.GetValue(XamOutlookBar.OutlookBarProperty) as XamOutlookBar;

            if (xob == null)
                return false;

            if (_contentRow == null)
                return false;

            if (DragIngrementUp < Double.Epsilon)
                return false;

            if (xob.NavigationAreaGroups.Count >= xob.Groups.Count)
                return false;

            GridLength l = new GridLength(_groupsRow.ActualHeight + DragIngrementUp);
            double hCurArea = _contentRow.ActualHeight - DragIngrementUp;
            if (hCurArea < 0)
                hCurArea = 0;
            GridLength lcurArea = new GridLength(hCurArea);

            if (lcurArea.Value < _contentRow.MinHeight)
                return false;

            return true;
        }

        #endregion //Event Handlers

        #endregion //Private methods	
    
        #endregion //Methods	
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