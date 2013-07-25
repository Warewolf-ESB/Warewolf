using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Shapes;
using Dev2.Activities.Designers;
using Dev2.CustomControls.Behavior;
using Dev2.CustomControls.Converters;
using Dev2.Util.ExtensionMethods;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// The container hosting the different adorner options (ie buttons)
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/24</date>
    public class OptionsAdorner : AbstractOptionsAdorner
    {
        #region fields

        private readonly Panel _rootGrid;
        private readonly IActivityDesigner _activityDesigner;
        private readonly MathFunctionDoubleToThicknessConverter _marginConverter;
        private readonly MathFunctionDoubleToThicknessConverter _paddingConverter;

        #endregion fields

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsAdorner"/> class.
        /// </summary>
        /// <param name="adornedElement">The adorned element.</param>
        /// <param name="overlaySizeBindingBehavior">The overlay size binding behavior.</param>
        /// <param name="titleBorder">The title border - used to determine the background/borderbrush.</param>
        /// <param name="displayNameWidthSetter">The display name width setter - used to determine the padding/margin.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public OptionsAdorner(UIElement adornedElement, ActualSizeBindingBehavior overlaySizeBindingBehavior, 
            Border titleBorder, Rectangle displayNameWidthSetter) :
            base(adornedElement)
        {
            _activityDesigner = adornedElement as IActivityDesigner;

            if (_activityDesigner == null)
            {
                return;
            }

            //Instantiate a behavior to keep track of the actual activity size
            //Cant bind to ActualWidth seeing that it as a dependency property
            var actualSizeBindingBehavior = new ActualSizeBindingBehavior
            {
                HorizontalOffset = 1
            };
            Interaction.GetBehaviors(adornedElement).Add(actualSizeBindingBehavior);

            //Use container grid to stretch complete top section of activity, and set this as root
            _rootGrid = new Grid
                {
                    Focusable = false
                };
            AddVisualChild(_rootGrid);

            //then insert a border that only fills the top right hand side
            var border = new Border
            {
                Height = 22,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                BorderThickness = new Thickness(0, 1, 1, 1),
                Focusable = false
            };
            _rootGrid.Children.Add(border);

            //add the actual container that hosts the children buttons
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Focusable = false
            };
            border.Child = stackPanel;

            //add a behavior to keep track of the width of the displayname
            var displayNameBindingBehavior = new ActualSizeBindingBehavior();
            Interaction.GetBehaviors(displayNameWidthSetter).Add(displayNameBindingBehavior);

            //add the bindings to keep the size of the title consistent when opening/closing adorners
            if (overlaySizeBindingBehavior != null)
            {
                //use two seperate converters because their offsets differ
                _marginConverter = new MathFunctionDoubleToThicknessConverter();
                _paddingConverter = new MathFunctionDoubleToThicknessConverter();

                var marginBinding = ActualSizeBindingBehavior.GetWidthMultiBinding(displayNameBindingBehavior, null, _marginConverter);
                _rootGrid.SetBinding(MarginProperty, marginBinding);

                var paddingBinding = ActualSizeBindingBehavior.GetWidthMultiBinding(actualSizeBindingBehavior, overlaySizeBindingBehavior, _paddingConverter);
                border.SetBinding(Border.PaddingProperty, paddingBinding);
            }

            //add the binding to keep the background colour in sync
            var backgroundBinding = new Binding
            {
                Source = titleBorder,
                Path = new PropertyPath("Background")
            };
            border.SetBinding(Border.BackgroundProperty, backgroundBinding);

            //add the binding to keep the border colour in sync
            var borderBrushBinding = new Binding
            {
                Source = titleBorder,
                Path = new PropertyPath("BorderBrush")
            };
            border.SetBinding(Border.BorderBrushProperty, borderBrushBinding);

            //attach appropriate events
            border.MouseDown += (o, e) => OnMouseDown(e);
            stackPanel.MouseLeave += (o, e) => OnMouseLeave(e);
            stackPanel.SizeChanged += (o, e) => UpdateOffsets(stackPanel, displayNameWidthSetter);
            displayNameWidthSetter.SizeChanged += (o, e) => UpdateOffsets(stackPanel, displayNameWidthSetter);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the border which is the first child of the rootgrid
        /// </summary>
        /// <value>
        /// The border.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        private Border Border
        {
            get
            {
                return (Border)_rootGrid.Children[0];
            }
        }

        /// <summary>
        /// Gets the actual button container which is a stackpanel.
        /// </summary>
        /// <value>
        /// The button container.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        private Panel ButtonContainer
        {
            get
            {
                return (Panel)(Border).Child;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Adds a button to the adorner options
        /// </summary>
        /// <param name="button">The button to add.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override void AddButton(ButtonBase button)
        {
            button.SetValue(DockPanel.DockProperty, Dock.Right);
            ButtonContainer.Children.Add(button);

            //These events control the dragging of an activity when the user clicks and drags on one of the buttons
            button.PreviewMouseMove += (o, e) => OnPreviewMouseMove(e);
            button.PreviewMouseLeftButtonDown += (o, e) => OnPreviewMouseLeftButtonDown(e);

            if (!(button is AdornerToggleButton))
            {
                return;
            }

            //If it is a toggle button we also need to react to the checked/unchcked events
            //to manage the other toggles and toggled content
            var btn = (AdornerToggleButton)button;
            btn.Checked += OnButtonChecked;
            btn.Unchecked += OnButtonUnchecked;
        }

        /// <summary>
        /// Removes the button.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override void RemoveButton(ButtonBase button)
        {
            ButtonContainer.Children.Remove(button);
        } 
        #endregion

        #region public overrides

        /// <summary>
        /// Hides the content of the adorner.
        /// </summary>
        /// <date>2013/07/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override void HideContent()
        {
            Border.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the content of the adorner.
        /// </summary>
        /// <date>2013/07/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override void ShowContent()
        {
            Border.Visibility = Visibility.Visible;
            Border.BringToFront();
        }

        #endregion

        #region protected overrides
        /// <summary>
        /// Gets the visual children count for this <see cref="OptionsAdorner" />.
        /// As the <see cref="OptionsAdorner" /> can only host one control to render
        /// the input hint, the visual children count should be either one or zero.
        /// </summary>
        /// <returns>The number of visual child elements for this element.</returns>
        protected override int VisualChildrenCount
        {
            get
            {
                return (_rootGrid == null ? 0 : 1);
            }
        }

        /// <summary>
        /// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)" />, 
        /// and returns a child at the specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>
        /// The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
        /// </returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override Visual GetVisualChild(int index)
        {
            return _rootGrid;
        }

        /// <summary>
        /// Measures the size for this <see cref="ActivityAdorner"/> to render.
        /// </summary>
        /// <param name="size"></param>
        /// <returns>
        /// The <see cref="Size"/> required for this <see cref="ActivityAdorner"/>.
        /// If the internal <see cref="Control"/> has not been initialized,
        /// zero size will be returned.
        /// </returns>
        protected override Size MeasureOverride(Size size)
        {
            var child = GetVisualChild(0) as UIElement;
            if (child != null)
            {
                var childConstraint = new Size(Double.PositiveInfinity, Double.PositiveInfinity);
                child.Measure(childConstraint);
                return child.DesiredSize;
            }

            return new Size(0, 0);
        }

        /// <summary>
        /// Arranges the final size for current <see cref="ActivityAdorner"/> before rendering.
        /// </summary>
        /// <param name="size"></param>
        /// <returns>
        /// The final <see cref="Size"/> arranged for this <see cref="ActivityAdorner"/>
        /// </returns>
        protected override Size ArrangeOverride(Size size)
        {
            var visualChild = GetVisualChild(0) as UIElement;
            if (visualChild != null)
            {
                visualChild.Arrange(new Rect(visualChild.DesiredSize));
                return size;
            }

            return new Size();
        }

        #endregion overrides

        #region event handlers

        /// <summary>
        /// Updates the offsets of the container hosting the buttons
        /// </summary>
        /// <param name="stackPanel">The stack panel.</param>
        /// <param name="titleRectangle">The title rectangle.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        private void UpdateOffsets(FrameworkElement stackPanel, FrameworkElement titleRectangle)
        {
            _marginConverter.Offset = 0;
            _paddingConverter.Offset = titleRectangle.ActualWidth + stackPanel.ActualWidth;
            UpdateSize();
        }

        /// <summary>
        /// Called when [button unchecked].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        private void OnButtonUnchecked(object sender, RoutedEventArgs e)
        {
            NotifyOfSelection();
            e.Handled = true;
        }

        /// <summary>
        /// Called when a button is checked. unchecks the other buttons
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        private void OnButtonChecked(object sender, RoutedEventArgs e)
        {
            //uncheck all the other adorner togglebutton when one is checked
            ButtonContainer.Children.OfType<AdornerToggleButton>().ToList().ForEach(toggle =>
            {
                if (!ReferenceEquals(toggle, sender))
                {
                    toggle.IsChecked = false;
                }
            });
            NotifyOfSelection();
            e.Handled = true;
        }
        #endregion

        #region private helpers 

        /// <summary>
        /// Updates the size of the container hosting the buttons.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        private void UpdateSize()
        {
            //the size is controlled by setting the margin from the left hands side and setting the padding
            var bindingExpression = BindingOperations.GetBindingExpressionBase(_rootGrid, MarginProperty);
            if (bindingExpression != null)
            {
                bindingExpression.UpdateTarget();
            }

            bindingExpression = BindingOperations.GetBindingExpressionBase(Border, Border.PaddingProperty);
            if (bindingExpression != null)
            {
                bindingExpression.UpdateTarget();
            }
        }

        /// <summary>
        /// Notifies listeners that the selection has changes.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        private void NotifyOfSelection()
        {
            var selectedButton = ButtonContainer.Children.OfType<AdornerToggleButton>()
                .FirstOrDefault(b => b.IsChecked == true);
            OnSelectionChanged(new ButtonSelectionChangedEventArgs(selectedButton));
        }

        #endregion private helpers

    }
}
