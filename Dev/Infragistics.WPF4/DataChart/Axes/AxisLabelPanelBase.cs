using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Base class for creating an axis label panel.
    /// </summary>

    [DesignTimeVisible(false)]


    public abstract class AxisLabelPanelBase
        : Panel
    {
        internal virtual AxisLabelPanelBaseView CreateView()
        {
            return new AxisLabelPanelBaseView(this);
        }
        internal virtual void OnViewCreated(AxisLabelPanelBaseView view)
        {

        }
        internal AxisLabelPanelBaseView View { get; set; }

        internal AxisLabelPanelBase() : base()
        {
            ViewportRect = Rect.Empty;



            View = CreateView();
            OnViewCreated(View);
            View.OnInit();
            TextBlocks = new List<FrameworkElement>();
            LabelPositions = new List<LabelPosition>();
            LabelBounds = new List<Rect>();

            UseStaggering = false;
            UseRotation = false;
            UseWrapping = false;
            FoundCollisions = false;
        }

        internal double Interval { get; set; }

        [Weak]
        internal Axis Axis { get; set; }

        internal List<object> LabelDataContext { get; set; }

        internal List<LabelPosition> LabelPositions { get; set; }

        internal Rect ViewportRect { get; set; }




        internal Rect WindowRect { get; set; }

        private double _crossingValue;

        internal double CrossingValue
        {
            get { return _crossingValue; }
            set { _crossingValue = value; }
        }





        /// <summary>
        /// Gets or sets the list of label elements.
        /// </summary>
        protected internal List<FrameworkElement> TextBlocks { get; set; }

        /// <summary>
        /// Gets or sets the list of label placeholder bounds.
        /// </summary>
        protected internal List<Rect> LabelBounds { get; set; }

        /// <summary>
        /// Gets or sets whether label collisions were detected.
        /// </summary>
        protected internal bool FoundCollisions { get; set; }

        /// <summary>
        /// Gets or sets whether label rotation should be used.
        /// </summary>
        protected internal bool UseRotation { get; set; }

        /// <summary>
        /// Gets or sets whether label staggering should be used.
        /// </summary>
        protected internal bool UseStaggering { get; set; }

        /// <summary>
        /// Gets or sets whether label text wrapping should be used.
        /// </summary>
        protected internal bool UseWrapping { get; set; }
        //private double RotationAngle { get; set; }

        /// <summary>
        /// Gets or sets the longest label control.
        /// </summary>
        protected internal object LongestTextBlock { get; set; }

        /// <summary>
        /// Determines if there are label collisions.
        /// </summary>
        /// <param name="rectangles">List of label bounds</param>
        /// <returns>True if collisions were detected; otherwise false</returns>
        protected bool DetectCollisions(List<Rect> rectangles)
        {
            //Rect outerbound = new Rect(0, 0, ActualWidth, ActualHeight);

            for (int i = 0; i < rectangles.Count - 1; i++)
            {
                for (int j = i; j < rectangles.Count - 1; j++)
                {
                    if (rectangles[i].IntersectsWith(rectangles[j + 1]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Stagger the labels.
        /// </summary>
        /// <param name="largestHeight">Largest label height</param>
        /// <param name="rectangles">List of label bounds</param>
        /// <returns>Number of staggered rows</returns>
        protected virtual int StaggerLabels(double largestHeight, ref List<Rect> rectangles)
        {
            return 0;
        }

        /// <summary>
        /// Creates the label placeholder bounds.
        /// </summary>
        /// <returns>List of label rectangles</returns>
        protected internal virtual List<Rect> CreateBoundsRectangles()
        {
            return null;
        }

        /// <summary>
        /// Binds label panel's extent to axis extent.
        /// </summary>
        protected internal virtual void BindExtent()
        {
           
        }

        /// <summary>
        /// Sets up a rotate transform on the label panel.
        /// </summary>
        /// <param name="finalSize">Final size of the panel</param>
        protected internal virtual void ApplyPanelRotation(Size finalSize)
        {
            
        }

        /// <summary>
        /// Returns the rotation angle of the labels.
        /// </summary>
        /// <returns>Rotation angle of the labels</returns>
        protected internal virtual double GetEffectiveAngle()
        {
            return Axis.LabelSettings != null ? Axis.LabelSettings.Angle : 0.0;
        }

        /// <summary>
        /// Determines if the label should be displayed.
        /// </summary>
        /// <param name="index">Label index</param>
        /// <param name="bounds">Label bounds</param>
        /// <returns>True if the label should be displayed; otherwise false</returns>
        protected internal virtual bool ShouldDisplay(int index, Rect bounds)
        {
            return true;
        }

        /// <summary>
        /// Gets the desired width of the element.
        /// </summary>
        /// <param name="element">target element</param>
        /// <returns>Desired width of the element</returns>
        protected internal double GetDesiredWidth(object element)
        {
            return View.GetDesiredWidth(element);
        }

        /// <summary>
        /// Gets the desired height of the element.
        /// </summary>
        /// <param name="element">target element</param>
        /// <returns>Desired height of the element</returns>
        protected internal double GetDesiredHeight(object element)
        {
            return View.GetDesiredHeight(element);
        }

        /// <summary>
        /// Calculates label bounds.
        /// </summary>
        /// <returns>List of calculated bounds</returns>
        protected internal virtual List<Rect> DetermineLabelBounds()
        {
            if (Children.Count < 1 || LabelPositions.Count == 0)
            {
                return new List<Rect>();
            }

            TextBlocks = new List<FrameworkElement>();
            LabelBounds = new List<Rect>();

            BindExtent();
            View.DetermineLongestLabel();

            double angle = GetEffectiveAngle();
            if (angle % 360 == 0)
            {
                //clearing the property value also destroys the associated bindings
                //this.Axis.LabelSettings.ClearValue(AxisLabelSettings.AngleProperty);
                UseRotation = false;
            }

            if (TextBlocks.Count == 0)
            {
                return new List<Rect>();
            }

            LabelBounds = CreateBoundsRectangles();

            return LabelBounds;
        }


        /// <summary>
        /// Positions child elements and determines a size for this element.
        /// </summary>
        /// <param name="finalSize">The size available to this element for arranging its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            return ArrangeLabels(finalSize);
        }


        internal virtual Size ArrangeLabels(Size finalSize)
        {
            if (Axis == null || Children.Count == 0)
            {
                return finalSize;
            }

            LabelBounds = DetermineLabelBounds();

            if (TextBlocks.Count != LabelBounds.Count)
            {
                return finalSize;
            }

            if (UseRotation)
            {
                for (int i = 0; i < TextBlocks.Count; i++)
                {
                    if (ShouldDisplay(i, LabelBounds[i]))
                    {
                        View.ArrangeToBounds(TextBlocks[i], LabelBounds[i]);
                    }
                    SetLabelRotationTransform(TextBlocks[i], GetEffectiveAngle());
                }
            }

            for (int i = 0; i < TextBlocks.Count; i++)
            {
                if (!UseRotation)
                {
                    View.ClearTransforms(TextBlocks[i]);
                }
                if (ShouldDisplay(i, LabelBounds[i]))
                {
                    View.ArrangeToBounds(TextBlocks[i], LabelBounds[i]);
                }
                else
                {
                    View.ArrangeToBounds(TextBlocks[i], new Rect(0, 0, 0, 0));
                }
            }

            ApplyPanelRotation(finalSize);
            return finalSize;
        }

        /// <summary>
        /// Sets up the transform for the label.
        /// </summary>
        /// <param name="label">Target label</param>
        /// <param name="angle">Rotation angle</param>
        protected internal virtual void SetLabelRotationTransform(FrameworkElement label, double angle)
        {
            double effAngle = GetEffectiveAngle();
            View.HandleSetLabelRotationTransform(label, effAngle);
        }


        /// <summary>
        /// Provides the behavior for the Measure pass of Silverlight layout. Classes can override this method to define their own Measure pass behavior.
        /// </summary>
        /// <param name="availableSize">The available size that this object can give to child objects. Infinity (<see cref="F:System.Double.PositiveInfinity"/>) can be specified as a value to indicate that the object will size to whatever content is available.</param>
        /// <returns>
        /// The size that this object determines it needs during layout, based on its calculations of the allocated sizes for child objects; or based on other considerations, such as a fixed container size.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            return MeasureLabels(availableSize);
        }


        internal virtual Size MeasureLabels(Size availableSize)
        {
            foreach (object element in Children)
            {
                View.HandleMeasureLabel(element);
            }

            //using new internal Extent property instead of the public one.
            //double extent = (this.Axis == null || this.Axis.LabelSettings.Extent <= 0) ? 50 : this.Axis.LabelSettings.Extent;
            double extent = (Axis == null || Extent <= 0) ? 50 : Extent;
            Size size = new Size(extent, extent);

            if (double.IsInfinity(size.Width))
            {
                size.Width = 50;
            }
            if (double.IsInfinity(size.Height))
            {
                size.Height = 50;
            }

            return size;
        }


        #region Extent Dependency Property

        internal const string ExtentPropertyName = "Extent";

        /// <summary>
        /// Identifies the Extent dependency property.
        /// </summary>
        internal static readonly DependencyProperty ExtentProperty = DependencyProperty.Register(ExtentPropertyName, typeof(double), typeof(AxisLabelPanelBase),
            new PropertyMetadata(50.0, (sender, e) =>
            {
                
            }));

        /// <summary>
        /// Gets or sets the Extent property
        /// </summary>
        internal double Extent
        {
            get
            {
                return (double)this.GetValue(ExtentProperty);
            }
            set
            {
                this.SetValue(ExtentProperty, value);
            }
        }
        #endregion Extent Dependency Property

        internal virtual AxisLabelsLocation GetDefaultLabelsLocation()
        {
            return AxisLabelsLocation.OutsideBottom;
        }

        internal virtual bool ValidLocation(AxisLabelsLocation location)
        {
            return true;
        }

        internal IEnumerable<object> GetChildren()
        {
            foreach (var ele in this.Children)
            {
                yield return (UIElement)ele;
            }
        }

        internal string TrimTextBlock(int index, TextBlock textblock, double availableWidth)
        {


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


            string result = View.TrimTextBlock(textblock, availableWidth);




            return result;
        }

        internal void ConsiderForLongestTextBlock(object textElement)
        {
            if (LongestTextBlock == null
                || GetDesiredWidth(LongestTextBlock) < GetDesiredWidth(textElement))
            {
                LongestTextBlock = textElement;
            }
        }

        internal void OnProcessTextBlock(FrameworkElement textElement)
        {
            TextBlocks.Add(textElement);
        }

        internal IEnumerable<object> GetTextBlocks()
        {
            for (int i = 0; i < Axis.TextBlocks.Count; i++)
            {
                yield return Axis.TextBlocks[i];
            }
        }

        internal void EnsureExtentSet()
        {
            UpdateLabelBounds();
        }

        internal void UpdateLabelBounds()
        {
            LabelBounds = DetermineLabelBounds();
        }



#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

    }

    internal class LabelPosition
    {
        public double Value { get; set; }

        public LabelPosition(double value)
        {
            Value = value;
        }
    }

    //internal class Label2DPosition
    //    : LabelPosition
    //{
    //    public double YValue { get; set; }

    //    public Label2DPosition(double value, double yValue)
    //        : base(value)
    //    {
    //        YValue = yValue;
    //    }
    //}
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