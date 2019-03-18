#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using Dev2.Common;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.ViewModels.DataList;
using ServiceStack.Common.Extensions;

namespace Dev2.Studio.Core.DataList
{
    class RecordsetHandler : IRecordsetHandler
    {
        readonly DataListViewModel _vm;

        public RecordsetHandler(DataListViewModel vm)
        {
            _vm = vm;
        }

        #region Implementation of IRecordsetHandler

        public void AddRecordsetNamesIfMissing()
        {
            var recsetNum = _vm.RecsetCollectionCount;
            var recsetCount = 0;

            while (recsetCount < recsetNum)
            {
                var recset = _vm.RecsetCollection?[recsetCount];

                if (!string.IsNullOrWhiteSpace(recset?.DisplayName))
                {
                    FixNamingForRecset(recset);
                    var childrenNum = recset.Children.Count;
                    var childrenCount = 0;

                    while (childrenCount < childrenNum)
                    {
                        FixCommonNamingProblems(recset, childrenCount);
                        childrenCount++;
                    }
                }
                recsetCount++;
            }
        }

        static void FixCommonNamingProblems(IRecordSetItemModel recset, int childrenCount)
        {
            var child = recset.Children[childrenCount];
            if (child.Parent == null)
            {
                child.Parent = recset;
            }

            if (!string.IsNullOrWhiteSpace(child?.DisplayName))
            {
                var indexOfDot = child.DisplayName.IndexOf(".", StringComparison.Ordinal);
                if (indexOfDot > -1)
                {
                    var recsetName = child.DisplayName.Substring(0, indexOfDot + 1);
                    child.DisplayName = child.DisplayName.Replace(recsetName, child.Parent.DisplayName + ".");
                }
                FixCommonNamingProblems(child);
            }
        }

        public void RemoveBlankRecordsets()
        {
            var blankList = _vm.RecsetCollection.Where(c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank).ToList();
            if (blankList.Count <= 1)
            {
                return;
            }

            _vm.Remove(blankList.First());
        }

        public void RemoveBlankRecordsetFields()
        {
            foreach (var recset in _vm.RecsetCollection)
            {
                var blankChildList = recset.Children.Where(c => c.IsBlank).ToList();
                if (blankChildList.Count <= 1)
                {
                    continue;
                }

                recset.Children.Remove(blankChildList.First());
            }
        }

        public void ValidateRecordset()
        {
            MarkEmptyRecordsetErrors();
            ClearEmptyRecordsetErrors();
        }

        bool RecordSetHasChildren(IRecordSetItemModel model) => model.Children != null && model.Children.Count > 0;

        void MarkEmptyRecordsetErrors()
        {
            foreach (var recordset in _vm.RecsetCollection.Where(c => c.Children.Count == 0 || c.Children.Count == 1 && string.IsNullOrEmpty(c.Children[0].DisplayName) && !string.IsNullOrEmpty(c.DisplayName)))
            {
                recordset.SetError(StringResources.ErrorMessageEmptyRecordSet);
            }
        }

        void ClearEmptyRecordsetErrors()
        {
            foreach (var recset in _vm.RecsetCollection.Where(c => c.ErrorMessage == StringResources.ErrorMessageEmptyRecordSet && c.Children.Count >= 1 && !string.IsNullOrEmpty(c.Children[0].DisplayName)))
            {
                if (recset.ErrorMessage != StringResources.ErrorMessageDuplicateRecordset || recset.ErrorMessage != StringResources.ErrorMessageInvalidChar)
                {
                    recset.RemoveError();
                }
            }
        }

        public void AddRowToRecordsets()
        {
            var blankList = _vm.RecsetCollection.Where(c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank).ToList();

            if (blankList.Count == 0)
            {
                AddRecordSet();
            }

            foreach (var recset in _vm.RecsetCollection)
            {
                var blankChildList = recset.Children.Where(c => c.IsBlank).ToList();
                if (blankChildList.Count != 0)
                {
                    continue;
                }

                var newChild = DataListItemModelFactory.CreateRecordSetFieldItemModel(string.Empty);
                if (newChild != null)
                {
                    newChild.Parent = recset;
                    recset.Children.Add(newChild);
                }
            }
        }

        void FixNamingForRecset(IDataListItemModel recset)
        {
            if (!recset.DisplayName.EndsWith("()"))
            {
                recset.DisplayName = string.Concat(recset.DisplayName, "()");
            }
            FixCommonNamingProblems(recset);
        }

        public void AddRecordSet()
        {
            var recset = DataListItemModelFactory.CreateRecordSetItemModel(string.Empty);
            var childItem = DataListItemModelFactory.CreateRecordSetFieldItemModel(string.Empty);
            if (recset != null)
            {
                recset.IsComplexObject = false;
                recset.AllowNotes = false;
                recset.IsExpanded = false;
                if (childItem != null)
                {
                    childItem.IsComplexObject = false;
                    childItem.AllowNotes = false;
                    childItem.Parent = recset;
                    recset.Children.Add(childItem);
                }
                _vm.Add(recset);
            }
        }

        public void SortRecset(bool ascending)
        {
            IList<IRecordSetItemModel> newRecsetCollection = ascending ? _vm.RecsetCollection.Where(model => !model.IsBlank).OrderBy(c => c.DisplayName).ToList() : _vm.RecsetCollection.Where(model => !model.IsBlank).OrderByDescending(c =>  c.DisplayName).ToList();
            for (int i = 0; i < newRecsetCollection.Count; i++)
            {
                var recordSetItemModel = newRecsetCollection[i];
                IList<IRecordSetFieldItemModel> recSetChildrenSorted = ascending ? recordSetItemModel.Children.Where(model => !model.IsBlank).OrderBy(c => c.DisplayName).ToList() : recordSetItemModel.Children.Where(model => !model.IsBlank).OrderByDescending(c => c.DisplayName).ToList();
                recordSetItemModel.Children = new ObservableCollection<IRecordSetFieldItemModel>(recSetChildrenSorted);
                _vm.RecsetCollection.Move(_vm.RecsetCollection.IndexOf(recordSetItemModel), i);
            }
        }

        public void AddRecordSets(XmlNode xmlNode)
        {
            var recset = CreateRecordSet(xmlNode);
            foreach (XmlNode subc in xmlNode.ChildNodes)
            {
                // It is possible for the .Attributes property to be null, a check should be added
                CreateColumns(subc, recset);
            }
        }

        IRecordSetItemModel CreateRecordSet(XmlNode xmlNode)
        {
            IRecordSetItemModel recset;
            if (xmlNode.Attributes != null)
            {
                recset = DataListItemModelFactory.CreateRecordSetItemModel(xmlNode.Name, Common.ParseDescription(xmlNode.Attributes[Common.Description]), Common.ParseColumnIODirection(xmlNode.Attributes[GlobalConstants.DataListIoColDirection]));
                if (recset != null)
                {
                    recset.IsEditable = Common.ParseIsEditable(xmlNode.Attributes[Common.IsEditable]);
                    _vm.Add(recset);
                }
            }
            else
            {
                recset = DataListItemModelFactory.CreateRecordSetItemModel(xmlNode.Name, Common.ParseDescription(null));
                if (recset != null)
                {
                    recset.IsEditable = Common.ParseIsEditable(null);

                    _vm.Add(recset);
                }
            }
            return recset;
        }

        public void SetRecordSetItemsAsUsed()
        {
            if (_vm.RecsetCollection.Any(rc => !rc.IsUsed))
            {
                foreach (var dataListItemModel in _vm.RecsetCollection)
                {
                    dataListItemModel.IsUsed = true;
                    foreach (var listItemModel in dataListItemModel.Children)
                    {
                        listItemModel.IsUsed = true;
                    }
                }
            }
        }

        public void FindMissingPartsForRecordset(IDataListVerifyPart part, List<IDataListVerifyPart> missingDataParts)
        {
            if (part.IsScalar)
            {
                return;
            }

            var recset = _vm.RecsetCollection.Where(c => c.DisplayName == part.Recordset).ToList();
            if (!recset.Any())
            {
                missingDataParts.Add(part);
            }
            else
            {
                if (!string.IsNullOrEmpty(part.Field) && recset[0].Children.Count(c => c.DisplayName == part.Field) == 0)
                {
                    missingDataParts.Add(part);
                }
            }
        }

        public bool BuildRecordSetErrorMessages(IRecordSetItemModel model, out string errorMessage)
        {
            errorMessage = "";
            if (!RecordSetHasChildren(model))
            {
                return false;
            }

            if (model.HasError)
            {
                {
                    errorMessage = BuildErrorMessage(model);
                    return true;
                }
            }
            var childErrors = model.Children.Where(child => child.HasError).ToList();
            if (childErrors.Any())
            {
                errorMessage = string.Join(Environment.NewLine, childErrors.Select(BuildErrorMessage));
            }

            return true;
        }

        public void AddMissingTempRecordSetList(IEnumerable<IRecordSetItemModel> tmpRecsetList)
        {
            foreach (var item in tmpRecsetList)
            {
                if (item.Children.Count == 0)
                {
                    item.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel(item));
                }
                item.IsVisible = _vm.IsItemVisible(item.Name);
                if (_vm.RecsetCollectionCount > 0)
                {
                    _vm.RecsetCollection.Insert(_vm.RecsetCollectionCount - 1, item);
                }
                else
                {
                    _vm.Add(item);
                }
            }
        }

        public void AddMissingTempRecordSet(IDataListVerifyPart part, IRecordSetItemModel tmpRecset)
        {
            var child = DataListItemModelFactory.CreateRecordSetFieldItemModel(part.Field, part.Description, tmpRecset);
            if (child != null)
            {
                child.DisplayName = part.Recordset + "()." + part.Field;
                tmpRecset.Children.Add(child);
            }
        }

        public void AddMissingRecordSetPart(IRecordSetItemModel recsetToAddTo, IDataListVerifyPart part)
        {
            if (recsetToAddTo.Children.FirstOrDefault(c => c.DisplayName == part.Field) == null)
            {
                var child = DataListItemModelFactory.CreateRecordSetFieldItemModel(part.Field, part.Description, recsetToAddTo);
                child.IsVisible = _vm.IsItemVisible(child.Name);
                if (recsetToAddTo.Children.Count > 0)
                {
                    recsetToAddTo.Children.Insert(recsetToAddTo.Children.Count - 1, child);
                }
                else
                {
                    recsetToAddTo.Children.Add(child);
                }
            }
        }

        public void RemoveUnusedRecordSets()
        {
            var unusedRecordsets = _vm.RecsetCollection.Where(c => !c.IsUsed).ToList();
            if (unusedRecordsets.Any())
            {
                foreach (var dataListItemModel in unusedRecordsets)
                {
                    _vm.Remove(dataListItemModel);
                }
            }
            foreach (var recset in _vm.RecsetCollection)
            {
                if (recset.Children.Count <= 0)
                {
                    continue;
                }

                var unusedRecsetChildren = recset.Children.Where(c => !c.IsUsed).ToList();
                if (!unusedRecsetChildren.Any())
                {
                    continue;
                }

                foreach (var unusedRecsetChild in unusedRecsetChildren)
                {
                    recset.Children.Remove(unusedRecsetChild);
                }
            }
        }

        #endregion

        static void FixCommonNamingProblems(IDataListItemModel recset)
        {
            if (recset.DisplayName.Contains("[") || recset.DisplayName.Contains("]"))
            {
                recset.DisplayName = recset.DisplayName.Replace("[", "").Replace("]", "");
            }
        }

        void CreateColumns(XmlNode subc, IRecordSetItemModel recset)
        {
            if (subc.Attributes != null)
            {
                var child = DataListItemModelFactory.CreateDataListModel(new Models.DataList.ItemModel(Common.ParseIsEditable(subc.Attributes[Common.IsEditable])), subc.Name, Common.ParseDescription(subc.Attributes[Common.Description]), recset, Common.ParseColumnIODirection(subc.Attributes[GlobalConstants.DataListIoColDirection]));
                recset.Children.Add(child);
            }
            else
            {
                var child = DataListItemModelFactory.CreateDataListModel(subc.Name, Common.ParseDescription(null), Common.ParseColumnIODirection(null), recset);
                child.IsEditable = Common.ParseIsEditable(null);
                recset.Children.Add(child);
            }
        }


        static string BuildErrorMessage(IDataListItemModel model) => DataListUtil.AddBracketsToValueIfNotExist(model.DisplayName) + " : " + model.ErrorMessage;
    }
}