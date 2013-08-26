using Dev2.Activities.Designers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// Interaction logic for OverlayTemplate.xaml
    /// </summary>
    public partial class OverlayTemplate : UserControl
    {
        Action<UpdateCompletedEventArgs> _donebuttonClick;
        Action<bool> _toggleHelp;

        private bool _isHelpTextHidden;


        public OverlayTemplate(UIElement adornedElement, Border colourBorder,
                               OverlayAdorner adorner, Action<UpdateCompletedEventArgs> doneButtonClick,
                               Action<bool> toggleHelp, bool isHelpTextHidden)
        {
            _donebuttonClick = doneButtonClick;
            _toggleHelp = toggleHelp;
            _isHelpTextHidden = isHelpTextHidden;
            InitializeComponent();
            //OuterBorder.MinHeight = adornedElement.RenderSize.Height;

            var borderBrushBinding = new Binding
            {
                Source = colourBorder,
                Path = new PropertyPath("BorderBrush")
            };

            OuterBorder.SetBinding(Border.BorderBrushProperty, borderBrushBinding);
            Caliburn.Micro.Bind.SetModel(HelpContent, adorner);

            ExpandCollapseButtonImage.Source = GetToggleButtonImage();
            ExpandCollapseButtonImage.ToolTip = GetToolTipText();
        }

        private string GetToolTipText()
        {
            return _isHelpTextHidden ?
                "Show More Information" :
                "Hide Information";
        }

        private BitmapImage GetToggleButtonImage()
        {
            var logo = new BitmapImage();
            logo.BeginInit();
            string imageSource = _isHelpTextHidden ?
                    "pack://application:,,,/Dev2.Activities.Designers;component/Images/HelpTextExpander-16.png" :
                    "pack://application:,,,/Dev2.Activities.Designers;component/Images/HelpTextCollapser-16.png";
            logo.UriSource = new Uri(imageSource);
            logo.EndInit();
            return logo;
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            _donebuttonClick(new UpdateCompletedEventArgs(true));
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
