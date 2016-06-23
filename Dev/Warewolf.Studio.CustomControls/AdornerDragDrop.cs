using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Warewolf.Studio.CustomControls
{
    public class AdornerDragDrop : Adorner
    {
        readonly UIElement _elementToShow; // the element to show on screen
        Point _position;//the position where to draw the element

        //constructor to drag and drop
        public AdornerDragDrop(UIElement element, UIElement elementToShow)
            : base(element)
        {
            _elementToShow = CreateClone(elementToShow);
        }

        #region Force Layout system
        //make sure that the layout system knows of the element
        protected override Size MeasureOverride(Size constraint)
        {
            _elementToShow.Measure(constraint);
            return constraint;
        }

        //make sure that the layout system knows of the element
        protected override Size ArrangeOverride(Size finalSize)
        {
            _elementToShow.Arrange(new Rect(finalSize));
            return finalSize;
        }
        #endregion

        #region Force the visual to show
        //return the visual that we want to display
        protected override Visual GetVisualChild(int index)
        {
            return _elementToShow;
        }

        //return the count of the visuals
        protected override int VisualChildrenCount => 1;

        //moves the visual around
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var group = new GeneralTransformGroup();
            group.Children.Add(transform);
            group.Children.Add(new TranslateTransform(_position.X, _position.Y));
            return group;
        }
        #endregion

        //updates the position of the adorner
        public void UpdatePosition(Point point)
        {
            _position = point;
            var parentLayer = Parent as AdornerLayer;
            if (parentLayer != null)
                parentLayer.Update(AdornedElement);
        }

        #region Helpers
        //create a clone of the element being dragged
        private static ContentControl CreateClone(UIElement element)
        {
            var control = new ContentControl();
            var element1 = element as ContentControl;
            if (element1 != null)
            {
                control.Content = element1.Content;
                control.ContentTemplate = element1.ContentTemplate;
            }
            var element2 = element as ContentPresenter;
            if (element2 != null)
            {
                control.Content = element2.Content;
                control.ContentTemplate = element2.ContentTemplate;
            }

            return control;
        }
        #endregion

    }
}
