/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Dev2.Settings.Chatbot
{
    public class ResourceTreeItemViewModel : INotifyPropertyChanged
    {
        private bool _isChecked;
        private bool _isExpanded;
        private string _displayName;
        private ResourceTreeItemViewModel _parent;

        public ResourceTreeItemViewModel()
        {
            Children = new ObservableCollection<ResourceTreeItemViewModel>();
        }

        public Guid ResourceId { get; set; }
        public string ResourceName { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public bool IsFolder { get; set; }

        public string DisplayName
        {
            get => _displayName ?? ResourceName;
            set
            {
                _displayName = value;
                OnPropertyChanged();
            }
        }

        public ResourceTreeItemViewModel Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ResourceTreeItemViewModel> Children { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();

                    // Check/uncheck all children when this item is checked/unchecked
                    if (IsFolder)
                    {
                        CheckAllChildren(value);
                    }

                    // Update parent's checked state
                    Parent?.UpdateCheckedState();
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        private void CheckAllChildren(bool isChecked)
        {
            foreach (var child in Children)
            {
                child.IsChecked = isChecked;
            }
        }

        private void UpdateCheckedState()
        {
            // Update this item's checked state based on children
            if (Children.Count == 0)
            {
                return;
            }

            var checkedChildren = Children.Count(c => c.IsChecked);
            
            if (checkedChildren == 0)
            {
                _isChecked = false;
            }
            else if (checkedChildren == Children.Count)
            {
                _isChecked = true;
            }
            else
            {
                // Partial check - for now, treat as checked
                // In WPF, you could use a nullable bool for tri-state checkbox
                _isChecked = true;
            }

            OnPropertyChanged(nameof(IsChecked));
            Parent?.UpdateCheckedState();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
