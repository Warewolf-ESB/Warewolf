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
using System.Windows.Automation.Peers;
using Infragistics.Controls;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.Diagnostics;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Automation peer class for XamZoombar.
    /// </summary>
    public class XamZoombarAutomationPeer : FrameworkElementAutomationPeer, IScrollProvider
    {
        #region Constructor
        /// <summary>
        /// XamZoombarAutomationPeer constructor.
        /// </summary>
        /// <param name="zoombar"></param>
        public XamZoombarAutomationPeer(XamZoombar zoombar)
        :
            base(zoombar)
        {
            this.Zoombar = zoombar;

        }
        #endregion //Constructor

        #region Properties
        /// <summary>
        /// A reference to the XamZoombar.
        /// </summary>
        public XamZoombar Zoombar { get; set; }

        #endregion //Properties

        #region IScrollProvider
        /// <summary>
        /// Boolean indicating whether or not the zoombar is horizontally scrollable.
        /// </summary>
        public bool HorizontallyScrollable 
        {
            get
            {
                return this.Zoombar.Orientation == Orientation.Horizontal;
            }
        }
        /// <summary>
        /// A numeric value representing the horizontal scroll position as a percentage of the scrollable area.
        /// </summary>
        public double HorizontalScrollPercent 
        {
            get
            {
                if (this.Zoombar.Orientation == Orientation.Horizontal)
                {
                    return this.Zoombar.Range.Minimum + (this.Zoombar.Range.Maximum - this.Zoombar.Range.Minimum) / 2;
                }
                else
                {
                    return 0.0;
                }
            }
        }
        /// <summary>
        /// The horizontal range of the zoombar.
        /// </summary>
        public double HorizontalViewSize 
        {
            get
            {
                if (this.Zoombar.Orientation == Orientation.Horizontal)
                {
                    return this.Zoombar.Maximum - this.Zoombar.Minimum;
                }
                else
                {
                    return 0.0;
                }
            }
        }
        /// <summary>
        /// Boolean indicating whether or not the zoombar is vertically scrollable.
        /// </summary>
        public bool VerticallyScrollable 
        {
            get
            {
                return this.Zoombar.Orientation == Orientation.Vertical;
            }
        }
        /// <summary>
        /// A numeric value representing the vertical scroll position as a percentage of the scrollable area.
        /// </summary>
        public double VerticalScrollPercent 
        {
            get
            {
                if (this.Zoombar.Orientation == Orientation.Vertical)
                {
                    return this.Zoombar.Range.Minimum + (this.Zoombar.Range.Maximum - this.Zoombar.Range.Minimum) / 2;
                }
                else
                {
                    return 0.0;
                }
            }
        }
        /// <summary>
        /// The vertical range of the zoombar.
        /// </summary>
        public double VerticalViewSize 
        { 
            get
            {
                if (this.Zoombar.Orientation == Orientation.Vertical)
                {
                    return this.Zoombar.Maximum - this.Zoombar.Minimum;
                }
                else
                {
                    return 0.0;
                }
            }
        }
        /// <summary>
        /// Scrolls the zoombar to the given horizontal and vertical distance.
        /// </summary>
        /// <param name="horizontalAmount">The horizontal distance to scroll.</param>
        /// <param name="verticalAmount">The vertical distance to scroll.</param>
        public void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount)
        {
            double range = this.Zoombar.Range.Maximum - this.Zoombar.Range.Minimum;
            Debug.Assert(range <= 1);
            if (this.Zoombar.Orientation == Orientation.Horizontal)
            {
                switch (horizontalAmount)
                {
                    case ScrollAmount.LargeDecrement:
                        if (this.Zoombar.Range.Minimum >= this.Zoombar.LargeChange)
                        {
                            this.Zoombar.Range.Minimum -= this.Zoombar.LargeChange;
                            this.Zoombar.Range.Maximum -= this.Zoombar.LargeChange;
                        }
                        else
                        {
                            this.Zoombar.Range.Minimum = 0;
                            this.Zoombar.Range.Maximum = range;
                        }
                        break;
                    case ScrollAmount.SmallDecrement:
                        if (this.Zoombar.Range.Minimum >= this.Zoombar.SmallChange)
                        {
                            this.Zoombar.Range.Minimum -= this.Zoombar.SmallChange;
                            this.Zoombar.Range.Maximum -= this.Zoombar.SmallChange;
                        }
                        else
                        {
                            this.Zoombar.Range.Minimum = 0;
                            this.Zoombar.Range.Maximum = range;
                        }
                        break;
                    case ScrollAmount.LargeIncrement:
                        if (this.Zoombar.Range.Maximum + this.Zoombar.LargeChange <= 1)
                        {
                            this.Zoombar.Range.Minimum += this.Zoombar.LargeChange;
                            this.Zoombar.Range.Maximum += this.Zoombar.LargeChange;
                        }
                        else
                        {
                            this.Zoombar.Range.Minimum = 1 - range;
                            this.Zoombar.Range.Maximum = 1;
                        }
                        break;
                    case ScrollAmount.SmallIncrement:
                        if (this.Zoombar.Range.Maximum + this.Zoombar.SmallChange <= 1)
                        {
                            this.Zoombar.Range.Minimum += this.Zoombar.SmallChange;
                            this.Zoombar.Range.Maximum += this.Zoombar.SmallChange;
                        }
                        else
                        {
                            this.Zoombar.Range.Minimum = 1 - range;
                            this.Zoombar.Range.Maximum = 1;
                        }
                        break;
                }
            }
            else
            {
                switch (verticalAmount)
                {
                    case ScrollAmount.LargeDecrement:
                        if (this.Zoombar.Range.Minimum >= this.Zoombar.LargeChange)
                        {
                            this.Zoombar.Range.Minimum -= this.Zoombar.LargeChange;
                            this.Zoombar.Range.Maximum -= this.Zoombar.LargeChange;
                        }
                        else
                        {
                            this.Zoombar.Range.Minimum = 0;
                            this.Zoombar.Range.Maximum = range;
                        }
                        break;
                    case ScrollAmount.SmallDecrement:
                        if (this.Zoombar.Range.Minimum >= this.Zoombar.SmallChange)
                        {
                            this.Zoombar.Range.Minimum -= this.Zoombar.SmallChange;
                            this.Zoombar.Range.Maximum -= this.Zoombar.SmallChange;
                        }
                        else
                        {
                            this.Zoombar.Range.Minimum = 0;
                            this.Zoombar.Range.Maximum = range;
                        }
                        break;
                    case ScrollAmount.LargeIncrement:
                        if (this.Zoombar.Range.Maximum + this.Zoombar.LargeChange <= 1)
                        {
                            this.Zoombar.Range.Minimum += this.Zoombar.LargeChange;
                            this.Zoombar.Range.Maximum += this.Zoombar.LargeChange;
                        }
                        else
                        {
                            this.Zoombar.Range.Minimum = 1 - range;
                            this.Zoombar.Range.Maximum = 1;
                        }
                        break;
                    case ScrollAmount.SmallIncrement:
                        if (this.Zoombar.Range.Maximum + this.Zoombar.SmallChange <= 1)
                        {
                            this.Zoombar.Range.Minimum += this.Zoombar.SmallChange;
                            this.Zoombar.Range.Maximum += this.Zoombar.SmallChange;
                        }
                        else
                        {
                            this.Zoombar.Range.Minimum = 1 - range;
                            this.Zoombar.Range.Maximum = 1;
                        }
                        break;
                }
 
            }

            
        }
        /// <summary>
        /// Sets the scroll position of the zoombar, expressed as a percentage.
        /// </summary>
        /// <param name="horizontalPercent">The percentage to scroll by horizontally.</param>
        /// <param name="verticalPercent">The percentage to scroll by vertically.</param>
        public void SetScrollPercent(double horizontalPercent, double verticalPercent)
        {
            double range = this.Zoombar.Range.Maximum - this.Zoombar.Range.Minimum;
            Debug.Assert(range <= 1);

            double min = 0;
            double max = 1;
            if (this.Zoombar.Orientation == Orientation.Horizontal)
            {
                min = horizontalPercent / 100 - range / 2;
                max = horizontalPercent / 100 + range / 2;

                if (min < 0)
                {
                    min = 0;
                    max = range;
                }

                if (max > 1)
                {
                    min = 1 - range;
                    max = 1;
                }
            }
            else
            {
                min = verticalPercent / 100 - range / 2;
                max = verticalPercent / 100 + range / 2;

                if (min < 0)
                {
                    min = 0;
                    max = range;
                }

                if (max > 1)
                {
                    min = 1 - range;
                    max = 1;
                }
            }
            this.Zoombar.Range.Minimum = min;
            this.Zoombar.Range.Maximum = max;
        }
        #endregion //IScrollProvider

        #region Overrides
        /// <summary>
        /// Overrides the framework invocation of when a request is made for what types of user interaction are avalible. 
        /// </summary>
        /// <param name="patternInterface">This is the type of user interaction requested.</param>
        /// <returns>An object that can handle this pattern or null if none available.</returns>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Scroll)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }
        /// <summary>
        /// Overrides the framework invocation of what generic type of control this is.
        /// </summary>
        /// <returns>The <see cref="AutomationControlType"/> that describes this control.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }
        /// <summary>
        /// Overrides the framework invocation requesting a string that describes this control.
        /// </summary>
        /// <returns>A string describing the name of this control.</returns>
        protected override string GetClassNameCore()
        {
            return this.Zoombar.GetType().Name;
        }
        /// <summary>
        /// Gets the value the Zoombar's Name property.
        /// </summary>
        /// <returns>The value of the Zoombar's Name propery.</returns>
        protected override string GetNameCore()
        {
            if (string.IsNullOrEmpty(this.Zoombar.Name))
            {
                return this.Zoombar.GetType().Name;
            }
            else
            {
                return this.Zoombar.Name;
            }
        }
        #endregion //Overrides
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