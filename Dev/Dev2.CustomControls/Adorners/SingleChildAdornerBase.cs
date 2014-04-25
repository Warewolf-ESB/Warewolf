// Copyright (C) Josh Smith - February 2007

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace WPF.JoshSmith.Adorners
{
    /// <summary>
    /// Abstract class used to create adorners with only one visual child.
    /// By default the visual child is not added to the adorner's visual or logical tree.
    /// Subclasses can add the child to the visual and/or logical trees if necessary.
    /// This class also provides support for moving the adorner via the public OffsetTop
    /// and OffsetLeft properties.
    /// </summary>
    /// <remarks>
    /// Initial Concept: http://blogs.msdn.com/marcelolr/archive/2006/03/03/543301.aspx
    /// </remarks>
    public abstract class SingleChildAdornerBase : Adorner
    {
        #region Data

        /// <summary>
        /// The child element displayed in the adorner.
        /// </summary>
        protected UIElement child = null;
        private double offsetLeft;
        private double offsetTop;

        #endregion // Data

        #region Constructor

        /// <summary>
        /// Protected constructor.
        /// </summary>
        /// <param name="adornedElement">The element to which the adorner will be bound.</param>
        protected SingleChildAdornerBase(UIElement adornedElement)
            : base(adornedElement)
        {
        }

        #endregion // Constructor

        #region Public Interface

        #region GetDesiredTransform

        /// <summary>
        /// Override.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            // ReSharper disable AssignNullToNotNullAttribute
            result.Children.Add(base.GetDesiredTransform(transform));
            // ReSharper restore AssignNullToNotNullAttribute
            result.Children.Add(new TranslateTransform(offsetLeft, offsetTop));
            return result;
        }

        #endregion // GetDesiredTransform

        #region OffsetLeft

        /// <summary>
        /// Gets/sets the horizontal offset of the adorner.
        /// </summary>
        public double OffsetLeft
        {
            get { return offsetLeft; }
            set
            {
                offsetLeft = value;
                UpdateLocation();
            }
        }

        #endregion // OffsetLeft

        #region SetOffsets

        /// <summary>
        /// Updates the location of the adorner in one atomic operation.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        public void SetOffsets(double left, double top)
        {
            offsetLeft = left;
            offsetTop = top;
            UpdateLocation();
        }

        #endregion // SetOffsets

        #region OffsetTop

        /// <summary>
        /// Gets/sets the vertical offset of the adorner.
        /// </summary>
        public double OffsetTop
        {
            get { return offsetTop; }
            set
            {
                offsetTop = value;
                UpdateLocation();
            }
        }

        #endregion // OffsetTop

        #endregion // Public Interface

        #region Protected Overrides

        /// <summary>
        /// Override.
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {
            child.Measure(constraint);
            return child.DesiredSize;
        }

        /// <summary>
        /// Override.
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        /// <summary>
        /// Override.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override Visual GetVisualChild(int index)
        {
            return child;
        }

        /// <summary>
        /// Override.  Always returns 1.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        #endregion // Protected Overrides

        #region Private Helpers

        private void UpdateLocation()
        {
            AdornerLayer adornerLayer = Parent as AdornerLayer;
            if(adornerLayer != null)
                adornerLayer.Update(AdornedElement);
        }

        #endregion // Private Helpers
    }
}