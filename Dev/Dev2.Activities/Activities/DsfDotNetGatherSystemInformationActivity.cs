/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("Utility-SystemInformation", "Sys Info", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Sys_Info")]
    public class DsfDotNetGatherSystemInformationActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        IGetSystemInformation _getSystemInformation;
        IIdentity _currentIdentity;

        public IList<GatherSystemInformationTO> SystemInformationCollection { get; set; }

        public IGetSystemInformation GetSystemInformation
        {
            get => _getSystemInformation ?? (_getSystemInformation = new GetSystemInformationStandardHelper());
            set
            {
                _getSystemInformation = value;
            }
        }

        public override List<string> GetOutputs() => SystemInformationCollection.Select(to => to.Result).ToList();

        public DsfDotNetGatherSystemInformationActivity()
            : base("Gather System Information")
        {
            SystemInformationCollection = new List<GatherSystemInformationTO>();
        }

        void CleanArgs()
        {
            var count = 0;
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

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var allErrors = new ErrorResultTO();

            if (dataObject.ExecutingUser != null)
            {
                _currentIdentity = dataObject.ExecutingUser.Identity;
            }
            InitializeDebug(dataObject);
            try
            {
                TryExecute(dataObject, update, allErrors);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("DSFGatherSystemInformationTool", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                HandleErrors(dataObject, update, allErrors);
            }
        }

        private void TryExecute(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            CleanArgs();

            var indexCounter = 0;
            foreach (GatherSystemInformationTO item in SystemInformationCollection)
            {
                try
                {
                    indexCounter++;

                    if (dataObject.IsDebugMode())
                    {
                        var inputToAdd = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", indexCounter.ToString(CultureInfo.InvariantCulture)), inputToAdd);
                        AddDebugItem(new DebugItemStaticDataParams("", dataObject.Environment.EvalToExpression(item.Result, update), "", "="), inputToAdd);
                        AddDebugItem(new DebugItemStaticDataParams(item.EnTypeOfSystemInformation.GetDescription(), ""), inputToAdd);
                        _debugInputs.Add(inputToAdd);
                    }

                    if (!allErrors.HasErrors())
                    {
                        HandleNoErrorsFound(dataObject, update, allErrors, item);
                    }
                }
                catch (Exception err)
                {
                    dataObject.Environment.Assign(item.Result, null, update);
                    allErrors.AddError(err.Message);
                }
            }
            dataObject.Environment.CommitAssign();
            if (dataObject.IsDebugMode() && !allErrors.HasErrors())
            {
                var innerCount = 1;
                foreach (GatherSystemInformationTO item in SystemInformationCollection)
                {
                    var itemToAdd = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", "", innerCount.ToString(CultureInfo.InvariantCulture)), itemToAdd);
                    AddDebugItem(new DebugEvalResult(item.Result, "", dataObject.Environment, update), itemToAdd);
                    _debugOutputs.Add(itemToAdd);
                    innerCount++;
                }
            }
        }

        void HandleNoErrorsFound(IDSFDataObject dataObject, int update, ErrorResultTO allErrors, GatherSystemInformationTO item)
        {
            var val = GetCorrectSystemInformation(item.EnTypeOfSystemInformation);
            var expression = item.Result;

            var regions = DataListCleaningUtils.SplitIntoRegions(expression);
            if (regions.Count > 1)
            {
                allErrors.AddError(ErrorResource.MultipleVariablesInResultField);
            }
            else
            {
                foreach (var region in regions)
                {
                    dataObject.Environment.AssignWithFrame(new AssignValue(region, val), update);
                }
            }
        }

        void HandleErrors(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            var hasErrors = allErrors.HasErrors();
            if (hasErrors)
            {
                DisplayAndWriteError(nameof(DsfDotNetGatherSystemInformationActivity), allErrors);
                foreach (var error in allErrors.FetchErrors())
                {
                    dataObject.Environment.AddError(error);
                }
            }
            if (dataObject.IsDebugMode())
            {
                if (hasErrors)
                {
                    var innerCount = 1;
                    foreach (GatherSystemInformationTO item in SystemInformationCollection)
                    {
                        var itemToAdd = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), itemToAdd);
                        AddDebugItem(new DebugEvalResult(item.Result, "", dataObject.Environment, update), itemToAdd);
                        _debugOutputs.Add(itemToAdd);
                        innerCount++;
                    }
                }

                DispatchDebugState(dataObject, StateType.Before, update);
                DispatchDebugState(dataObject, StateType.After, update);
            }
        }

        public string GetCorrectSystemInformation(enTypeOfSystemInformationToGather enTypeOfSystemInformation)
        {
            switch (enTypeOfSystemInformation)
            {
                case enTypeOfSystemInformationToGather.ComputerName:
                    return GetSystemInformation.GetComputerName();
                case enTypeOfSystemInformationToGather.OperatingSystem:
                    return GetSystemInformation.GetOperatingSystemInformation();
                case enTypeOfSystemInformationToGather.OperatingSystemVersion:
                    return GetSystemInformation.GetOperatingSystemVersionInformation();
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
                case enTypeOfSystemInformationToGather.VirtualMemoryAvailable:
                    return GetSystemInformation.GetVirtualMemoryAvailableInformation();
                case enTypeOfSystemInformationToGather.VirtualMemoryTotal:
                    return GetSystemInformation.GetVirtualMemoryTotalInformation();
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
                case enTypeOfSystemInformationToGather.NumberOfServerNICS:
                    return GetSystemInformation.GetNumberOfNICS();
                case enTypeOfSystemInformationToGather.MacAddress:
                    return GetSystemInformation.GetMACAdresses();
                case enTypeOfSystemInformationToGather.GateWayAddress:
                    return GetSystemInformation.GetDefaultGateway();
                case enTypeOfSystemInformationToGather.DNSAddress:
                    return GetSystemInformation.GetDNSServer();
                case enTypeOfSystemInformationToGather.IPv4Address:
                    return GetSystemInformation.GetIPv4Adresses();
                case enTypeOfSystemInformationToGather.IPv6Address:
                    return GetSystemInformation.GetIPv6Adresses();
                case enTypeOfSystemInformationToGather.WarewolfMemory:
                    return GetSystemInformation.GetWarewolfServerMemory();
                case enTypeOfSystemInformationToGather.WarewolfCPU:
                    return GetSystemInformation.GetWarewolfCPU();
                case enTypeOfSystemInformationToGather.WarewolfServerVersion:
                    return GetSystemInformation.GetWareWolfVersion();
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

        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if (updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {
                    var t1 = t;
                    var items = SystemInformationCollection.Where(c => !string.IsNullOrEmpty(c.Result) && c.Result.Equals(t1.Item1));

                    foreach (var a in items)
                    {
                        a.Result = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            if (updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {
                    var t1 = t;
                    var items = SystemInformationCollection.Where(c => !string.IsNullOrEmpty(c.Result) && c.Result.Equals(t1.Item1));

                    foreach (var a in items)
                    {
                        a.Result = t.Item2;
                    }
                }
            }
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update) => _debugInputs;

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["SystemInformationCollection"];
            var mic = modelProperty?.Collection;
            if (mic == null)
            {
                return;
            }
            var listOfValidRows = SystemInformationCollection.Where(c => !c.CanRemove()).ToList();
            if (listOfValidRows.Count > 0)
            {
                var gatherSystemInformationTo = SystemInformationCollection.Last(c => !c.CanRemove());
                var startIndex = SystemInformationCollection.IndexOf(gatherSystemInformationTo) + 1;
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

        void AddToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["SystemInformationCollection"];
            var mic = modelProperty?.Collection;
            if (mic == null)
            {
                return;
            }
            var startIndex = 0;
            const enTypeOfSystemInformationToGather EnTypeOfSystemInformation = enTypeOfSystemInformationToGather.FullDateTime;
            mic.Clear();
            foreach (string s in listToAdd)
            {
                mic.Add(new GatherSystemInformationTO(EnTypeOfSystemInformation, s, startIndex + 1));
                startIndex++;
            }
            CleanUpCollection(mic, modelItem, startIndex);
        }

        void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if (startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, string.Empty, startIndex + 1));
            var modelProperty = modelItem.Properties["DisplayName"];
            if (modelProperty != null)
            {
                modelProperty.SetValue(CreateDisplayName(modelItem, startIndex + 1));
            }
        }

        string CreateDisplayName(ModelItem modelItem, int count)
        {
            var modelProperty = modelItem.Properties["DisplayName"];
            if (modelProperty != null)
            {
                var currentName = modelProperty.ComputedValue as string;
                if (currentName != null && currentName.Contains("(") && currentName.Contains(")"))
                {
                    currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
                }
                currentName = currentName + " (" + (count - 1) + ")";
                return currentName;
            }

            return string.Empty;
        }

        public int GetCollectionCount() => SystemInformationCollection.Count(caseConvertTo => !caseConvertTo.CanRemove());

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

        public bool Equals(DsfDotNetGatherSystemInformationActivity other)
        {
            var eq = this.DisplayName.Equals(other.DisplayName);
            if (!(this._getSystemInformation is null))
            {
                eq &= this._getSystemInformation.Equals(other._getSystemInformation);
            }
            if (!(this._currentIdentity is null)) {
                eq &= this._currentIdentity.Equals(other._currentIdentity);
            }

            return eq;
        }

        public override bool Equals(object obj)
        {
            if (obj is DsfDotNetGatherSystemInformationActivity instance)
            {
                return Equals(instance);
            }
            return false;
        }
    }
}
