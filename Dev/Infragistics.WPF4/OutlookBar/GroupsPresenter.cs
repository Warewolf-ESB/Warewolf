using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections.Specialized;
using Infragistics.Windows.OutlookBar.Internal;

namespace Infragistics.Windows.OutlookBar
{
    /// <summary>
    /// An ItemsControl derived control used to present a collection of <see cref="OutlookBarGroup"/> items in the navigation and overflow areas of the <see cref="XamOutlookBar"/>.
    /// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GroupsPresenter : ItemsControl
    {
        #region Member Variables

        private XamOutlookBar _xamBar;

        #endregion //Member Variables	
    
        #region Constructors

        static GroupsPresenter()
        {
            FrameworkElementFactory fPanel = new
                FrameworkElementFactory(typeof(GroupsPanel));
            
            ItemsPanelTemplate template = new ItemsPanelTemplate(fPanel);

            ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(GroupsPresenter),
                new FrameworkPropertyMetadata(template));
        }

		/// <summary>
		/// Initializes a new <see cref="GroupsPresenter"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamOutlookBar"/> in the control's default Templates. You do not normally need to create an instance of this class.</p>
		/// </remarks>
		public GroupsPresenter()
        {
        }
       
        #endregion //Constructors

        #region Properties

        #region Public Properties

        #region Orientation
        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(Orientation), typeof(GroupsPresenter),
            new PropertyMetadata(Orientation.Vertical));
        
        /// <summary>
        /// Returns/sets a value that represents the orientation of <see cref="GroupsPresenter"/> control.
        /// </summary>
		/// <remarks>
		/// When used in the navigation area of the control the Orientation of the control is vertical, when used in the overflow area the Orientation is horizontal.
		/// </remarks>
        //[Description("Returns/Sets orientation of GroupsPresenter control")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public System.Windows.Controls.Orientation Orientation
        {
            get { return (System.Windows.Controls.Orientation)this.GetValue(GroupsPresenter.OrientationProperty); }
            set { this.SetValue(GroupsPresenter.OrientationProperty, value); }
        }

        #endregion //Orientation

        #endregion //Public Properties

        #region Internal Properties

        #region GroupsPanel

        internal GroupsPanel GroupItemsPanel
        {
            get
            {
                if (VisualTreeHelper.GetChildrenCount(this) <= 0)
                    return null;
                DependencyObject o = VisualTreeHelper.GetChild(this, 0);
                for (; VisualTreeHelper.GetChildrenCount(o) > 0; o = VisualTreeHelper.GetChild(o, 0))
                    if (o is GroupsPanel)
                        break;
                return o as GroupsPanel;
            }
        }

        #endregion //GroupsPanel	
    
        #region NextAreaItemHeight






        internal double NextAreaItemHeight
        {
            get
            {
                if (OverflowItemsWithGroups.Count > 0)
                {
                    return OverflowItemsWithGroups[0].DesiredNavigationHeight;
                }
                return 0;
            }
        }
        #endregion //NextAreaItemHeight	
    
        #region NextAreaItemWidth






        internal double NextAreaItemWidth
        {
            get
            {
                if (OverflowItemsWithGroups.Count > 0)
                {
                    return OverflowItemsWithGroups[0].DesiredOverflowWidth;
                }
                return 0;
            }
        }
        #endregion //NextAreaItemWidth

        #region NextAreaSize






        internal double NextAreaSize
        {
            get
            {
                if (this.Orientation == Orientation.Vertical)
                    return NextAreaItemHeight;
                else
                    return NextAreaItemWidth;
            }
        }
        #endregion //NextAreaSize

        #region MaxVerticalItems







        internal int MaxVerticalItems
        {
            get
            {
                return this.Orientation == Orientation.Vertical && _xamBar != null ?
                    _xamBar.NavigationAreaMaxGroupsResolved : int.MaxValue;
            }
        }

        #endregion //MaxVerticalItems

        internal OutlookBarGroupLocation DisplayedLocation
        {
            get{
                if(this.Orientation==Orientation.Vertical)
                    return OutlookBarGroupLocation.NavigationGroupArea;
                else
                    return OutlookBarGroupLocation.OverflowArea;
            }
        }

        internal OutlookBarGroupCollection DisplayedItems
        {
            get
            {
                if(this.Orientation==Orientation.Vertical)
                    return _xamBar._navigationAreaGroups;
                else
                    return _xamBar._overflowAreaGroups;
            }
        }

        internal OutlookBarGroupCollection OverflowItems
        {
            get
            {
                if(_xamBar==null)
                    _xamBar = this.GetValue(XamOutlookBar.OutlookBarProperty) as XamOutlookBar;
                if (this.Orientation == Orientation.Vertical)
                    return _xamBar._overflowAreaGroups;
                else
                    return _xamBar._contextMenuGroups;
            }
        }

        internal OutlookBarGroupCollection OverflowItemsWithGroups
        {
            get
            {
                if(OverflowItems.Count>0)
                    return OverflowItems;
                else
                    return _xamBar._contextMenuGroups;
            }
        }

        internal OutlookBarGroupLocation OverflowLocation
        {
            get{
                if(this.Orientation==Orientation.Vertical)
                    return OutlookBarGroupLocation.OverflowArea;
                else
                    return OutlookBarGroupLocation.OverflowContextMenu;
            }
        }

        internal bool MustRemoveItems
        {
            get
            {
                GroupsPanel cp = this.GroupItemsPanel;
                if (cp == null)
                    return false;
                return (cp.Overflow || this.Items.Count > MaxVerticalItems) && this.Items.Count > 0;
            }
        }

        #endregion //Internal Properties

        #endregion //Properties

        #region Base Class Overrides

        #region ArrangeOverride 
        /// <summary>
        /// Positions child elements and determines a size for this element.
        /// </summary>
        /// <param name="constraint">The size available to this element for arranging its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
        protected override Size ArrangeOverride(Size constraint)
        {
            Size sz = base.ArrangeOverride(constraint); // call base

            if (_xamBar != null)
            {
                GroupsPanel cp = this.GroupItemsPanel;
                if ((cp != null) && (Orientation == Orientation.Vertical))
                    this.Height = cp.DesiredSize.Height;
                _xamBar.SetSplitterIncrement();
            }

            return sz;
        }
            
        #endregion //ArrangeOverride

        #region MeasureOverride
        /// <summary>
        /// Invoked to measure the element and its children.
        /// </summary>
        /// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size sz = base.MeasureOverride(constraint);
         
            GroupsPanel cp = this.GroupItemsPanel;

            if (cp == null || _xamBar == null)
                return sz;

            bool mustRemove = this.MustRemoveItems;
            cp.Measure(constraint);
            for (; this.MustRemoveItems; )
            {
                DropLastItem();
                cp.Measure(constraint);
                sz = cp.DesiredSize;

            }// end for- remove items

            for (; cp.HasFreeSpace && !mustRemove; )
            {
                if (OverflowItemsWithGroups.Count == 0)
                    break;

                if (cp.DeltaSize + 0.1 < NextAreaSize)
                    break;

                if (!GetItemFromOverflow())
                    break;

                cp.Measure(constraint);
                if (this.MustRemoveItems)
                {
                    DropLastItem();
                    cp.Measure(constraint);
                    break;
                }// end if- there is no space for last item
                sz = cp.DesiredSize;
            }//end for- add items if there is free space
            sz = base.MeasureOverride(constraint);
            return sz;
        }

        #endregion //MeasureOverride

        #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _xamBar = this.GetValue(XamOutlookBar.OutlookBarProperty) as XamOutlookBar;

            GroupsPanel cp = this.GroupItemsPanel;
            if (cp != null)
                cp.IsItemsHost = true;
        }

        #endregion //OnApplyTemplate	

        #endregion //Base Class Overrides

        #region Methods

        #region Private Methods

        private void DropLastItem()
        {
            int i = this.Items.Count - 1;
            OutlookBarGroup gr = DisplayedItems[i];
            DisplayedItems.RemoveAt(i);
            gr.SetLocation(OverflowLocation);
            OverflowItems.Insert(0, gr);
            gr.InvalidateMeasure();

        }

        private bool GetItemFromOverflow()
        {
            OutlookBarGroup gr = OverflowItemsWithGroups[0];
            OverflowItemsWithGroups.RemoveAt(0);
            gr.SetLocation(DisplayedLocation);
            DisplayedItems.Add(gr);
            gr.InvalidateMeasure();
            
            return true;
        }

        private void SetOrientation(Orientation o)
        {
            GroupsPanel cp = this.GroupItemsPanel;
            if (cp == null)
            {
                FrameworkElementFactory fPanel = this.ItemsPanel.VisualTree;

                if (o == Orientation.Vertical)
                    fPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                else
                    fPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                fPanel.SetValue(StackPanel.OrientationProperty, o);

                return;
            }
            cp.SetValue(StackPanel.OrientationProperty, o);
            if (o == Orientation.Vertical)
            {
                cp.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            }
            else
            {
                cp.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Right);
            }
        }

        #endregion // Private Methods

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