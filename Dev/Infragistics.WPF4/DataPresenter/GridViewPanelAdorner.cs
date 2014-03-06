using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using Infragistics.Windows.Internal;
using System.Windows.Threading;
using System.ComponentModel;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// The Adorner created by the <see cref="GridViewPanel"/> and used to hold various elements such as <see cref="RecordPresenter"/>s for displaying Headers.
    /// </summary>
    /// <remarks>
    /// <p class="body">The adorner is automatically created by the <see cref="GridViewPanel"/> and allows Header <see cref="RecordPresenter"/>s to float on top of all the items in the <see cref="GridViewPanel"/> to improve performance during scrolling.</p>
    /// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="GridViewPanel"/> when needed.  You do not ordinarily need to create an instance of this class directly.</p>
    /// </remarks>
    /// <seealso cref="GridViewPanel"/>
    /// <seealso cref="RecordPresenter"/>
    // JJD 8/20/08 - BR35341
    // Added AdornerEx abstract base class to handle adornerlayer re-creations based on template changes from higher level elements 
    //public class GridViewPanelAdorner : Adorner
    // JJD 4/30/09 - TFS17181
    // Implement IWeakEventListener
    //public class GridViewPanelAdorner : AdornerEx
    [DesignTimeVisible(false)] // JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class GridViewPanelAdorner : AdornerEx, IWeakEventListener
	{
		#region Member Variables

		private GridViewPanelNested 				_gridViewPanel = null;
		private List<UIElement>						_children = null;
		private bool								_orientationIsVertical = true;
		private double								_locationOffset = 0;

        // JJD 08/21/09 - TFS18897
        // Keep track of the extent in the primary dimension based on orientation
        private double                              _headerExtent;

        // JJD 10/23/08 - TFS8134 
        // Added men\mber to cache the fieldlayout's version
        private int                                 _fieldLayoutVersion = -1;

        // JJD 4/30/09 - TFS17181
        private FieldLayout                         _fieldLayout;

		// JM BR28982 12-06-07 - Now using member variable of type Size in CreateAndMeasureHeaderRecordPresenter.
		//private double								_totalExtentUsedForHeaderRecords = 0;

		// AS 10/16/09 - TFS23821
		private PropertyValueTracker _flowDirectionTracker;

		// AS 6/11/09 TFS18382
		private Size								_constraint;

        // JJD 12/18/09 - TFS25645
        private Size                                _lastMeasureConstraint;

		#endregion //Member Variables	
    
		#region Constructor

		static GridViewPanelAdorner()
		{
			// AS 10/16/09 - TFS23821
			// See some comments in the CreateAndMeasureHeaderRecordPresenter for TFS21090.
			// This seems to be another incarnation. It seems that the value of the DP is 
			// being changed to the current value so we don't get a property changed but 
			// that is blowing away the bindingexpression we tried to put in for TFS21090
			// to work around the fact that they were removing their binding. Since we cannot 
			// rely on binding this object's flowdirection to that of the associated panel 
			// we will instead coerce them to keep them in sync.
			//
			FlowDirectionProperty.OverrideMetadata(typeof(GridViewPanelAdorner), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceFlowDirection)));
		}

		// JJD 8/20/08 - BR35341
        // AdornerEx abstract base class contructor requires a FrameworkElement 
//        internal GridViewPanelAdorner(UIElement adornedElement)
        internal GridViewPanelAdorner(GridViewPanelNested adornedElement)
            : base(adornedElement)
		{
			this._gridViewPanel = adornedElement;

			// AS 10/16/09 - TFS23821
			_flowDirectionTracker = new PropertyValueTracker(_gridViewPanel, FlowDirectionProperty, new PropertyValueTracker.PropertyValueChangedHandler(OnPanelFlowDirectionChanged));
		}

		#endregion //Constructor	

		#region Base Class Overrides

			#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			// ============================================================================
			// Arrange the 'fixed on top' and 'header' RecordPresenters (if any).
			Rect	arrangeRect				= new Rect(finalSize);
			double	extentUsedByLastItem	= 0;

            double panelNonScrollingExtent = 0;

            // JJD 12/18/09 - TFS25645
            // For the root panel we should make sure the header goes all the way across
            // so get the panel's extent in the non-scrolling dimension from our
            // last measure constraint
            if (this._gridViewPanel != null &&
                 this._gridViewPanel.IsRootPanel)
            {
                if (this._orientationIsVertical)
                    panelNonScrollingExtent = this._lastMeasureConstraint.Width;
                else
                    panelNonScrollingExtent = this._lastMeasureConstraint.Height;

                if (double.IsPositiveInfinity(panelNonScrollingExtent))
                    panelNonScrollingExtent = 0;
            }


			foreach (UIElement element in this.Children)
			{
				DataRecordPresenter drp = element as DataRecordPresenter;
				if (drp != null && drp.IsHeaderRecord == true)
				{
					if (this._orientationIsVertical)
					{
						arrangeRect.X			= this._locationOffset;
						arrangeRect.Y			+= extentUsedByLastItem;
						extentUsedByLastItem	= drp.DesiredSize.Height;
						arrangeRect.Height		= extentUsedByLastItem;
                        arrangeRect.Width		= Math.Max(finalSize.Width, drp.DesiredSize.Width);
                        
                        // JJD 12/18/09 - TFS25645
                        // Make sure the header spans the width for root level headers if
                        // we are not auto-fitting
                        if ( drp.FieldLayout.IsAutoFitWidth == false )
						    arrangeRect.Width		= Math.Max(panelNonScrollingExtent - arrangeRect.X, arrangeRect.Width);

						if (Utilities.DoubleIsZero(arrangeRect.Y))
							arrangeRect.Y		= 0;
					}
					else
					{
						arrangeRect.X			+= extentUsedByLastItem;
						arrangeRect.Y			= this._locationOffset;
						extentUsedByLastItem	= drp.DesiredSize.Width;
						arrangeRect.Height	    = Math.Max(finalSize.Height, drp.DesiredSize.Height);

                        // JJD 12/18/09 - TFS25645
                        // Make sure the header spans the height for root level headers if
                        // we are not autofitting
                        if (drp.FieldLayout.IsAutoFitHeight == false)
                            arrangeRect.Height = Math.Max(panelNonScrollingExtent - arrangeRect.Y, arrangeRect.Height);

						if (Utilities.DoubleIsZero(arrangeRect.X))
							arrangeRect.X		= 0;
					}


					drp.Arrange(arrangeRect);
				}
			}

			return finalSize;

		}

			#endregion //ArrangeOverride

			#region GetVisualChild

		/// <summary>
		/// Gets the visual child at a specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the specific child visual.</param>
		/// <returns>The visual child at the specified index.</returns>
		protected override Visual GetVisualChild(int index)
		{
			if (index < 0 || index >= this.Children.Count)
				return null;

			return this.Children[index];
		}

			#endregion //GetVisualChild

			#region MeasureOverride
		/// <summary>
		/// Called to give an element the opportunity to return its desired size and measure its children.
		/// </summary>
		/// <param name="availableSize"></param>
		/// <returns>The desired size</returns>
        protected override Size MeasureOverride(Size availableSize)
		{
            // JJD 12/18/09 - TFS25645
            // Cache the measue constraint for use in the ArrangeOverride method
            this._lastMeasureConstraint = availableSize;

			Size desiredSize = new Size();

			// AS 6/11/09 TFS18382
			// We need to measure the records with the same constraint that the 
			// gridviewpanel is using otherwise the size of the adorned element, 
			// which may be different, is used.
			//
			foreach (UIElement child in Children)
			{
				child.Measure(_constraint);

				Size childSize = child.DesiredSize;

				desiredSize.Width = Math.Max(desiredSize.Width, childSize.Width);
				desiredSize.Height = Math.Max(desiredSize.Height, childSize.Height);
			}

			return desiredSize;
		} 
			#endregion //MeasureOverride

            #region OnChildDesiredSizeChanged

        /// <summary>
        /// Called whena child element's desired size has changed
        /// </summary>
        /// <param name="child"></param>
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);

            // JJD 08/21/09 - TFS18897
            // See if the primary extent has changed and if so then invalidate the gridviewpanel's measure
            double primaryExtent = GridUtilities.GetPrimaryExtent(child.DesiredSize, this._orientationIsVertical);

            if ( primaryExtent != this._headerExtent )
            {
                this._headerExtent = primaryExtent;

                if ( this._gridViewPanel != null )
                    this._gridViewPanel.InvalidateMeasure();

                return;
            }

            // JJD 4/30/09 - TFS17181 
            // If the associated GridViewPanel doesn't have any children then
            // we need to invalidate its measure so that the size of the scrollbars
            // can be maintained accurately.
            if (this._gridViewPanel != null &&
                 VisualTreeHelper.GetChildrenCount(this._gridViewPanel) == 0)
                this._gridViewPanel.InvalidateMeasure();
        }

            #endregion //OnChildDesiredSizeChanged	
    
			#region VisualChildrenCount

		/// <summary>
		/// Returns the total numder of visual children (read-only).
		/// </summary>
		protected override int VisualChildrenCount
		{
			get { return this.Children.Count; }
		}

			#endregion //VisualChildrenCount

		#endregion //Base Class Overrides

		#region Methods

			#region Internal Methods

				#region ClearHeaderRecordPresenters

		internal void ClearHeaderRecordPresenters()
		{
			int childrenCount = this.Children.Count;
			for (int i = childrenCount - 1; i >= 0; i--)
			{
				DataRecordPresenter drp = this.Children[i] as DataRecordPresenter;
				if (drp != null && drp.IsHeaderRecord)
				{
					drp.ClearContainerForItem(null);

					this.RemoveElementAsLogicalChild(drp);
					this.RemoveVisualChild(drp);
					this.Children.RemoveAt(i);
				}
			}


			// Invalidate the arrangement of our children.
			this.InvalidateArrange();
		}

				#endregion //ClearHeaderRecordPresenters	

				#region CreateAndMeasureHeaderRecordPresenter



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		// JM BR28982 12-06-07
		//internal double CreateAndMeasureHeaderRecordPresenter(Record record, Size constraint, bool orientationIsVertical, double locationOffset)
		internal Size CreateAndMeasureHeaderRecordPresenter(Record record, Size constraint, bool orientationIsVertical, double locationOffset)
		{
			// AS 10/16/09 TFS23821
			// We don't need this workaround anymore since we will now coerce the flowdirection to 
			// keep it in sync with that of the associated panel.
			//
			//// AS 8/20/09 TFS21090
			//// In the ctor of the Adorner class, it does a delayed binding of its FlowDirection to that 
			//// of the AdornedElement. At some point when the theme is set, this binding is getting cleared 
			//// and so the flow direction of the adorned element and the adorner (this instance) are then 
			//// different. When the flowdirection of the adorner and adorned elements differ, the adorner 
			//// layout adds its own transform to flip the render transform of the adorned element which is 
			//// what lead to the strange layout issue observed. To address this we will rebind the flow 
			//// direction ourselves if they should get out of sync.
			////
			//if (this.FlowDirection != _gridViewPanel.FlowDirection)
			//    this.SetBinding(FlowDirectionProperty, Utilities.CreateBindingObject(FlowDirectionProperty, System.Windows.Data.BindingMode.OneWay, _gridViewPanel));

			// AS 6/11/09 TFS18382
			_constraint = constraint;

			// Save some parameters - we'll need them in the ArrangeOverride.
            bool orientationChanged     = this._orientationIsVertical != orientationIsVertical;
			this._orientationIsVertical = orientationIsVertical;
			this._locationOffset		= locationOffset;

            // JJD 10/23/08 - TFS8134 
            // If the version number on the fieldlayout changes then do an asynchronous
            // invalidation of the visualtree from the headerpresenter to the Adorner.
            // This fixes an intermittent problem when AutoFit is true since there
            // appears to be some type of timing issue with the measure calculations.
            FieldLayout fl = record.FieldLayout;

            // JJD 4/30/09 - TFS17181
            // Remove the old property change listener
            if (this._fieldLayout != null)
            {
                PropertyChangedEventManager.RemoveListener(this._fieldLayout, this, "GridColumnWidthVersion");
                PropertyChangedEventManager.RemoveListener(this._fieldLayout, this, "GridRowHeightVersion");
            }

            // JJD 4/30/09 - TFS17181
            // Keep track of the fieldlayout 
            this._fieldLayout = fl;

            if (this._fieldLayout != null)
            {
                int version = this._fieldLayout.InternalVersion;

                if (version != this._fieldLayoutVersion)
                {
                    // The 1st time thru we don't need to post a delayed path invalidation

                    // JJD 2/19/09 - support for printing.
                    // We can't do asynchronous operations during a report operation
                    //if (this._fieldLayoutVersion != -1)
                    if (this._fieldLayoutVersion != -1 && 
                        ( fl.DataPresenter != null && fl.DataPresenter.IsReportControl == false))
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new GridUtilities.MethodDelegate(DelayedPathInvalidation)); 

                    this._fieldLayoutVersion = version;
                }

                // JJD 4/30/09 - TFS17181 
                // We need to listen for changes to either the 'GridColumnWidthVersion'
                // or 'GridRowHeightVersion' properties (based on orientation) on the FieldLayout. 
                // If the associated GridViewPanel doesn't have any children then
                // we need to invalidate its measure so that the size of the scrollbars
                // can be maintained accurately.
                if (this._fieldLayout.IsHorizontal)
                    PropertyChangedEventManager.AddListener(this._fieldLayout, this, "GridRowHeightVersion");
                else
                    PropertyChangedEventManager.AddListener(this._fieldLayout, this, "GridColumnWidthVersion");
            }

			// Initialize - Set the IsActiveHeaderRecord property on our child 'header' RecordPresenters (if any) to false.
			//			  - Reset the total extent used for header records
			DataRecordPresenter drp = null;
			foreach (UIElement element in this.Children)
			{
				drp = element as DataRecordPresenter;
				if (drp != null && drp.IsHeaderRecord == true)
				{
					drp.IsActiveHeaderRecord = false;
					drp.IsArrangedInView = false;

					// JJD 9/26/07 - BR25884
					// Unwire the previous handler
					drp.SizeChanged -= new SizeChangedEventHandler(OnHeaderSizeChanged);
				}
			}


			// Create a new (or reuse existing) RecordPresenter to display a header for the specified
			// record's FieldLayout.
			// JM BR28982 12-06-07
			//this._totalExtentUsedForHeaderRecords = 0;

			drp = null;

            if (orientationChanged == false)
            {
                foreach (UIElement element in this.Children)
                {
                    drp = element as DataRecordPresenter;
                    if (drp != null && drp.IsHeaderRecord == true)
                    {
                        if (drp.Record != null && drp.Record.FieldLayout == record.FieldLayout)
                            break;
                    }

                    drp = null;
                }
            }

			if (drp == null)
			{
				drp = new DataRecordPresenter();
				drp.PrepareContainerForItem(record, true);
				this.AddChildElement(drp);
			}

            // JJD 4/25/08 - BR31538
            // we are now initializing this properrty inside the PrepareContainerForItem
            // method called above
            //drp.SetValue(RecordPresenter.IsHeaderRecordPropertyKey, KnownBoxes.TrueBox);
			drp.SetValue(RecordPresenter.DataContextProperty, record);
			drp.IsActiveHeaderRecord = true;
			drp.IsArrangedInView = true;
			drp.InitializeHeaderContent(true);
			drp.InitializeNestedContent(null);
			drp.InitializeRecordContentVisibility(false);
			drp.InitializeExpandableRecordContentVisibility(false);
			drp.InitializeGroupByRecordContentVisibility(false);

			// AS 8/21/09 TFS19388
			drp.SetHeaderRecordCollection(_gridViewPanel.ViewableRecords);

			// Measure the record presenter.
			drp.InvalidateMeasure();
			drp.Measure(constraint);
			if (drp.DesiredSize.Width == 0 || drp.DesiredSize.Height == 0)
			{
				drp.InvalidateMeasure();
				drp.Measure(constraint);
			}


			// Update the extent used for header records.
			// JM BR28982 12-06-07
			//this._totalExtentUsedForHeaderRecords = (orientationIsVertical ? drp.DesiredSize.Height : drp.DesiredSize.Width);
			Size totalExtentUsedForHeaderRecords = drp.DesiredSize;
            
            // JJD 08/21/09 - TFS18897
            // Keep track of the extent in the primary dimension based on orientation
            this._headerExtent = GridUtilities.GetPrimaryExtent(totalExtentUsedForHeaderRecords, this._orientationIsVertical);

			// Remove DataRecordPresenters that are not currently being used (if any).
			int childrenCount = this.Children.Count;
			for (int i = childrenCount - 1; i >= 0; i--)
			{
                // AS 3/4/09
                // We should not be reusing the local variable here since we use
                // this below to hook into the SizeChanged. So if the last element 
                // we process in this loop gets removed (i.e. its not the original
                // value of drp) then we hook into the SizeChanged of an element we 
                // just removed instead of the one we just created/setup.
                //
				//drp = this.Children[i] as DataRecordPresenter;
				DataRecordPresenter drpTemp = this.Children[i] as DataRecordPresenter;

				if (drpTemp						 != null	&&
					drpTemp.IsHeaderRecord		 == true	&&
					drpTemp.IsActiveHeaderRecord == false)
				{
                    drpTemp.ClearContainerForItem(null);
                    drpTemp.IsArrangedInView = false;

					// AS 8/21/09 TFS19388
					drpTemp.SetHeaderRecordCollection(null);

                    this.RemoveElementAsLogicalChild(drpTemp);
                    this.RemoveVisualChild(drpTemp);
					this.Children.RemoveAt(i);
				}
			}


			// Invalidate the arrangement of our children.
			this.InvalidateArrange();

			// JJD 9/26/07 - BR25884
			// Wire up a handler to listen for size changes on the header record so we
			// can notify the GridViewPanel to re-arrange its children with the new offset
			drp.SizeChanged += new SizeChangedEventHandler(OnHeaderSizeChanged);

			// JM BR28982 12-06-07
			//return this._totalExtentUsedForHeaderRecords;
			return totalExtentUsedForHeaderRecords;
		}

				#endregion //CreateAndMeasureHeaderRecordPresenter

                // AS 1/22/09 NA 2009 Vol 1 - Fixed Fields
                #region InitializeOffset
        internal void InitializeOffset(double newOffset)
        {
            this._locationOffset = newOffset;
            this.InvalidateArrange();
        }
                #endregion //InitializeOffset

			#endregion //Internal Methods

			#region Private Methods

				#region AddChildElement

		private void AddChildElement(UIElement childElement)
		{
			this.AddVisualChild(childElement);
			this.Children.Add(childElement);


			// Add the element as the logical child of the associated GridViewPanel.
			if (this._gridViewPanel != null)
			{
				DependencyObject logicalParent = LogicalTreeHelper.GetParent(childElement);
				if (logicalParent == null)
					this._gridViewPanel.AddLogicalChildInternal(childElement);
			}
		}

				#endregion //AddChildElement

				// AS 10/16/09 - TFS23821
				#region CoerceFlowDirection
		private static object CoerceFlowDirection(DependencyObject d, object newValue)
		{
			GridViewPanelAdorner adorner = (GridViewPanelAdorner)d;

			return adorner._gridViewPanel.GetValue(FlowDirectionProperty);
		}
				#endregion //CoerceFlowDirection

                // JJD 10/23/08 - TFS8134 - added
                #region DelayedPathInvalidation

        // AS 1/27/09
        // Optimization - only have 1 parameterless void delegate class defined.
        //
        //delegate void MethodDelegate();

        private void DelayedPathInvalidation()
        {
            // Get a descendant HeaderPresenter
            FrameworkElement fe = Utilities.GetDescendantFromType(this, typeof(HeaderPresenter), true) as FrameworkElement;

            // walk up the visual tree to the Adornerlayer invalidating each element in the chain
            while (fe != null)
            {
                fe.InvalidateMeasure();

                if (fe is AdornerLayer)
                    break;

                fe = VisualTreeHelper.GetParent(fe) as FrameworkElement;
            }
        }

                #endregion //DelayedPathInvalidation	

				#region OnHeaderSizeChanged // JJD 9/26/07 - BR25884

		// JJD 9/26/07 - BR25884
		// Added method to listen for size changes on the header so we can let the GridViewPanel
		// know to re-arrange its children based on the new extent offset of the header
		private void OnHeaderSizeChanged(object sender, SizeChangedEventArgs e)
		{
			DataRecordPresenter drp = sender as DataRecordPresenter;

			Debug.Assert(drp != null);

			if (drp == null)
				return;

			Debug.Assert(this._gridViewPanel != null);
			Debug.Assert(this._gridViewPanel == drp.Parent);
			
			if (this._gridViewPanel != null &&
				this._gridViewPanel == drp.Parent)
			{
				// JJD 9/26/07 - BR25884
				// Call the GridViewPanel's OnHeaderExtentChanged method
				// AS 6/11/09 TFS18382
				// We were only processing the logical height but in an autofit situation
				// the width matters too since the gridviewpanel uses the width of the 
				// drp in the adorner when measuring the children.
				//
				//this._gridViewPanel.OnHeaderExtentChanged(drp.FieldLayout, this._orientationIsVertical ? drp.ActualHeight : drp.ActualWidth);
				double extent = this._orientationIsVertical ? drp.ActualHeight : drp.ActualWidth;
				double nonPrimary = this._orientationIsVertical ? e.NewSize.Width : e.NewSize.Height;
				this._gridViewPanel.OnHeaderExtentChanged(drp.FieldLayout, extent, nonPrimary);
			}

		}

				#endregion //OnHeaderSizeChanged	
    
				// AS 10/16/09 - TFS23821
				#region OnPanelFlowDirectionChanged
		private void OnPanelFlowDirectionChanged()
		{
			this.CoerceValue(FlowDirectionProperty);
		}
				#endregion //OnPanelFlowDirectionChanged

				#region RemoveElementAsLogicalChild

		private void RemoveElementAsLogicalChild(DependencyObject childElement)
		{
			if (this._gridViewPanel == null)
				return;

			DependencyObject logicalParent = LogicalTreeHelper.GetParent(childElement);
			if (logicalParent == this._gridViewPanel)
				this._gridViewPanel.RemoveLogicalChildInternal(childElement);
		}

				#endregion //RemoveElementAsLogicalChild	
    
			#endregion //Private Methods

		#endregion //Methods

		#region Properties

			#region Children

		private List<UIElement> Children
		{
			get
			{
				if (this._children == null)
					this._children = new List<UIElement>(3);

				return this._children;
			}
		}

			#endregion //Children

			// JJD 2/9/11 - TFS63916 - added
			#region GridViewPanel

		internal GridViewPanelNested GridViewPanel { get { return this._gridViewPanel; } }

			#endregion //GridViewPanel	
    
		#endregion //Properties

        // JJD 4/30/09 - TFS17181 - added
        #region IWeakEventListener Members

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(PropertyChangedEventManager))
            {
                // JJD 4/30/09 - TFS17181 
                // We are listening for a change to either the 'GridColumnWidthVersion'
                // or 'GridRowHeightVersion' properties on the FieldLayout. 
                // If the associated GridViewPanel doesn't have any children then
                // we need to invalidate its measure so that the size of the scrollbars
                // can be maintained accurately.
                if (this._gridViewPanel != null &&
                     VisualTreeHelper.GetChildrenCount(this._gridViewPanel) == 0)
                    this._gridViewPanel.InvalidateMeasure();

                return true;
            }

            Debug.Fail("Invalid managerType in ReceiveWeakEvent for GridViewPanelAdorner, type: " + managerType != null ? managerType.ToString() : "null");

            return false;
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