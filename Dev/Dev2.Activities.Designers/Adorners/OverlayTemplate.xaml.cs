using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Caliburn.Micro;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    ///     Interaction logic for OverlayTemplate.xaml
    /// </summary>
    public partial class OverlayTemplate : UserControl
    {
        private readonly Action<bool> _toggleHelp;

        private bool _isHelpTextHidden;


        public OverlayTemplate(UIElement adornedElement, Border colourBorder,
                               OverlayAdorner adorner,
                               Action<bool> toggleHelp, bool isHelpTextHidden)
        {
            _toggleHelp = toggleHelp;
            _isHelpTextHidden = isHelpTextHidden;
            InitializeComponent();
          
            var borderBrushBinding = new Binding
                {
                    Source = colourBorder,
                    Path = new PropertyPath("BorderBrush")
                };

            OuterBorder.SetBinding(Border.BorderBrushProperty, borderBrushBinding);
            Bind.SetModel(HelpContent, adorner);

            ExpandCollapseButtonImage.Source = GetToggleButtonImage();
            ExpandCollapseButtonImage.ToolTip = GetToolTipText();
        }

        private string GetToolTipText()
        {
            return _isHelpTextHidden
                       ? "Show More Information"
                       : "Hide Information";
        }

        private BitmapImage GetToggleButtonImage()
        {
            var logo = new BitmapImage();
            logo.BeginInit();
            string imageSource = _isHelpTextHidden
                                     ? "pack://application:,,,/Dev2.Activities.Designers;component/Images/HelpTextExpander-16.png"
                                     : "pack://application:,,,/Dev2.Activities.Designers;component/Images/HelpTextCollapser-16.png";
            logo.UriSource = new Uri(imageSource);
            logo.EndInit();
            return logo;
        }
        
        private void ExpandCollapseButtonImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isHelpTextHidden = !_isHelpTextHidden;
            ExpandCollapseButtonImage.Source = GetToggleButtonImage();
            ExpandCollapseButtonImage.ToolTip = GetToolTipText();
            _toggleHelp(_isHelpTextHidden);
        }
    }
}