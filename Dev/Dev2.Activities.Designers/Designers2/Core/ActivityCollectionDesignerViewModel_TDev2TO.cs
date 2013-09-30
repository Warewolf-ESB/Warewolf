using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Studio.Core.ViewModels.Base;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class ActivityCollectionDesignerViewModel<TDev2TOFn> : ActivityCollectionDesignerViewModel
        where TDev2TOFn : class, IDev2TOFn, IPerformsValidation, new()
    {
        ObservableCollection<TDev2TOFn> _items;

        protected ActivityCollectionDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            InitializeItems();
        }

        public ObservableCollection<TDev2TOFn> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
            }
        }

        void InitializeItems()
        {
            // ReSharper disable ExplicitCallerInfoArgument
            var items = GetProperty<IList<TDev2TOFn>>(CollectionName);
            // ReSharper restore ExplicitCallerInfoArgument
            if(items == null)
            {
                _items = new ObservableCollection<TDev2TOFn>();
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
                // ReSharper disable ExplicitCallerInfoArgument
                SetProperty(Items, CollectionName);
                // ReSharper restore ExplicitCallerInfoArgument
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

        void AdjustScrollViewer(object originalSource)
        {
            if(originalSource == null)
            {
                return;
            }
            var source = originalSource as DependencyObject;
            if(source != null)
            {
                var scrollViewer = source.GetSelfAndAncestors().OfType<ScrollViewer>().ToList();
                if(scrollViewer.Count > 0)
                {
                    scrollViewer[0].ScrollToVerticalOffset(scrollViewer[0].VerticalOffset + 0.000001);
                }
            }
        }

        public void AddRow()
        {
            var canAdd = true;
            foreach(var item in Items)
            {
                if(!item.CanAdd())
                {
                    canAdd = false;
                }
            }

            if(canAdd)
            {
                var dev2TOFn = (TDev2TOFn)DTOFactory.CreateNewDTO(new TDev2TOFn(), Items.Count + 1);
                Items.Add(dev2TOFn);
            }
        }

        public void UpdateRows()
        {
            RemoveRow();
            CreateDisplayName();
        }

        public void RemoveRow()
        {
            var blankCount = (from item in Items
                              where SelectedValue != null && item != SelectedValue
                              where item.CanRemove()
                              select item.IndexNumber).ToList();

            if(blankCount.Count > 1 && Items.Count > 2)
            {
                DeleteItem(blankCount[0]);
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
                if(Items.Count >= 2)
                {
                    for(var i = indexNum - 1; i < Items.Count; i++)
                    {
                        var tmp = Items[i];
                        tmp.IndexNumber--;
                    }
                }
                else
                {
                    var newVal = (TDev2TOFn)DTOFactory.CreateNewDTO(new TDev2TOFn());
                    newVal.IndexNumber = indexNum;
                    Items.Insert(indexNum - 1, newVal);
                }
            }
        }

        void CreateDisplayName()
        {
            var currentName = DisplayName;
            if(currentName.Contains("(") && currentName.Contains(")"))
            {
                currentName = currentName.Remove(currentName.Contains(" (")
                    ? currentName.IndexOf(" (", System.StringComparison.Ordinal)
                    : currentName.IndexOf("(", System.StringComparison.Ordinal));
            }
            currentName = currentName + " (" + (Items.Count - 1) + ")";
            DisplayName = currentName;
        }

        public override ICommand DeleteItemCommand
        {
            get
            {
                return new RelayCommand(o => DeleteItem((int)o));
            }
        }

        protected override void AddToCollection(IEnumerable<string> source, bool overwrite)
        {
            if(!overwrite)
            {
                InsertToCollection(source);
            }
            else
            {
                AddToCollection(source);
            }
        }

        void InsertToCollection(IEnumerable<string> listToAdd)
        {
            //gets non empty rows which is valid
            List<TDev2TOFn> listOfValidRows = Items.Where(c => !c.CanRemove()).ToList();

            //if there are valid (non-empty rows)
            if(listOfValidRows.Count > 0)
            {
                //find last valid row
                int startIndex = Items.Last(c => !c.CanRemove()).IndexNumber;

                //and insert before that
                foreach(var s in listToAdd)
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

        void AddToCollection(IEnumerable<string> listToAdd)
        {
            int startIndex = 0;
            Items.Clear();
            foreach(var s in listToAdd)
            {
                var newItem = (TDev2TOFn)DTOFactory.CreateNewDTO(new TDev2TOFn(), startIndex + 1, false, s);
                Items.Add(newItem);
                startIndex++;
            }
            CleanUpCollection(startIndex);
        }

        void CleanUpCollection(int startIndex)
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
            foreach(var item in _items)
            {
                var i = item.IndexNumber;
                if(i >= index)
                {
                    item.IndexNumber++;
                }
            }
            newVal.IndexNumber = index;
            Items.Insert(index - 1, newVal);
            SelectedValue = newVal;
            SelectedIndex = index - 1;
        }


        #region Overrides of ActivityDesignerViewModel

        public override void Validate()
        {
            var errors = new List<IActionableErrorInfo>();
            foreach(var error in Items.SelectMany(item => item.Errors))
            {
                errors.AddRange(error.Value);
            }
            Errors = errors.Count == 0 ? null : errors;
        }

        #endregion
    }
}