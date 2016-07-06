using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dev2.Common;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.ViewModels.DataList;
using Warewolf.Storage;

namespace Dev2.Studio.Core.DataList
{
    public class RecordsetHandler : IRecordsetHandler
    {
        private readonly DataListViewModel _vm;

        public RecordsetHandler(DataListViewModel vm)
        {
            _vm = vm;
        }

        #region Implementation of IRecordsetHandler

        public IEnumerable<string> RefreshRecordSets(IEnumerable<IRecordSetItemModel> toList, IList<string> accList)
        {
            foreach (var dataListItemModel in toList)
            {
                if (!string.IsNullOrEmpty(dataListItemModel.DisplayName))
                {
                    var recsetAppend = DataListUtil.MakeValueIntoHighLevelRecordset(dataListItemModel.DisplayName);
                    var recsetStar = DataListUtil.MakeValueIntoHighLevelRecordset(dataListItemModel.DisplayName, true);

                    accList.Add(DataListUtil.AddBracketsToValueIfNotExist(recsetAppend));
                    accList.Add(DataListUtil.AddBracketsToValueIfNotExist(recsetStar));
                }
                foreach (var listItemModel in dataListItemModel.Children)
                {
                    if (!string.IsNullOrEmpty(listItemModel.Name))
                    {
                        var rec = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(dataListItemModel.DisplayName, listItemModel.DisplayName, ""));
                        if (ExecutionEnvironment.IsRecordsetIdentifier(rec))
                        {
                            accList.Add(DataListUtil.ReplaceRecordBlankWithStar(rec));
                            accList.Add(rec);
                        }
                    }
                }
                foreach (var listItemModel in _vm.RecsetCollection)
                {
                    if (!string.IsNullOrEmpty(listItemModel.DisplayName))
                    {
                        var rec = "[[" + listItemModel.DisplayName + "]]";
                        if (ExecutionEnvironment.IsScalar(rec))
                        {
                            accList.Add(rec);
                        }
                    }
                }

            }
            return accList;
        }

        public void AddRecordsetNamesIfMissing()
        {
            var recsetNum = _vm.RecsetCollection?.Count ?? 0;
            int recsetCount = 0;

            while (recsetCount < recsetNum)
            {
                IRecordSetItemModel recset = _vm.RecsetCollection?[recsetCount];

                if (!string.IsNullOrWhiteSpace(recset?.DisplayName))
                {
                    FixNamingForRecset(recset);
                    int childrenNum = recset.Children.Count;
                    int childrenCount = 0;

                    while (childrenCount < childrenNum)
                    {
                        IRecordSetFieldItemModel child = recset.Children[childrenCount];

                        if (!string.IsNullOrWhiteSpace(child?.DisplayName))
                        {
                            int indexOfDot = child.DisplayName.IndexOf(".", StringComparison.Ordinal);
                            if (indexOfDot > -1)
                            {
                                string recsetName = child.DisplayName.Substring(0, indexOfDot + 1);
                                child.DisplayName = child.DisplayName.Replace(recsetName, child.Parent.DisplayName + ".");
                            }
                            else
                            {
                                child.DisplayName = string.Concat(child.Parent.DisplayName, ".", child.DisplayName);
                            }
                            FixCommonNamingProblems(child);
                        }
                        childrenCount++;
                    }
                }
                recsetCount++;
            }
        }

        public void RemoveBlankRecordsets()
        {
            List<IRecordSetItemModel> blankList = _vm.RecsetCollection.Where(c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank).ToList();
            if (blankList.Count <= 1) return;
            _vm.RecsetCollection.Remove(blankList.First());
        }

        public void RemoveBlankRecordsetFields()
        {
            foreach (var recset in _vm.RecsetCollection)
            {
                List<IRecordSetFieldItemModel> blankChildList = recset.Children.Where(c => c.IsBlank).ToList();
                if (blankChildList.Count <= 1) continue;
                recset.Children.Remove(blankChildList.First());
            }
        }

        public void ValidateRecordsetChildren(IRecordSetItemModel recset)
        {
            CheckForEmptyRecordset();
            if (recset != null)
            {
                Common.CheckDataListItemsForDuplicates(recset.Children);
            }
            CheckForFixedEmptyRecordsets();
        }

        public void ValidateRecordset()
        {
            CheckForEmptyRecordset();
            Common.CheckDataListItemsForDuplicates(_vm.DataList);
            CheckForFixedEmptyRecordsets();
        }

        public bool RecordSetHasChildren(IRecordSetItemModel model)
        {
            return model.Children != null && model.Children.Count > 0;
        }

        public void CheckForEmptyRecordset()
        {
            foreach (var recordset in _vm.RecsetCollection.Where(c => c.Children.Count == 0 || c.Children.Count == 1 && string.IsNullOrEmpty(c.Children[0].DisplayName) && !string.IsNullOrEmpty(c.DisplayName)))
            {
                recordset.SetError(StringResources.ErrorMessageEmptyRecordSet);
            }
        }

        public void CheckForFixedEmptyRecordsets()
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
            List<IRecordSetItemModel> blankList = _vm.RecsetCollection.Where(c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank).ToList();

            if (blankList.Count == 0)
            {
                AddRecordSet();
            }

            foreach (var recset in _vm.RecsetCollection)
            {
                List<IRecordSetFieldItemModel> blankChildList = recset.Children.Where(c => c.IsBlank).ToList();
                if (blankChildList.Count != 0) continue;

                IRecordSetFieldItemModel newChild = DataListItemModelFactory.CreateRecordSetFieldItemModel(string.Empty);
                if (newChild != null)
                {
                    newChild.Parent = recset;
                    recset.Children.Add(newChild);
                }
            }
        }

        public void FixNamingForRecset(IDataListItemModel recset)
        {
            if (!recset.DisplayName.EndsWith("()"))
            {
                recset.DisplayName = string.Concat(recset.DisplayName, "()");
            }
            FixCommonNamingProblems(recset);
        }

        public void AddRecordSet()
        {
            IRecordSetItemModel recset = DataListItemModelFactory.CreateRecordSetItemModel(string.Empty);
            IRecordSetFieldItemModel childItem = DataListItemModelFactory.CreateRecordSetFieldItemModel(string.Empty);
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
                _vm.RecsetCollection.Add(recset);
            }
        }

        public void SortRecset(bool @ascending)
        {
            IList<IRecordSetItemModel> newRecsetCollection = @ascending ? _vm.RecsetCollection.OrderBy(c => c.DisplayName).ToList() : _vm.RecsetCollection.OrderByDescending(c => c.DisplayName).ToList();
            _vm.RecsetCollection.Clear();
            foreach (var item in newRecsetCollection.Where(c => !c.IsBlank))
            {
                _vm.RecsetCollection.Add(item);
            }
            AddRecordSet();
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

        public IRecordSetItemModel CreateRecordSet(XmlNode xmlNode)
        {
            IRecordSetItemModel recset;
            if (xmlNode.Attributes != null)
            {
                recset = DataListItemModelFactory.CreateRecordSetItemModel(xmlNode.Name, Common.ParseDescription(xmlNode.Attributes[Common.Description]), null, null, false, "", true, true, false, Common.ParseColumnIODirection(xmlNode.Attributes[GlobalConstants.DataListIoColDirection]));
                if (recset != null)
                {
                    recset.IsEditable = Common.ParseIsEditable(xmlNode.Attributes[Common.IsEditable]);
                    _vm.RecsetCollection.Add(recset);
                }
            }
            else
            {
                recset = DataListItemModelFactory.CreateRecordSetItemModel(xmlNode.Name, Common.ParseDescription(null));
                if (recset != null)
                {
                    recset.IsEditable = Common.ParseIsEditable(null);

                    _vm.RecsetCollection.Add(recset);
                }
            }
            return recset;
        }

        public void SetRecordSetItemsAsUsed()
        {
            if (_vm.RecsetCollection.Any(rc => rc.IsUsed == false))
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
            if (part.IsScalar) return;
            var recset = _vm.RecsetCollection.Where(c => c.DisplayName == part.Recordset).ToList();
            if (!recset.Any())
                missingDataParts.Add(part);
            else
            {
                if (!string.IsNullOrEmpty(part.Field) && recset[0].Children.Count(c => c.DisplayName == part.Field) == 0)
                    missingDataParts.Add(part);
            }
        }

        public bool BuildRecordSetErrorMessages(IRecordSetItemModel model, out string errorMessage)
        {
            errorMessage = "";
            if (!RecordSetHasChildren(model)) return false;
            if (model.HasError)
            {
                {
                    errorMessage = BuildErrorMessage(model);
                    return true;
                }
            }
            var childErrors = model.Children.Where(child => child.HasError).ToList();
            if (childErrors.Any())
                errorMessage = string.Join(Environment.NewLine, childErrors.Select(BuildErrorMessage));
            return true;
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
                var child = DataListItemModelFactory.CreateDataListModel(subc.Name, Common.ParseDescription(subc.Attributes[Common.Description]), recset, false, "", Common.ParseIsEditable(subc.Attributes[Common.IsEditable]), true, false, Common.ParseColumnIODirection(subc.Attributes[GlobalConstants.DataListIoColDirection]));
                recset.Children.Add(child);
            }
            else
            {
                var child = DataListItemModelFactory.CreateDataListModel(subc.Name, Common.ParseDescription(null), Common.ParseColumnIODirection(null), recset);
                child.IsEditable = Common.ParseIsEditable(null);
                recset.Children.Add(child);
            }
        }
       

        private static string BuildErrorMessage(IDataListItemModel model)
        {
            return DataListUtil.AddBracketsToValueIfNotExist(model.DisplayName) + " : " + model.ErrorMessage;
        }
    }
}