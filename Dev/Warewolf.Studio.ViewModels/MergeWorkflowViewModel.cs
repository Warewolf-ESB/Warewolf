using System.Windows.Media;
using Dev2.Common.Interfaces;
using System.Windows;

namespace Warewolf.Studio.ViewModels
{
    public class MergeWorkflowViewModel : IMergeWorkflowViewModel
    {
        public MergeWorkflowViewModel()
        {
            Icon = Application.Current.TryFindResource("Data-Assign-Icon") as DrawingBrush;
        }

        public DrawingBrush Icon { get; set; }
    }
}
