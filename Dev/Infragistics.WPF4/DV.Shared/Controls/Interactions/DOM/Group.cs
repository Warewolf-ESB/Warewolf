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
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Markup;
using System.Collections;

namespace Infragistics
{
    /// <summary>
    /// Represents a group of groupable elements.
    /// </summary>    
    [ContentProperty("Children")]
    internal class Group : Groupable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        public Group()
        {
            this.Children = new ObservableCollection<Groupable>();
        }

        private bool _addingItem;
        private bool AddingItem
        {
            get { return this._addingItem; }
            set { this._addingItem = value; }
        }

        #region Children

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        /// <value>The children.</value>
        public ObservableCollection<Groupable> Children
        {
            get { return (ObservableCollection<Groupable>)GetValue(ChildrenProperty); }
            set { SetValue(ChildrenProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Children"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ChildrenProperty =
            DependencyProperty.Register("Children", typeof(ObservableCollection<Groupable>), typeof(Group),
            new PropertyMetadata(null, OnChildrenChanged));

        private static void OnChildrenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Group)d).OnChildrenChanged((ObservableCollection<Groupable>)e.OldValue, (ObservableCollection<Groupable>)e.NewValue);
        }

        private void OnChildrenChanged(ObservableCollection<Groupable> oldValue, ObservableCollection<Groupable> newValue)
        {
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= Children_CollectionChanged;
                RemoveItems(oldValue);
            }

            if (newValue != null)
            {
                AddItems(newValue);
                newValue.CollectionChanged += Children_CollectionChanged;
                
            }
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    //for (int i = 0; i < e.OldItems.Count; i++)
                    //{

                    //}
                    break;
                case NotifyCollectionChangedAction.Reset:                   
                    break;
            }
        }

        /// <summary>
        /// Adds the items.
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddItems(IList items)
        {
            if (items == null)
            {
                return;
            }

            foreach (Groupable item in items)
            {
                item.SetView(this.View);

                item.Parent = this;                

                this.AddingItem = true;

                AccumulateBounds(item);

                this.AddingItem = false;

                AddItem(item);
            }
        }



        /// <summary>
        /// Removes the items.
        /// </summary>
        /// <param name="items">The items.</param>
        public void RemoveItems(IList items)
        {
            if (items == null)
            {
                return;
            }

            foreach (Groupable item in items)
            {
                item.Parent = this.Parent;

                RemoveItem(item);
            }
        }

        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected virtual void AddItem(Groupable item)
        {
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected virtual void RemoveItem(Groupable item)
        {
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

            if (this.Initializing == false && this.AddingItem == false)
            {
                this.ArrangeChildren(oldValue, newValue);
            }
        }
        /// <summary>
        /// Called when the value of the Visibility property is changed.
        /// </summary>
        /// <param name="oldValue">Old value of the Visibility property.</param>
        /// <param name="newValue">New value of the Visibility property.</param>
        protected override void OnVisibilityChanged(Visibility oldValue, Visibility newValue)
        {
            base.OnVisibilityChanged(oldValue, newValue);

            foreach (Groupable item in this.Children)
            {
                item.Visibility = newValue;
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

            foreach (Groupable item in this.Children)
            {
                item.ZIndex = newValue;
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

            foreach (Groupable item in this.Children)
            {
                item.Cursor = newValue;
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

            foreach (Groupable item in this.Children)
            {
                item.Fill = newValue;
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

            foreach (Groupable item in this.Children)
            {
                item.Stroke = newValue;
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

            foreach (Groupable item in this.Children)
            {
                item.StrokeThickness = newValue;
            }
        }

        /// <summary>
        /// Renders this element in the current canvas.
        /// </summary>
        public override void Render()
        {
            foreach (VisualElement item in this.Children)
            {
                if (item.View == null)
                {
                    item.SetView(this.View);
                }

                item.Render();
            }
        }
        /// <summary>
        /// Removes this element from the current canvas.
        /// </summary>
        public override void Remove()
        {
            foreach (VisualElement item in this.Children)
            {
                if (item.View != null)
                {
                    item.Remove();
                }
            }
        }

        /// <summary>
        /// Determines whether or not the given point is over an active part of this group.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>True if the given point is over an active part of this group, otherwise False.</returns>
        public override bool HitTest(Point point)
        {
            if (this.Visibility == Visibility.Collapsed)
            {
                return false;
            }

            foreach (Groupable groupable in this.Children)
            {
                if (groupable.HitTest(point))
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Determines whether or not the given rectangle is entirely contained within the bounds of an active part of this group.
        /// </summary>
        /// <param name="rect">The rectangle to test.</param>
        /// <returns>True if the given rectangle is entirely contained within the bounds of an active part of this group, otherwise False.</returns>
        public override bool HitTest(Rect rect)
        {
            if (this.Visibility == Visibility.Collapsed)
            {
                return false;
            }

            foreach (Groupable groupable in this.Children)
            {
                if (groupable.HitTest(rect))
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Method invoked when the mouse pointer leaves this group.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        public override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            foreach (InteractiveElement element in this.Children)
            {
                if (element.IsMouseOver)
                {
                    element.OnMouseLeave(e);
                }
            }
        }
        /// <summary>
        /// Method invoked when the mouse pointer moves over this group.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point docMousePosition = this.View.LastInput.DocMousePosition;

            foreach (InteractiveElement element in this.Children)
            {
                if (element.HitTest(docMousePosition))
                {
                    if (element.IsMouseOver)
                    {
                        element.OnMouseMove(e);
                    }
                    else
                    {
                        element.OnMouseEnter(e);
                    }
                }
                else
                {
                    if (element.IsMouseOver)
                    {
                        element.OnMouseLeave(e);
                    }
                }
            }
        }
        /// <summary>
        /// Method invoked when the mouse pointer is pressed over this group.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        public override void OnMouseLeftButtonDown(MouseEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            Point docMousePosition = this.View.LastInput.DocMousePosition;

            foreach (InteractiveElement element in this.Children)
            {
                if (element.HitTest(docMousePosition))
                {
                    element.OnMouseLeftButtonDown(e);
                }
            }
        }
        /// <summary>
        /// Method invoked when the mouse pointer is released over this group.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        public override void OnMouseLeftButtonUp(MouseEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            Point docMousePosition = this.View.LastInput.DocMousePosition;

            foreach (InteractiveElement element in this.Children)
            {
                if (element.HitTest(docMousePosition))
                {
                    element.OnMouseLeftButtonUp(e);
                }
            }
        }        
        /// <summary>
        /// Method invoked when this element is double-clicked with the left mouse button.
        /// </summary>
        /// <param name="e">The MouseButtonEventArgs in context.</param>
        public override void OnMouseLeftButtonDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDoubleClick(e);

            Point docMousePosition = this.View.LastInput.DocMousePosition;

            for (int i = 0; i < this.Children.Count; i++)
            {
                InteractiveElement element = this.Children[i];

                if (element.HitTest(docMousePosition))
                {
                    element.OnMouseLeftButtonDoubleClick(e);
                }
            }
        }

        /// <summary>
        /// Method invoked when a key is pressed while this group has focus.
        /// </summary>
        /// <param name="e">The KeyEventArgs in context.</param>
        public override void OnKeyDown(KeyEventArgs e)
        {
            Point docMousePosition = this.View.LastInput.DocMousePosition;

            foreach (InteractiveElement element in this.Children)
            {
                if (element.HitTest(docMousePosition))
                {
                    element.OnKeyDown(e);
                }
            }
        }
        /// <summary>
        /// Method invoked when a key is released while this group has focus.
        /// </summary>
        /// <param name="e">The KeyEventArgs in context.</param>
        public override void OnKeyUp(KeyEventArgs e)
        {
            Point docMousePosition = this.View.LastInput.DocMousePosition;

            foreach (InteractiveElement element in this.Children)
            {
                if (element.HitTest(docMousePosition))
                {
                    element.OnKeyUp(e);
                }
            }
        }

        /// <summary>
        /// Applies the given transform to this group.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public override void TransformBounds(Transform transform)
        {
            this.AddingItem = true;

            Rect newBounds = TransformRect(transform, this.Bounds);

            ArrangeChildren(this.Bounds, newBounds);

            base.TransformBounds(transform);

            this.AddingItem = false;
        }

        #endregion

        /// <summary>
        /// Arranges the children.
        /// </summary>
        public virtual void ArrangeChildren(Rect oldBounds, Rect newBounds)
        {
            Transform transfrom = CalculateTransform(newBounds, oldBounds);

            foreach (Groupable groupable in this.Children)
            {
                groupable.ArrangeInGroup(oldBounds, newBounds, transfrom);
            }
        }

        /// <summary>
        /// Calculates the bounds of the given items.
        /// </summary>
        /// <param name="items">Groupable items to calculate the bounds for.</param>
        /// <returns>The bounds of the given items.</returns>
        protected static Rect CalculateBounds(Groupable[] items)
        {
            Rect rect = new Rect(0, 0, 0, 0);

            foreach (VisualElement boundable in items)
            {
                bool isNull = rect.X == 0 && rect.Y == 0 && rect.Width == 0 && rect.Height == 0;

                if (rect.IsEmpty || isNull)
                {
                    rect = boundable.Bounds;
                }
                else
                {
                    rect.Union(boundable.Bounds);
                }
            }

            return rect;
        }
        /// <summary>
        /// Augments this Group's bounds by the bounds of the given item.
        /// </summary>
        /// <param name="item">The item to augment this Group's bounds by.</param>
        protected void AccumulateBounds(VisualElement item)
        {
            Rect rect = this.Bounds;

            bool isNull = rect.X == 0 && rect.Y == 0 && rect.Width == 0 && rect.Height == 0;

            if (rect.IsEmpty || isNull)
            {
                rect = item.Bounds;
            }
            else
            {
                rect.Union(item.Bounds);
            }

            this.Bounds = rect;
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