using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using System;

namespace Dev2.ViewModels.Merge
{
    public abstract class ConflictRow : BindableBase, IConflictRow, IConflictCheckable
    {
        public abstract bool IsEmptyItemSelected { get; set; }
        public bool HasConflict => !Current.Equals(Different);
        public abstract bool IsChecked { get; set; }
        public abstract Guid UniqueId { get; set; }
        public abstract IConflictItem Current { get; }
        public abstract IConflictItem Different { get; }


        protected ConflictRow()
        {
        }

        public bool IsCurrentChecked
        {
            get => Current.IsChecked;
            set => Current.IsChecked = value;
        }
    }
}
