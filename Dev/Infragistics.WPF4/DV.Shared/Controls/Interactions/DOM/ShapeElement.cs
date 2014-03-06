using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections;

namespace Infragistics
{
    /// <summary>
    /// 
    /// </summary>
    internal class ShapeElement : Groupable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeElement"/> class.
        /// </summary>
        public ShapeElement()
        {
            this.FrameworkElements = new ObservableCollection<FrameworkElement>();
        }

        #region FrameworkElements

        /// <summary>
        /// Gets or sets the framework elements.
        /// </summary>
        /// <value>The framework elements.</value>        
        public ObservableCollection<FrameworkElement> FrameworkElements
        {
            get { return (ObservableCollection<FrameworkElement>)GetValue(FrameworkElementsProperty); }
            set { SetValue(FrameworkElementsProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="FrameworkElements"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FrameworkElementsProperty =
            DependencyProperty.Register("FrameworkElements", typeof(ObservableCollection<FrameworkElement>), typeof(ShapeElement),
            new PropertyMetadata(null, OnFrameworkElementsChanged));

        private static void OnFrameworkElementsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ShapeElement)d).OnFrameworkElementsChanged((ObservableCollection<FrameworkElement>)e.OldValue, (ObservableCollection<FrameworkElement>)e.NewValue);
        }

        /// <summary>
        /// Called when framework elements changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnFrameworkElementsChanged(ObservableCollection<FrameworkElement> oldValue, ObservableCollection<FrameworkElement> newValue)
        {
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= FrameworkElements_CollectionChanged;
                RemoveFrameworkElementsSettings(oldValue);
            }

            if (newValue != null)
            {
                newValue.CollectionChanged += FrameworkElements_CollectionChanged;
                SetFrameworkElementsSettings(newValue);
            }
        }

        private void FrameworkElements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    SetFrameworkElementsSettings(e.NewItems);
                    break;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    RemoveFrameworkElementsSettings(e.OldItems);
                    break;
            }
        }

        private void SetFrameworkElementsSettings(IList items)
        {
            if (items == null)
            {
                return;
            }

            foreach (FrameworkElement newValue in items)
            {
                newValue.MouseEnter += FrameworkElement_MouseEnter;
                newValue.MouseLeave += FrameworkElement_MouseLeave;

                if (this.View != null && this.Canvas != null)
                {
                    this.Canvas.Children.Add(newValue);
                }

                ChangeFrameworkElementSettings(newValue);
            }
        }
        private void RemoveFrameworkElementsSettings(IList items)
        {
            if (items == null)
            {
                return;
            }

            foreach (FrameworkElement oldValue in items)
            {
                oldValue.MouseEnter -= FrameworkElement_MouseEnter;
                oldValue.MouseLeave -= FrameworkElement_MouseLeave;

                if (this.View != null && this.Canvas != null)
                {
                    this.Canvas.Children.Remove(oldValue);
                }
            }
        }

        private void FrameworkElement_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.IsMouseCaptured == false)
            {
                this.ActiveFrameworkElement = sender as FrameworkElement;
            }

            this.IsMouseEntered = true;
        }
        private void FrameworkElement_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.IsMouseCaptured == false)
            {
                this.ActiveFrameworkElement = null;
            }

            this.IsMouseEntered = false;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Called when the value of the Bounds property is changed.
        /// </summary>
        /// <param name="oldValue">Old value of the Bounds property.</param>
        /// <param name="newValue">New value of the Bounds property.</param>
        protected override void OnBoundsChanged(Rect oldValue, Rect newValue)
        {
            base.OnBoundsChanged(oldValue, newValue);

            SetFrameworkElementsBounds();
        }
        /// <summary>
        /// Called when the value of the Visibility property is changed.
        /// </summary>
        /// <param name="oldValue">Old value of the Visibility property.</param>
        /// <param name="newValue">New value of the Visibility property.</param>
        protected override void OnVisibilityChanged(Visibility oldValue, Visibility newValue)
        {
            base.OnVisibilityChanged(oldValue, newValue);

            foreach (FrameworkElement frameworkElement in this.FrameworkElements)
            {
                frameworkElement.Visibility = newValue;
            }

            if (newValue == Visibility.Collapsed)
            {
                this.IsMouseEntered = false;
            }
        }
        /// <summary>
        /// Called when the value of the ZIndex property is changed.
        /// </summary>
        /// <param name="oldValue">Old value of the ZIndex property.</param>
        /// <param name="newValue">New value of the ZIndex property.</param>
        protected override void OnZIndexChanged(int oldValue, int newValue)
        {
            base.OnZIndexChanged(oldValue, newValue);

            foreach (FrameworkElement frameworkElement in this.FrameworkElements)
            {
                Canvas.SetZIndex(frameworkElement, newValue);
            }
        }
        /// <summary>
        /// Called when the value of the Cursor property is changed.
        /// </summary>
        /// <param name="oldValue">Old value of the Cursor property.</param>
        /// <param name="newValue">New value of the Cursor property.</param>
        protected override void OnCursorChanged(Cursor oldValue, Cursor newValue)
        {
            base.OnCursorChanged(oldValue, newValue);
            foreach (FrameworkElement frameworkElement in this.FrameworkElements)
            {
                frameworkElement.Cursor = newValue;
            }
        }

        /// <summary>
        /// FillProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnFillChanged(Brush oldValue, Brush newValue)
        {
            base.OnFillChanged(oldValue, newValue);

            foreach (FrameworkElement frameworkElement in this.FrameworkElements)
            {
                Shape shape = frameworkElement as Shape;
                if (shape != null)
                {
                    shape.Fill = newValue;
                }
            }
        }

        /// <summary>
        /// StrokeProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnStrokeChanged(Brush oldValue, Brush newValue)
        {
            base.OnStrokeChanged(oldValue, newValue);

            foreach (FrameworkElement frameworkElement in this.FrameworkElements)
            {
                Shape shape = frameworkElement as Shape;
                if (shape != null)
                {
                    shape.Stroke = newValue;
                }
            }
        }

        /// <summary>
        /// StrokeThicknessProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnStrokeThicknessChanged(double oldValue, double newValue)
        {
            base.OnStrokeThicknessChanged(oldValue, newValue);

            foreach (FrameworkElement frameworkElement in this.FrameworkElements)
            {
                Shape shape = frameworkElement as Shape;
                if (shape != null)
                {
                    shape.StrokeThickness = newValue;
                }
            }
        }

        /// <summary>
        /// Renders this element in the current canvas.
        /// </summary>
        public override void Render()
        {
            if (this.View == null || this.Canvas == null)
            {
                return;
            }

            foreach (FrameworkElement frameworkElement in this.FrameworkElements)
            {
                if (this.Canvas.Children.Contains(frameworkElement) == false)
                {
                    this.Canvas.Children.Add(frameworkElement);
                }
            }
        }
        /// <summary>
        /// Removes this element from the current canvas.
        /// </summary>
        public override void Remove()
        {
            foreach (FrameworkElement frameworkElement in this.FrameworkElements)
            {
                this.Canvas.Children.Remove(frameworkElement);
            }
        }
        /// <summary>
        /// Determines whether or not the given point is over an active part of this element.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>True if the given point is over an active part of this element, otherwise False.</returns>        
        public override bool HitTest(Point point)
        {
            return this.IsMouseEntered;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            //InteractiveControl view = this.View;
            //if (view != null)
            //{
            //    IInputElement inputElement = view.Canvas.InputHitTest(view.LastInput.DocMousePosition);

            //    FrameworkElement element = inputElement as FrameworkElement;

            //    while (element != null)
            //    {
            //        if (element == this.FrameworkElement)
            //        {
            //            return true;
            //        }

            //        if (element.Parent != null)
            //        {
            //            element = element.Parent as FrameworkElement;
            //        }
            //        else
            //        {
            //            element = element.TemplatedParent as FrameworkElement;
            //        }
            //    }
            //}

            //return false;

        }
        /// <summary>
        /// Method invoked when the left mouse button is pressed over this element.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        public override void OnMouseLeftButtonDown(MouseEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (this.ActiveFrameworkElement.Visibility == Visibility.Visible)
            {
                this.ActiveFrameworkElement.CaptureMouse();
                this.View.CapturedElement = this;
            }
        }
        /// <summary>
        /// Method invoked when the left mouse button is released over this element.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        public override void OnMouseLeftButtonUp(MouseEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (this.ActiveFrameworkElement.Visibility == Visibility.Visible)
            {
                this.View.IsManuallyReleasingMouseCapture = true;
                this.ActiveFrameworkElement.ReleaseMouseCapture();                
            }

            this.View.CapturedElement = null;
        }

        #endregion

        private FrameworkElement _activeFrameworkElement;
        internal FrameworkElement ActiveFrameworkElement
        {
            get { return _activeFrameworkElement; }
            set { _activeFrameworkElement = value; }
        }

        private bool _isMouseEntered;
        private bool IsMouseEntered
        {
            get { return _isMouseEntered; }
            set { _isMouseEntered = value; }
        }

        /// <summary>
        /// Changes the framework element settings.
        /// </summary>
        protected virtual void ChangeFrameworkElementSettings(FrameworkElement frameworkElement)
        {
            this.Initializing = true;

            // init bounds
            this.Bounds = GetBounds(frameworkElement);
            SetFrameworkElementBounds(frameworkElement);

            this.Initializing = false;
        }

        private void SetFrameworkElementsBounds()
        {
            foreach (FrameworkElement frameworkElement in this.FrameworkElements)
            {
                SetFrameworkElementBounds(frameworkElement);
            }
        }
        private void SetFrameworkElementBounds(FrameworkElement frameworkElement)
        {
            Thickness margin = frameworkElement.Margin;

            double marginWidth = margin.Left + margin.Right;
            double marginHeight = margin.Top + margin.Bottom;

            double width = this.Bounds.Width - marginWidth;
            double height = this.Bounds.Height - marginHeight;

            if (width < 0)
            {
                width = 0;
            }

            if (height < 0)
            {
                height = 0;
            }

            frameworkElement.Width = System.Math.Round(width);
            frameworkElement.Height = System.Math.Round(height);

            Canvas.SetLeft(frameworkElement, System.Math.Round(this.Bounds.X));
            Canvas.SetTop(frameworkElement, System.Math.Round(this.Bounds.Y));
        }

        private Rect GetBounds(FrameworkElement frameworkElement)
        {
            Rect bounds = this.Bounds;

            if (ReadLocalValue(XProperty) == DependencyProperty.UnsetValue)
            {
                double x = Canvas.GetLeft(frameworkElement);
                if (double.IsNaN(x) == false)
                {
                    bounds.X = x;
                }
            }

            if (ReadLocalValue(YProperty) == DependencyProperty.UnsetValue)
            {
                double y = Canvas.GetTop(frameworkElement);
                if (double.IsNaN(y) == false)
                {
                    bounds.Y = y;
                }
            }

            if (ReadLocalValue(WidthProperty) == DependencyProperty.UnsetValue)
            {
                double width = frameworkElement.Width;
                if (double.IsNaN(width) == false)
                {
                    bounds.Width = width;
                }
            }

            if (ReadLocalValue(HeightProperty) == DependencyProperty.UnsetValue)
            {
                double height = frameworkElement.Height;
                if (double.IsNaN(height) == false)
                {
                    bounds.Height = height;
                }
            }

            return bounds;
        }
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