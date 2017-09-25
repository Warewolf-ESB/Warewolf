using System;
using Dev2.Common.Interfaces;
using System.Collections.ObjectModel;

namespace Dev2.ViewModels.Merge
{
    public class CompleteConflict : ICompleteConflict
    {
        public CompleteConflict()
        {
            Children = new ObservableCollection<ICompleteConflict>();
        }

        public IMergeToolModel CurrentViewModel { get; set; }
        public IMergeToolModel DiffViewModel { get; set; }
        public ObservableCollection<ICompleteConflict> Children { get; set; }
        public Guid UniqueId { get; set; }
        public bool HasConflict { get; set; }
    }
}
