using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
// ReSharper disable NonLocalizedString
// ReSharper disable ConvertToAutoProperty

namespace Dev2.Activities.Designers2.Core
{
    public abstract class ActivityCollectionDesignerObservableViewModel<TDev2TOFn> : ActivityCollectionDesignerViewModel
        where TDev2TOFn : class, IDev2TOFn, IPerformsValidation, new()
    {
        TDev2TOFn _initialDto = new TDev2TOFn();
        ObservableCollection<IDev2TOFn> _collection;

        protected ActivityCollectionDesignerObservableViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public int ItemCount => Collection.Count;


        protected void InitializeItems(ObservableCollection<IDev2TOFn> collection)
        {
            Collection = collection;

            // Do this before, because AddDTO() also attaches events
            AttachEvents(0);

            switch (collection.Count)
            {
                case 0:
                    AddDto(1);
                    AddDto(2);
                    break;
                case 1:
                    AddDto(2);
                    break;
            }

            AddBlankRow();
         

            Collection.CollectionChanged+=ModelItemCollectionOnCollectionChanged;
        }

        void ModelItemCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            //
        }

        public override void OnSelectionChanged(ModelItem oldItem, ModelItem newItem)
        {
            var dto = oldItem?.GetCurrentValue() as TDev2TOFn;
            if(dto != null && dto.CanRemove())
            {
                // old row is blank so remove
                var index = Collection.IndexOf(dto) + 1;
                RemoveDto(dto, index);
            }
            if(newItem != null)
            {
                CurrentModelItem = newItem;
            }
        }

        public ModelItem CurrentModelItem { get; set; }

        public override void UpdateDisplayName()
        {
            var currentName = DisplayName;
            if(currentName.Contains("(") && currentName.Contains(")"))
            {
                currentName = currentName.Remove(currentName.Contains(" (")
                    ? currentName.IndexOf(" (", StringComparison.Ordinal)
                    : currentName.IndexOf("(", StringComparison.Ordinal));
            }
            currentName = currentName + " (" + (ItemCount - 1) + ")";
            DisplayName = currentName;
        }

        public sealed override void Validate()
        {
            var result = new List<IActionableErrorInfo>();
            result.AddRange(ValidateThis());

            ProcessModelItemCollection(0, mi => result.AddRange(ValidateCollectionItem(mi)));

            //Errors = result.Count == 0 ? 0 : result;
            Errors = result.Count == 0 ? null : result;
        }

        protected abstract IEnumerable<IActionableErrorInfo> ValidateThis();

        protected abstract IEnumerable<IActionableErrorInfo> ValidateCollectionItem(IDev2TOFn mi);

        public override bool CanRemoveAt(int indexNumber)
        {
            return ItemCount > 2 && indexNumber < ItemCount;
        }

        public override bool CanInsertAt(int indexNumber)
        {
            return ItemCount > 2 && indexNumber < ItemCount;
        }

        public override void RemoveAt(int indexNumber)
        {
            if(!CanRemoveAt(indexNumber))
            {
                return;
            }

            if (ItemCount == 2)
            {
                if(indexNumber == 1)
                {
                    var dto = GetDto(indexNumber);
                    dto.ClearRow();
                }
            }
            else
            {
                var dto = GetDto(indexNumber);
                RemoveDto(dto, indexNumber);
            }
        }

        public override void InsertAt(int indexNumber)
        {
            if(!CanInsertAt(indexNumber))
            {
                return;
            }
            AddDto(indexNumber);
            UpdateDisplayName();
        }

        protected override void AddToCollection(IEnumerable<string> source, bool overwrite)
        {
            var firstModelItem = Collection.FirstOrDefault();
            if(firstModelItem != null)
            {
                _initialDto = (TDev2TOFn)firstModelItem;
            }

            var indexNumber = GetIndexForAdd(overwrite);

            // Always insert items before blank row
            foreach(var s in source.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                AddDto(indexNumber, s);
                indexNumber++;
            }

            var lastModelItem = GetModelItem(ItemCount);
            SetIndexNumber(lastModelItem, indexNumber);

            UpdateDisplayName();

            // Restore 
            _initialDto = new TDev2TOFn();
        }

        /// <summary>
        /// Gets the insert index for <see cref="AddToCollection"/>. 
        /// Returns the index of the last blank row.
        /// </summary>
        int GetIndexForAdd(bool overwrite)
        {
            var indexNumber = 1;
            if(overwrite)
            {
                Collection.Clear();

                // AddMode blank row
                AddDto(indexNumber);
            }
            else
            {
                var lastDto = GetLastDto();
                indexNumber = Collection.IndexOf(GetModelItem(ItemCount)) + 1;

                if (ItemCount == 2)
                {
                    // Check whether we have 2 blank rows
                    var firstDto = GetDto(1);
                    if(firstDto.CanRemove() && lastDto.CanRemove())
                    {
                        RemoveAt(indexNumber, lastDto);
                        indexNumber = indexNumber - 1;
                    }
                }
            }
            return indexNumber;
        }

        IDev2TOFn GetModelItem(int indexNumber)
        {
            return Collection[indexNumber - 1];
        }

        TDev2TOFn GetDto(int indexNumber)
        {
            var item = GetModelItem(indexNumber);
            return item as TDev2TOFn;
        }

        TDev2TOFn GetLastDto()
        {
            return GetDto(ItemCount);
        }

        void AddBlankRow()
        {
            var lastDto = GetLastDto();
            var index = ItemCount + 1;
            var isLastRowBlank = lastDto.CanRemove();
            if(!isLastRowBlank)
            {
                AddDto(index + 1);
                UpdateDisplayName();
            }
        }

        public ObservableCollection<IDev2TOFn> Collection
        {
            get
            {
                return _collection;
            }
            protected set
            {
                _collection = value;
            }
        }

        void AddDto(int indexNumber, string initializeWith = "")
        {
            //
            // DO NOT invoke Renumber() from here - this method is called MANY times when invoking AddToCollection()!!
            //
            var dto = CreateDto(indexNumber, initializeWith);
            if(dto != null)
            {
                AttachEvents(dto);

                var idx = indexNumber - 1;
                if(idx >= Collection.Count)
                {
                    Collection?.Add(dto);
                }
                else
                {
                    Collection?.Insert(idx, dto);
                }
            }
           UpdateDto(dto);
        }

        public virtual void UpdateDto(IDev2TOFn dto)
        {
        }

        protected virtual IDev2TOFn CreateDto(int indexNumber, string initializeWith)
        {
            return DTOFactory.CreateNewDTO(_initialDto, indexNumber, false, initializeWith);
        }

        void RemoveDto(TDev2TOFn dto, int indexNumber)
        {

            if (ItemCount > 2 && indexNumber < ItemCount)
            {
                RemoveAt(indexNumber, dto);
                UpdateDisplayName();
            }
        }

        void RemoveAt(int indexNumber, INotifyPropertyChanged notify)
        {
            notify.PropertyChanged -= OnDtoPropertyChanged;
            var idx = indexNumber - 1;
            Collection.RemoveAt(idx);
        }

        void OnDtoPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            DoCustomAction(args.PropertyName);
            if(args.PropertyName != "CanRemove")
            {
                return;
            }

            var dto = (TDev2TOFn)sender;
            if(dto.CanAdd())
            {
                if (ItemCount == 1)
                {
                    var firstDto = GetDto(1);
                    if(!firstDto.CanRemove())
                    {
                        // first row is not blank
                        AddBlankRow();
                    }
                }
                else
                {
                    AddBlankRow();
                }
            }
        }

        protected virtual void DoCustomAction(string propertyName)
        {            
        }

        /// <summary>
        /// Attaches events to the ModelItemCollection starting at the specified zero-based index.
        /// </summary>
        void AttachEvents(int startIndex)
        {
            ProcessModelItemCollection(startIndex, mi =>
            {
                var dto = mi as TDev2TOFn;
                if(dto != null)
                {
                    AttachEvents(dto);
                }
            });
        }

        void AttachEvents(INotifyPropertyChanged dto)
        {
            dto.PropertyChanged += OnDtoPropertyChanged;
        }


        /// <summary>
        /// Process the ModelItemCollection starting at the specified zero-based index.
        /// </summary>
        void ProcessModelItemCollection(int startIndex, Action<IDev2TOFn> processModelItem)
        {
            startIndex = Math.Max(startIndex, 0);
            for (var i = startIndex; i < Collection.Count; i++)
            {
                processModelItem(Collection[i]);
            }
        }

        static void SetIndexNumber(IDev2TOFn mi, int indexNumber)
        {
            mi.IndexNumber = indexNumber;
        }

        protected override void OnDispose()
        {
          
            ProcessModelItemCollection(0, mi =>
            {
                var dto = mi as TDev2TOFn;
                if(dto != null)
                {
                    CEventHelper.RemoveAllEventHandlers(dto);
                }
                CEventHelper.RemoveAllEventHandlers(mi);

            });
            base.OnDispose();
        }

    }
}