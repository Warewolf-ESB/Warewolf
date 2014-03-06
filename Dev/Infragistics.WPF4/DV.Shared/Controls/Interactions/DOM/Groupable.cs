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

namespace Infragistics
{
    /// <summary>
    /// Base class for objects which can be spatially grouped.
    /// </summary>    
    internal abstract class Groupable : InteractiveElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Groupable"/> class.
        /// </summary>
        protected Groupable()
        {
        }

        #region AutoRescales

        /// <summary>
        /// Gets or sets a value indicating whether [auto rescales].
        /// </summary>
        /// <value><c>true</c> if [auto rescales]; otherwise, <c>false</c>.</value>        
        public bool AutoRescales
        {
            get { return (bool)GetValue(AutoRescalesProperty); }
            set { SetValue(AutoRescalesProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="AutoRescales"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AutoRescalesProperty =
            DependencyProperty.Register("AutoRescales", typeof(bool), typeof(Groupable),
            new PropertyMetadata(true, OnAutoRescalesChanged));

        private static void OnAutoRescalesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Groupable)d).OnAutoRescalesChanged((bool)e.OldValue, (bool)e.NewValue);
        }
        /// <summary>
        /// Called when the value of the AutoRescales property is changed.
        /// </summary>
        /// <param name="oldValue">The old value of the AutoRescales property.</param>
        /// <param name="newValue">The new value of the AutoRescales property.</param>
        protected virtual void OnAutoRescalesChanged(bool oldValue, bool newValue)
        {
        }

        #endregion


        #region HorizontalAlignment

        /// <summary>
        /// Gets or sets the horizontal alignment.
        /// </summary>
        /// <value>The horizontal alignment.</value>        
        public HorizontalAlignment HorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); }
            set { SetValue(HorizontalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="HorizontalAlignment"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HorizontalAlignmentProperty =
            DependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(Groupable),
            new PropertyMetadata(HorizontalAlignment.Left, OnHorizontalAlignmentChanged));

        private static void OnHorizontalAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Groupable)d).OnHorizontalAlignmentChanged((HorizontalAlignment)e.OldValue, (HorizontalAlignment)e.NewValue);
        }
        /// <summary>
        /// Called when the value of the HorizontalAlignment property is changed.
        /// </summary>
        /// <param name="oldValue">Old value of the HorizontalAlignment property.</param>
        /// <param name="newValue">New value of the HorizontalAlignment property.</param>
        protected virtual void OnHorizontalAlignmentChanged(HorizontalAlignment oldValue, HorizontalAlignment newValue)
        {
        }

        #endregion

        #region VerticalAlignment

        /// <summary>
        /// Gets or sets the vertical alignment.
        /// </summary>
        /// <value>The vertical alignment.</value>
        public VerticalAlignment VerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalAlignmentProperty); }
            set { SetValue(VerticalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="VerticalAlignment"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VerticalAlignmentProperty =
            DependencyProperty.Register("VerticalAlignment", typeof(VerticalAlignment), typeof(Groupable),
            new PropertyMetadata(VerticalAlignment.Top, OnVerticalAlignmentChanged));

        private static void OnVerticalAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Groupable)d).OnVerticalAlignmentChanged((VerticalAlignment)e.OldValue, (VerticalAlignment)e.NewValue);
        }
        /// <summary>
        /// Called when the value of the VerticalAlignment property is changed.
        /// </summary>
        /// <param name="oldValue">Old value of the VerticalAlignment property.</param>
        /// <param name="newValue">New value of the VerticalAlignment property.</param>
        protected virtual void OnVerticalAlignmentChanged(VerticalAlignment oldValue, VerticalAlignment newValue)
        {
        }

        #endregion

        #region Margin

        /// <summary>
        /// Gets or sets the margin.
        /// </summary>
        /// <value>The margin.</value>
        public Thickness Margin
        {
            get { return (Thickness)GetValue(MarginProperty); }
            set { SetValue(MarginProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Margin"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.Register("Margin", typeof(Thickness), typeof(Groupable),
            new PropertyMetadata(new Thickness(), OnMarginChanged));

        private static void OnMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Groupable)d).OnMarginChanged((Thickness)e.OldValue, (Thickness)e.NewValue);
        }

        /// <summary>
        /// MarginProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnMarginChanged(Thickness oldValue, Thickness newValue)
        {
        }

        #endregion

        private Group _parent;
        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public Group Parent
        {
            get { return this._parent; }
            internal set { this._parent = value; }
        }

        /// <summary>
        /// Arranges the in group.
        /// </summary>
        /// <param name="oldGroupBounds">The old group bounds.</param>
        /// <param name="newGroupBounds">The new group bounds.</param>
        /// <param name="groupTransform">The group transform.</param>
        public virtual void ArrangeInGroup(Rect oldGroupBounds, Rect newGroupBounds, Transform groupTransform)
        {
            if (this.AutoRescales)
            {
                TransformBounds(groupTransform);
            }
            else
            {
                ArrangeRelative(oldGroupBounds, newGroupBounds);
            }
        }

        /// <summary>
        /// Applies the given transform to a rectangle and returns the resulting rectangle.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        /// <param name="rect">The rectangle to apply the transform to.</param>
        /// <returns>A rectangle created as a result of applying the given transform to the given rectangle.</returns>
        protected static Rect TransformRect(GeneralTransform transform, Rect rect)
        {
            Point pt1 = transform.Transform(new Point(rect.X, rect.Y));
            Point pt2 = transform.Transform(new Point(rect.Right, rect.Bottom));

            return new Rect(pt1, pt2);
        }
        /// <summary>
        /// Determines the translate transform which can be used to transform one rectangle to another.
        /// </summary>
        /// <param name="newBounds">The rectangle that must be the result of a transfom of oldBounds.</param>
        /// <param name="oldBounds">The rectangle that must be transformed.</param>
        /// <returns>The translate transform that can be used to transform oldBounds to newBounds.</returns>
        protected static TranslateTransform CalculateTranslateTransform(Rect newBounds, Rect oldBounds)
        {
            TranslateTransform translateTransform = new TranslateTransform();

            if (newBounds.IsEmpty || oldBounds.IsEmpty)
            {
                return translateTransform;
            }

            double dx = newBounds.X - oldBounds.X;
            double dy = newBounds.Y - oldBounds.Y;

            translateTransform.X = dx;
            translateTransform.Y = dy;

            return translateTransform;
        }
        /// <summary>
        /// Determines the scale transform which can be used to transform one rectangle to another.
        /// </summary>
        /// <param name="newBounds">The rectangle that must be the result of a transform of oldBounds.</param>
        /// <param name="oldBounds">The rectangle that must be transformed.</param>
        /// <returns>The scale transform that can be used to transform oldBounds to newBounds.</returns>
        protected static ScaleTransform CalculateScaleTransform(Rect newBounds, Rect oldBounds)
        {
            ScaleTransform scaleTransform = new ScaleTransform();

            if (oldBounds.Width == 0 || oldBounds.Height == 0 ||
                newBounds.Width == 0 || newBounds.Height == 0 ||
                oldBounds.IsEmpty || newBounds.IsEmpty)
            {
                return scaleTransform;
            }

            scaleTransform.CenterX = oldBounds.X;
            scaleTransform.CenterY = oldBounds.Y;

            double dx = newBounds.Width / oldBounds.Width;
            double dy = newBounds.Height / oldBounds.Height;

            scaleTransform.ScaleX = dx;
            scaleTransform.ScaleY = dy;

            return scaleTransform;
        }
        /// <summary>
        /// Determines the transform group which can be used to transform one rectangle to another.
        /// </summary>
        /// <param name="newBounds">The rectangle that must be the result of a transform of oldBounds.</param>
        /// <param name="oldBounds">The rectangle that must be transformed.</param>
        /// <returns>The transform group that can be used to transform oldBounds to newBounds.</returns>
        protected static TransformGroup CalculateTransform(Rect newBounds, Rect oldBounds)
        {
            TransformGroup transformGroup = new TransformGroup();

            ScaleTransform scaleTr = CalculateScaleTransform(newBounds, oldBounds);
            transformGroup.Children.Add(scaleTr);

            TranslateTransform translateTr = CalculateTranslateTransform(newBounds, oldBounds);
            transformGroup.Children.Add(translateTr);

            return transformGroup;
        }

        /// <summary>
        /// Transforms the bounds of this element.
        /// </summary>
        /// <param name="transform">The transform.</param>
        public virtual void TransformBounds(Transform transform)
        {
            this.Bounds = TransformRect(transform, this.Bounds);
        }

        /// <summary>
        /// Arranges relative.
        /// </summary>
        /// <param name="oldGroupBounds">The old group bounds.</param>
        /// <param name="newGroupBounds">The new group bounds.</param>
        public virtual void ArrangeRelative(Rect oldGroupBounds, Rect newGroupBounds)
        {
            Rect bounds = this.Bounds;
            Thickness margin = this.Margin;

            switch (this.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    bounds.X = newGroupBounds.X + margin.Left;
                    break;
                case HorizontalAlignment.Center:
                    bounds.X = newGroupBounds.X + (newGroupBounds.Width - bounds.Width) / 2.0;
                    break;
                case HorizontalAlignment.Right:
                    bounds.X = newGroupBounds.Right - bounds.Width - margin.Right;
                    break;
                case HorizontalAlignment.Stretch:
                    bounds.X = newGroupBounds.X + margin.Left;
                    bounds.Width = newGroupBounds.Width - margin.Left - margin.Left;
                    break;
            }

            switch (this.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    bounds.Y = newGroupBounds.Y + margin.Top;
                    break;
                case VerticalAlignment.Center:
                    bounds.Y = newGroupBounds.Y + (newGroupBounds.Height - bounds.Height) / 2.0;
                    break;
                case VerticalAlignment.Bottom:
                    bounds.Y = newGroupBounds.Bottom - bounds.Height - margin.Bottom;
                    break;
                case VerticalAlignment.Stretch:
                    bounds.Y = newGroupBounds.Y + margin.Top;
                    bounds.Height = newGroupBounds.Height - margin.Top - margin.Bottom;
                    break;
            }

            this.Bounds = bounds;
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