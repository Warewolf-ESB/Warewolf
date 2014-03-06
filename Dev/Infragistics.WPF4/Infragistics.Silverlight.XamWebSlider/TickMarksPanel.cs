using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A generic Panel, used to arrange the <see cref="SliderTickMarks&lt;T&gt;"/>.
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    public class TickMarksPanel<T> : Panel
    {
        #region Members

        private Size _previousSize = new Size(0, 0);

        #endregion Members

        #region Overrides

        /// <summary>
        /// Provides the behavior for the Measure pass of the layout. Classes can override this method to define their own Measure pass behavior.
        /// </summary>
        /// <param name="availableSize">The available size that this object can give to child objects. Infinity can be specified as a value to indicate that the object will size to whatever content is available.</param>
        /// <returns>
        /// The size that this object determines it needs during layout, based on its calculations of child object allotted sizes.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement tick in this.Children)
            {
                tick.Measure(availableSize);
            }

            return base.MeasureOverride(availableSize);
        }

        /// <summary>
        /// Provides the behavior for the Arrange pass of the layout. Classes can override this method to define their own Arrange pass behavior.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this object should use to arrange itself and its children.</param>
        /// <returns>
        /// The actual size used once the element is arranged.
        /// </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.Owner != null)
            {
                double minValue = this.Owner.ToDouble(this.Owner.MinValue);
                double maxValue = this.Owner.ToDouble(this.Owner.MaxValue);

                double range = maxValue - minValue;

                if (range <= 0)
                {
                    return base.ArrangeOverride(finalSize);
                }

                if (finalSize.Width > 0 && finalSize.Height > 0 && !finalSize.Equals(this._previousSize) &&
                    (this.Children.Count == 0 && this.Owner.TickMarks.Count > 0))
                {
                    this.GenerateTicks(finalSize);
                    this._previousSize = finalSize;
                }
           
                double length = (this.Owner.Orientation == Orientation.Horizontal)
                                ? finalSize.Width
                                : finalSize.Height;

                foreach (FrameworkElement tick in this.Children)
                {
                    double startX;
                    double startY;
                    double value = this.Owner.ToDouble((T) tick.DataContext);
                    double scaledValue = (value - minValue) / range * length;
                    if (this.Owner.Orientation == Orientation.Horizontal)
                    {
                        if (this.Owner.IsDirectionReversed)
                        {
                            startX = (length - scaledValue) - (tick.DesiredSize.Width / 2);
                            startY = (finalSize.Height / 2) - (tick.DesiredSize.Height / 2);
                        }
                        else
                        {
                            startX = scaledValue - (tick.DesiredSize.Width / 2);
                            startY = (finalSize.Height / 2) - (tick.DesiredSize.Height / 2);
                        }
                    }
                    else
                    {
                        if (this.Owner.IsDirectionReversed)
                        {
                            startX = (finalSize.Width / 2) - (tick.DesiredSize.Width / 2);
                            startY = scaledValue - (tick.DesiredSize.Height / 2);
                        }
                        else
                        {
                            startX = (finalSize.Width / 2) - (tick.DesiredSize.Width / 2);
                            startY = length - scaledValue - (tick.DesiredSize.Height / 2);
                        }                       
                    }

                    Point startPoint = new Point((int)startX, (int)startY);
                    tick.Arrange(new Rect(startPoint, new Point(startPoint.X + tick.DesiredSize.Width, startPoint.Y + tick.DesiredSize.Height)));
                }
            }

            return base.ArrangeOverride(finalSize);
        }

        #endregion Overrides

        #region Properties
        #region Owner

        /// <summary>
        /// Gets or sets the <see cref="XamSliderBase&lt;T&gt;"/> owner.
        /// </summary>
        /// <value>The owner.</value>
        public XamSliderBase<T> Owner
        {
            get;
            protected internal set;
        }

        #endregion // Owner

        #endregion Properties

        #region Methods

        internal void GenerateTicks()
        {
            double width = this.ActualWidth;
            double height = this.ActualHeight;
            this.GenerateTicks(new Size(width, height));            
        }

        internal void GenerateTicks(Size size)
        {
            if ((size.Height > 0 && size.Width > 0) && (this.Owner.ActualHeight > 0 || this.Owner.ActualWidth > 0))
            {
                this.Children.Clear();
                if (this.Owner != null)
                {
                    foreach (SliderTickMarks<T> tickMarks in this.Owner.TickMarks)
                    {
                        //This flag prevents any type of recursive calls...
                        tickMarks.TickmarkGenerationInProcess = true;
                        //WHY WERE WE NOT CACHING THIS!!!!!
                        var resolvedTickMarks = tickMarks.ResolvedTickMarks;

                        long countTickMarks = resolvedTickMarks.Count;

                        if (countTickMarks > 0)
                        {
                            //Create the first tick mark
                            this.CreateTick(resolvedTickMarks[0], tickMarks);

                            int step = GetStepSize(countTickMarks);

                            for (int i = 0 + step; i < countTickMarks; i += step)
                            {
                                this.CreateTick(resolvedTickMarks[i], tickMarks);
                            }
                        }
                        tickMarks.TickmarkGenerationInProcess = false;
                    }
                }
            }
        }

        /// <summary>
        /// Takes a look at the available visual space and determines the max number of tick marks to display, and returns the number of tick marks to skip between displayed tick marks
        /// </summary>
        /// <param name="countTickMarks">The number of Tick Marks we calculated</param>
        /// <returns>The number of tick marks to skip between displayed tick marks</returns>
        private int GetStepSize(long countTickMarks)
        {
            int step = 1;
            double tickSize = 1;
            double trackSize = 0;

            if (this.Owner.Orientation == Orientation.Horizontal)
            {
                trackSize = this.Owner.HorizontalTrack.ActualWidth;

                if (this.Children.Count > 0)
                {
                    var tickFE = this.Children[0] as FrameworkElement;
                    if (tickFE != null)
                        tickSize = tickFE.ActualWidth;
                }
            }
            else
            {
                trackSize = this.Owner.VerticalTrack.ActualHeight;

                if (this.Children.Count > 0)
                {
                    var tickFE = this.Children[0] as FrameworkElement;
                    if (tickFE != null)
                        tickSize = tickFE.ActualHeight;
                }
            }

            var validTickCount = (tickSize == 0 ? trackSize : (trackSize / tickSize)) / 4;

            if (validTickCount != 0 && (validTickCount < countTickMarks))
                step = (int)(countTickMarks / validTickCount);
            
            return step;
        }

        private void CreateTick(T tickValue, SliderTickMarksBase tickmarks)
        {
            double value = this.Owner.ToDouble(tickValue);
            double minValue = this.Owner.ToDouble(this.Owner.MinValue);
            double maxValue = this.Owner.ToDouble(this.Owner.MaxValue);
            if (value < minValue || value > maxValue)
            {
                return;
            }

            FrameworkElement element = this.LoadTemplate(tickValue, tickmarks);
            element.Visibility = Visibility.Visible;
            this.Children.Add(element);
        }

        private FrameworkElement LoadTemplate(T tickValue, SliderTickMarksBase tickmarks)
        {
            DataTemplate horizontalTemplate = tickmarks.HorizontalTickMarksTemplate ??
                                  this.Owner.HorizontalTickMarksTemplate;

            DataTemplate verticaltemplate = tickmarks.VerticalTickMarksTemplate ?? this.Owner.VerticalTickMarksTemplate;

            DataTemplate template = (this.Owner.Orientation == Orientation.Horizontal) ? horizontalTemplate : verticaltemplate;

            FrameworkElement element = template.LoadContent() as FrameworkElement;

            if (element != null)
            {
                element.DataContext = tickValue;
            }

            return element;
        }

        #endregion Methods
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