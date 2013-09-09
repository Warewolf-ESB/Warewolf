using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Dev2.Activities.Designers;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// Interaction logic for OverlayTemplate.xaml
    /// </summary>
    public partial class OverlayTemplate
    {
        public const double ResizerWidth = 12 + 4;  // BottomRightResizeThumbStyle.Width + (???? -> manual adjustment so that collapsed resizer column does not overlap content)

        readonly ActivityViewModelBase _activityViewModel;
        readonly double _buttonsContainerHeight;

        public OverlayTemplate(Border colourBorder, OverlayAdorner adorner, ActivityViewModelBase activityViewModel)
        {
            InitializeComponent();

            if(activityViewModel == null)
            {
                throw new ArgumentNullException("activityViewModel");
            }
            _activityViewModel = activityViewModel;

            var borderBrushBinding = new Binding
            {
                Source = colourBorder,
                Path = new PropertyPath("BorderBrush")
            };

            OuterBorder.SetBinding(Border.BorderBrushProperty, borderBrushBinding);
            _buttonsContainerHeight = ButtonsContainer.Height;
            Caliburn.Micro.Bind.SetModel(HelpContent, adorner);
            EnsureHelpVisibility();
          
        }

        public double ActualHelpWidth { get { return ResizerWidth + (IsHelpViewCollapsed ? 0 : Help.HelpView.DefaultWidth); } }

        bool IsHelpViewCollapsed
        {
            get { return _activityViewModel.IsHelpViewCollapsed; }
            set
            {
                if(_activityViewModel.IsHelpViewCollapsed == value)
                {
                    return;
                }
                EnsureHelpVisibility(value);
            }
        }

        public void EnsureHelpVisibility()
        {
            EnsureHelpVisibility(IsHelpViewCollapsed);
        }

        void EnsureHelpVisibility(bool isHelpViewCollapsed)
        {
            _activityViewModel.IsHelpViewCollapsed = isHelpViewCollapsed;
            ThumbResizeBehavior.MinWidthOffset = ActualHelpWidth;
            ThumbResizeBehavior.MinHeightOffset = _buttonsContainerHeight;
            AdornerHelpScrollViewer.Visibility = isHelpViewCollapsed ? Visibility.Collapsed : Visibility.Visible;

            HelpExpandCollapseButtonImage.Source = GetHelpToggleButtonImage();
            HelpExpandCollapseButtonImage.ToolTip = GetHelpToolTipText();

            UpdateContentSize();
        }

        string GetHelpToolTipText()
        {
            return IsHelpViewCollapsed ? "Show More Information" : "Hide Information";
        }

        BitmapImage GetHelpToggleButtonImage()
        {
            var logo = new BitmapImage();
            logo.BeginInit();
            var imageSource = IsHelpViewCollapsed ?
                                  "pack://application:,,,/Dev2.Activities.Designers;component/Images/HelpTextExpander-16.png" :
                                  "pack://application:,,,/Dev2.Activities.Designers;component/Images/HelpTextCollapser-16.png";
            logo.UriSource = new Uri(imageSource);
            logo.EndInit();
            return logo;
        }

        void HelpExpandCollapseButtonImageOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsHelpViewCollapsed = !IsHelpViewCollapsed;
        }

        public void UpdateContentSize()
        {
            var content = ContentPresenter.Content as FrameworkElement;
            if(content == null)
            {
                return;
            }
            var minHeight = content.MinHeight + _buttonsContainerHeight;
            var minWidth = content.MinWidth + ActualHelpWidth;

            OuterBorder.Height = double.IsNaN(OuterBorder.Height) ? minHeight : Math.Max(OuterBorder.Height, minHeight);
            OuterBorder.Width = double.IsNaN(OuterBorder.Width) ? minWidth : Math.Max(OuterBorder.Width, minWidth);
        }
    }
}
