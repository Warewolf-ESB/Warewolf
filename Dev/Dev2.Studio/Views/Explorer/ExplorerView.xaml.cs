using System.Windows;
using System.Windows.Controls;
using Dev2.AppResources.Repositories;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Models;

// ReSharper disable once CheckNamespace


namespace Dev2.Studio.Views.Explorer
{
    /// <summary>
    /// Interaction logic for ExplorerView.xaml
    /// </summary>
    public partial class ExplorerView
    {
        public ExplorerView()
        {
            InitializeComponent();
        }


        void TreeViewDrop(object sender, System.Windows.DragEventArgs e)
        {
            TreeViewItem t = sender as TreeViewItem;
            IStudioResourceRepository rep = StudioResourceRepository.Instance;
            if(t != null)
            {
                ExplorerItemModel destination = t.Header as ExplorerItemModel;
                if (destination != null)
                {
                    var dataObject = e.Data;
                    if (dataObject != null && dataObject.GetDataPresent(GlobalConstants.ExplorerItemModelFormat))
                    {
                        var explorerItemModel = dataObject.GetData(GlobalConstants.ExplorerItemModelFormat);
                        try
                        {
                            ExplorerItemModel source = explorerItemModel as ExplorerItemModel;
                            MoveItem(source, destination, rep);
                        }
                        finally { e.Handled = true; }
                    }
                }
            }
        }

        public static void MoveItem(ExplorerItemModel source, ExplorerItemModel destination, IStudioResourceRepository rep)
        {
            if (source != null)
            {
                if(source.EnvironmentId != destination.EnvironmentId)
                {
                    var popup = CustomContainer.Get<IPopupController>();
                    popup.Description = "You are not allowed to move items between Servers using the explorer. Please use the deploy instead";
                    popup.Buttons = MessageBoxButton.OK;
                    popup.Header = "Invalid Action";
                    popup.ImageType = MessageBoxImage.Error;
                    popup.Show();

                }
                else
                switch (destination.ResourceType)
                {
                    case ResourceType.Folder:
                    case ResourceType.Server:
                        rep.MoveItem(source, destination.ResourcePath);
                        break;
                    default:
                        rep.MoveItem(source, destination.Parent.ResourcePath);
                        break;
                }
            }
        }
       
    }
}
