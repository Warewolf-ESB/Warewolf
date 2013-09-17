using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.Activities.QuickVariableInput;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers
{
    public abstract class ActivityCollectionViewModelBase : ActivityViewModelBase, IActivityCollectionViewModel
    {
        protected abstract string CollectionName { get; }

        protected ActivityCollectionViewModelBase(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public abstract void AddListToCollection(IEnumerable<string> listToAdd, bool overwrite);
    }

    public abstract class ActivityCollectionViewModelBase<TDev2TOFn> : ActivityCollectionViewModelBase
        where TDev2TOFn : class, IDev2TOFn, new()
    {
        private ObservableCollection<TDev2TOFn> _items;

        protected ActivityCollectionViewModelBase(ModelItem modelItem)
            : base(modelItem)
        {
            InitializeItems();
            InitializeQuickVariableInput();
        }

        #region fields

        private int _selectedIndex = -1;
        private object _selectedValue;
        private QuickVariableInputViewModel _quickVariableInputViewModel;

        #endregion fields


        public QuickVariableInputViewModel QuickVariableInputViewModel
        {
            get
            {
                return _quickVariableInputViewModel;
            }
            set
            {
                if(_quickVariableInputViewModel == value)
                {
                    return;
                }

                _quickVariableInputViewModel = value;
                NotifyOfPropertyChange(() => QuickVariableInputViewModel);
            }
        }
        public object SelectedValue
        {
            get
            {
                return _selectedValue;
            }
            set
            {
                if(_selectedValue == value)
                {
                    return;
                }

                _selectedValue = value;
                NotifyOfPropertyChange(() => SelectedValue);
            }
        }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if(_selectedIndex == value)
                {
                    return;
                }
                _selectedIndex = value;                
                NotifyOfPropertyChange(() => SelectedIndex);
            }
        }

        public int CountRows()
        {
            if(Items != null)
            {
                return Items.Count();
            }
            return 0;
        }

        public ObservableCollection<TDev2TOFn> Items
        {
            get { return _items; }
        }

        public override void OnModelItemChanged(ModelItem newItem)
        {
            base.OnModelItemChanged(newItem);
            InitializeItems();
            InitializeQuickVariableInput();
        }


        private void InitializeQuickVariableInput()
        {
            QuickVariableInputViewModel = new QuickVariableInputViewModel(this);
        }

        private void InitializeItems()
        {
            var items = ModelItem.GetProperty(CollectionName) as IList<TDev2TOFn>;
            if(items == null)
            {
                _items= new ObservableCollection<TDev2TOFn>();
                return;
            }

            _items = new ObservableCollection<TDev2TOFn>(items);
            _items.CollectionChanged += (o, e) =>
                {
                    if(e.OldItems != null)
                    {
                        e.OldItems.Cast<TDev2TOFn>().ToList().ForEach(i =>
                            {
                                if(items.Contains(i))
                                {
                                    items.Remove(i); 
                                }
                            });
                    }
                    if(e.NewItems != null)
                    {
                        e.NewItems.Cast<TDev2TOFn>().ToList().ForEach(i =>
                            {
                                if(!items.Contains(i))
                                {
                                    items.Add(i); 
                                }
                            });
                    }
                    ModelProperty collection = ModelItem.Properties[CollectionName];
                    if(collection != null)
                    {
                        collection.SetValue(Items);
                    }
                };

            if(_items.Count <= 0)
            {
                var dto = DTOFactory.CreateNewDTO(new TDev2TOFn(), 1) as TDev2TOFn;
                if(dto != null)
                {
                    dto.Inserted = true;
                    _items.Add(dto);
                }
                var item2 = DTOFactory.CreateNewDTO(new TDev2TOFn(), 2) as TDev2TOFn;
                if(item2 != null)
                {
                    _items.Add(item2);
                }
            }

            CreateDisplayName();

            SyncParent();

            NotifyOfPropertyChange(() => Items);
        }

        private void SyncParent()
        {
            ModelItem parent = ModelItem.Parent;

            while(parent != null)
            {
                if(parent.Properties["Argument"] != null)
                {
                    break;
                }

                parent = parent.Parent;
            }
        }

        public void RemoveRow()
        {
            var BlankCount = new List<int>();

            foreach(dynamic item in _items)
            {
                if(SelectedValue != null && item != SelectedValue)
                {
                    if(item.CanRemove())
                    {
                        BlankCount.Add(item.IndexNumber);
                    }
                }
            }
            if(BlankCount.Count > 1 && _items.Count > 2)
            {
                DeleteItem(BlankCount[0]);
            }
        }

        /// <summary>
        /// Deletes the item.
        /// </summary>
        /// <param name="indexNum">The index num.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/31</date>
        public void DeleteItem(int indexNum)
        {
            if(indexNum != Items.Count)
            {
                Items.RemoveAt(indexNum - 1);
                if(_items.Count >= 2)
                {
                    for(var i = indexNum - 1; i < _items.Count; i++)
                    {
                        dynamic tmp = _items[i];
                        tmp.IndexNumber--;
                    }
                }
                else
                {
                    var newVal = (TDev2TOFn)DTOFactory.CreateNewDTO(new TDev2TOFn());
                    newVal.IndexNumber = indexNum;
                    Items.Insert(indexNum - 1, newVal);
                }                
                NotifyOfPropertyChange(() => Items);
            }
        }

        public void AddRow()
        {
            var canAdd = true;
            foreach(var item in _items)
            {
                if(!item.CanAdd())
                {
                    canAdd = false;
                }
            }
          

                    if(canAdd)
            {
                Items.Add((TDev2TOFn)DTOFactory.CreateNewDTO(new TDev2TOFn(), _items.Count + 1));                
            }
        }

        public void DeleteItem()
        {
            if(SelectedIndex >= 0)
            {
                DeleteItem(SelectedIndex + 1);
            }
        }

        public void InsertItem()
        {
            if(SelectedIndex >= 0)
            {
                InsertItem(SelectedIndex);
            }
        }

        public void InsertItem(int index)
        {
            index++;
            var modelItem = _items[0];
            var newVal = (TDev2TOFn)DTOFactory.CreateNewDTO(modelItem);
            foreach(dynamic item in _items)
            {
                int i = item.IndexNumber;
                if(i >= index)
                {
                    item.IndexNumber++;
                }
            }
            newVal.IndexNumber = index;
            Items.Insert(index - 1, newVal);
            SelectedValue = newVal;
            SelectedIndex = index - 1;
            NotifyOfPropertyChange(() => Items);
        }

        string CreateDisplayName()
        {
            var currentName = ModelItem.Properties["DisplayName"].ComputedValue as string;
            if(currentName.Contains("(") && currentName.Contains(")"))
            {
                if(currentName.Contains(" ("))
                {
                    currentName = currentName.Remove(currentName.IndexOf(" ("));
                }
                else
                {
                    currentName = currentName.Remove(currentName.IndexOf("("));
                }
            }
            currentName = currentName + " (" + (Items.Count - 1) + ")";
            return currentName;
        }


        public void UpdateRows()
        {
            RemoveRow();
            CreateDisplayName();
        }

        public void AddRows(RoutedEventArgs args)
        {
            AddRow();
            AdjustScrollViewer(args.OriginalSource);

            var activityDto = SelectedValue as TDev2TOFn;
            if(activityDto != null && activityDto.Inserted)
            {
                activityDto.Inserted = false;
            }

            CreateDisplayName();
        }

        private void AdjustScrollViewer(object originalSource)
        {
            if(originalSource != null)
            {
                DependencyObject source = originalSource as DependencyObject;
                var scrollViewer = source.GetSelfAndAncestors().OfType<ScrollViewer>().ToList();
                if(scrollViewer != null)
                {
                    if(scrollViewer.Count > 0)
                    {
                        scrollViewer[0].ScrollToVerticalOffset(scrollViewer[0].VerticalOffset + 0.000001);
                    }

                }
            }
        }

        public int GetCollectionCount()
        {
            return Items.Count(to => !to.CanRemove());
        }

        public override void AddListToCollection(IEnumerable<string> listToAdd, bool overwrite)
        {
            if(!overwrite)
            {
                InsertToCollection(listToAdd);
            }
            else
            {
                AddToCollection(listToAdd);
            }
        }

        private void InsertToCollection(IEnumerable<string> listToAdd)
        {
            //gets non empty rows which is valid
            List<TDev2TOFn> listOfValidRows = Items.Where(c => !c.CanRemove()).ToList();

            //if there are valid (non-empty rows)
            if(listOfValidRows.Count > 0)
            {
                //find last valid row
                int startIndex = Items.Last(c => !c.CanRemove()).IndexNumber;

                //and insert before that
                foreach(string s in listToAdd)
                {
                    var newItem = (TDev2TOFn)DTOFactory.CreateNewDTO(new TDev2TOFn(), startIndex + 1, false, s);
                    Items.Add(newItem);   
                    startIndex++;
                }

                //now remove the old one
                CleanUpCollection(startIndex);
            }
            //if no empty rows just add
            else
            {
                AddToCollection(listToAdd);
            }
        }

        private void AddToCollection(IEnumerable<string> listToAdd)
        {
            int startIndex = 0;
            Items.Clear();
            foreach(string s in listToAdd)
            {
                var newItem = (TDev2TOFn)DTOFactory.CreateNewDTO(new TDev2TOFn(), startIndex + 1, false, s);
                Items.Add(newItem);    
                startIndex++;
            }
            CleanUpCollection(startIndex);
        }

        /// <summary>
        /// Cleans up collection.
        /// </summary>
        /// <param name="mic">The collection that is being cleaned</param>
        /// <param name="modelItem">The model item which is the owner of this collection</param>
        /// <param name="startIndex">The index of the last valid row.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/08/13</date>
        private void CleanUpCollection(int startIndex)
        {
            //removes last valid row
            if(startIndex < Items.Count)
            {
                Items.RemoveAt(startIndex);
            }

            //and add a new blank row to the end
            Items.Add((TDev2TOFn)DTOFactory.CreateNewDTO(new TDev2TOFn(), startIndex + 1, true));    
            CreateDisplayName();
        }
    }
}
