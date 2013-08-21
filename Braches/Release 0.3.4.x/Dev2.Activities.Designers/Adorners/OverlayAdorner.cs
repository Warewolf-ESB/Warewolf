using System;
using System.Activities.Presentation;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Dev2.Activities.Annotations;
using Dev2.Activities.Designers;
using Dev2.CustomControls.Behavior;
using Dev2.Studio.AppResources.Behaviors;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.UI;
using Dev2.Util.ExtensionMethods;
using System.ComponentModel;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// The adorner used to host the overlay content
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/24</date>
    public sealed class OverlayAdorner : AbstractOverlayAdorner, INotifyPropertyChanged
    {
        #region fields

        private VisualCollection _visuals;
        private Grid _contentGrid;
        private Border _contentBorder;
        private ContentPresenter _contentPresenter;
        private HelpViewModel _helpContent;
        private ScrollViewer _contentScrollViewer;
        private ActualSizeBindingBehavior _actualSizeBindingBehavior;
        private ScrollViewer _helpScrollViewer;
        private ThumbResizeBehavior _thumbResizeBehavior;

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
            var element = adornedElement as Grid;
            if (element == null)
            {
                return;
            }

            CreateContentContainer(colourBorder);
            FocusManager.SetIsFocusScope(this, true);

            element.DataContextChanged += OnElementOnDataContextChanged;
            DataContext = element.DataContext;
            HelpContent = new HelpViewModel();
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

        public HelpViewModel HelpContent
        {
            get
            {
                return _helpContent;
            }
            set
            {
                if (_helpContent == value)
                {
                    return;
                }

                _helpContent = value;
                OnPropertyChanged();
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
            adorner.HelpContent.HelpText = newText;
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
            if (_contentBorder.MinHeight < AdornedElement.RenderSize.Height)
            {
                _contentBorder.MinHeight = AdornedElement.RenderSize.Height + 40;
            }

            //SetFocusToFirstElement();
            //SetCanContentScroll();

            _contentBorder.Visibility = Visibility.Visible;
            _contentBorder.DataContext = DataContext;
        }

        //private void SetCanContentScroll()
        //{
        //    if (Content is CollectionActivityTemplate)
        //    {
        //        if (_contentGrid.Children.Contains(_contentScrollViewer))
        //        {
        //            _contentGrid.Children.Remove(_contentScrollViewer);
        //            _contentScrollViewer.Content = null;
        //            _contentGrid.Children.Add(_contentPresenter);
        //        }

        //    }
        //    else
        //    {
        //        if (!_contentGrid.Children.Contains(_contentScrollViewer))
        //        {
        //            _contentGrid.Children.Remove(_contentPresenter);
        //            _contentScrollViewer.Content = _contentPresenter;
        //            _contentGrid.Children.Add(_contentScrollViewer);
        //        }
        //    }
        //}

        private void SetFocusToFirstElement()
        {
            //var activityTemplate = (ActivityTemplate) Content;
            //var collectionTemplate = activityTemplate as CollectionActivityTemplate;
            //if (collectionTemplate != null)
            //{
            //    var txt = new DataGridFocusTextOnLoadBehavior().GetVisualChild<TextBox>(collectionTemplate.ItemsControl);
            //    if (txt != null)
            //    {
            //        txt.Focus();
            //    }
            //}
        }

        public override void BringToFront()
        {
            var children = _contentBorder.FindVisualChildren<FrameworkElement>();
            children.ToList().ForEach(c => c.BringToMaxFront());
            _contentBorder.BringToFront();
            this.BringToMaxFront();
        }

        public override void SendtoBack()
        {
            var children = _contentBorder.FindVisualChildren<FrameworkElement>();
            children.ToList().ForEach(c => c.SendToBack());
            _contentBorder.SendToBack();
            this.SendToBack();
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
            if (!(content is ActivityTemplate))
            {
                throw new Exception("The user control templates for activities needs to inherit from ActivityTemplate! Please inherit from ActivityTemplate");
            }

            var activityTemplate = (ActivityTemplate) content;

            if (activityTemplate.MinWidth.Equals(0D))
            {
                _contentBorder.MinWidth = 290;
                _contentBorder.Width = 290;
            }
            else
            {
                _contentBorder.MinWidth = activityTemplate.MinWidth;
                _contentBorder.Width = activityTemplate.MinWidth;
            }

            if (!activityTemplate.MaxHeight.Equals(0D))
            {
                _contentBorder.MaxHeight = activityTemplate.MaxHeight + 40;
            }

            if (activityTemplate.MaxWidth.Equals(0D))
            {
                _contentBorder.MaxWidth = 606;
            }
            else
            {
               _contentBorder.MaxWidth = activityTemplate.MaxWidth + 204;               
            }

            if (activityTemplate.MinHeight.Equals(0D))
            {
                _contentBorder.Height = AdornedElement.RenderSize.Height + 5;
            }
            else
            {
                _contentBorder.Height = activityTemplate.MinHeight + 40;
                _contentBorder.MinHeight = activityTemplate.MinHeight + 40;
            }

            if (activityTemplate.HideHelpContent && _helpScrollViewer.Visibility == Visibility.Visible)
            {
                _helpScrollViewer.Visibility = Visibility.Collapsed;
                DecreaseWidth(150);
            }
            else if (!activityTemplate.HideHelpContent && _helpScrollViewer.Visibility == Visibility.Collapsed)
            {
                _helpScrollViewer.Visibility = Visibility.Visible;
                IncreaseWidth(150);
            }
        
            var collectionActivityTemplate = content as CollectionActivityTemplate;
            if (collectionActivityTemplate != null)
            {
                    collectionActivityTemplate.Loaded += (sender, args) =>
                        {
                            var template = collectionActivityTemplate;
                            var itemsControl = template.ItemsControl;
                            if (itemsControl == null)
                            {
                                throw new Exception(
                                    "The user control templates for collection activities needs to contain an itemscontrol for representing the collection");
                            }
                            var widthBinding = new Binding
                                {
                                    Path = new PropertyPath("ActualWidth"),
                                    Source = _actualSizeBindingBehavior
                                };
                            var heightBinding = new Binding
                            {
                                Path = new PropertyPath("ActualHeight"),
                                Source = _actualSizeBindingBehavior
                            };
                            itemsControl.SetBinding(MaxWidthProperty, widthBinding);
                            itemsControl.SetBinding(MaxHeightProperty, heightBinding);                            
                            itemsControl.SetValue(DataGrid.CanUserResizeColumnsProperty, true);
                            var inputElements = itemsControl.FindVisualChildren<IntellisenseTextBox>();
                            inputElements.ToList().ForEach(i => i.SetValue(MaxHeightProperty, 48D));
                            var sizeSyncBehavior = new DataGridColumnSizeSynchronizationBehavior();
                            var focusBehavior = new DataGridFocusTextOnLoadBehavior();
                            Interaction.GetBehaviors(itemsControl).Add(sizeSyncBehavior);
                            Interaction.GetBehaviors(itemsControl).Add(focusBehavior);
                        };
            }

            Content = content;
            //SetCanContentScroll();
            var uiElement = content as FrameworkElement;
            uiElement.AllowDrop = true;
            uiElement.SetValue(AutomationProperties.AutomationIdProperty, contentAutomationID);

            if (uiElement.DataContext == null)
            {
                uiElement.DataContext = DataContext;
            }

                Keyboard.Focus(uiElement);

            ShowContent();

        }

        public override void DecreaseWidth(double width)
        {
            _contentBorder.Width = _contentBorder.Width - width;
            _thumbResizeBehavior.MinWidthOffset -= width;
        }

        public override void IncreaseWidth(double width)
        {
            _contentBorder.Width = _contentBorder.Width + width;
            _thumbResizeBehavior.MinWidthOffset += width;
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
                AllowDrop = true
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

            _contentGrid.RowDefinitions.Add
                (
                    new RowDefinition
                    {
                        Height = GridLength.Auto
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
                        Width = GridLength.Auto
                    }
                );

            //Initialize help viewmodel
            var helpContentControl = new ContentControl
                {
                    Focusable = false
                };

            helpContentControl.SetValue(NameProperty, "HelpContent");
            helpContentControl.SetValue(Grid.RowSpanProperty, 2);
            Caliburn.Micro.Bind.SetModel(helpContentControl, this);

            var doneButton = new Button
                {
                    Content = "Done",
                    Width = 80,
                    Focusable = true,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0,0,5,5)
                };
            doneButton.SetValue(Grid.ColumnSpanProperty, 2);
            doneButton.SetValue(Grid.RowProperty, 1);
            doneButton.SetValue(AutomationProperties.AutomationIdProperty, "DoneButton");

            doneButton.Click += (o, e) =>
                {
                    OnUpdateComplete(new UpdateCompletedEventArgs(true));
                };
            _contentGrid.Children.Add(doneButton);

            var resizeThumb = new Thumb
                {
                    Style = Application.Current.Resources["BottomRightResizeThumbStyle"] as Style,
                };
            resizeThumb.SetValue(Grid.RowProperty, 1);

            _thumbResizeBehavior = new ThumbResizeBehavior();
            _contentPresenter = new ContentPresenter();

            _actualSizeBindingBehavior = new ActualSizeBindingBehavior
            {
                HorizontalOffset = 24,
                VerticalOffset = 50
            };
            Interaction.GetBehaviors(_contentPresenter).Add(_actualSizeBindingBehavior);

            FocusManager.SetIsFocusScope(_contentPresenter, true);

            _thumbResizeBehavior.TargetElement = _contentBorder;
            Interaction.GetBehaviors(resizeThumb).Add(_thumbResizeBehavior);

            _contentScrollViewer = CreateScrollViewer("AdornerScrollViewer", _contentPresenter);
            _contentScrollViewer.CanContentScroll = false;
            _contentScrollViewer.Padding = new Thickness(0);
            _contentScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            _contentScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            _contentGrid.Children.Add(_contentScrollViewer);

            _helpScrollViewer = CreateScrollViewer("AdornerHelpScrollViewer", helpContentControl);
            _helpScrollViewer.SetValue(Grid.ColumnProperty, 1);
            _contentGrid.Children.Add(_helpScrollViewer);

            _contentGrid.Children.Add(resizeThumb);
            _visuals.Add(_contentBorder);
        }

        private ScrollViewer CreateScrollViewer(string automationID, UIElement content)
        {
            var scrollViewer = new ScrollViewer
                {
                    Padding = new Thickness(5),
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = content
                };

            scrollViewer.SetValue(AutomationProperties.AutomationIdProperty, automationID);
            return scrollViewer;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
