using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Deploy
{
    public interface IDeploySourceExplorerViewModel:IExplorerViewModel
    {
        /// <summary>
        /// root and all children of selected items
        /// </summary>
        ICollection<IExplorerTreeItem> SelectedItems { get; set; }
        IEnumerable<IExplorerTreeItem> Preselected { get; set; }

        void SelectItemsForDeploy(System.Collections.IEnumerable selectedItems);
        Version ServerVersion { get; }
    }
}