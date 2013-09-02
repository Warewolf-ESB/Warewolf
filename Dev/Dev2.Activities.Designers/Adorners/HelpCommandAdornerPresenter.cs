using Dev2.Activities.Designers;
using Dev2.Services.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Utils;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Dev2.Activities.Adorners
{
    public class HelpCommandAdornerPresenter : AdornerPresenterBase
    {
        private ModelItem _modelItem;
        public HelpCommandAdornerPresenter(ModelItem modelItem)
        {
            VerifyArgument.IsNotNull("modelItem", modelItem);
            _modelItem = modelItem;
            ImageSourceUri =
                "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png";
            OverlayType = OverlayType.Help;
            ToolTip = "See an example in a workflow";

            CommandManager.RegisterClassCommandBinding(typeof(ActivityDesignerBase),
                  new CommandBinding(ApplicationCommands.Help,
                  (x, y) => WorkflowDesignerUtils.ShowExampleWorkflow(_modelItem.ItemType.Name, ServerUtil.GetLocalhostServer().Environment, null)));
        }

        private void SetToggleButton(AdornerToggleButton helpButton)
        {
            _button = helpButton;
            _button.Content = new Image
            {
                Source = new BitmapImage(new Uri(ImageSourceUri, UriKind.RelativeOrAbsolute))
            };
            _button.ToolTip = ToolTip;
            _button.Click += OnButtonClick;
            AutomationProperties.SetAutomationId(_button, "AdornerHelpButton");
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            WorkflowDesignerUtils.ShowExampleWorkflow(_modelItem.ItemType.Name, ServerUtil.GetLocalhostServer().Environment, null);
        }

        private AdornerToggleButton _button;
        public override ButtonBase Button { 
            get 
            {
                if (_button == null) SetToggleButton(new AdornerToggleButton());
                return _button;
            }
        }
    }
}
