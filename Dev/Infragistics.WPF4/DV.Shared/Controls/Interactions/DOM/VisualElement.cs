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
using System.ComponentModel;

namespace Infragistics
{
    /// <summary>
    /// Base class for objects which can be rendered to a canvas as part of an InteractiveControl.
    /// </summary>
    internal abstract class VisualElement : DependencyObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisualElement"/> class.
        /// </summary>
        protected VisualElement()
        {
            this.StrokeDashArray = new DoubleCollection();
        }

        #region Bounds

        /// <summary>
        /// Gets or sets the bounds.
        /// </summary>
        /// <value>The bounds.</value>
        public Rect Bounds
        {
            get { return (Rect)GetValue(BoundsProperty); }
            set { SetValue(BoundsProperty, value); }
        }
        /// <summary>
        /// Identifies the Bounds dependency property.
        /// </summary>
        public static readonly DependencyProperty BoundsProperty =
            DependencyProperty.Register("Bounds", typeof(Rect), typeof(VisualElement),
            new PropertyMetadata(new Rect(0.0, 0.0, 0.0, 0.0), OnBoundsChanged));

        private static void OnBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VisualElement)d).OnBoundsChanged((Rect)e.OldValue, (Rect)e.NewValue);
        }
        /// <summary>
        /// Called when the value of the Bounds property is changed.
        /// </summary>
        /// <param name="oldValue">Old value of the Bounds property.</param>
        /// <param name="newValue">New value of the Bounds property.</param>
        protected virtual void OnBoundsChanged(Rect oldValue, Rect newValue)
        {
            this.ChangingBounds = true;

            if (this.X != newValue.X)
            {
                this.X = newValue.X;
            }

            if (this.Y != newValue.Y)
            {
                this.Y = newValue.Y;
            }

            if (this.Width != newValue.Width)
            {
                this.Width = newValue.Width;
            }

            if (this.Height != newValue.Height)
            {
                this.Height = newValue.Height;
            }

            this.ChangingBounds = false;
        }

        private bool _changingBounds;
        private bool ChangingBounds
        {
            get { return _changingBounds; }
            set { _changingBounds = value; }
        }

        #endregion

        #region ZIndex

        /// <summary>
        /// Gets or sets the Z-index.
        /// </summary>
        /// <value>The index of the Z.</value>
        public int ZIndex
        {
            get { return (int)GetValue(ZIndexProperty); }
            set { SetValue(ZIndexProperty, value); }
        }
        /// <summary>
        /// Identifies the ZIndex dependency property.
        /// </summary>
        public static readonly DependencyProperty ZIndexProperty =
            DependencyProperty.Register("ZIndex", typeof(int), typeof(VisualElement),
            new PropertyMetadata(0, new PropertyChangedCallback(OnZIndexChanged)));

        private static void OnZIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VisualElement)d).OnZIndexChanged((int)e.OldValue, (int)e.NewValue);
        }
        /// <summary>
        /// Called when the value of the ZIndex property is changed.
        /// </summary>
        /// <param name="oldValue">Old value of the ZIndex property.</param>
        /// <param name="newValue">New value of the ZIndex property.</param>
        protected virtual void OnZIndexChanged(int oldValue, int newValue)
        {
        }

        #endregion

        #region Visibility

        /// <summary>
        /// Gets or sets the visibility.
        /// </summary>
        /// <value>The visibility.</value>
        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Visibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.Register("Visibility", typeof(Visibility), typeof(VisualElement),
            new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));

        private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VisualElement)d).OnVisibilityChanged((Visibility)e.OldValue, (Visibility)e.NewValue);
        }
        /// <summary>
        /// Called when the value of the Visibility property is changed.
        /// </summary>
        /// <param name="oldValue">Old value of the Visibility property.</param>
        /// <param name="newValue">New value of the Visibility property.</param>
        protected virtual void OnVisibilityChanged(Visibility oldValue, Visibility newValue)
        {
        }

        #endregion

        #region Positioning

        #region X

        /// <summary>
        /// Gets or sets the x-axis position of the left side of the element.
        /// </summary>
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="X"/> dependency property
        /// </summary>
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(VisualElement),
            new PropertyMetadata(0.0, OnXChanged));

        private static void OnXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VisualElement)d).OnXChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// XProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnXChanged(double oldValue, double newValue)
        {
            if (this.ChangingBounds == false)
            {
                this.Bounds = new Rect(newValue, this.Bounds.Y, this.Bounds.Width, this.Bounds.Height);
            }
        }

        #endregion

        #region Y

        /// <summary>
        /// Gets or sets the y-axis position of the top side of the element.
        /// </summary>
        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property
        /// </summary>
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(VisualElement),
            new PropertyMetadata(0.0, OnYChanged));

        private static void OnYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VisualElement)d).OnYChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// YProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnYChanged(double oldValue, double newValue)
        {
            if (this.ChangingBounds == false)
            {
                this.Bounds = new Rect(this.Bounds.X, newValue, this.Bounds.Width, this.Bounds.Height);
            }
        }

        #endregion

        #region Width

        /// <summary>
        /// Gets or sets the width of the bounds of the element. 
        /// </summary>
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Width"/> dependency property
        /// </summary>
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(VisualElement),
            new PropertyMetadata(0.0, OnWidthChanged));

        private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VisualElement)d).OnWidthChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// WidthProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnWidthChanged(double oldValue, double newValue)
        {
            if (this.ChangingBounds == false)
            {
                this.Bounds = new Rect(this.Bounds.X, this.Bounds.Y, newValue, this.Bounds.Height);
            }
        }

        #endregion

        #region Height

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the height of the bounds of the element.
        /// </summary>
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(VisualElement),
            new PropertyMetadata(0.0, OnHeightChanged));

        private static void OnHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VisualElement)d).OnHeightChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// HeightProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnHeightChanged(double oldValue, double newValue)
        {
            if (this.ChangingBounds == false)
            {
                this.Bounds = new Rect(this.Bounds.X, this.Bounds.Y, this.Bounds.Width, newValue);
            }
        }

        #endregion

        /// <summary>
        /// Gets the x-axis position of the left side of the element.
        /// </summary>
        /// <value>The left.</value>
        public double Left
        {
            get
            {
                return this.Bounds.Left;
            }
        }

        /// <summary>
        /// Gets the y-axis position of the top side of the element.
        /// </summary>
        /// <value>The top.</value>
        public double Top
        {
            get
            {
                return this.Bounds.Top;
            }
        }

        /// <summary>
        /// Gets the x-axis position of the right side of the element.
        /// </summary>
        /// <value>The right.</value>
        public double Right
        {
            get
            {
                return this.Bounds.Right;
            }
        }

        /// <summary>
        /// Gets the y-axis position of the bottom side of the element.
        /// </summary>
        /// <value>The bottom.</value>
        public double Bottom
        {
            get
            {
                return this.Bounds.Bottom;
            }
        }

        /// <summary>
        /// Gets the center.
        /// </summary>
        /// <value>The center.</value>
        public Point Center
        {
            get
            {
                Point center = new Point(this.X + this.Width / 2.0, this.Y + this.Height / 2.0);

                return center;
            }
        }

        #endregion



        #region Fill

        /// <summary>
        /// Gets or sets the fill.
        /// </summary>
        /// <value>The fill.</value>
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Fill"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(VisualElement),
            new PropertyMetadata(null, OnFillChanged));

        private static void OnFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VisualElement)d).OnFillChanged((Brush)e.OldValue, (Brush)e.NewValue);
        }

        /// <summary>
        /// FillProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnFillChanged(Brush oldValue, Brush newValue)
        {
        }

        #endregion


        #region Stroke

        /// <summary>
        /// Gets or sets the stroke.
        /// </summary>
        /// <value>The stroke.</value>
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Stroke"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(VisualElement),
            new PropertyMetadata(null, OnStrokeChanged));

        private static void OnStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VisualElement)d).OnStrokeChanged((Brush)e.OldValue, (Brush)e.NewValue);
        }

        /// <summary>
        /// StrokeProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnStrokeChanged(Brush oldValue, Brush newValue)
        {
        }

        #endregion

        #region StrokeThickness

        /// <summary>
        /// Gets or sets the stroke thickness.
        /// </summary>
        /// <value>The stroke thickness.</value>
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="StrokeThickness"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(VisualElement),
            new PropertyMetadata(0.0, OnStrokeThicknessChanged));

        private static void OnStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VisualElement)d).OnStrokeThicknessChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// StrokeThicknessProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnStrokeThicknessChanged(double oldValue, double newValue)
        {
        }

        #endregion

        #region StrokeDashArray

        /// <summary>
        /// Gets or sets the stroke dash array.
        /// </summary>
        /// <value>The stroke dash array.</value>
        public DoubleCollection StrokeDashArray
        {
            get { return (DoubleCollection)GetValue(StrokeDashArrayProperty); }
            set { SetValue(StrokeDashArrayProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="StrokeDashArray"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeDashArrayProperty =
            DependencyProperty.Register("StrokeDashArray", typeof(DoubleCollection), typeof(VisualElement),
            new PropertyMetadata(null, OnStrokeDashArrayChanged));

        private static void OnStrokeDashArrayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VisualElement)d).OnStrokeDashArrayChanged((DoubleCollection)e.OldValue, (DoubleCollection)e.NewValue);
        }

        /// <summary>
        /// StrokeDashArrayProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnStrokeDashArrayChanged(DoubleCollection oldValue, DoubleCollection newValue)
        {
        }

        #endregion


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="VisualElement"/> is initializing.
        /// </summary>
        /// <value><c>true</c> if initializing; otherwise, <c>false</c>.</value>
        public bool Initializing { get; set; }

        private InteractiveControl _view;
        /// <summary>
        /// Gets the view.
        /// </summary>
        /// <value>The view.</value>
        public virtual InteractiveControl View
        {
            get
            {
                return this._view;
            }
            protected set
            {
                this._view = value;
            }
        }

        /// <summary>
        /// Gets the canvas.
        /// </summary>
        /// <value>The canvas.</value>
        protected Canvas Canvas
        {
            get
            {
                return this.View.Canvas;
            }
        }

        internal virtual void SetView(InteractiveControl view)
        {
            this.View = view;
        }

        /// <summary>
        /// Tests whether or not the given point is over the bounds of this object.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>True if the point is within this object's bounds, otherwise False.</returns>
        public virtual bool HitTest(Point point)
        {
            return this.Bounds.Contains(point);
        }

        /// <summary>
        /// Tests whether or not the given rectangle is entirely within the bounds of this object.
        /// </summary>
        /// <param name="rect">The rectangle to test.</param>
        /// <returns>True if the rectangle is entirely within the bounds of this object, otherwise False.</returns>
        public virtual bool HitTest(Rect rect)
        {
            if (rect.X <= this.Bounds.X && rect.Y <= this.Bounds.Y &&
                rect.Right >= this.Bounds.Right && rect.Bottom >= this.Bounds.Bottom)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Renders this element in the current canvas.
        /// </summary>
        public abstract void Render();
        /// <summary>
        /// Removes this element from the current canvas.
        /// </summary>
        public abstract void Remove();
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