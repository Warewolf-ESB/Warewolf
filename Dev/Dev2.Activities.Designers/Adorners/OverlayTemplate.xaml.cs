using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Dev2.Activities.Designers;
using Dev2.Activities.Help;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    ///     Interaction logic for OverlayTemplate.xaml
    /// </summary>
    public partial class OverlayTemplate
    {
        readonly ActivityViewModelBase _activityViewModel;
       
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
            Bind.SetModel(HelpContent, adorner);
            HelpContent.Width = HelpView.DefaultWidth;
            ToggleHelpVisibility();
        }

        bool IsHelpViewCollapsed
        {
            get
            {
                return _activityViewModel.IsHelpViewCollapsed;
            } 
            set
            {
                _activityViewModel.IsHelpViewCollapsed = value;
            }
        }

        private void ToggleHelpVisibility()
        {
            HelpContent.Visibility = IsHelpViewCollapsed ? Visibility.Collapsed : Visibility.Visible;
            HelpExpandCollapseButtonImage.Source = GetHelpToggleButtonImage();
            HelpExpandCollapseButtonImage.ToolTip = GetHelpToolTipText();
        }

        string GetHelpToolTipText()
        {
            return IsHelpViewCollapsed ? "Show More Information" : "Hide Information";
        }

        BitmapImage GetHelpToggleButtonImage()
        {
            var logo = new BitmapImage();
            logo.BeginInit();
            string imageSource = IsHelpViewCollapsed ?
                                     "pack://application:,,,/Dev2.Activities.Designers;component/Images/HelpTextExpander-16.png" :
                                     "pack://application:,,,/Dev2.Activities.Designers;component/Images/HelpTextCollapser-16.png";
            logo.UriSource = new Uri(imageSource);
            logo.EndInit();
            return logo;
        }

        void HelpExpandCollapseButtonImageOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsHelpViewCollapsed = !IsHelpViewCollapsed;
            ToggleHelpVisibility();
        }
    }
}