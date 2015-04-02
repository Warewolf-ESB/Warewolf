
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Enums;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Activities
{
    public class DsfGatherSystemInformationActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        #region Fields

        IGetSystemInformation _getSystemInformation;
        IIdentity _currentIdentity;

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
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ErrorResultTO allErrors = new ErrorResultTO();
        
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            toUpsert.IsDebug = (dataObject.IsDebugMode());
            toUpsert.ResourceID = dataObject.ResourceID;
            if(dataObject.ExecutingUser != null)
            {
                _currentIdentity = dataObject.ExecutingUser.Identity;
            }
            var indexCounter = 0;
            InitializeDebug(dataObject);
            try
            {
                CleanArgs();

                foreach(GatherSystemInformationTO item in SystemInformationCollection)
                {
                    try
                    {
                        indexCounter++;

                        if(dataObject.IsDebugMode())
                        {
                            var inputToAdd = new DebugItem();
                            AddDebugItem(new DebugItemStaticDataParams("", indexCounter.ToString(CultureInfo.InvariantCulture)), inputToAdd);
                            AddDebugItem(new DebugItemStaticDataParams("", item.Result, "", "="), inputToAdd);
                            AddDebugItem(new DebugItemStaticDataParams(item.EnTypeOfSystemInformation.GetDescription(), ""), inputToAdd);
                            _debugInputs.Add(inputToAdd);
                        }

                        var hasErrors = allErrors.HasErrors();
                        if(!hasErrors)
                        {
                            string val = GetCorrectSystemInformation(item.EnTypeOfSystemInformation);
                            string expression = item.Result;

                            foreach(var region in DataListCleaningUtils.SplitIntoRegions(expression))
                            {
                                dataObject.Environment.Assign(region, val);
                            }
                        }
                    }
                    catch(Exception err)
                    {
                        dataObject.Environment.Assign(item.Result, null);
                        allErrors.AddError(err.Message);
                    }
                }


               
              

                if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                {
                    int innerCount = 1;
                    foreach (GatherSystemInformationTO item in SystemInformationCollection)
                    {
                        var itemToAdd = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("","", innerCount.ToString(CultureInfo.InvariantCulture)), itemToAdd);
                        AddDebugItem(new DebugEvalResult(item.Result,"",dataObject.Environment), itemToAdd);
                        _debugOutputs.Add(itemToAdd);
                        innerCount++;
                    }
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("DSFGatherSystemInformationTool", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfExecuteCommandLineActivity", allErrors);
                    foreach(var error in allErrors.FetchErrors())
                    {
                        dataObject.Environment.AddError(error);
                    }
                }
                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        int innerCount = 1;
                        foreach (GatherSystemInformationTO item in SystemInformationCollection)
                        {
                            var itemToAdd = new DebugItem();
                            AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), itemToAdd);
                            AddDebugItem(new DebugEvalResult(item.Result, "", dataObject.Environment), itemToAdd);
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
                    return GetSystemInformation.GetUserRolesInformation(_currentIdentity);
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

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
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
                        GatherSystemInformationTO gatherSystemInformationTo = SystemInformationCollection.Last(c => !c.CanRemove());
                        int startIndex = SystemInformationCollection.IndexOf(gatherSystemInformationTo) + 1;
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
                    currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
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
            return SystemInformationCollection.Count(caseConvertTo => !caseConvertTo.CanRemove());
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
