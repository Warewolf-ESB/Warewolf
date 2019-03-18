#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Dev2.Activities.Designers2.DataMerge;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.Core
{
    // -------------------------------------------------------------------------------------------------------------
    // NOTES
    // -------------------------------------------------------------------------------------------------------------
    // - All indexNumber parameters are 1-based
    // - MUST ALWAYS modify DTO properties via ModelItem properties - otherwise you WILL experience binding issues.
    // -------------------------------------------------------------------------------------------------------------

    public abstract class ActivityCollectionDesignerViewModel<TDev2TOFn> : ActivityCollectionDesignerViewModel
        where TDev2TOFn : class, IDev2TOFn, IPerformsValidation, new()
    {
        TDev2TOFn _initialDto = new TDev2TOFn();
        readonly object _syncLock = new object();

        protected ActivityCollectionDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public int ItemCount => ModelItemCollection.Count;
        public int ModelItemCount
        {
            get
            {
                dynamic mi = ModelItem;
                return mi.MergeCollection.Count;
            }
        }

        protected void InitializeItems(ModelItemCollection modelItemCollection)
        {
            ModelItemCollection = modelItemCollection;
            BindingOperations.EnableCollectionSynchronization(ModelItemCollection, _syncLock);
            // Do this before, because AddDTO() also attaches events
            AttachEvents(0);

            if (modelItemCollection.Count == 1)
            {
                AddDto(2);
            }
            if (modelItemCollection.Count == 0)
            {
                AddDto(1);
                AddDto(2);
            }

            AddBlankRow();
            UpdateDisplayName();

            if (ModelItemCollection != null)
            {
                ModelItemCollection.CollectionChanged += ModelItemCollectionOnCollectionChanged;
            }

        }

        void ModelItemCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
        }

        public override void OnSelectionChanged(ModelItem oldItem, ModelItem newItem)
        {
            // old row is blank so remove
            if (oldItem?.GetCurrentValue() is TDev2TOFn dto && dto.CanRemove() && ModelItemCollection != null)
            {
                var index = ModelItemCollection.IndexOf(oldItem) + 1;
                RemoveDto(dto, index);
            }

            if (newItem != null)
            {
                CurrentModelItem = newItem;
            }
        }

        public ModelItem CurrentModelItem { get; set; }

        public override void UpdateDisplayName()
        {
            var currentName = DisplayName;
            if (currentName.Contains("(") && currentName.Contains(")"))
            {
                currentName = currentName.Remove(currentName.Contains(" (")
                    ? currentName.IndexOf(" (", StringComparison.Ordinal)
                    : currentName.IndexOf("(", StringComparison.Ordinal));
            }
            var count = ItemCount - 2;
            if (ItemCount > 0)
            {
                var indexNumber = ItemCount - 1;
                var dto = GetDto(indexNumber);
                if (dto.CanAdd())
                {
                    count = indexNumber;
                }
            }
            if (count < 0)
            {
                count = 0;
            }
            currentName = currentName + " (" + count + ")";
            DisplayName = currentName;
        }

        public sealed override void Validate()
        {
            var result = new List<IActionableErrorInfo>();
            result.AddRange(ValidateThis());
            ProcessModelItemCollection(0, mi => result.AddRange(ValidateCollectionItem(mi)));
            Errors = result.Count == 0 ? null : result;
        }

        protected abstract IEnumerable<IActionableErrorInfo> ValidateThis();

        protected abstract IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi);

        public override bool CanRemoveAt(int indexNumber) => ModelItemCollection != null && (ModelItemCollection.Count > 2 && indexNumber < ModelItemCollection.Count);

        public override bool CanInsertAt(int indexNumber) => ModelItemCollection != null && (ModelItemCollection.Count > 2 && indexNumber < ModelItemCollection.Count);

        public override void RemoveAt(int indexNumber)
        {
            if (!CanRemoveAt(indexNumber))
            {
                return;
            }

            if (ModelItemCollection.Count == 2)
            {
                if (indexNumber == 1)
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
            if (!CanInsertAt(indexNumber))
            {
                return;
            }
            AddDto(indexNumber);
            UpdateDisplayName();
        }

        protected override void AddToCollection(IEnumerable<string> sources, bool overwrite)
        {
            if (GetType() == typeof(DataMergeDesignerViewModel))
            {
                var lastPopulated = ModelItemCollection?.LastOrDefault(p => !string.IsNullOrWhiteSpace(p.GetProperty("At").ToString()));
                if (lastPopulated != null)
                {
                    _initialDto = (TDev2TOFn)lastPopulated.GetCurrentValue();
                }
            }
            var indexNumber = GetIndexForAdd(overwrite);

            // Always insert items before blank row
            foreach (var s in sources.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                AddDto(indexNumber, s);
                indexNumber++;
            }
            AddBlankRow(overwrite);
            var lastModelItem = GetModelItem(ItemCount);
            SetIndexNumber(lastModelItem, indexNumber);

            UpdateDisplayName();

            // Restore
            _initialDto = new TDev2TOFn();
        }
        
        int GetIndexForAdd(bool overwrite)
        {
            int indexNumber = 1;
            if (overwrite)
            {
                ModelItemCollection?.Clear();
            }
            else
            {
                indexNumber = GetIndexWithoutOverwrite();
            }
            return indexNumber;
        }

        int GetIndexWithoutOverwrite()
        {
            int indexNumber = 1;
            var lastDto = GetLastDto();
            if (ModelItemCollection != null)
            {
                indexNumber = ModelItemCollection.IndexOf(GetModelItem(ItemCount)) + 1;
                if (ModelItemCollection.Count == 2)
                {
                    // Check whether we have 2 blank rows
                    var firstDto = GetDto(1);
                    if (firstDto.CanRemove() && lastDto.CanRemove())
                    {
                        RemoveAt(indexNumber, lastDto);
                        indexNumber = indexNumber - 1;
                    }
                }
            }

            return indexNumber;
        }

        ModelItem GetModelItem(int indexNumber)
        {
            var index = indexNumber - 1;
            if (ItemCount < index)
            {
                index = ItemCount == 0 ? 0 : ItemCount - 1;
            }
            if (index < 0)
            {
                index = 0;
            }
            return ModelItemCollection[index];
        }

        protected TDev2TOFn GetDto(int indexNumber)
        {
            var item = GetModelItem(indexNumber);
            return item.GetCurrentValue() as TDev2TOFn;
        }

        TDev2TOFn GetLastDto() => GetDto(ItemCount);

        void AddBlankRow(bool overwrite = false)
        {
            var lastDto = GetLastDto();
            var index = ItemCount + 1;
            var isLastRowBlank = lastDto.CanRemove();
            if (!isLastRowBlank)
            {
                var lastIndex = index + 1;
                if (overwrite)
                {
                    _initialDto = new TDev2TOFn();
                }

                AddDto(lastIndex);
                if (GetType() == typeof(DataMergeDesignerViewModel))
                {
                    RunValidation(ModelItemCount - 1);
                }
            }
            UpdateDisplayName();
        }

        protected virtual void RunValidation(int index)
        {
        }
        void AddDto(int indexNumber, string initializeWith = "")
        {
            //
            // DO NOT invoke Renumber() from here - this method is called MANY times when invoking AddToCollection()!!
            //
            var dto = CreateDto(indexNumber, initializeWith);
            AttachEvents(dto);

            var idx = indexNumber - 1;
            if (ModelItemCollection != null && idx >= ModelItemCollection.Count)
            {
                var modelItem = ModelItemUtils.CreateModelItem(dto);
                ModelItemCollection.Add(modelItem);

            }
            else
            {
                ModelItemCollection?.Insert(idx, dto);
            }
            RunValidation(idx);
        }

        protected virtual IDev2TOFn CreateDto(int indexNumber, string initializeWith) => DTOFactory.CreateNewDTO(_initialDto, indexNumber, false, initializeWith);

        protected virtual void RemoveDto(IDev2TOFn dto, int indexNumber)
        {

            if (ModelItemCollection.Count > 2 && indexNumber < ModelItemCollection.Count)
            {
                RemoveAt(indexNumber, dto);
                UpdateDisplayName();
            }
        }

        protected void RemoveAt(int indexNumber, INotifyPropertyChanged notify)
        {
            notify.PropertyChanged -= OnDtoPropertyChanged;
            var idx = indexNumber - 1;
            ModelItemCollection.RemoveAt(idx);
        }

        void OnDtoPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (!IsMerge)
            {
                DoCustomAction(args.PropertyName);
                if (args.PropertyName != "CanRemove")
                {
                    return;
                }

                var canAdd = true;
                var parent = ModelItemCollection.Parent;
                if (parent != null)
                {
                    var parentContentPane = FindDependencyParent.FindParent<DesignerView>(parent.View);
                    var dataContext = parentContentPane?.DataContext;
                    if (dataContext != null && (dataContext.GetType().Name == "ServiceTestViewModel"))
                    {
                        canAdd = false;
                    }
                }

                if (canAdd)
                {
                    AddBlank(sender);
                }
            }
        }

        private void AddBlank(object sender)
        {
            var dto = (TDev2TOFn)sender;
            if (dto.CanAdd())
            {
                if (ModelItemCollection.Count == 2)
                {
                    var firstDto = GetDto(1);
                    if (!firstDto.CanRemove())
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
        
        void AttachEvents(int startIndex)
        {
            ProcessModelItemCollection(startIndex, mi =>
            {
                if (mi.GetCurrentValue() is TDev2TOFn dto)
                {
                    AttachEvents(dto);
                }
            });
        }

        void AttachEvents(INotifyPropertyChanged dto)
        {
            dto.PropertyChanged += OnDtoPropertyChanged;
        }
        
        void ProcessModelItemCollection(int startIndex, Action<ModelItem> processModelItem)
        {
            if (ModelItemCollection != null && !IsMerge)
            {
                startIndex = Math.Max(startIndex, 0);
                for (var i = startIndex; i < ModelItemCollection.Count; i++)
                {
                    processModelItem?.Invoke(ModelItemCollection[i]);
                }
            }
        }

        static void SetIndexNumber(ModelItem mi, int indexNumber)
        {
            mi.SetProperty("IndexNumber", indexNumber);
        }

        protected override void OnDispose()
        {

            ProcessModelItemCollection(0, mi =>
              {
                  if (mi.GetCurrentValue() is TDev2TOFn dto)
                  {
                      CEventHelper.RemoveAllEventHandlers(dto);
                  }
                  CEventHelper.RemoveAllEventHandlers(mi);

              });
            if (ModelItemCollection != null)
            {
                BindingOperations.DisableCollectionSynchronization(ModelItemCollection);
            }
            ModelItemCollection = null;
            base.OnDispose();
        }

    }
}
