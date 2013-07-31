using System;
using System.Activities.Presentation;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Dev2.Activities.Designers;
using Dev2.Studio.AppResources.Behaviors;
using Dev2.Util.ExtensionMethods;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// The adorner used to host the overlay content
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/24</date>
    public class OverlayAdorner : AbstractOverlayAdorner
    {
        #region fields

        private VisualCollection _visuals;
        private Grid _contentGrid;
        private Border _contentBorder;
        private ContentPresenter _contentPresenter;
        private readonly HelpViewModel HelpViewModel;

        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="OverlayAdorner"/> class.
        /// </summary>
        /// <param name="adornedElement">The adorned element.</param>
        /// <param name="colourBorder">The colour border.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public OverlayAdorner(UIElement adornedElement, Border colourBorder)
            : base(adornedElement)
        {
            var element = adornedElement as ActivityDesigner;
            if (element == null)
            {
                return;
            }

            CreateContentContainer(colourBorder);
            FocusManager.SetIsFocusScope(this, true);

            element.DataContextChanged += OnElementOnDataContextChanged;
            DataContext = element.DataContext;
            HelpViewModel = new HelpViewModel();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OverlayAdorner"/> class.
        /// </summary>
        /// <param name="adornedElement">The adorned element.</param>
        /// <param name="content">The content.</param>
        /// <param name="colourBorder">The colour border.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public OverlayAdorner(UIElement adornedElement, Visual content, Border colourBorder)
            : this(adornedElement, colourBorder)
        {
            Content = content;
        }

        #endregion ctor

        #region public properties

        /// <summary>
        /// Gets or sets the content being displayed by this adorner
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override object Content
        {
            get
            {
                return _contentPresenter.Content;
            }
            protected set
            {
                _contentPresenter.Content = value;
            }
        }

        #endregion

        #region dependency properties

        public override string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register("HelpText", typeof(string), 
            typeof(OverlayAdorner), new PropertyMetadata(string.Empty, HelpTextChangedCallback));

        private static void HelpTextChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var adorner = (OverlayAdorner) o;
            var newText = (string) args.NewValue;
            adorner.HelpViewModel.HelpText = newText;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Hides the content of the adorner.
        /// </summary>
        /// <date>2013/07/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override void HideContent()
        {
            _contentBorder.Visibility = Visibility.Collapsed;
            _contentBorder.BringToFront();
        }

        /// <summary>
        /// Shows the content of the adorner.
        /// </summary>
        /// <date>2013/07/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override void ShowContent()
        {
            _contentBorder.Visibility = Visibility.Visible;
            _contentBorder.DataContext = DataContext;
        }

        /// <summary>
        /// Changes the content of the adorner, and makes it visible.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentAutomationID">The content automation ID.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override void ChangeContent(object content, string contentAutomationID)
        {
            Content = content;
            var uiElement = content as FrameworkElement;
            if (uiElement != null)
            {
                uiElement.SetValue(AutomationProperties.AutomationIdProperty, contentAutomationID);
                uiElement.DataContext = DataContext;
                Keyboard.Focus(uiElement);
            }
            ShowContent();
        }

        #endregion

        #region protected overrides

        /// <summary>
        /// Implements any custom measuring behavior for the adorner.
        /// </summary>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        /// <returns>
        /// A <see cref="T:System.Windows.Size" /> object representing the amount of layout space needed by the adorner.
        /// </returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override Size MeasureOverride(Size constraint)
        {
            _contentBorder.Measure(constraint);
            return _contentBorder.DesiredSize;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement" /> derived class.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>
        /// The actual size used.
        /// </returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override Size ArrangeOverride(Size finalSize)
        {
            _contentBorder.Arrange(new Rect(0, 22,
                 finalSize.Width, finalSize.Height));
            return _contentBorder.RenderSize;
        }

        /// <summary>
        /// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)" />, and returns a child at the specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>
        /// The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
        /// </returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override Visual GetVisualChild(int index)
        {
            return _visuals[index];
        }

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        /// <returns>The number of visual child elements for this element.</returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override int VisualChildrenCount
        {
            get
            {
                return _visuals.Count;
            }
        }

        #endregion

        #region event handlers

        private void OnElementOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
           DataContext = args.NewValue;
        }

        #endregion

        #region private helpers

        /// <summary>
        /// Creates the content container, and sets the appropriate properties.
        /// </summary>
        /// <param name="colourBorder">The colour border.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        private void CreateContentContainer(Border colourBorder)
        {
            _visuals = new VisualCollection(this);

            _contentBorder = new Border
            {
                BorderThickness = new Thickness(1,0,1,1),
                Background = new SolidColorBrush(Colors.White),
                MinHeight = AdornedElement.RenderSize.Height,
                MinWidth = AdornedElement.RenderSize.Width
            };
            var borderBrushBinding = new Binding
            {
                Source = colourBorder,
                Path = new PropertyPath("BorderBrush")
            };
            _contentBorder.SetBinding(Border.BorderBrushProperty, borderBrushBinding);

            _contentGrid = new Grid();
            _contentBorder.Child = _contentGrid;

            _contentGrid.RowDefinitions.Add
                (
                    new RowDefinition
                        {
                            Height = new GridLength(1, GridUnitType.Star)
                        }
                );
            _contentGrid.ColumnDefinitions.Add
                (
                    new ColumnDefinition
                        {
                            Width = new GridLength(1, GridUnitType.Star)
                        }
                );
            _contentGrid.ColumnDefinitions.Add
                (
                    new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Auto)
                    }
                );

            //Initialize hlep viewmodel
            var helpContentControl = new ContentControl {Content = HelpViewModel};
            helpContentControl.SetValue(Grid.ColumnProperty, 1);

            if(Application.Current != null)
            {
                var resizeThumb = new Thumb
                {
                    Style = Application.Current.Resources["BottomRightResizeThumbStyle"] as Style
                };

                var resizeBehavior = new ThumbResizeBehavior();
                _contentPresenter = new ContentPresenter();

                FocusManager.SetIsFocusScope(_contentPresenter, true);

                var scrollViewer = new ScrollViewer
                {
                    Padding = new Thickness(5),
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = _contentPresenter,             
                };

                scrollViewer.SetValue(AutomationProperties.AutomationIdProperty, "AdornerScrollViewer");

                resizeBehavior.TargetElement = _contentBorder;
                Interaction.GetBehaviors(resizeThumb).Add(resizeBehavior);
                _contentGrid.Children.Add(scrollViewer);
                _contentGrid.Children.Add(resizeThumb);
            }
            _contentGrid.Children.Add(helpContentControl);

            _visuals.Add(_contentBorder);
        }

        #endregion
    }
}
