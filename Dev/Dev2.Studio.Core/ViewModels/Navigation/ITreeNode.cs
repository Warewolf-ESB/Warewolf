using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.ViewModels.Navigation
{
    /// <summary>
    /// Implemented by all treenodes in the explorer and deploy tabs
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public interface ITreeNode : INotifyPropertyChangedEx, IComparable
    {
        ITreeNode TreeParent { get; set; }
        ObservableCollection<ITreeNode> Children { get; set; }
        IEnvironmentModel EnvironmentModel { get; }
        int ChildrenCount { get; }
        bool? IsChecked { get; set; }
        bool IsOverwrite { get; set; }
        string IconPath { get; }
        bool IsFiltered { get; set; }
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        string FilterText { get; set; }
        bool IsConnected { get; }
        bool IsAuthorized { get; }
        bool IsAuthorizedDeployFrom { get; }
        bool IsAuthorizedDeployTo { get; }
        string DisplayName { get; set; }
        ICollectionView FilteredChildren { get; }
        bool IsNew { get; set; }

        void Add(ITreeNode child);
        void Remove(ITreeNode child);
        ITreeNode FindChild(ITreeNode childToFind);
        ITreeNode FindChild<T>(T resourceToFind);
        void VerifyCheckState();
        void SetIsChecked(bool? value, bool updateChildren, bool updateParent);
        IEnumerable<ITreeNode> GetChildren(Func<ITreeNode, bool> predicate);
        void UpdateFilteredNodeExpansionStates(string filterText);
        //void VerifyFilterState(string filterText);
        void SetFilter(string filterText, bool updateChildren);
        void NotifyOfFilterPropertyChanged(bool updateParent);
        INavigationContext FindRootNavigationViewModel();
    }

    /// <summary>
    /// Interface providing strongly typed access to treenode datacontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Jurie.smit</author>
    /// <date>2/26/2013</date>
    public interface ITreeNode<T> : ITreeNode
    {
        T DataContext { get; set; }
    }
}
