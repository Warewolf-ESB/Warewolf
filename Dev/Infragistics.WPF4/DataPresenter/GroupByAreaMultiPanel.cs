using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Infragistics.Windows.Helpers;
using System.Windows.Media;
using Infragistics.Windows.Automation.Peers.DataPresenter;
using System.Diagnostics;
using System;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Infragistics.Windows.DataPresenter
{
	#region GroupByAreaMultiPanel Class

	/// <summary>
	/// A panel used to position <see cref="FieldLayoutGroupByInfo"/> objects or <see cref="LabelPresenter"/>s 
    /// representing <see cref="Field"/>s inside a <see cref="GroupByAreaMulti"/>
	/// </summary>
	/// <seealso cref="GroupByAreaMulti"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaMode"/>
	/// <seealso cref="FieldLayoutGroupByInfo"/>
    [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GroupByAreaMultiPanel : StackPanel
    {
        #region Private Members
        
        private readonly static double MinOffsetY = 5d;
        private Dictionary<UIElement, Rect> _elementLocationMap;

        #endregion //Private Members	
    
        #region Base class overrides

            #region MeasureOverride

        /// <summary>
        /// Called during the measure pass
        /// </summary>
        /// <param name="constraint">The constraint size</param>
        /// <returns>The desired size</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            UIElementCollection children = this.Children;

            int count = children.Count;

            if (count > 0)
            {
                int zindex = count;

                // Loop over the children reversing their z-order
                foreach (UIElement child in children)
                {
                    Panel.SetZIndex(child, zindex);
                    zindex--;
                }
            }

            // We need to invalid the visual so that our OnRender will always be called
            this.InvalidateVisual();

            return base.MeasureOverride(constraint);
        }

            #endregion //MeasureOverride	
    
            #region OnRender

        /// <summary>
        /// Called to render the element. 
        /// </summary>
        /// <param name="dc"></param>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            UIElementCollection children = this.Children;

            int count = children.Count;

            // if we have less than 1 children then the are no connector lines
            if (count < 1)
                return;

            Pen pen = this.ConnectorLinePen;
            if (pen == null)
                return;

            Thickness thickness = new Thickness(pen.Thickness);
            if (thickness.Bottom < 1)
                return;
            
            bool isVertical = this.Orientation == Orientation.Vertical;

            UIElement   priorElement = children[0];
            Rect        priorElementRect = this.GetElementRect(priorElement );
            object      dataContext = priorElement.GetValue(FrameworkElement.DataContextProperty);
            FieldLayoutGroupByInfo
                        priorElementFieldLayoutInfo = dataContext as FieldLayoutGroupByInfo;
            Field       priorElementField = dataContext as Field;

            DataPresenterBase dp = priorElementFieldLayoutInfo != null 
                ? priorElementFieldLayoutInfo.FieldLayout.DataPresenter 
                : priorElementField != null
                    ? priorElementField.DataPresenter 
                    : null;

            try
            {
                // We want to draw a connector line from the edge of the panel
                // to the 1st field element if the FieldLayout Description is visible
                // but only if we are in full mode
                if (dp != null && priorElementField != null)
                {
                    FieldLayoutGroupByInfo flgbi = this.DataContext as FieldLayoutGroupByInfo;

                    if (flgbi != null &&
                         flgbi.FieldLayoutDescriptionVisibility == Visibility.Visible)
                    {
                        UIElement feDescription = GetDescriptionElement(this);

                        Rect descriptionRect;

                        if (feDescription != null)
                            descriptionRect = this.GetElementRect(feDescription);
                        else
                            descriptionRect = new Rect(0, priorElementRect.Top, 0, priorElementRect.Bottom);

                        this.DrawConnector(dc, descriptionRect, priorElementRect, pen, thickness);

                        // add to the ElementLocationMap so we can verify locations in OnLayoutUpdatd
                        if (feDescription != null)
                            this.AddToElementLocationMap(feDescription, descriptionRect);
                    }
                }

                // if we have less than 2 children then the are no connector lines
                if (count < 2)
                    return;

                if (priorElementFieldLayoutInfo != null)
                {
                    UIElement feDescription = GetDescriptionElement(priorElement);

                    if (feDescription != null && feDescription.IsVisible)
                    {
                        priorElement = feDescription;
                        priorElementRect = this.GetElementRect(feDescription);
                    }

                    if (dp != null && dp.GroupByAreaMode == GroupByAreaMode.MultipleFieldLayoutsFull)
                    {

                        // Since we are in MultipleFieldLayoutsFull mode and this panel contains
                        // FieldLayoutGroupByInfos we want to draw lins from parent FieldLayouts
                        // to their children

                        // Allocate a dictionary to key child elements  by their fieldlayouts
                        Dictionary<FieldLayout, UIElement> map = new Dictionary<FieldLayout, UIElement>();

                        // add the 1st element to the map
                        map.Add(priorElementFieldLayoutInfo.FieldLayout, priorElement);

                        // loop over the rest of the elements 
                        for (int i = 1; i < count; i++)
                        {
                            UIElement element = GetDescriptionElement(children[i]);

                            if (element == null || !element.IsVisible)
                                element = children[i];

                            Rect elementRect = this.GetElementRect(element);
                            FieldLayoutGroupByInfo
                                    elementFieldLayoutInfo = element.GetValue(FrameworkElement.DataContextProperty) as FieldLayoutGroupByInfo;

                            Debug.Assert(elementFieldLayoutInfo != null, "All children should have a FieldLayoutGroupByInfo DataContext in GroupByAreaMultiPanel.OnRender");

                            if (elementFieldLayoutInfo == null)
                                continue;

                            // add the element to the map
                            map.Add(elementFieldLayoutInfo.FieldLayout, element);

                            // get the parent fieldlayout
                            FieldLayout parentLayout = elementFieldLayoutInfo.FieldLayout.ParentFieldLayout;

                            // if this is a root fieldlayout then we don't
                            // want to draw any connector lines
                            if (parentLayout == null)
                                continue;

                            // see if the parentlayout matches the one cached in the stack variable
                            if (parentLayout != priorElementFieldLayoutInfo.FieldLayout)
                            {
                                if (!map.ContainsKey(parentLayout))
                                {
                                    Debug.Fail("The parentLayout should be in the map in GroupByAreaMultiPanel.OnRender");
                                    continue;
                                }

                                // initalize the stack variables so we can draw the connector lines correctly
                                priorElement = map[parentLayout];
                                priorElementRect = this.GetElementRect(priorElement);
                                priorElementFieldLayoutInfo = priorElement.GetValue(FrameworkElement.DataContextProperty) as FieldLayoutGroupByInfo;
                            }

                            // draw the connector lines from parent to child FieldLayout elements
                            this.DrawConnector(dc, priorElementRect, elementRect, pen, thickness);

                            // add to the ElementLocationMap so we can verify locations in OnLayoutUpdatd
                            this.AddToElementLocationMap(element, elementRect);

                        }
                    }
                    else
                    {
                        // do not draw connectors in compact mode between field layouts
                    }
                }
                else
                {
                    // loop over the rest of the elements drawing connector lines
                    // from one to the other
                    for (int i = 1; i < count; i++)
                    {
                        UIElement element = GetDescriptionElement(children[i]);

                        if (element == null || !element.IsVisible)
                            element = children[i];

                        Rect elementRect = this.GetElementRect(element);

                        this.DrawConnector(dc, priorElementRect, elementRect, pen, thickness);

                        // add to the ElementLocationMap so we can verify locations in OnLayoutUpdatd
                        this.AddToElementLocationMap(element, elementRect);

                        priorElement = element;
                        priorElementRect = elementRect;
                        priorElementFieldLayoutInfo = priorElement.GetValue(DataContextProperty) as FieldLayoutGroupByInfo;
                    }
                }
            }
            finally
            {
                // If we drew any lines we need to wire LayoutUpdated because we can't
                // rely on the element rects since they may not have been arranged yet
                if (this._elementLocationMap != null &&
                    this._elementLocationMap.Count > 0)
                {
					// JJD 3/15/11 - TFS65143 - Optimization
					// Instead of having every element wire LayoutUpdated we can maintain a list of pending callbacks
					// and just wire LayoutUpdated on the DP
					//EventHandler handler = new EventHandler(OnLayoutUpdated);
					//this.LayoutUpdated -= handler;
					//this.LayoutUpdated += handler;

					if (dp != null)
						dp.WireLayoutUpdated(this.OnLayoutUpdated);
                }
            }
        }

            #endregion //OnRender

        #endregion //Base class overrides

        #region Properties

            #region ConnectorLinePen

        /// <summary>
        /// Identifies the <see cref="ConnectorLinePen"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ConnectorLinePenProperty = GroupByAreaMulti.ConnectorLinePenProperty.AddOwner(
            typeof(GroupByAreaMultiPanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets/sets the Pen that will be used for drawing connector lines between fields
        /// </summary>
        /// <seealso cref="ConnectorLinePenProperty"/>
        //[Description("Gets/sets the Pen that will be used for drawing connector lines between fields")]
        //[Category("Appearance")]
        public Pen ConnectorLinePen
        {
            get
            {
                return (Pen)this.GetValue(GroupByAreaMulti.ConnectorLinePenProperty);
            }
            set
            {
                this.SetValue(GroupByAreaMulti.ConnectorLinePenProperty, value);
            }
        }

            #endregion //ConnectorLinePen

        #endregion //Properties

        #region Methods

            #region Private Methods

                #region AddToElementLocationMap

        private void AddToElementLocationMap(UIElement element, Rect rect)
        {
            if (this._elementLocationMap == null)
                this._elementLocationMap = new Dictionary<UIElement, Rect>();

            this._elementLocationMap[element] = rect;
        }

                #endregion //AddToElementLocationMap	

                #region AdjustPoint

        private static Point AdjustPoint(Point pt, Thickness thickness)
        {
            // adjust the point to center the vertical line
            double adjustment = -(thickness.Left * 0.5);

            pt.X -= -(thickness.Left * 0.5);

            // adjust the point to center the horizontal line
            pt.Y -= (thickness.Bottom * 0.5);

            return pt;
        }

                #endregion //AdjustPoint	
        
                #region DrawConnector

        private void DrawConnector(DrawingContext dc, Rect priorElementRect, Rect elementRect, Pen pen, Thickness thickness)
        {
            double deltaBottom = Math.Max(elementRect.Bottom - priorElementRect.Bottom, 0);
            double deltaLeft = Math.Max(elementRect.X - priorElementRect.X, 0);
            double spacingX = Math.Max(elementRect.X - priorElementRect.Right, 0);

            bool drawLeftConnector = false;
            Point leftConnectorPoint1 = new Point();
            Point leftConnectorPoint2 = new Point();

            bool drawBottomConnector = false;
            Point bottomConnectorPoint1 = new Point();
            Point bottomConnectorPoint2 = new Point();

            // see if the element is completely to the right of the prior element
            if (spacingX > 0)
            {
                // see if there is enough vertical space at the bottom to draw a vertical 
                // connector line
                if (deltaBottom > MinOffsetY            && 
                    deltaBottom > 2 * thickness.Bottom  &&
                    elementRect.Top >= priorElementRect.Top)
                {
                    drawLeftConnector = true;
                    leftConnectorPoint1 = new Point(priorElementRect.Left + (priorElementRect.Width / 2), priorElementRect.Bottom);
                    
                    // JJD 5/19/09 - TFS17720
                    // Calculate the y 2 different ways and take the greater of the 2.
                    // The 1st way calculates the center point between the bottom of the 2 rects and is appropriate when the
                    // rects overlap in the y dimension.
                    // The 2nd way calculates the vertical center of the target element and is more appropriate
                    // when the vertical offsets are large.
                    //leftConnectorPoint2 = new Point(leftConnectorPoint1.X, leftConnectorPoint1.Y + Math.Max(thickness.Bottom, (elementRect.Bottom - priorElementRect.Bottom) / 2));
					// JJD 11/21/11 - TFS21512
					// Take the thickness of the pen into account to determine where the horizontal connector line should go 
					//double bottomDiffVCenter = leftConnectorPoint1.Y + Math.Max(thickness.Bottom, (elementRect.Bottom - priorElementRect.Bottom) / 2);
                    //double targetElementVCenter = elementRect.Top + (elementRect.Height / 2);
                    double bottomDiffVCenter = leftConnectorPoint1.Y + Math.Max(thickness.Bottom, (elementRect.Bottom - (thickness.Bottom + priorElementRect.Bottom)) / 2);
                    double targetElementVCenter = elementRect.Top + ((thickness.Bottom + elementRect.Height ) / 2);

                    leftConnectorPoint2 = new Point(leftConnectorPoint1.X, Math.Max(bottomDiffVCenter, targetElementVCenter));
                }
                else if (priorElementRect.Top > elementRect.Bottom - thickness.Bottom)
                {
                    drawLeftConnector = true;
                    leftConnectorPoint1 = new Point(priorElementRect.Left + (priorElementRect.Width / 2), priorElementRect.Top);
                    
                    // Calculate the y 2 different ways and take the greater of the 2.
                    // The 1st way calculates the center point between the top of the 2 rects and is appropriate when the
                    // rects overlap in the y dimension.
                    // The 2nd way calculates the vertical center of the target element and is more appropriate
                    // when the vertical offsets are large.
					// JJD 11/21/11 - TFS21512
					// Take the thickness of the pen into account to determine where the horizontal connector line should go 
                    //double topDiffVCenter = leftConnectorPoint1.Y - Math.Max(thickness.Bottom, (priorElementRect.Top - priorElementRect.Top) / 2);
                    //double targetElementVCenter = elementRect.Top + (elementRect.Height / 2);
                    double topDiffVCenter = leftConnectorPoint1.Y - Math.Max(thickness.Bottom, (priorElementRect.Top - (thickness.Bottom + priorElementRect.Top )) / 2);
                    double targetElementVCenter = elementRect.Top + ((thickness.Bottom + elementRect.Height ) / 2);

                    leftConnectorPoint2 = new Point(leftConnectorPoint1.X, Math.Min(topDiffVCenter, targetElementVCenter));
                }
                else
                {
					// JJD 11/21/11 - TFS21512
					// Take the thickness of the pen into account to determine where the horizontal connector line should go 
					//leftConnectorPoint2 = new Point(priorElementRect.Right, priorElementRect.Top + (Math.Max(elementRect.Bottom - priorElementRect.Top, 0) / 2));
                    leftConnectorPoint2 = new Point(priorElementRect.Right, priorElementRect.Top + (Math.Max(thickness.Bottom + elementRect.Bottom - priorElementRect.Top, 0) / 2));
                }

                if (elementRect.Left > leftConnectorPoint2.X)
                {
                    drawBottomConnector = true;
                    bottomConnectorPoint1 = leftConnectorPoint2;
                    bottomConnectorPoint2 = new Point(elementRect.Left, bottomConnectorPoint1.Y);
                }
            }
            else
            {
                if (deltaBottom < MinOffsetY || deltaBottom < 2 * Math.Max(thickness.Bottom, 1))
                    return;

                // see if there is enough horizontal space at the left to draw a vertical 
                // connector line
                if (deltaLeft > 4 * Math.Max(thickness.Left, 1))
                {
                    drawLeftConnector = true;
                    leftConnectorPoint1 = new Point(priorElementRect.Left + (deltaLeft / 2), priorElementRect.Bottom);
					// JJD 11/21/11 - TFS21512
					// Take the thickness of the pen into account to determine where the horizontal connector line should go 
                    //leftConnectorPoint2 = new Point(leftConnectorPoint1.X, elementRect.Top + ((elementRect.Bottom - elementRect.Top) / 2));
                    leftConnectorPoint2 = new Point(leftConnectorPoint1.X, elementRect.Top + ((thickness.Bottom + elementRect.Bottom - elementRect.Top) / 2));
                    drawBottomConnector = true;
                    bottomConnectorPoint1 = leftConnectorPoint2;
                    bottomConnectorPoint2 = new Point(elementRect.Left - 1, bottomConnectorPoint1.Y);
                }
            }

			if (drawLeftConnector)
			{
				// JJD 11/21/11 - TFS21512
				// Adjust for the pen's thickness here so that the vertical
				// line is drawn down to the bottom of the horizonatal one
				leftConnectorPoint2.Y += thickness.Bottom / 2;

				dc.DrawLine(pen, AdjustPoint(leftConnectorPoint1, thickness), AdjustPoint(leftConnectorPoint2, thickness));
			}

            if (drawBottomConnector)
                dc.DrawLine(pen, AdjustPoint( bottomConnectorPoint1, thickness ), AdjustPoint( bottomConnectorPoint2, thickness ));
        }

                #endregion //DrawConnector	

                #region GetConnectorLineTarget

        private UIElement GetConnectorLineTarget(UIElement element)
        {
            Utilities.DependencyObjectSearchCallback<UIElement> callback = new Utilities.DependencyObjectSearchCallback<UIElement>(delegate(UIElement uielement)
            {
                return GroupByAreaMulti.GetIsConnectorLineTarget(uielement);
            });

            UIElement descendantElement = Utilities.GetDescendantFromType<UIElement>(element, true, callback, new Type[] { typeof(GroupByAreaMultiPanel) });

            if (descendantElement == null)
                return element;

            // see if there is a nested element flagged within the 1st descendant element
            // this allows some flexibility for setting the attached IsConnectorLineTarget property
            // inside th template of the FieldLayoutDescriptionTemplate
            UIElement descendantElement2 = Utilities.GetDescendantFromType<UIElement>(descendantElement, true, callback, new Type[] { typeof(GroupByAreaMultiPanel) });

            if (descendantElement2 == null)
                return descendantElement;

            return descendantElement2;

        }

                #endregion //GetConnectorLineTarget	
    
                #region GetDescriptionElement

        private UIElement GetDescriptionElement(UIElement element)
        {
            UIElement elementRoot = null;

            if (this.DataContext is FieldLayoutGroupByInfo)
            {
                DependencyObject parent = element;

                while (parent != null)
                {
                    parent = Utilities.GetParent(parent);

                    if (parent is GroupByAreaMultiPanel)
                        break;

                    if (parent is UIElement)
                        elementRoot = parent as UIElement;
                }
            }
            else
            {
                elementRoot = element;
            }

            return elementRoot;

        }

                #endregion //GetDescriptionElement	
        
                #region GetElementRect

        private Rect GetElementRect(UIElement element)
        {
            if ((element is ContentPresenter ||
                 element is ContentControl ) &&
                VisualTreeHelper.GetChildrenCount(element) == 1)
            {
                DependencyObject child = VisualTreeHelper.GetChild(element, 0);

                if (child is UIElement)
                    element = child as UIElement;
            }

            UIElement IsConnectorLineTarget = GetConnectorLineTarget(element);

            if (IsConnectorLineTarget == null)
                IsConnectorLineTarget = element;

            Point ptLeftTop = IsConnectorLineTarget.TranslatePoint(new Point(), this);

            return new Rect(ptLeftTop, IsConnectorLineTarget.RenderSize);
        }

                #endregion //GetElementRect	

                #region OnLayoutUpdated

		// JJD 3/15/11 - TFS65143 - Optimization
		// Instead of having every element wire LayoutUpdated we can maintain a list of pending callbacks
		// and just wire LayoutUpdated on the DP
        //private void OnLayoutUpdated(object sender, EventArgs e)
        private void OnLayoutUpdated()
        {
            // unwire the event
			// JJD 3/15/11 - TFS65143 - Optimization
			// No need to unhook since the DP is maintaining the callback list and automatically removes the entry
			// unhook the event
			//this.LayoutUpdated -= new EventHandler(this.OnLayoutUpdated);

            if (this._elementLocationMap == null)
                return;

            // see if any of the rects have changed from OnRender
            foreach (KeyValuePair<UIElement, Rect> kv in this._elementLocationMap)
            {
                Rect rect = this.GetElementRect(kv.Key);

                if (rect != kv.Value)
                {
                    // since the rect isn't the same invalidate the visual so we get
                    // another OnRender to fixup our lines
                    this.InvalidateVisual();
                    break;
                }
            }

            // clear the element location map
            this._elementLocationMap = null;
        }

                #endregion //OnLayoutUpdated	
        
            #endregion //Private Methods	
    
        #endregion //Methods
    }

	#endregion //GroupByAreaMultiPanel Class
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