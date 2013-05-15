using System.Windows.Controls;
using Unlimited.Applications.BusinessDesignStudio;
using System.Windows.Input;
using System.ComponentModel.Composition;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core;

namespace CircularDependencyTool
{
    public partial class GraphView : UserControl
    {

        public GraphView()
        {
            InitializeComponent();            
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string resourceName = string.Empty;
            Border border = sender as Border;
            if (border != null)
            {
                if (border.DataContext.GetType() == typeof(Node))
                {
                    Node node = border.DataContext as Node;
                    resourceName = node.ID;
                }
            }

            if (!string.IsNullOrEmpty(resourceName))
            {
                //IResourceModel resource = MainViewModel.ResourceRepository.FindSingle(c => c.ResourceName == resourceName);
                //if (resource != null)
                //{
                //    Mediator.SendMessage(MediatorMessages.AddWorkflowDesigner, resource);
                //}
            }
        }   
    }
}