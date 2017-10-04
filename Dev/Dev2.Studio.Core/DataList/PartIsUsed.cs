using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common;
using Dev2.Data.Interfaces;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using ServiceStack.Common.Extensions;

namespace Dev2.Studio.Core.DataList
{
    public class PartIsUsed : IPartIsUsed
    {
        readonly ObservableCollection<IRecordSetItemModel> _recsetCollection;
        private readonly ObservableCollection<IScalarItemModel> _scalarCollection;
        private readonly ObservableCollection<IComplexObjectItemModel> _complexObjectItemModels;

        public PartIsUsed(ObservableCollection<IRecordSetItemModel> recsetCollection, ObservableCollection<IScalarItemModel> scalarCollection, ObservableCollection<IComplexObjectItemModel> complexObjectItemModels)
        {
            _recsetCollection = recsetCollection;
            _scalarCollection = scalarCollection;
            _complexObjectItemModels = complexObjectItemModels;
        }

        public void SetScalarPartIsUsed(IDataListVerifyPart part, bool isUsed)
        {
            var scalarsToRemove = _scalarCollection.Where(c => c.DisplayName == part.Field);
            scalarsToRemove.ToList().ForEach(scalarToRemove =>
            {
                scalarToRemove.IsUsed = isUsed;
            });
        }

        public void SetRecordSetPartIsUsed(IDataListVerifyPart part, bool isUsed)
        {
            var recsetsToRemove = _recsetCollection.Where(c => c.DisplayName == part.Recordset);
            recsetsToRemove.ToList().ForEach(recsetToRemove => ProcessFoundRecordSets(part, recsetToRemove, isUsed));
        }

        public void SetComplexObjectSetPartIsUsed(IDataListVerifyPart part, bool isUsed)
        {
            var objects = _complexObjectItemModels.Flatten(model => model.Children).Where(model => model.DisplayName == part.DisplayValue.Trim('@'));
            objects.ForEach(model =>
            {
                model.IsUsed = isUsed;
            });
        }

        private static void ProcessFoundRecordSets(IDataListVerifyPart part, IRecordSetItemModel recsetToRemove, bool isUsed)
        {
            if (recsetToRemove == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(part.Field))
            {
                recsetToRemove.IsUsed = isUsed;
            }
            else
            {
                var childrenToRemove = recsetToRemove.Children.Where(c => c.DisplayName == part.Field);
                childrenToRemove.ToList().ForEach(childToRemove =>
                {
                    childToRemove.IsUsed = isUsed;
                });
            }
        }
    }
}