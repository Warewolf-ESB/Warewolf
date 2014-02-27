using Dev2.Activities.Debug;
using Dev2.Data.Enums;
using Dev2.Data.Factories;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfGatherSystemInformationActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        #region Fields

        IGetSystemInformation _getSystemInformation;
        private int _indexCounter;

        #endregion

        /// <summary>
        /// The property that holds all the conversions
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

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        // ReSharper restore RedundantOverridenMember

        private void CleanArgs()
        {
            int count = 0;
            while(count < SystemInformationCollection.Count)
            {
                if(string.IsNullOrWhiteSpace(SystemInformationCollection[count].Result))
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
            ErrorResultTO errors;
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            toUpsert.IsDebug = (dataObject.IsDebugMode());
            toUpsert.ResourceID = dataObject.ResourceID;

            InitializeDebug(dataObject);
            try
            {
                CleanArgs();

                foreach(GatherSystemInformationTO item in SystemInformationCollection)
                {
                    _indexCounter++;

                    IBinaryDataListEntry resultEntry = compiler.Evaluate(executionId, enActionType.User, item.Result, false, out errors);
                    allErrors.MergeErrors(errors);

                    var inputToAdd = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", _indexCounter.ToString(CultureInfo.InvariantCulture)), inputToAdd);
                    AddDebugItem(DebugUtil.EvaluateEmptyRecordsetBeforeAddingToDebugOutput(item.Result, "", executionId), inputToAdd);
                    AddDebugItem(new DebugItemStaticDataParams(GetCorrectSystemInformation(item.EnTypeOfSystemInformation), ""), inputToAdd);
                    _debugInputs.Add(inputToAdd);

                    var hasErrors = allErrors.HasErrors();
                    if(resultEntry != null && !hasErrors)
                    {
                        IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(resultEntry);
                        while(itr.HasMoreRecords())
                        {
                            IList<IBinaryDataListItem> cols = itr.FetchNextRowData();

                            if(cols != null)
                            {
                                string val = GetCorrectSystemInformation(item.EnTypeOfSystemInformation);
                                string expression = item.Result;

                                foreach(var region in DataListCleaningUtils.SplitIntoRegions(expression))
                                {
                                    toUpsert.Add(region, val);
                                }
                            }
                        }
                    }
                }

                compiler.Upsert(executionId, toUpsert, out errors);
                allErrors.MergeErrors(errors);

                if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                {
                    int innerCount = 1;
                    foreach(DebugOutputTO debugOutputTO in toUpsert.DebugOutputs)
                    {
                        var itemToAdd = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), itemToAdd);
                        AddDebugItem(new DebugItemVariableParams(debugOutputTO, ""), itemToAdd);
                        _debugOutputs.Add(itemToAdd);
                        innerCount++;
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
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfExecuteCommandLineActivity", allErrors);
                    compiler.UpsertSystemTag(executionId, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        int innerCount = 1;
                        foreach(DebugOutputTO debugOutputTO in toUpsert.DebugOutputs)
                        {
                            var itemToAdd = new DebugItem();
                            AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), itemToAdd);
                            AddDebugItem(new DebugItemVariableParams(debugOutputTO, ""), itemToAdd);
                            _debugOutputs.Add(itemToAdd);
                            innerCount++;
                        }
                    }

                    DispatchDebugState(context, StateType.Before);
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

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var enumerable = SystemInformationCollection.Select(to => to.Result);
            return GetForEachItems(enumerable.ToArray());
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var enumerable = SystemInformationCollection.Select(to => to.Result);
            return GetForEachItems(enumerable.ToArray());
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    Tuple<string, string> t1 = t;
                    var items = SystemInformationCollection.Where(c => !string.IsNullOrEmpty(c.Result) && c.Result.Equals(t1.Item1));

                    // issues updates
                    foreach(var a in items)
                    {
                        a.Result = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    Tuple<string, string> t1 = t;
                    var items = SystemInformationCollection.Where(c => !string.IsNullOrEmpty(c.Result) && c.Result.Equals(t1.Item1));

                    // issues updates
                    foreach(var a in items)
                    {
                        a.Result = t.Item2;
                    }
                }
            }
        }

        #region Overrides of DsfNativeActivity<string>

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion

        #region Private Methods

        private void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["SystemInformationCollection"];
            if(modelProperty != null)
            {
                ModelItemCollection mic = modelProperty.Collection;

                if(mic != null)
                {
                    List<GatherSystemInformationTO> listOfValidRows = SystemInformationCollection.Where(c => !c.CanRemove()).ToList();
                    if(listOfValidRows.Count > 0)
                    {
                        GatherSystemInformationTO gatherSystemInformationTO = SystemInformationCollection.Last(c => !c.CanRemove());
                        int startIndex = SystemInformationCollection.IndexOf(gatherSystemInformationTO) + 1;
                        foreach(string s in listToAdd)
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
        }

        private void AddToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["SystemInformationCollection"];
            if(modelProperty != null)
            {
                ModelItemCollection mic = modelProperty.Collection;

                if(mic != null)
                {
                    int startIndex = 0;
                    const enTypeOfSystemInformationToGather EnTypeOfSystemInformation = enTypeOfSystemInformationToGather.FullDateTime;
                    mic.Clear();
                    foreach(string s in listToAdd)
                    {
                        mic.Add(new GatherSystemInformationTO(EnTypeOfSystemInformation, s, startIndex + 1));
                        startIndex++;
                    }
                    CleanUpCollection(mic, modelItem, startIndex);
                }
            }
        }

        private void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if(startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, string.Empty, startIndex + 1));
            var modelProperty = modelItem.Properties["DisplayName"];
            if(modelProperty != null)
            {
                modelProperty.SetValue(CreateDisplayName(modelItem, startIndex + 1));
            }
        }

        private string CreateDisplayName(ModelItem modelItem, int count)
        {
            var modelProperty = modelItem.Properties["DisplayName"];
            if(modelProperty != null)
            {
                string currentName = modelProperty.ComputedValue as string;
                if(currentName != null && (currentName.Contains("(") && currentName.Contains(")")))
                {
                    if(currentName.Contains(" ("))
                    {
                        currentName = currentName.Remove(currentName.IndexOf(" (", StringComparison.Ordinal));
                    }
                    else
                    {
                        currentName = currentName.Remove(currentName.IndexOf("(", StringComparison.Ordinal));
                    }
                }
                currentName = currentName + " (" + (count - 1) + ")";
                return currentName;
            }

            return string.Empty;
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
            if(!overwrite)
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