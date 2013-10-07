using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.Core
{
    // Note: All indexNumber parameters are 1-based
    public abstract class ActivityCollectionDesignerViewModel<TDev2TOFn> : ActivityCollectionDesignerViewModel
        where TDev2TOFn : class, IDev2TOFn, IPerformsValidation, new()
    {
        ModelItemCollection _modelItemCollection;

        protected ActivityCollectionDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public int ItemCount { get { return _modelItemCollection.Count; } }

        protected void InitializeItems(ModelItemCollection modelItemCollection)
        {
            _modelItemCollection = modelItemCollection;

            // Do this before, because AddDTO() also attaches events
            AttachEvents(0);

            switch(modelItemCollection.Count)
            {
                case 0:
                    AddDTO(1);
                    AddDTO(2);
                    break;
                case 1:
                    AddDTO(2);
                    break;
            }

            AddBlankRow();
            UpdateDisplayName();
        }

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

        public override void Validate()
        {
            var errors = new List<IActionableErrorInfo>();
            foreach(var error in _modelItemCollection.SelectMany(mi => ((TDev2TOFn)mi.GetCurrentValue()).Errors))
            {
                errors.AddRange(error.Value);
            }
            Errors = errors.Count == 0 ? null : errors;
        }

        public override bool CanRemoveAt(int indexNumber)
        {
            return indexNumber < _modelItemCollection.Count;
        }

        public override bool CanInsertAt(int indexNumber)
        {
            return _modelItemCollection.Count > 2 && indexNumber < _modelItemCollection.Count;
        }

        public override void RemoveAt(int indexNumber)
        {
            if(!CanRemoveAt(indexNumber))
            {
                return;
            }
            if(_modelItemCollection.Count == 2)
            {
                if(indexNumber == 1)
                {
                    var dto = GetDTO(indexNumber);
                    dto.ClearRow();
                }
            }
            else
            {
                var dto = GetDTO(indexNumber);
                RemoveDTO(dto);
            }
        }

        public override void InsertAt(int indexNumber)
        {
            if(!CanInsertAt(indexNumber))
            {
                return;
            }
            AddDTO(indexNumber);
            Renumber(indexNumber);
            UpdateDisplayName();
        }

        protected override void AddToCollection(IEnumerable<string> source, bool overwrite)
        {
            var indexNumber = GetIndexForAdd(overwrite);

            // Always insert items before blank row
            foreach(var s in source.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                AddDTO(indexNumber, s);
                indexNumber++;
            }

            var lastDTO = GetLastDTO();
            lastDTO.IndexNumber = indexNumber;

            UpdateDisplayName();
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
                _modelItemCollection.Clear();

                // Add blank row
                AddDTO(indexNumber);
            }
            else
            {
                var lastDTO = GetLastDTO();
                indexNumber = lastDTO.IndexNumber;

                if(_modelItemCollection.Count == 2)
                {
                    // Check whether we have 2 blank rows
                    var firstDTO = GetDTO(1);
                    if(firstDTO.CanRemove() && lastDTO.CanRemove())
                    {
                        RemoveAt(lastDTO.IndexNumber, lastDTO);
                    }
                    indexNumber = 1;
                }
            }
            return indexNumber;
        }

        TDev2TOFn GetDTO(int indexNumber)
        {
            var item = _modelItemCollection[indexNumber - 1];
            return item.GetCurrentValue() as TDev2TOFn;
        }

        TDev2TOFn GetLastDTO()
        {
            return GetDTO(_modelItemCollection.Count);
        }

        void AddBlankRow()
        {
            var lastDTO = GetLastDTO();
            var isLastRowBlank = lastDTO.CanRemove();
            if(!isLastRowBlank)
            {
                AddDTO(lastDTO.IndexNumber + 1);
                UpdateDisplayName();
            }
        }

        void AddDTO(int indexNumber, string initializeWith = "")
        {
            //
            // DO NOT invoke Renumber() from here - this method is called MANY times when invoking AddToCollection()!!
            //
            var dto = DTOFactory.CreateNewDTO(new TDev2TOFn(), indexNumber, false, initializeWith);
            dto.PropertyChanged += OnDTOPropertyChanged;

            var idx = dto.IndexNumber - 1;
            if(idx == _modelItemCollection.Count)
            {
                _modelItemCollection.Add(dto);
            }
            else
            {
                _modelItemCollection.Insert(idx, dto);
            }
        }

        void RemoveDTO(TDev2TOFn dto)
        {
            if(_modelItemCollection.Count > 2 && dto.IndexNumber >= 0 && dto.IndexNumber < _modelItemCollection.Count)
            {
                RemoveAt(dto.IndexNumber, dto);
                Renumber(dto.IndexNumber - 1);
                UpdateDisplayName();
            }
        }

        void RemoveAt(int indexNumber, INotifyPropertyChanged notify)
        {
            notify.PropertyChanged -= OnDTOPropertyChanged;
            var idx = indexNumber - 1;
            _modelItemCollection.RemoveAt(idx);
        }

        void OnDTOPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if(args.PropertyName != "CanRemove")
            {
                return;
            }

            var dto = (TDev2TOFn)sender;
            if(dto.CanRemove())
            {
                RemoveDTO(dto);
            }
            else if(dto.CanAdd())
            {
                AddBlankRow();

                if(_modelItemCollection.Count == 3)
                {
                    // We may have been editing row 2 while row 1 was blank
                    // check if first row is blank and remove it
                    var firstDTO = GetDTO(1);
                    if(firstDTO.CanRemove())
                    {
                        RemoveDTO(firstDTO);
                    }
                }
            }
        }

        /// <summary>
        /// Attaches events to the ModelItemCollection starting at the specified zero-based index.
        /// </summary>
        void AttachEvents(int startIndex)
        {
            ProcessModelItemCollection(dto => dto.PropertyChanged += OnDTOPropertyChanged, startIndex);
        }

        /// <summary>
        /// Renumbers the ModelItemCollection starting at the specified zero-based index.
        /// </summary>
        void Renumber(int startIndex)
        {
            var indexNumber = startIndex + 1;
            ProcessModelItemCollection(dto => dto.IndexNumber = indexNumber++, startIndex);
        }

        /// <summary>
        /// Process the ModelItemCollection starting at the specified zero-based index.
        /// </summary>
        void ProcessModelItemCollection(Action<TDev2TOFn> processDTO, int startIndex)
        {
            startIndex = Math.Max(startIndex, 0);
            for(var i = startIndex; i < _modelItemCollection.Count; i++)
            {
                var dto = _modelItemCollection[i].GetCurrentValue() as TDev2TOFn;
                if(dto != null)
                {
                    processDTO(dto);
                }
            }
        }
    }
}