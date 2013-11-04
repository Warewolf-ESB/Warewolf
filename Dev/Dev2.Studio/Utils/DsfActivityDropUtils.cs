using System;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Explorer;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.Views.Workflow;

namespace Dev2.Studio.Utils
{
    public class DsfActivityDropUtils
    {
        
        public static bool TryPickResource(string typeName, out DsfActivityDropViewModel result)
        {
            // PBI 10652 - 2013.11.04 - TWR - Refactored from WorkflowDesignerViewModel.ViewPreviewDrop to enable re-use!
            result = DetermineDropActivityType(typeName);

            if(result != null)
            {
                result.Init();
                if(DoDroppedActivity(result))
                {
                    return true;
                }
            }
            return false;
        }

        static bool DoDroppedActivity(DsfActivityDropViewModel viewModel)
        {
            DsfActivityDropWindow dropWindow = new DsfActivityDropWindow();

            dropWindow.DataContext = viewModel;
            dropWindow.ShowDialog();
            var response = viewModel.DialogResult;
            if (response == ViewModelDialogResults.Okay)
            {
                return true;
            }

            return false;
        }

        static DsfActivityDropViewModel DetermineDropActivityType(string typeOfResource)
        {
            DsfActivityDropViewModel vm = null;
            if (typeOfResource.Contains("DsfWorkflowActivity"))
            {
                ExplorerViewModel explorer = new ExplorerViewModel(true, enDsfActivityType.Workflow);
                vm = new DsfActivityDropViewModel(explorer, enDsfActivityType.Workflow);
            }
            else if (typeOfResource.Contains("DsfServiceActivity"))
            {
                ExplorerViewModel explorer = new ExplorerViewModel(true, enDsfActivityType.Service);
                vm = new DsfActivityDropViewModel(explorer, enDsfActivityType.Service);
            }
            return vm;
        }        
    }
}
