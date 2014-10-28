
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
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


        void TreeViewDrop(object sender, DragEventArgs e)
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
                            if (ShouldNotMove(source, destination))
                            {
                                e.Handled = true;
                            }
                            else
                            {
                                MoveItem(source, destination, rep);
                            }
                        }
                        finally { e.Handled = true; }
                    }
                }
            }
        }

        public static bool ShouldNotMove(IExplorerItemModel source, IExplorerItemModel destination)
        {
            if(source != null && (source == destination || destination.IsVersion || source.IsVersion || source.ResourcePath.Equals(destination.ResourcePath, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return false;
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
