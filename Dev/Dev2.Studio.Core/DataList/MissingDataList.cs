/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Core.DataList
{
    class MissingDataList : IMissingDataList
    {
        readonly ObservableCollection<IRecordSetItemModel> _recsetCollection;
        readonly ObservableCollection<IScalarItemModel> _scalarCollection;
        public MissingDataList(ObservableCollection<IRecordSetItemModel> recsetCollection, ObservableCollection<IScalarItemModel> scalarCollection)
        {
            _recsetCollection = recsetCollection;
            _scalarCollection = scalarCollection;
        }

        public IEnumerable<IDataListVerifyPart> MissingRecordsets(IList<IDataListVerifyPart> partsToVerify)
        {
            var missingWorkflowParts = new List<IDataListVerifyPart>();
            foreach (var dataListItem in _recsetCollection.Where(model => !string.IsNullOrEmpty(model.DisplayName)))
            {
                if (dataListItem.Children.Count > 0)
                {
                    AddIsEditablePartsToVerify(partsToVerify, missingWorkflowParts, dataListItem);
                }
                else
                {
                    if (!partsToVerify.Any(part => part.Field == dataListItem.DisplayName && part.IsScalar)&& dataListItem.IsEditable)
                    {
                        missingWorkflowParts.Add(
                            IntellisenseFactory.CreateDataListValidationScalarPart(dataListItem.DisplayName,
                                dataListItem.Description));
                    }
                }
            }
            return missingWorkflowParts;
        }

        private static void AddIsEditablePartsToVerify(IList<IDataListVerifyPart> partsToVerify, List<IDataListVerifyPart> missingWorkflowParts, IRecordSetItemModel dataListItem)
        {
            if (!partsToVerify.Any(part => part.Recordset == dataListItem.DisplayName) && dataListItem.IsEditable)
            {
                AddMissingWorkFlowRecordsetPart(missingWorkflowParts, dataListItem);
                foreach (var child in dataListItem.Children.Where(p => !string.IsNullOrEmpty(p.DisplayName)))
                {
                    AddMissingWorkFlowRecordsetPart(missingWorkflowParts, dataListItem, child);
                }
            }
            else
            {
                missingWorkflowParts.AddRange(
                    from child in dataListItem.Children
                    where !partsToVerify.Any(part => child.Parent != null && part.Field == child.DisplayName && part.Recordset == child.Parent.DisplayName) && child.IsEditable
                    select IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.DisplayName, child.DisplayName, child.Description));
            }
        }

        public IEnumerable<IDataListVerifyPart> MissingScalars(IEnumerable<IDataListVerifyPart> partsToVerify)
        {
            return (from dataListItem in _scalarCollection
                    where !string.IsNullOrEmpty(dataListItem.DisplayName)
                    where !partsToVerify.Any(part => part.Field == dataListItem.DisplayName && part.IsScalar)
                    where dataListItem.IsEditable
                    select IntellisenseFactory.CreateDataListValidationScalarPart(dataListItem.DisplayName, dataListItem.Description)).ToList();
        }

        static void AddMissingWorkFlowRecordsetPart(List<IDataListVerifyPart> missingWorkflowParts,
        IRecordSetItemModel dataListItem,
        IRecordSetFieldItemModel child = null)
        {
            if (dataListItem.IsEditable)
            {
                missingWorkflowParts.Add(child != null ? IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.DisplayName,
                            child.DisplayName, child.Description) : IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.DisplayName,
                                    string.Empty, dataListItem.Description));
            }
        }
    }
}
