using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

namespace Infragistics.Windows.OutlookBar.Internal
{
    /// <summary>
    /// Arranges <see cref="OutlookBarGroup"/>s in the navigation area of the <see cref="XamOutlookBar"/>.
    /// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GroupsPanel : StackPanel
    {
        #region Member Variables

        bool _overflow = false;     // indicates that there is not enough space for all items 
        double _deltaSize = 0;      // the difference between constraint and desired 
        bool _hasFreeSpace = false; // indicates that there is more space for items

        double _maxHeightHorizontal;// max height when Orientation is Horizontal

        #endregion //Member Variables

        #region Constructor

		/// <summary>
		/// Initializes a new <see cref="GroupsPanel"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamOutlookBar"/> in the control's default Templates. You do not normally need to create an instance of this class.</p>
		/// </remarks>
		public GroupsPanel()
            : base()
        {
            CanVerticallyScroll = false;
            _maxHeightHorizontal = 0;
        }

        #endregion //Constructor

        #region Properties

        #region Internal

        #region Overflow





        internal bool Overflow
        {
            get { return _overflow; }
            set { _overflow = value; }
        }

        #endregion //Overflow

        #region DeltaSize





        internal double DeltaSize
        {
            get { return _deltaSize; }
            set { _deltaSize = value; }
        }

        #endregion //DeltaSize

        #region HasFreeSpace





        internal bool HasFreeSpace
        {
            get { return _hasFreeSpace; }
            set { _hasFreeSpace = value; }
        }

        #endregion //HasFreeSpace

        #endregion //Internal

        #region Private

        #region GroupItemsPresenter

        private GroupsPresenter GroupItemsPresenter
        {
            get
            {
                DependencyObject o = VisualTreeHelper.GetParent(this);
                for (; o != null; o = VisualTreeHelper.GetParent(o))
                    if (o is GroupsPresenter)
                        break;
                return o as GroupsPresenter;
            }
        }

        #endregion //GroupItemsPresenter

        #endregion //Private

        #endregion //Properties

        #region Base Class Overrides

        #region EndInit

        /// <summary>
        /// Called when the initialization process for the element is complete.
        /// </summary>
        public override void EndInit()
        {
            if (this.GroupItemsPresenter != null)
            {
                this.Orientation = this.GroupItemsPresenter.Orientation;
                if (this.Orientation == Orientation.Horizontal)
                    this.HorizontalAlignment = HorizontalAlignment.Right;
            }

            base.EndInit();
        }

        #endregion //EndInit	

        #region MeasureOverride
        /// <summary>
        /// Invoked to measure the element and its children.
        /// </summary>
        /// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size sz = base.MeasureOverride(constraint);
            bool hasMoreSize;
            if (this.Orientation == Orientation.Vertical)
            {
                _deltaSize = constraint.Height - sz.Height; // 64-70
                hasMoreSize = !double.IsInfinity(constraint.Height);
            }
            else
            {
                _deltaSize = constraint.Width - sz.Width;
                hasMoreSize = !double.IsInfinity(constraint.Width);
                if (_maxHeightHorizontal > sz.Height)
                    sz.Height = _maxHeightHorizontal;
                else
                    _maxHeightHorizontal = sz.Height;
            }

            _overflow = _deltaSize + 0.001 < 0;
            _hasFreeSpace = _deltaSize > 0.001 && hasMoreSize;

            if (_overflow)
            {
                if (this.Orientation == Orientation.Vertical)
                    sz.Height = constraint.Height; // 64-70
                else
                    sz.Width = constraint.Width;
                this.GroupItemsPresenter.InvalidateMeasure();
            }

            return sz;
        }
        #endregion //MeasureOverride

        #region OnChildDesiredSizeChanged

        /// <summary>
        /// Supports layout behavior when a child element is resized.
        /// </summary>
        /// <param name="child">The child element that is being resized.</param>
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            OutlookBarGroup gr = child as OutlookBarGroup;
            if (gr != null)
            {
                if (gr.OutlookBar != null && gr.Visibility != Visibility.Collapsed)
                {
                    bool refreshAll = gr.DesiredNavigationHeight != gr.DesiredSize.Height;
                    gr.DesiredNavigationHeight = gr.DesiredSize.Height;
                    gr.OutlookBar.RefreshNavigationArea(refreshAll);
                    
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

                }
            }// end if- 
            base.OnChildDesiredSizeChanged(child); 
        }

        #endregion //OnChildDesiredSizeChanged	
    
        #endregion //Base Class Overrides
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