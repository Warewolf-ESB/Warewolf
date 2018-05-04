using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dev2.Common;
using Dev2.Data.Interfaces;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.ViewModels.DataList;

namespace Dev2.Studio.Core.DataList
{
    class ScalarHandler : IScalarHandler
    {
        readonly DataListViewModel _vm;

        public ScalarHandler(DataListViewModel dataListViewModel)
        {
            _vm = dataListViewModel;
        }

        #region Implementation of IScalarHandler

        public void FindMissingForScalar(IDataListVerifyPart part, List<IDataListVerifyPart> missingDataParts)
        {
            if (!part.IsScalar)
            {
                return;
            }

            if (!_vm.ScalarCollection.Any(c => c.DisplayName == part.Field))
            {
                missingDataParts.Add(part);
            }
        }

        public void SetScalarItemsAsUsed()
        {
            foreach (var dataListItemModel in _vm.ScalarCollection.Where(model => !model.IsUsed))
            {
                dataListItemModel.IsUsed = true;
            }
        }

        public void AddScalars(XmlNode xmlNode)
        {
            if (xmlNode.Attributes != null)
            {
                var scalar = DataListItemModelFactory.CreateScalarItemModel(xmlNode.Name, Common.ParseDescription(xmlNode.Attributes[Common.Description]), Common.ParseColumnIODirection(xmlNode.Attributes[GlobalConstants.DataListIoColDirection]));
                if (scalar != null)
                {
                    scalar.IsEditable = Common.ParseIsEditable(xmlNode.Attributes[Common.IsEditable]);
                    scalar.IsVisible = _vm.IsItemVisible(scalar.Name);
                    _vm.Add(scalar);
                }
            }
            else
            {
                var scalar = DataListItemModelFactory.CreateScalarItemModel(xmlNode.Name, Common.ParseDescription(null), Common.ParseColumnIODirection(null));
                if (scalar != null)
                {
                    scalar.IsEditable = Common.ParseIsEditable(null);
                    scalar.IsVisible = _vm.IsItemVisible(scalar.Name);
                    _vm.Add(scalar);
                }
            }
        }

        private void UpdateScalar(IScalarItemModel scalar)
        {
            scalar.IsVisible = _vm.IsItemVisible(scalar.Name);
            if (_vm.ScalarCollectionCount > 0)
            {
                _vm.ScalarCollection.Insert(_vm.ScalarCollectionCount - 1, scalar);
            }
            else
            {
                _vm.Add(scalar);
            }
        }

        public void SortScalars(bool ascending)
        {
            IList<IScalarItemModel> newScalarCollection = @ascending ? _vm.ScalarCollection.Where(c => !c.IsBlank).OrderBy(c => c.DisplayName).ToList() : _vm.ScalarCollection.Where(c => !c.IsBlank).OrderByDescending(c => c.DisplayName).ToList();
            for (int i = 0; i < newScalarCollection.Count; i++)
            {
                _vm.ScalarCollection.Move(_vm.ScalarCollection.IndexOf(newScalarCollection[i]), i);
            }            
        }

        public void FixNamingForScalar(IDataListItemModel scalar)
        {
            if (scalar.DisplayName.Contains("()"))
            {
                scalar.DisplayName = scalar.DisplayName.Replace("()", "");
            }
            FixCommonNamingProblems(scalar);
        }

        public void AddRowToScalars()
        {
            var blankList = _vm.ScalarCollection.Where(c => c.IsBlank).ToList();
            if (blankList.Count != 0)
            {
                return;
            }

            var scalar = DataListItemModelFactory.CreateScalarItemModel(string.Empty);
            _vm.Add(scalar);
        }

        public void RemoveBlankScalars()
        {
            var blankList = _vm.ScalarCollection.Where(c => c.IsBlank).ToList();
            if (blankList.Count <= 1)
            {
                return;
            }

            _vm.Remove(blankList.First());
        }

        public void RemoveUnusedScalars()
        {
            var unusedScalars = _vm.ScalarCollection.Where(c => !c.IsUsed).ToList();
            if (unusedScalars.Any())
            {
                foreach (var dataListItemModel in unusedScalars)
                {
                    _vm.Remove(dataListItemModel);
                }
            }
        }

        public void AddMissingScalarParts(IDataListVerifyPart part)
        {
            if (_vm.ScalarCollection.FirstOrDefault(c => c.DisplayName == part.Field) == null)
            {
                var scalar = DataListItemModelFactory.CreateScalarItemModel(part.Field, part.Description);
                UpdateScalar(scalar);
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
    }
}