#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;

#endregion

namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    /// Implemented by all treenodes in the explorer and deploy tabs
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public interface ITreeNode : INotifyPropertyChangedEx, IComparable
    {
        ITreeNode TreeParent { get; set; }
        ObservableCollection<ITreeNode> Children { get; }
        IEnvironmentModel EnvironmentModel { get; }
        int ChildrenCount { get; }
        bool? IsChecked { get; set; }
        string IconPath { get; }
        bool IsFiltered { get; set; }
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        bool IsConnected { get; }
        string DisplayName { get; set; }

        void Add(ITreeNode child);
        bool Remove(ITreeNode child);
        ITreeNode FindChild(ITreeNode childToFind);
        ITreeNode FindChild<T>(T resourceToFind);
        void VerifyCheckState();
        void SetIsChecked(bool? value, bool updateChildren, bool updateParent, bool sendMessage);
        void SetFilter(string value);
        IEnumerable<ITreeNode> GetChildren(Func<ITreeNode, bool> predicate);

        //
        //Juries 2013/01/21 Do not remove - to use in combination with filteredcollection when needed
        //
        //ICollectionView FilteredChildren { get; } 
    }
}
