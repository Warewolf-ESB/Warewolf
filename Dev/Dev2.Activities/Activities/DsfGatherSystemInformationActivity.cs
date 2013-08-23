using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Data.Enums;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfGatherSystemInformationActivity : DsfActivityAbstract<string>, ICollectionActivity

    {
        #region Fields

        IGetSystemInformation _getSystemInformation;
        private int _indexCounter = 0;

        #endregion

        /// <summary>
        /// The property that holds all the convertions
        /// </summary>
        public IList<GatherSystemInformationTO> SystemInformationCollection { get; set; }

       public IGetSystemInformation GetSystemInformation
        {
            get
            {
                return _getSystemInformation ?? (_getSystemInformation = new GetSystemInformationHelper());
            }
            set
            {
                _getSystemInformation = value;
            }
        }       

        #region Overrides of DsfNativeActivity<string>

        public DsfGatherSystemInformationActivity()
            : base("Gather System Information")
        {
            SystemInformationCollection = new List<GatherSystemInformationTO>();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        private void CleanArgs()
        {
            int count = 0;
            while (count < SystemInformationCollection.Count)
            {
                if (string.IsNullOrWhiteSpace(SystemInformationCollection[count].Result))
                {
                    SystemInformationCollection.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }
        }
        /// <summary>
        /// When overridden runs the activity's execution logic
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid executionId = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            try
            {
                CleanArgs();
                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                foreach (GatherSystemInformationTO item in SystemInformationCollection)
                {
                    _indexCounter++;

                    IBinaryDataListEntry tmp = compiler.Evaluate(executionId, enActionType.User, item.Result, false, out errors);
                    allErrors.MergeErrors(errors);
                    if (tmp != null)
                    {
                        IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(tmp);
                        int indexToUpsertTo = 1;
                        while (itr.HasMoreRecords())
                        {
                            IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                            foreach (IBinaryDataListItem c in cols)
                            {
                                indexToUpsertTo = c.ItemCollectionIndex;//2013.02.13: Ashley Lewis - Bug 8725, Task 8836
                                string val = GetCorrectSystemInformation(item.EnTypeOfSystemInformation);
                                string expression = item.Result;

                                if (DataListUtil.IsValueRecordset(item.Result) && DataListUtil.GetRecordsetIndexType(item.Result) == enRecordsetIndexType.Star)
                                {
                                    expression = item.Result.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                                    //indexToUpsertTo++;(2013.02.13: Ashley Lewis - Bug 8725, Task 8836)
                                }
                                //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                                foreach (var region in DataListCleaningUtils.SplitIntoRegions(expression))
                                {
                                    toUpsert.Add(region, val);
                                    if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                                    {
                                        AddDebugOutputItem(region, val, item.EnTypeOfSystemInformation);
                                    }
                                }
                            }
                            compiler.Upsert(executionId, toUpsert, out errors);
                            allErrors.MergeErrors(errors);
                            toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfExecuteCommandLineActivity", allErrors);
                    compiler.UpsertSystemTag(executionId, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    DispatchDebugState(context,StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
           
        }

        public string GetCorrectSystemInformation(enTypeOfSystemInformationToGather enTypeOfSystemInformation)
        {
            switch(enTypeOfSystemInformation)
            {
                case enTypeOfSystemInformationToGather.OperatingSystem:
                    return GetSystemInformation.GetOperatingSystemInformation();
                case enTypeOfSystemInformationToGather.ServicePack:
                    return GetSystemInformation.GetServicePackInformation();
                case enTypeOfSystemInformationToGather.OSBitValue:
                    return GetSystemInformation.GetOSBitValueInformation();
                case enTypeOfSystemInformationToGather.FullDateTime:
                    return GetSystemInformation.GetFullDateTimeInformation();
                case enTypeOfSystemInformationToGather.DateTimeFormat:
                    return GetSystemInformation.GetDateTimeFormatInformation();
                case enTypeOfSystemInformationToGather.DiskAvailable:
                    return GetSystemInformation.GetDiskSpaceAvailableInformation();
                case enTypeOfSystemInformationToGather.DiskTotal:
                    return GetSystemInformation.GetDiskSpaceTotalInformation();
                case enTypeOfSystemInformationToGather.PhysicalMemoryAvailable:
                    return GetSystemInformation.GetPhysicalMemoryAvailableInformation();
                case enTypeOfSystemInformationToGather.PhysicalMemoryTotal:
                    return GetSystemInformation.GetPhysicalMemoryTotalInformation();
                case enTypeOfSystemInformationToGather.CPUAvailable:
                    return GetSystemInformation.GetCPUAvailableInformation();
                case enTypeOfSystemInformationToGather.CPUTotal:
                    return GetSystemInformation.GetCPUTotalInformation();
                case enTypeOfSystemInformationToGather.Language:
                    return GetSystemInformation.GetLanguageInformation();
                case enTypeOfSystemInformationToGather.Region:
                    return GetSystemInformation.GetRegionInformation();
                case enTypeOfSystemInformationToGather.UserRoles:
                    return GetSystemInformation.GetUserRolesInformation();
                case enTypeOfSystemInformationToGather.UserName:
                    return GetSystemInformation.GetUserNameInformation();
                case enTypeOfSystemInformationToGather.Domain:
                    return GetSystemInformation.GetDomainInformation();
                case enTypeOfSystemInformationToGather.NumberOfWarewolfAgents:
                    return GetSystemInformation.GetNumberOfWareWolfAgentsInformation();
                default:
                    throw new ArgumentOutOfRangeException("enTypeOfSystemInformation");
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
           
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
           
        }

        #region Overrides of DsfNativeActivity<string>

//        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
//        {
//            foreach (IDebugItem debugInput in _debugInputs)
//            {
//                debugInput.FlushStringBuilder();
//            }
//            return _debugInputs;
//        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion

        #region Private Methods

//        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
//        {
//            DebugItem itemToAdd = new DebugItem();
//
//            if (!string.IsNullOrWhiteSpace(labelText))
//            {
//                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
//            }
//
//            if (valueEntry != null)
//            {
//                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
//            }
//
//            _debugInputs.Add(itemToAdd);
//        }

        private void AddDebugOutputItem(string expression, string value, enTypeOfSystemInformationToGather informationToGather)
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = _indexCounter.ToString(CultureInfo.InvariantCulture) });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = expression});
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = informationToGather.GetDescription() });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value =value });
            _debugOutputs.Add(itemToAdd);
        }

        private void InsertToCollection(IList<string> listToAdd, ModelItem modelItem)
        {
            ModelItemCollection mic = modelItem.Properties["SystemInformationCollection"].Collection;

            if (mic != null)
            {
                List<GatherSystemInformationTO> listOfValidRows = SystemInformationCollection.Where(c => !c.CanRemove()).ToList();
                if (listOfValidRows.Count > 0)
                {
                    int startIndex = SystemInformationCollection.Last(c => !c.CanRemove()).IndexNumber;
                    foreach (string s in listToAdd)
                    {
                        mic.Insert(startIndex, new GatherSystemInformationTO(SystemInformationCollection[startIndex - 1].EnTypeOfSystemInformation, s, startIndex + 1));
                        startIndex++;
                    }
                    CleanUpCollection(mic, modelItem, startIndex);
                }
                else
                {
                    AddToCollection(listToAdd, modelItem);
                }
            }
        }

        private void AddToCollection(IList<string> listToAdd, ModelItem modelItem)
        {
            ModelItemCollection mic = modelItem.Properties["SystemInformationCollection"].Collection;

            if (mic != null)
            {
                int startIndex = 0;
                enTypeOfSystemInformationToGather enTypeOfSystemInformation = enTypeOfSystemInformationToGather.FullDateTime;
                mic.Clear();
                foreach (string s in listToAdd)
                {
                    mic.Add(new GatherSystemInformationTO(enTypeOfSystemInformation, s, startIndex + 1));
                    startIndex++;
                }
                CleanUpCollection(mic, modelItem, startIndex);
            }
        }

        private void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if (startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, string.Empty, startIndex + 1));
            modelItem.Properties["DisplayName"].SetValue(CreateDisplayName(modelItem, startIndex + 1));
        }

        private string CreateDisplayName(ModelItem modelItem, int count)
        {
            string currentName = modelItem.Properties["DisplayName"].ComputedValue as string;
            if (currentName.Contains("(") && currentName.Contains(")"))
            {
                if (currentName.Contains(" ("))
                {
                    currentName = currentName.Remove(currentName.IndexOf(" ("));
                }
                else
                {
                    currentName = currentName.Remove(currentName.IndexOf("("));
                }
            }
            currentName = currentName + " (" + (count - 1) + ")";
            return currentName;
        }
        #endregion

        #endregion

        #region Implementation of ICollectionActivity

        public int GetCollectionCount()
        {
            return SystemInformationCollection.Count(caseConvertTO => !caseConvertTO.CanRemove());
        }

        public void AddListToCollection(IList<string> listToAdd, bool overwrite, ModelItem modelItem)
        {
            if (!overwrite)
            {
                InsertToCollection(listToAdd, modelItem);
            }
            else
            {
                AddToCollection(listToAdd, modelItem);
            }
        }

        #endregion
    }

    
}