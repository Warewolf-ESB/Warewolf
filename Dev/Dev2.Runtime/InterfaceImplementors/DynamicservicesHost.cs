
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


#region Change Log
//  Author:         Sameer Chunilall
//  Date:           2010-01-24
//  Log No:         9299
//  Description:    Provides a concrete representation of the xml service definitions
//                  The DynamicServices class contains a directory of available 
//                  services for consumption by clients.
//                  This class is hydrated with the data in the service definition file
//                  apon instantiation this is achieved by calling the LoadServices 
//                  method in the classes constructor.
#endregion

using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Runtime;
using Dev2.Runtime.InterfaceImplementors;
using Dev2.Runtime.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Unlimited.Framework;

// ReSharper disable CheckNamespace
namespace Dev2.DynamicServices
// ReSharper restore CheckNamespace
{

    #region Dynamic Endpoint Class - The Service Directory containing Endpoint, BizRules and Data Sources
    /// <summary>
    ///Provides a concrete representation of the xml service definitions
    ///The DynamicServices class contains a directory of available 
    ///services for consumption by clients.
    ///This class is hydrated with the data in the service definition file
    ///apon instantiation this is achieved by calling the LoadServices 
    ///method in the classes constructor.
    /// </summary>
    public class DynamicServicesHost : IDynamicServicesHost
    {
        #region Fields

        readonly string _workspacePath;

        private readonly ReaderWriterLockSlim _serviceLock;
        private readonly ReaderWriterLockSlim _reservedLock;
        private readonly ReaderWriterLockSlim _activityLock;
        private readonly ReaderWriterLockSlim _sourceLock;
        // ReSharper disable InconsistentNaming
        public List<string> compilerErrorList = new List<string>();
        // ReSharper restore InconsistentNaming
        readonly IFrameworkDuplexDataChannel _managementChannel;
        object _lockForFileRead = new object();

        #endregion

        #region Public Properties
        /// <summary>
        /// Contains all default sources loaded GetDefaultServices method
        /// </summary>
        public List<Source> ReservedSources { get; set; }
        /// <summary>
        /// Contains all default dynamic services loaded GetDefaultServices method
        /// </summary>
        public List<DynamicService> ReservedServices { get; set; }
        /// <summary>
        /// Contains all dynamic services loaded from the service definition file
        /// </summary>
        public List<DynamicService> Services { get; set; }
        /// <summary>
        /// Contains all the bizrules loaded from the service defintion file
        /// </summary>
        public List<BizRule> BizRules { get; set; }
        /// <summary>
        /// Contains all the data sources loaded from the service definition file
        /// </summary>
        public List<Source> Sources { get; set; }

        public List<WorkflowActivityDef> WorkflowActivityDefs { get; set; }

        public string WorkspacePath { get { return _workspacePath; } }

        #endregion

        #region Constructors
        /// <summary>
        /// Loads service definitions and initializes all services
        /// </summary>
        public DynamicServicesHost(IFrameworkDuplexDataChannel managementChannel = null, string workspacePath = null)
        {
            // 2012.10.17 - 5782: TWR - Introduced path to facilitate loading of multiple workspaces
            _workspacePath = workspacePath ?? string.Empty;

            _serviceLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _reservedLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _activityLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _sourceLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

            Services = new List<DynamicService>();
            BizRules = new List<BizRule>();
            Sources = new List<Source>();
            WorkflowActivityDefs = new List<WorkflowActivityDef>();

            if(managementChannel != null)
            {
                _managementChannel = managementChannel;
            }

            // 17.09.2012 : Travis.Frisinger - Changed to facilitate the current server stack.
            RestoreResources();


            // 2012.10.17 - 5782: TWR - Moved from DynamicServicesEndpoint constructor
            var defaultSources = GetDefaultSources();
            ReservedSources = defaultSources;
            defaultSources.ForEach(c => Sources.Add(c));

            var defaultServices = GetDefaultServices();
            ReservedServices = defaultServices;
            defaultServices.ForEach(c => Services.Add(c));
        }

        #endregion

        #region Private Methods

        private ServiceHydrationTO ExtractDataListIOMapping(string def)
        {

            // ReSharper disable SuggestUseVarKeywordEvident
            ServiceHydrationTO result = new ServiceHydrationTO();
            // ReSharper restore SuggestUseVarKeywordEvident

            result.DataList = null;
            result.InputMapping = null;
            result.OutputMapping = null;

            if(!string.IsNullOrEmpty(def))
            {

                // extract and set data list
                int start = def.IndexOf("<DataList>", StringComparison.Ordinal);
                if(start >= 0)
                {
                    int end = def.IndexOf("</DataList>", StringComparison.Ordinal);
                    if(end > start)
                    {
                        end += 11;
                        result.DataList = def.Substring(start, (end - start));
                    }
                }
            }

            return result;
        }

        private DynamicServiceObjectBase FindResource(string resourceName, string resourceType = "Service")
        {
            DynamicServiceObjectBase resource = null;

            switch(resourceType)
            {
                case "Service":
                    {
                        LockServices();
                        try
                        {
                            resource = Services.Find(service => service.Name == resourceName);
                        }
                        finally
                        {
                            UnlockServices();
                        }
                        break;
                    }
                case "Source":
                    {
                        LockSources();
                        try
                        {
                            resource = Sources.Find(source => source.Name == resourceName);
                        }
                        finally
                        {
                            UnlockSources();
                        }
                        break;
                    }
                case "WorkflowActivityDef":
                    {
                        LockActivities();
                        try
                        {
                            resource = WorkflowActivityDefs.Find(activity => activity.Name == resourceName);
                        }
                        finally
                        {
                            UnlockActivities();
                        }

                        break;
                    }
            }

            return resource;
        }

        private void SaveResource(string directoryName, DynamicServiceObjectBase resource)
        {
            Runtime.ServiceModel.Resources.Save(_workspacePath, directoryName, resource);
        }


        private void RemoveResource(string directoryName, DynamicServiceObjectBase resource)
        {
            RemoveResource(_workspacePath, directoryName, resource);
            if(!string.IsNullOrEmpty(_workspacePath))
            {
                // 6418: TWR - Deleting a workspace item so delete from the server too...
                RemoveResource(string.Empty, directoryName, resource);
            }
        }

        static void RemoveResource(string workspacePath, string directoryName, DynamicServiceObjectBase resource)
        {
            directoryName = Path.Combine(workspacePath, directoryName);
            if(Directory.Exists(directoryName))
            {
                string versionDirectory = string.Format("{0}\\{1}", directoryName, "VersionControl");

                if(!Directory.Exists(versionDirectory))
                {
                    Directory.CreateDirectory(versionDirectory);
                }

                // ReSharper disable TooWideLocalVariableScope
                int count;
                // ReSharper restore TooWideLocalVariableScope
                string fileName = string.Format("{0}\\{1}.xml", directoryName, resource.Name);

                if(File.Exists(fileName))
                {
                    count = Directory.GetFiles(versionDirectory, string.Format("{0}*.xml", resource.Name)).Count();
                    // ReSharper disable FormatStringProblem
                    File.Copy(fileName, string.Format("{1}\\{2}.V{3}.xml", directoryName, versionDirectory, resource.Name, (count + 1).ToString(CultureInfo.InvariantCulture)), true);
                    // ReSharper restore FormatStringProblem
                }

                string deletePath = string.Format("{0}\\{1}.xml", directoryName, resource.Name);
                if(File.Exists(deletePath)) File.Delete(deletePath);
            }
        }

        public void SendMessageToConnectedClients(string message)
        {
            if(_managementChannel != null)
            {
                _managementChannel.SendMessage("System", message);
            }
        }

        #endregion

        #region Public Methods

        #region Find

        public IDynamicServiceObject Find(string serviceName, enDynamicServiceObjectType serviceType)
        {
            DynamicServiceObjectBase resource = null;

            switch(serviceType)
            {
                case enDynamicServiceObjectType.DynamicService:
                    {
                        LockServices();
                        try
                        {
                            resource = Services.Find(service => service.Name == serviceName);
                        }
                        finally
                        {
                            UnlockServices();
                        }
                        break;
                    }
                case enDynamicServiceObjectType.Source:
                    {
                        LockSources();
                        try
                        {
                            resource = Sources.Find(source => source.Name == serviceName);
                        }
                        finally
                        {
                            UnlockSources();
                        }
                        break;
                    }
                case enDynamicServiceObjectType.WorkflowActivity:
                    {
                        LockActivities();
                        try
                        {
                            resource = WorkflowActivityDefs.Find(activity => activity.Name == serviceName);
                        }
                        finally
                        {
                            UnlockActivities();
                        }

                        break;
                    }
            }

            return resource;
        }

        #endregion

        public void LockServices()
        {
            _serviceLock.EnterReadLock();
        }

        public void UnlockServices()
        {
            _serviceLock.ExitReadLock();
        }

        public void LockReservedServices()
        {
            _reservedLock.EnterReadLock();
        }

        public void UnlockReservedServices()
        {
            _reservedLock.ExitReadLock();
        }

        public void LockReservedSources()
        {
            _reservedLock.EnterReadLock();
        }

        public void UnlockReservedSources()
        {
            _reservedLock.ExitReadLock();
        }

        public void LockSources()
        {
            _sourceLock.EnterReadLock();
        }

        public void UnlockSources()
        {
            _sourceLock.ExitReadLock();
        }

        public void LockActivities()
        {
            _activityLock.EnterReadLock();
        }

        public void UnlockActivities()
        {
            _activityLock.ExitReadLock();
        }

        #region Copy

        // 2012.10.19 - 5782: TWR
        public void CopyTo(string destWorkspacePath, bool overwrite = false, IList<string> filesToIgnore = null)
        {
            SyncTo(destWorkspacePath, overwrite, false, filesToIgnore);
        }

        /// <summary>
        /// Syncs the file in the current workspace to a destination workspace
        /// </summary>
        /// <param name="destWorkspacePath">The destination workspace path.</param>
        /// <param name="overwrite">if set to <c>true</c> override file in destination.</param>
        /// <param name="delete">if set to <c>true</c> delete files in destination that aren't present in current workspace.</param>
        /// <param name="filesToIgnore">The files to ignore.</param>
        public void SyncTo(string destWorkspacePath, bool overwrite = true, bool delete = true, IList<string> filesToIgnore = null)
        {
            if(filesToIgnore == null)
            {
                filesToIgnore = new List<string>();
            }

            // ReSharper disable SuggestUseVarKeywordEvident
            List<string> directories = new List<string> { "Sources", "Services" };
            // ReSharper restore SuggestUseVarKeywordEvident

            foreach(string directory in directories)
            {
                DirectoryInfo source = new DirectoryInfo(Path.Combine(_workspacePath, directory));
                DirectoryInfo destination = new DirectoryInfo(Path.Combine(destWorkspacePath, directory));

                if(!source.Exists)
                {
                    continue;
                }

                if(!destination.Exists)
                {
                    destination.Create();
                }

                //
                // Get the files from the source and desitnations folders, excluding the files which are to be ignored
                //
                List<FileInfo> sourceFiles = source.GetFiles()
                    .Where(f => !filesToIgnore.Contains(f.Name))
                    .ToList();
                List<FileInfo> destinationFiles = destination.GetFiles()
                    .Where(f => !filesToIgnore.Contains(f.Name))
                    .ToList();

                //
                // Calculate the files which are to be copied from source to destination, this respects the override parameter
                //

                // ReSharper disable SuggestUseVarKeywordEvident
                List<FileInfo> filesToCopyFromSource = new List<FileInfo>();
                // ReSharper restore SuggestUseVarKeywordEvident
                if(overwrite)
                {
                    filesToCopyFromSource.AddRange(sourceFiles);
                }
                else
                {
                    filesToCopyFromSource.AddRange(sourceFiles
                        // ReSharper disable SimplifyLinqExpression
                        .Where(sf => !destinationFiles.Any(df => String.Compare(df.Name, sf.Name, StringComparison.OrdinalIgnoreCase) == 0)));
                    // ReSharper restore SimplifyLinqExpression
                }

                //
                // Calculate the files which are to be deleted from the destination, this respects the delete parameter
                //
                List<FileInfo> filesToDeleteFromDestination = new List<FileInfo>();
                if(delete)
                {
                    filesToDeleteFromDestination.AddRange(destinationFiles
                        // ReSharper disable SimplifyLinqExpression
                        .Where(sf => !sourceFiles.Any(df => String.Compare(df.Name, sf.Name, StringComparison.OrdinalIgnoreCase) == 0)));
                    // ReSharper restore SimplifyLinqExpression
                }

                //
                // Copy files from source to desination
                //
                foreach(FileInfo file in filesToCopyFromSource)
                {
                    file.CopyTo(Path.Combine(destination.FullName, file.Name), true);
                }

                //
                // Delete files from destination
                //
                //foreach(FileInfo file in filesToDeleteFromDestination)
                //{
                //    file.Delete();
                //}
            }
        }

        #endregion

        public bool RollbackResource(string resourceName, int versionNo, string resourceType = "Service")
        {
            var resource = FindResource(resourceName, resourceType);
            if(resource == null)
            {
                return false;
            }

            var fileName = Path.Combine(_workspacePath, string.Format(@"Services\VersionControl\{0}.V{1}.xml", resource.Name, versionNo));
            if(File.Exists(fileName))
            {
                var fileContent = File.ReadAllText(fileName);
                var isValid = HostSecurityProvider.Instance.VerifyXml(fileContent);
                if(isValid)
                {
                    List<DynamicServiceObjectBase> res = GenerateObjectGraphFromString(fileContent);
                    foreach(var item in res)
                    {
                        switch(item.ObjectType)
                        {
                            case enDynamicServiceObjectType.DynamicService:
                                AddDynamicService((item as DynamicService), item.AuthorRoles);
                                break;

                            case enDynamicServiceObjectType.Source:
                                AddSource((item as Source), item.AuthorRoles);
                                break;

                            case enDynamicServiceObjectType.WorkflowActivity:
                                AddWorkflowActivity((item as WorkflowActivityDef), item.AuthorRoles);
                                break;
                        }

                    }
                    return true;
                }
            }
            return false;
        }

        public void RestoreResources()
        {
            RestoreResources(new[] { "Sources", "Services" });
        }

        public void RestoreResources(string[] directoryNames, string resourceName = "")
        {
            var filePaths = new List<string>();
            foreach(string directoryName in directoryNames)
            {
                DirectoryInfo directory = new DirectoryInfo(Path.Combine(_workspacePath, directoryName));
                if (directory.Exists)
                {
                    // TODO 2013-01-13, brendon.page, this linq query was put in to ensure only valid file types are loaded.
                    //                                It is NOT a permenant solution. This code needs to be refactored so that it is extensible and testible, 
                    //                                ideally to using the repository pattern.
                    List<string> validFileNames = directory.GetFiles().Where(f => f.Extension.Equals(GlobalConstants.ResourceFileExtension, StringComparison.InvariantCultureIgnoreCase)).Select(f => f.FullName).ToList();
                    filePaths.AddRange(validFileNames);
                }
            }
            RestoreResources(filePaths, resourceName);
        }

        void RestoreResources(IList<string> filePaths, string resourceName = "")
        {
            // Must call RestoreResources once with a complete list !
            //
            // 2012.10.01: TWR - 5392 - Server does not dynamically reload resources 
            //             Refactored to enable unit testing
            var resourceIndex = new Dictionary<string, string>();
            var resources = new List<DynamicServiceObjectBase>();
            
            foreach(string fileName in filePaths)
            {
                string fileContent;
                lock(_lockForFileRead)
                {
                    using(var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Delete))
                    {
                        using(var textReader = new StreamReader(fileStream))
                        {
                            fileContent = textReader.ReadToEnd();
                        }
                    }
                }

                var isValid = HostSecurityProvider.Instance.VerifyXml(fileContent);
                if(isValid)
                {
                    // TODO 2013-01-13, brendon.page, this logic query was put in to ensure duplicate resources aren't loaded from different files.
                    //                                It is NOT a permenant solution. This code needs to be refactored so that it is extensible and testible, 
                    //                                ideally to using the repository pattern.
                    List<DynamicServiceObjectBase> generatedResources = GenerateObjectGraphFromString(fileContent);
                    foreach (DynamicServiceObjectBase dynamicServiceObjectBase in generatedResources)
                    {
                        if (!Path.GetFileNameWithoutExtension(fileName).Equals(dynamicServiceObjectBase.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            TraceWriter.WriteTrace(string.Format("Resource '{0}' wasn't loaded from file '{1}' because the file name doesn't match the resource name.", dynamicServiceObjectBase.Name, fileName));
                            continue;
                        }

                        string existingFileName;
                        if (resourceIndex.TryGetValue(dynamicServiceObjectBase.Name, out existingFileName))
                        {
                            TraceWriter.WriteTrace(string.Format("Resource '{0}' from file '{1}' wasn't loaded because a resource with the same name has already been loaded from file '{2}'.", dynamicServiceObjectBase.Name, fileName, existingFileName));
                            continue;
                        }

                        resourceIndex.Add(dynamicServiceObjectBase.Name, fileName);
                        resources.Add(dynamicServiceObjectBase);
                    }
                }
            }
            RestoreResources(resources, resourceName);
        }

        void RestoreResources(List<DynamicServiceObjectBase> resources, string resourceName = "")
        {
            // Must call RestoreResources once with a complete list !
            //
            // 2012.10.01: TWR - 5392 - Server does not dynamically reload resources 
            //             Refactored to enable unit testing
            if(string.IsNullOrEmpty(resourceName))
            {
                //
                // TODO: Fix how we determine what resource list needs to be updated if incoming resources is empty? 
                //       Needs to be incorporated in re-work!!
                // HACK: No reliable way at the moment, so assume all lists need to be cleared - should never be hit practically.
                //
                var servicesUpdated = resources.Count == 0 || resources.Exists(r => r.ObjectType == enDynamicServiceObjectType.DynamicService);
                var sourcesUpdated = resources.Count == 0 || resources.Exists(r => r.ObjectType == enDynamicServiceObjectType.Source);
                var workflowActivityDefsUpdated = resources.Count == 0 || resources.Exists(r => r.ObjectType == enDynamicServiceObjectType.WorkflowActivity);
                if(sourcesUpdated)
                {
                    RemoveItemsNotInResources(resources, ReservedSources, Sources, _sourceLock, enDynamicServiceObjectType.Source);
                }
                if(servicesUpdated)
                {
                    RemoveItemsNotInResources(resources, ReservedServices, Services, _serviceLock, enDynamicServiceObjectType.DynamicService);
                }
                if(workflowActivityDefsUpdated)
                {
                    RemoveItemsNotInResources(resources, null, WorkflowActivityDefs, _activityLock, enDynamicServiceObjectType.WorkflowActivity);
                }
            }
            
            foreach(var resource in resources)
            {
                if(string.IsNullOrEmpty(resourceName) ||
                    (!string.IsNullOrEmpty(resourceName) &&
                     resourceName.Equals(resource.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    #region Add Resource to relevant list

                    string resourceDef = resource.ResourceDefinition;
                    switch(resource.ObjectType)
                    {
                        case enDynamicServiceObjectType.DynamicService:
                            var ds = (DynamicService)resource;
                            AddDynamicService(ds, "Domain Admins", resourceDef, false);
                            break;

                        case enDynamicServiceObjectType.Source:
                            var src = (Source)resource;
                            AddSource(src, "Domain Admins", false);
                            break;

                        case enDynamicServiceObjectType.WorkflowActivity:
                            var wad = (WorkflowActivityDef)resource;
                            AddWorkflowActivity(wad, "Domain Admins", false);
                            break;

                        default:
                            TraceWriter.WriteTrace(
                                "Encountered unexpected dynamic service object type during service restore");
                            throw new ArgumentException(
                                "Encountered unexpected dynamic service object type during service restore");

                    }
                    #endregion
                }
            }
        }

        void RemoveItemsNotInResources<T>(List<DynamicServiceObjectBase> resources, List<T> reservedItems, List<T> cachedItems, ReaderWriterLockSlim cachedItemLock, enDynamicServiceObjectType cachedItemType)
            where T : DynamicServiceObjectBase
        {
            var resourceItems = (from r in resources
                                 where r.ObjectType == cachedItemType
                                 select r.Name).ToList();

            List<T> itemsToRemove;

            _reservedLock.EnterReadLock();
            cachedItemLock.EnterReadLock();
            try
            {

                var items = reservedItems == null ? cachedItems : cachedItems.Except(reservedItems);
                itemsToRemove = items.Where(
                        ci => !resourceItems.Contains(ci.Name, StringComparer.Create(CultureInfo.CurrentCulture, true))).
                        ToList();
            }
            finally
            {
                cachedItemLock.ExitReadLock();
                _reservedLock.ExitReadLock();
            }

            if(itemsToRemove.Count > 0)
            {
                cachedItemLock.EnterWriteLock();
                try
                {
                    foreach(var itemToRemove in itemsToRemove)
                    {
                        cachedItems.Remove(itemToRemove);
                    }
                }
                finally
                {
                    cachedItemLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Adds Dynamic Service Resources e.g. Services and Sources into hosting
        /// Where they can be leveraged by callers
        /// </summary>
        /// <param name="resources">A List of resources that need to be hosted. These can be a mix of Sources, Services or BizRules</param>
        /// <param name="roles">The roles.</param>
        /// <returns>
        /// UnlimitedObject containing the result of attempt to add hosted resources
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public dynamic AddResources(List<DynamicServiceObjectBase> resources, string roles)
        {
            Exceptions.ThrowArgumentNullExceptionIfObjectIsNull("resources", resources);

            dynamic returnData = new UnlimitedObject();

            var sources = resources.Where(c => c.ObjectType == enDynamicServiceObjectType.Source);
            sources.ToList().ForEach(c => returnData.AddResponse(AddSource(c as Source, roles)));

            var services = resources.Where(c => c.ObjectType == enDynamicServiceObjectType.DynamicService);
            services.ToList().ForEach(c => returnData.AddResponse(AddDynamicService(c as DynamicService, roles)));

            var activities = resources.Where(c => c.ObjectType == enDynamicServiceObjectType.WorkflowActivity);
            activities.ToList().ForEach(c => returnData.AddResponse(AddWorkflowActivity(c as WorkflowActivityDef, roles)));

            return returnData;
        }

        public dynamic AddWorkflowActivity(WorkflowActivityDef activity, string roles, bool saveResource = true)
        {
            dynamic returnData = new UnlimitedObject();
            WorkflowActivityDef activityService;

            LockActivities();

            try
            {
                activityService = WorkflowActivityDefs.Find(c => c.ServiceName.Equals(activity.ServiceName, StringComparison.CurrentCultureIgnoreCase) && !c.Name.Equals(activity.Name, StringComparison.CurrentCultureIgnoreCase));
            }
            finally
            {
                UnlockActivities();
            }

            if(activityService != null)
            {
                returnData.Error = string.Format("Activity '{0}' failed compilation: {1}", activity.Name, string.Format("Activity '{0}' already that references Service '{1}' ", activityService.Name, activityService.ServiceName));
                return returnData;
            }

            MapActivityToService(activity);

            if(activity.Compile())
            {
                WorkflowActivityDef wd;
                LockActivities();

                try
                {
                    wd = WorkflowActivityDefs.Find(c => c.Name.Equals(activity.Name, StringComparison.CurrentCultureIgnoreCase));
                }
                finally
                {
                    UnlockActivities();
                }

                if(wd != null)
                {
                    //if(!activity.IsUserInRole(roles, activity.AuthorRoles))
                    //{
                    //    returnData.Error = string.Format("Activity '{0}' failed compilation: {1}", activity.Name, "Access Violation: you are attempting to overwrite an activity that you do not have rights to");
                    //    return returnData;
                    //}

                    _activityLock.EnterWriteLock();

                    try
                    {
                        WorkflowActivityDefs.Remove(wd);
                        WorkflowActivityDefs.Add(activity);
                    }
                    finally
                    {
                        _activityLock.ExitWriteLock();
                    }

                    returnData.CompilerMessage = string.Format("Updated Activity '{0}'", activity.Name);

                }
                else
                {
                    //Add the service object to all service actions that are of type invokedynamicservice
                    //dynamicService.Actions.ForEach(c => MapServiceActionDependencies(c));
                    _activityLock.EnterWriteLock();

                    try
                    {
                        WorkflowActivityDefs.Add(activity);
                    }
                    finally
                    {
                        _activityLock.ExitWriteLock();
                    }
                    returnData.CompilerMessage = string.Format("Added Activity '{0}'", activity.Name);
                }

                if(saveResource)
                {
                    SaveResource("ActivityDefs", activity);
                }
            }
            else
            {
                returnData.Error = string.Format("Activity '{0}' failed compilation", activity.Name);
                returnData.AddResponse(activity.GetCompilerErrors());
            }

            dynamic message = returnData.CompilerMessage;

            if(message is string)
            {
                SendMessageToConnectedClients(message);
            }
            return returnData;

        }

        /// <summary>
        /// Compiles a BizRule object then adds it into hosting if compilation is successful
        /// </summary>
        /// <param name="bizRule">The BizRule object to host</param>
        /// <returns>UnlimitedObject containing the result of attempt to compile and add a resource into hosting</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public dynamic AddBizRule(BizRule bizRule)
        {
            Exceptions.ThrowArgumentNullExceptionIfObjectIsNull("bizRule", bizRule);
            dynamic returnData = new UnlimitedObject();
            LockServices();

            try
            {
                bizRule.Service = Services.Find(c => c.Name.Equals(bizRule.ServiceName, StringComparison.CurrentCultureIgnoreCase));
            }
            finally
            {
                UnlockServices();
            }

            if(bizRule.Compile())
            {
                //Add the Service implementation for the biz rule to the bizrule object
                //Search for a biz rule with the same name, if it exists then replace it with the current bizRule
                BizRule bizRuleMatch = BizRules.Find(c => c.Name.Equals(bizRule.Name, StringComparison.CurrentCultureIgnoreCase));
                if(bizRuleMatch != null)
                {
                    // ReSharper disable RedundantAssignment
                    bizRuleMatch = bizRule;
                    // ReSharper restore RedundantAssignment
                    returnData.CompilerMessage = string.Format("Updated biz rule '{0}'", bizRule.Name);
                }
                else
                {
                    BizRules.Add(bizRule);
                    returnData.CompilerMessage = string.Format("Added biz rule '{0}'", bizRule.Name);
                }

            }
            else
            {
                returnData.Error = string.Format("Biz Rule '{0}' failed compilation", bizRule.Name);
                returnData.AddResponse(bizRule.GetCompilerErrors());
            }

            dynamic message = returnData.CompilerMessage;

            if(message is string)
            {
                SendMessageToConnectedClients(message);
            }
            return returnData;
        }

        /// <summary>
        /// Compiles a DynamicService object then adds it into hosting if compilation is successful
        /// </summary>
        /// <param name="dynamicService">The DynamicService object to Host</param>
        /// <param name="roles">The roles.</param>
        /// <param name="resourceDef">The resource def.</param>
        /// <param name="saveResource">if set to <c>true</c> [save resource].</param>
        /// <returns>
        /// UnlimitedObject containing the result of attempt to compile and add a resource into hosting
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public dynamic AddDynamicService(DynamicService dynamicService, string roles, string resourceDef = null, bool saveResource = true)
        {
            Exceptions.ThrowArgumentNullExceptionIfObjectIsNull("dynamicService", dynamicService);

            dynamic returnData = new UnlimitedObject();
            LockSources();

            try
            {
                if(Sources.Where(c => c.Name.Equals(dynamicService.Name, StringComparison.CurrentCultureIgnoreCase)).Count() > 0)
                {
                    returnData.Error = "Compilation Error: There is a source with the same name ";
                    return returnData;
                }
            }
            finally
            {
                UnlockSources();
            }

            // Travis :  extract the required items
            ServiceHydrationTO dataItems = ExtractDataListIOMapping(resourceDef);

            if(dataItems.DataList != null)
            {
                dynamicService.DataListSpecification = dataItems.DataList;
            }

            dynamicService.Actions.ForEach(c => MapServiceActionDependencies(c));
            if(dynamicService.Compile())
            {
                DynamicService ds;
                LockServices();
                try
                {
                    ds = Services.Find(c => c.Name.Equals(dynamicService.Name, StringComparison.CurrentCultureIgnoreCase));
                }
                finally
                {
                    UnlockServices();
                }

                if(ds != null)
                {
                    //if(!dynamicService.IsUserInRole(roles, dynamicService.AuthorRoles))
                    //{
                    //    returnData.Error = string.Format("Service '{0}' failed compilation: {1}", dynamicService.Name, "Access Violation: you are attempting to overwrite a service that you do not have rights to");
                    //    return returnData;
                    //}

                    dynamicService.VersionNo = ds.VersionNo;
                    _serviceLock.EnterWriteLock();

                    try
                    {
                        Services.Remove(ds);
                        Services.Add(dynamicService);
                    }
                    finally
                    {
                        _serviceLock.ExitWriteLock();
                    }

                    returnData.CompilerMessage = string.Format("Updated Service '{0}'", dynamicService.Name);
                }
                else
                {
                    //Add the service object to all service actions that are of type invokedynamicservice
                    //dynamicService.Actions.ForEach(c => MapServiceActionDependencies(c));

                    _serviceLock.EnterWriteLock();

                    try
                    {
                        Services.Add(dynamicService);
                    }
                    finally
                    {
                        _serviceLock.ExitWriteLock();
                    }

                    returnData.CompilerMessage = string.Format("Added Service '{0}'", dynamicService.Name);
                }

                MapUnitTests(dynamicService);

                if(saveResource)
                {
                    //Persist to local file system
                    SaveResource("Services", dynamicService);
                }
            }
            else
            {
                string serviceName = dynamicService.Name == null ? string.Empty : dynamicService.Name;

                returnData.Error = string.Format("Service '{0}' failed compilation", serviceName);
                returnData.AddResponse(dynamicService.GetCompilerErrors());
            }

            dynamic message = returnData.CompilerMessage;

            if(message is string)
            {
                SendMessageToConnectedClients(message);
            }
            return returnData;
        }

        /// <summary>
        /// Compiles a Source object then adds it into hosting if compilation is successful
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="roles">The roles.</param>
        /// <param name="saveResource">if set to <c>true</c> [save resource].</param>
        /// <returns>
        /// UnlimitedObject containing the result of attempt to compile and add a resource into hosting
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public dynamic AddSource(Source source, string roles, bool saveResource = true)
        {
            Exceptions.ThrowArgumentNullExceptionIfObjectIsNull("source", source);
            dynamic returnData = new UnlimitedObject();
            LockServices();

            try
            {
                if(Services.Where(c => c.Name.Equals(source.Name, StringComparison.CurrentCultureIgnoreCase)).Count() > 0)
                {
                    returnData.Error = "Compilation Error: There is a service with the same name ";
                    return returnData;
                }
            }
            finally
            {
                UnlockServices();
            }

            if(source.Compile())
            {
                Source src;
                LockSources();

                try
                {
                    src = Sources.Find(c => c.Name.Equals(source.Name, StringComparison.CurrentCultureIgnoreCase));
                }
                finally
                {
                    UnlockSources();
                }

                if(src != null)
                {
                    //if(!source.IsUserInRole(roles, source.AuthorRoles))
                    //{
                    //    returnData.Error = string.Format("Source '{0}' failed compilation: {1}", source.Name, "Access Violation: you are attempting to overwrite a source that you do not have rights to");
                    //    return returnData;
                    //}
                    source.VersionNo = src.VersionNo;

                    _sourceLock.EnterWriteLock();

                    try
                    {
                        Sources.Remove(src);
                        Sources.Add(source);
                    }
                    finally
                    {
                        _sourceLock.ExitWriteLock();
                    }


                    returnData.CompilerMessage = string.Format("Updated Source '{0}'", source.Name);
                }
                else
                {
                    _sourceLock.EnterWriteLock();

                    try
                    {
                        Sources.Add(source);
                    }
                    finally
                    {
                        _sourceLock.ExitWriteLock();
                    }

                    returnData.CompilerMessage = string.Format("Added Source '{0}'", source.Name);
                }

                if(saveResource)
                {
                    //Persist to local file system
                    SaveResource("Sources", source);
                }

                IEnumerable<DynamicService> services;
                LockServices();

                try
                {
                    services = Services.Where(service => service.Actions.Where(action => (!string.IsNullOrEmpty(action.SourceName)) && action.SourceName.Equals(source.Name, StringComparison.CurrentCultureIgnoreCase)).Count() >= 1);
                }
                finally
                {
                    UnlockServices();
                }

                services.ToList().ForEach(service => service.Actions.ForEach(action => MapServiceActionDependencies(action)));
            }
            else
            {
                string sourceName = source.Name == null ? string.Empty : source.Name;
                returnData.Error = string.Format("Source '{0}' failed compilation", sourceName);
                returnData.AddResponse(source.GetCompilerErrors());
            }

            dynamic message = returnData.CompilerMessage;

            if(message is string)
            {
                SendMessageToConnectedClients(message);
            }
            return returnData;
        }

        /// <summary>
        /// Removes a DynamicService and optionally deletes its resource file.
        /// </summary>
        /// <param name="dynamicService">The DynamicService object to remove</param>
        /// <param name="roles">The roles.</param>
        /// <param name="deleteResource">if set to <c>true</c> [delete resource].</param>
        /// <returns>
        /// UnlimitedObject containing the result of the attempt to remove the service from hosting.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public dynamic RemoveDynamicService(DynamicService dynamicService, string roles, bool deleteResource = true)
        {
            Exceptions.ThrowArgumentNullExceptionIfObjectIsNull("dynamicService", dynamicService);
            dynamic returnData = new UnlimitedObject();
            int serviceIndex = -1;
            LockServices();

            try
            {
                for(int i = Services.Count - 1; i >= 0; i--)
                {
                    if(Services[i].Name.Equals(dynamicService.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        serviceIndex = i;
                        i = -1;
                    }
                }
            }
            finally
            {
                UnlockServices();
            }

            if(serviceIndex == -1)
            {
                returnData.Error = "Deletion Error: There is no service with that name ";
                return returnData;
            }

            //if(!dynamicService.IsUserInRole(roles, dynamicService.AuthorRoles))
            //{
            //    returnData.Error = string.Format("Service '{0}' failed deletion: {1}", dynamicService.Name, "Access Violation: you are attempting to delete a service that you do not have rights to");
            //    return returnData;
            //}

            _serviceLock.EnterWriteLock();

            try
            {
                Services.RemoveAt(serviceIndex);
            }
            finally
            {
                _serviceLock.ExitWriteLock();
            }

            if(deleteResource)
            {
                RemoveResource("Services", dynamicService);
            }

            returnData.Result = "Success";

            return returnData;
        }

        /// <summary>
        /// Removes a Source and optionally deletes its resource file.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="roles">The roles.</param>
        /// <param name="deleteResource">if set to <c>true</c> [delete resource].</param>
        /// <returns>
        /// UnlimitedObject containing the result of attempt to remove the source from hosting.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public dynamic RemoveSource(Source source, string roles, bool deleteResource = true)
        {
            Exceptions.ThrowArgumentNullExceptionIfObjectIsNull("source", source);
            dynamic returnData = new UnlimitedObject();
            int sourceIndex = -1;
            LockSources();

            try
            {
                for(int i = Sources.Count - 1; i >= 0; i--)
                {
                    if(Sources[i].Name.Equals(source.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        sourceIndex = i;
                        i = -1;
                    }
                }
            }
            finally
            {
                UnlockSources();
            }

            if(sourceIndex == -1)
            {
                returnData.Error = "Deletion Error: There is no source with that name ";
                return returnData;
            }

            //if(!source.IsUserInRole(roles, source.AuthorRoles))
            //{
            //    returnData.Error = string.Format("Source '{0}' failed deletion: {1}", source.Name, "Access Violation: you are attempting to delete a source that you do not have rights to");
            //    return returnData;
            //}

            _sourceLock.EnterWriteLock();

            try
            {
                Sources.RemoveAt(sourceIndex);
            }
            finally
            {
                _sourceLock.ExitWriteLock();
            }

            if(deleteResource)
            {
                // PBI 6597: TWR - Changed folder from Services to Sources
                RemoveResource("Sources", source);
            }

            returnData.Result = "Success";
            return returnData;
        }

        /// <summary>
        /// Invokes a dynamic service and returns a string containing the response from the
        /// data store. The Service required to invoke is contained in the 'Service' parameter
        /// enclosed in the XmlRequest UnlimitedObject parameter.
        /// This method is still within the static service
        /// </summary>
        /// <param name="xmlRequest">The XML request as an UnlimitedObject from the caller</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns>
        /// An UnlimitedObject as a response
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public Guid InvokeService(dynamic xmlRequest, Guid dataListID)
        {
            Exceptions.ThrowArgumentNullExceptionIfObjectIsNull("xmlRequest", xmlRequest);
            IDynamicServicesInvoker invoker = new DynamicServicesInvoker();
            // ReSharper disable SuggestUseVarKeywordEvident
            ErrorResultTO errors;
            // ReSharper restore SuggestUseVarKeywordEvident
            return invoker.Invoke(this, xmlRequest, dataListID, out errors);
        }

        #region Generate an object graph from the domain specific language string for the DSF
        /// <summary>
        /// Generates the object graph from string.
        /// </summary>
        /// <param name="serviceDefinitionsXml">The service definitions XML.</param>
        /// <returns></returns>
        public List<DynamicServiceObjectBase> GenerateObjectGraphFromString(string serviceDefinitionsXml)
        {
            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("serviceDefinitionXml", serviceDefinitionsXml);
            compilerErrorList.Clear();
            //This will store the return data of this method
            //which represents the services that were successfully loaded

            // ReSharper disable SuggestUseVarKeywordEvident
            List<DynamicServiceObjectBase> objectsLoaded = new List<DynamicServiceObjectBase>();
            // ReSharper restore SuggestUseVarKeywordEvident

            dynamic dslObject = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(serviceDefinitionsXml);

            #region Create MetatData about this resource
            string authorRoles = string.Empty;
            string comment = string.Empty;
            string helpLink = null;
            string unitTestTarget = string.Empty;
            string category = string.Empty;
            string tags = string.Empty;
            string dataList = string.Empty;

            if(dslObject.AuthorRoles is string)
            {
                authorRoles = dslObject.AuthorRoles;
            }

            if(dslObject.Comment is string)
            {
                comment = dslObject.Comment;
            }

            if(dslObject.Category is string)
            {
                category = dslObject.Category;
            }

            if(dslObject.Tags is string)
            {
                tags = dslObject.Tags;
            }

            if(dslObject.UnitTestTargetWorkflowService is string)
            {
                unitTestTarget = dslObject.UnitTestTargetWorkflowService;
            }

            if(dslObject.HelpLink is string)
            {
                if(!string.IsNullOrEmpty(dslObject.HelpLink))
                {
                    if(Uri.IsWellFormedUriString(dslObject.HelpLink, UriKind.RelativeOrAbsolute))
                    {
                        helpLink = dslObject.HelpLink;
                    }
                }
            }

            // Travis Added for Data List
            if(dslObject.DataList != null)
            {
                // Try..catch refactored out by Michael (Verified by Travis)
                // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                if((dslObject.DataList).GetType() == typeof(UnlimitedObject))
                // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                {
                    dataList = dslObject.DataList.XmlString;
                }
                else
                {
                    dataList = "<ADL></ADL>";
                }

            }
            #endregion

            #region Create and Hydrate BizRules then add them to the service directory
            //Retrieve a list of UnlimitedObjects that 
            //each contain an individual BizRule node from the
            //Service Definition file

            // ReSharper disable LocalVariableHidesMember
            // ReSharper disable InconsistentNaming
            dynamic BizRules = dslObject.BizRule;
            // ReSharper restore InconsistentNaming
            // ReSharper restore LocalVariableHidesMember

            //We check if a list is being returned as this
            //will be the case when loading complex xml 
            //elements that have attributes
            //as in the case of the service definition file
            //All classes are hydrated in this way so these
            //comments will not be repeated later on in this source file
            if(BizRules is List<UnlimitedObject>)
            {
                //Iterate the bizrule collection of UnlimitedObjects and 
                //Hydrate an instance of the BizRule class each time
                foreach(dynamic bizrule in BizRules)
                {
                    // ReSharper disable SuggestUseVarKeywordEvident
                    BizRule br = new BizRule();
                    // ReSharper restore SuggestUseVarKeywordEvident
                    br.Name = bizrule.Name;
                    br.ServiceName = bizrule.ServiceName;
                    br.Expression = bizrule.Expression;

                    // ReSharper disable RedundantExplicitArrayCreation
                    br.ExpressionColumns = bizrule.ExpressionColumns.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    // ReSharper restore RedundantExplicitArrayCreation

                    //Add the newly instantiated and Hydrated bizrule class
                    //to the BizRules list of the DynamicServices service directory
                    //this.BizRules.Add(br);
                    objectsLoaded.Add(br);
                    Trace.WriteLine(string.Format("successfully parsed biz rule '{0}'", br.Name));
                }
            }
            #endregion

            #region Create and Hydrate Workflow ActivityMetaData
            dynamic activities = dslObject.WorkflowActivityDef;
            if(activities is List<UnlimitedObject>)
            {
                foreach(dynamic item in activities)
                {
                    // ReSharper disable SuggestUseVarKeywordEvident
                    WorkflowActivityDef wd = new WorkflowActivityDef();
                    // ReSharper restore SuggestUseVarKeywordEvident
                    wd.AuthorRoles = authorRoles;
                    wd.Comment = comment;
                    wd.Tags = tags;
                    wd.Category = category;
                    wd.HelpLink = helpLink;
                    wd.ResourceDefinition = item.XmlString;
                    wd.DataListSpecification = dataList;

                    if(item.ServiceName is string)
                    {
                        wd.ServiceName = item.ServiceName;
                    }
                    if(item.Name is string)
                    {
                        wd.Name = item.Name;
                    }
                    if(item.IconPath is string)
                    {
                        wd.IconPath = item.IconPath;
                    }
                    if(item.DataTags is string)
                    {
                        wd.DataTags = item.DataTags;
                    }
                    if(item.DeferExecution is string)
                    {
                        bool defer;
                        bool.TryParse(item.DeferExecution, out defer);
                        wd.DeferExecution = defer;
                    }
                    if(item.ResultValidationExpression is string)
                    {
                        wd.ResultValidationExpression = item.ResultValidationExpression;
                    }
                    if(item.ResultValidationRequiredTags is string)
                    {
                        wd.ResultValidationRequiredTags = item.ResultValidationRequiredTags;
                    }
                    if(item.AuthorRoles is string)
                    {
                        wd.AuthorRoles = item.AuthorRoles;
                    }
                    if(item.AdminRoles is string)
                    {
                        wd.AdminRoles = item.AdminRoles;
                    }

                    objectsLoaded.Add(wd);
                }

            }

            #endregion

            #region Create and Hydrate Sources then add them to the service directory
            //Retrieve a list of UnlimitedObjects that 
            //each contain in individual Source node from the
            //Service Definition file
            dynamic sources = dslObject.Source;
            if(sources is List<UnlimitedObject>)
            {
                foreach(dynamic source in sources)
                {
                    bool dlCheck = false;



                    try
                    {
                        //XmlDocument xDoc = new XmlDocument();
                        //xDoc.LoadXml("<x>" + (source as UnlimitedObject).XmlString + "</x>");
                        //XmlNodeList nl = xDoc.GetElementsByTagName("Action");
                        //if (nl.Count > 0) {
                        //dlCheck = true;
                        //}
                        if(source.Type is string)
                        {

                            enSourceType sourceType;
                            // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                            if(!Enum.TryParse<enSourceType>(source.Type, out sourceType))
                            // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                            {
                                dlCheck = false;
                            }
                            else
                            {
                                dlCheck = true;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        string error = ex.Message;
                        Debug.WriteLine("Michael Warning: " + error);
                    }

                    //(source as UnlimitedObject).xmlData.HasElements;

                    if(dlCheck)
                    { // Travis : filter out the Source in DataList issue
                        Source src = new Source();
                        src.AuthorRoles = authorRoles;
                        src.Tags = tags;
                        src.Comment = comment;
                        src.Category = category;
                        src.HelpLink = helpLink;
                        src.ResourceDefinition = source.XmlString;
                        src.DataListSpecification = dataList;

                        if(source.Name is string)
                        {
                            src.Name = source.Name;
                        }

                        if (source.Type is string)
                        {
                            enSourceType sourceType;
                            if(!Enum.TryParse<enSourceType>(source.Type, out sourceType))
                            {
                                if (source.ServerType is string)
                                {
                                    src.Type = !Enum.TryParse<enSourceType>(source.ServerType, out sourceType) ? enSourceType.Unknown : sourceType;
                                }
                                else src.Type = enSourceType.Unknown;
                            }
                            else
                            {
                                src.Type = sourceType;
                            }
                        }
                        else
                        {
                        }

                        if(source.ConnectionString is string)
                        {
                            if(!string.IsNullOrEmpty(source.ConnectionString))
                            {
                                src.ConnectionString = source.ConnectionString;
                            }
                        }

                        if(source.Uri is string)
                        {
                            if(!string.IsNullOrEmpty(source.Uri))
                            {
                                src.WebServiceUri = new Uri(source.Uri);
                            }
                        }

                        if(source.AssemblyName is string)
                        {
                            if(!string.IsNullOrEmpty(source.AssemblyName))
                            {
                                src.AssemblyName = source.AssemblyName;
                            }
                        }

                        if(source.AssemblyLocation is string)
                        {
                            if(!string.IsNullOrEmpty(source.AssemblyLocation))
                            {
                                src.AssemblyLocation = source.AssemblyLocation;
                            }
                        }

                        // PBI 6597: TWR - added source ID check
                        SetID(src, source);

                        objectsLoaded.Add(src);
                    }
                }
            }
            #endregion

            #region Build an object graph for each service in the domain specific language string
            dynamic services = dslObject.Service;
            if(services is List<UnlimitedObject>)
            {
                foreach(dynamic service in services)
                {
                    // ReSharper disable SuggestUseVarKeywordEvident
                    DynamicService ds = new DynamicService();
                    // ReSharper restore SuggestUseVarKeywordEvident
                    ds.AuthorRoles = authorRoles;
                    ds.Category = category;
                    ds.Tags = tags;
                    ds.Comment = comment;
                    ds.HelpLink = helpLink;
                    ds.ResourceDefinition = service.XmlString;
                    ds.UnitTestTargetWorkflowService = unitTestTarget;
                    ds.DataListSpecification = dataList;

                    if(service.IconPath is string)
                    {
                        ds.IconPath = service.IconPath;
                    }

                    if(service.DisplayName is string)
                    {
                        ds.DisplayName = service.DisplayName;
                    }

                    if(service.Name is string)
                    {
                        ds.Name = service.Name;
                    }
                    else
                    {
                        // Travis : we have a DataList clash between this "dynamic" property and the DataList XML when Name is an element
                        // ReSharper disable SuggestUseVarKeywordEvident
                        UnlimitedObject tmpObj = (service as UnlimitedObject);
                        // ReSharper restore SuggestUseVarKeywordEvident

                        try
                        {
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.LoadXml(tmpObj.XmlString);
                            XmlNode n = xDoc.SelectSingleNode("Service");
                            ds.Name = n.Attributes["Name"].Value;
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch
                        // ReSharper restore EmptyGeneralCatchClause
                        {

                            // Best effort ;)
                        }
                    }

                    #region Build Actions
                    // ReSharper disable InconsistentNaming
                    dynamic Actions = service.Action;
                    // ReSharper restore InconsistentNaming

                    if(Actions is List<UnlimitedObject>)
                    {

                        foreach(dynamic action in Actions)
                        {
                            #region Process Actions
                            // ReSharper disable SuggestUseVarKeywordEvident
                            ServiceAction sa = new ServiceAction();
                            // ReSharper restore SuggestUseVarKeywordEvident

                            if(action.Name is string)
                            {
                                sa.Name = action.Name;
                            }

                            if(action.OutputDescription is string)
                            {
                                sa.OutputDescription = action.OutputDescription;
                            }

                            sa.Parent = action.Parent;

                            if(action.Type is string)
                            {
                                enActionType actionType;

                                // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                                if(!Enum.TryParse<enActionType>(action.Type, out actionType))
                                // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                                {
                                    sa.ActionType = enActionType.Unknown;
                                }
                                else
                                {
                                    sa.ActionType = actionType;
                                }
                            }

                            if(action.SourceName is string)
                            {
                                sa.SourceName = action.SourceName;
                                sa.PluginDomain = AppDomain.CreateDomain(action.SourceName);
                            }

                            if(action.SourceMethod is string)
                            {
                                sa.SourceMethod = action.SourceMethod;
                            }

                            //sa.ActionType = Enum.Parse(typeof(enActionType), action.Type);
                            //Biz Rules are special actions 
                            //so we need to treat everything else differently

                            switch(sa.ActionType)
                            {
                                case enActionType.BizRule:
                                    //sa.BizRuleName = action.BizRuleName;
                                    //var BizRule = from c in this.BizRules
                                    //              where c.Name == sa.BizRuleName
                                    //              select c;

                                    //if (BizRule.Count() > 0) {
                                    //    sa.BizRule = BizRule.First();
                                    //}
                                    break;

                                case enActionType.InvokeDynamicService:
                                case enActionType.InvokeManagementDynamicService:
                                case enActionType.InvokeServiceMethod:
                                case enActionType.InvokeWebService:
                                case enActionType.Plugin:
                                    break;

                                case enActionType.InvokeStoredProc:
                                    if(action.CommandTimeout is string)
                                    {
                                        int timeout;
                                        int.TryParse(action.CommandTimeout, out timeout);

                                        sa.CommandTimeout = timeout;


                                    }
                                    break;

                                case enActionType.Workflow:
                                    if(action.XamlDefinition is string)
                                    {
                                        sa.XamlDefinition = action.XamlDefinition;
                                        sa.ServiceName = ds.Name;
                                    }
                                    break;
                            }

                            if(action.ResultsToClient is string)
                            {
                                sa.ResultsToClient = bool.Parse(action.ResultsToClient);
                            }

                            if(action.ServiceName is string)
                            {
                                sa.ServiceName = action.ServiceName;
                            }



                            if(action.TerminateServiceOnFault is string)
                            {
                                sa.TerminateServiceOnFault = bool.Parse(action.TerminateServiceOnFault);
                            }

                            #endregion

                            ds.Actions.Add(sa);

                            #region Process Inputs for Action
                            // ReSharper disable InconsistentNaming
                            dynamic Inputs = action.Input;
                            // ReSharper restore InconsistentNaming
                            if(Inputs is List<UnlimitedObject>)
                            {
                                foreach(dynamic input in Inputs)
                                {
                                    ServiceActionInput sai = new ServiceActionInput();
                                    if(input.Name is string)
                                    {
                                        sai.Name = input.Name;
                                    }
                                    if(input.Source is string)
                                    {
                                        sai.Source = input.Source;
                                    }

                                    if(input.CascadeSource is string)
                                    {
                                        bool val;
                                        if(bool.TryParse(input.CascadeSource, out val))
                                        {
                                            sai.CascadeSource = val;
                                        }
                                    }

                                    if(input.Required is string)
                                    {
                                        bool val;
                                        if(bool.TryParse(input.IsRequired, out val))
                                        {
                                            sai.IsRequired = val;
                                        }
                                    }


                                    if(input.DefaultValue is string)
                                    {
                                        sai.DefaultValue = input.DefaultValue;

                                    }

                                    // 16.10.2012 - Travis.Frisinger  : EmptyToNull amendments
                                    if(input.EmptyToNull is string)
                                    {
                                        bool result;
                                        Boolean.TryParse(input.EmptyToNull, out result);
                                        sai.EmptyToNull = result;
                                    }
                                    else
                                    {
                                        sai.EmptyToNull = false;
                                    }

                                    // 16.10.2012 - Travis.Frisinger  : EmptyToNull amendments
                                    if(input.NativeType is string)
                                    {
                                        sai.NativeType = input.NativeType;
                                    }
                                    else
                                    {
                                        sai.NativeType = AppServerStrings.AnyTypeConstant;
                                    }

                                    // ReSharper disable InconsistentNaming
                                    dynamic Validators = input.Validator;
                                    // ReSharper restore InconsistentNaming

                                    if(Validators is List<UnlimitedObject>)
                                    {

                                        foreach(dynamic validator in Validators)
                                        {
                                            Validator v = new Validator();

                                            if(validator.Type is string)
                                            {
                                                enValidationType validatorType;
                                                if(!Enum.TryParse<enValidationType>(validator.Type, out validatorType))
                                                {
                                                    v.ValidatorType = enValidationType.Required;
                                                }
                                                else
                                                {
                                                    v.ValidatorType = validatorType;
                                                }

                                            }

                                            //v.ValidatorType = Enum.Parse(typeof(enValidationType), validator.Type);

                                            if(validator.RegularExpression is string)
                                            {
                                                v.RegularExpression = validator.RegularExpression;
                                            }

                                            sai.Validators.Add(v);
                                        }
                                    }
                                    sa.ServiceActionInputs.Add(sai);
                                }
                            }
                            #endregion

            
                            // Travis.Frisinger : 29.01.2013 - Bug 8352 / 8412
                            #region Output Mapping

                            dynamic Outputs = action.Output;
                            if(Outputs is List<UnlimitedObject>)
                            {
                                StringBuilder defs = new StringBuilder(GlobalConstants.OutputDefOpenTag);
                                foreach(dynamic output in Outputs)
                                {
                                    string def = output.XmlString;
                                    defs.Append(def);
                                }

                                defs.Append(GlobalConstants.OutputDefCloseTag);


                                // TODO : Create IDev2Defition to add
                                IDev2LanguageParser lp = DataListFactory.CreateOutputParser();
                                IList<IDev2Definition> theRealDefs = lp.Parse(defs.ToString());

                                sa.ServiceActionOutputs = theRealDefs;

                            }

                            #endregion

                        }
                    }
                    #endregion Process Actions

                    #region Build CasesHolder
                    dynamic casesParent = service.Cases;
                    List<ServiceActionCases> casesHolderList = new List<ServiceActionCases>();

                    if(casesParent is List<UnlimitedObject>)
                    {
                        foreach(dynamic casesInstance in casesParent)
                        {
                            ServiceActionCases casesObject = new ServiceActionCases();

                            if(casesInstance.DataElementName is string)
                            {
                                casesObject.DataElementName = casesInstance.DataElementName;
                            }

                            if(casesInstance.CascadeSource is string)
                            {
                                bool cascadeSource;
                                bool.TryParse(casesInstance.CascadeSource, out cascadeSource);
                                casesObject.CascadeSource = cascadeSource;
                            }
                            casesObject.Parent = casesInstance.Parent;
                            casesHolderList.Add(casesObject);
                        }
                    }

                    #endregion

                    #region Build Case
                    dynamic cases = service.Case;
                    List<ServiceActionCase> casesList = new List<ServiceActionCase>();

                    if(cases is List<UnlimitedObject>)
                    {
                        foreach(dynamic caseInstance in cases)
                        {
                            ServiceActionCase caseObject = new ServiceActionCase();
                            if(caseInstance.Regex is string)
                            {
                                caseObject.Regex = caseInstance.Regex;
                            }

                            if(caseInstance.IsDefault is string)
                            {
                                bool isDefault;
                                bool.TryParse(caseInstance.IsDefault, out isDefault);

                                caseObject.IsDefault = isDefault;
                            }

                            caseObject.Parent = caseInstance.Parent;
                            casesList.Add(caseObject);
                        }
                    }

                    #endregion

                    #region Map each action inside a case to the Case instance
                    List<ServiceAction> removeAction = new List<ServiceAction>();
                    foreach(ServiceAction sa in ds.Actions)
                    {

                        if(sa.Parent != null)
                        {
                            if(sa.Parent.Regex is string)
                            {
                                // ReSharper disable AccessToForEachVariableInClosure
                                var caseInstance = casesList.Where(c => c.Regex == sa.Parent.Regex);
                                // ReSharper restore AccessToForEachVariableInClosure
                                if(caseInstance.Count() > 0)
                                {
                                    caseInstance.First().Actions.Add(sa);
                                    removeAction.Add(sa);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Map each case to CasesHolder
                    foreach(ServiceActionCase sc in casesList)
                    {
                        if(sc.Parent != null)
                        {
                            if(sc.Parent.DataElementName is string)
                            {
                                // ReSharper disable AccessToForEachVariableInClosure
                                var holder = casesHolderList.Where(c => c.DataElementName == sc.Parent.DataElementName);
                                // ReSharper restore AccessToForEachVariableInClosure
                                if(holder.Count() > 0)
                                {
                                    // ReSharper disable PossibleMultipleEnumeration
                                    holder.First().Cases.Add(sc);
                                    // ReSharper restore PossibleMultipleEnumeration

                                    //Default Case
                                    // ReSharper disable PossibleMultipleEnumeration
                                    var defaultCases = holder.First().Cases.Where(c => c.IsDefault);
                                    // ReSharper restore PossibleMultipleEnumeration

                                    ServiceActionCase defaultCase;
                                    // ReSharper disable UseMethodAny.1
                                    if(defaultCases.Count() >= 1)
                                    // ReSharper restore UseMethodAny.1
                                    {
                                        defaultCase = defaultCases.First();
                                        // ReSharper disable PossibleMultipleEnumeration
                                        holder.First().DefaultCase = defaultCase;
                                        // ReSharper restore PossibleMultipleEnumeration
                                    }
                                }
                            }
                        }
                    }


                    #endregion

                    #region Map CaseHolder to Switch Action
                    foreach(ServiceActionCases sch in casesHolderList)
                    {
                        if(sch.Parent != null)
                        {
                            if(sch.Parent.Name is string)
                            {
                                // ReSharper disable AccessToForEachVariableInClosure
                                var action = ds.Actions.Where(c => c.Name == sch.Parent.Name);
                                // ReSharper restore AccessToForEachVariableInClosure
                                if(action.Count() > 0)
                                {
                                    action.First().Cases = sch;
                                }
                            }
                        }
                    }
                    #endregion

                    foreach(ServiceAction sa in removeAction)
                    {
                        ds.Actions.Remove(sa);
                    }

                    // PBI: 801: TWR - added ID check
                    SetID(ds, service);


                    objectsLoaded.Add(ds);
                }
            }
            #endregion



            return objectsLoaded;
        }

        #endregion

        #region SetID

        static void SetID(IDynamicServiceObject dso, dynamic resource)
        {
            Guid id;
            if(resource.ID is string && !string.IsNullOrEmpty(resource.ID))
            {
                id = Guid.Parse(resource.ID);
            }
            else
            {
                id = Guid.NewGuid();
                var xml = XElement.Parse(dso.ResourceDefinition);
                xml.Add(new XAttribute("ID", id.ToString()));
                dso.ResourceDefinition = xml.ToString(SaveOptions.DisableFormatting);
            }

            // TODO: Add ID property to IDynamicServiceObject
            var source = dso as Source;
            if(source != null)
            {
                source.ID = id;
            }
            else
            {
                var ds = dso as DynamicService;
                if(ds != null)
                {
                    ds.ID = id;
                }
            }
        }

        #endregion

        public void MapServiceActionDependencies(ServiceAction serviceAction)
        {
            if(serviceAction.Cases != null)
            {
                foreach(ServiceActionCase sac in serviceAction.Cases.Cases)
                {
                    foreach(ServiceAction sa in sac.Actions)
                    {
                        MapServiceActionDependencies(sa);
                    }
                }
            }

            IEnumerable<DynamicService> service;
            LockServices();

            try
            {
                service = Services.Where(c => c.Name == serviceAction.ServiceName);
            }
            finally
            {
                UnlockServices();
            }

            // ReSharper disable UseMethodAny.1
            // ReSharper disable PossibleMultipleEnumeration
            if(service.Count() >= 1)
            // ReSharper restore PossibleMultipleEnumeration
            // ReSharper restore UseMethodAny.1
            {
                // ReSharper disable PossibleMultipleEnumeration
                serviceAction.Service = service.First();
                // ReSharper restore PossibleMultipleEnumeration
            }

            Source source;
            LockSources();

            try
            {
                source = Sources.Find(c => c.Name == serviceAction.SourceName);
            }
            finally
            {
                UnlockSources();
            }

            if(source != null)
            {
                serviceAction.Source = source;
            }
        }

        public void MapActivityToService(WorkflowActivityDef activity)
        {
            IEnumerable<DynamicService> service;
            LockServices();

            try
            {
                service = Services.Where(c => c.Name == activity.ServiceName);
            }
            finally
            {
                UnlockServices();
            }

            // ReSharper disable UseMethodAny.1
            // ReSharper disable PossibleMultipleEnumeration
            if(service.Count() >= 1)
            // ReSharper restore PossibleMultipleEnumeration
            // ReSharper restore UseMethodAny.1
            {
                // ReSharper disable PossibleMultipleEnumeration
                activity.Service = service.First();
                // ReSharper restore PossibleMultipleEnumeration
            }
        }

        public void MapUnitTests(DynamicService service)
        {
            LockServices();

            try
            {
                Services
                    .Where(c => (!string.IsNullOrEmpty(c.Name)) && c.Name == service.UnitTestTargetWorkflowService)
                    .ToList()
                    .ForEach(c => service.UnitTests.Add(c));
            }
            finally
            {
                UnlockServices();
            }
        }
        #endregion

        #region 2012.10.17 - 5782: TWR - Moved from DynamicServicesEndpoint --> GetDefaultSources, GetDefaultServices

        #region GetDefaultSources

        static List<Source> GetDefaultSources()
        {
            // ReSharper disable SuggestUseVarKeywordEvident
            List<Source> sources = new List<Source>();
            // ReSharper restore SuggestUseVarKeywordEvident

            Source s = new Source();
            s.Name = "ManagementDynamicService";
            s.Type = enSourceType.ManagementDynamicService;
            if(s.Compile())
            {
                sources.Add(s);
            }

            return sources;
        }

        #endregion

        #region GetDefaultServices

        static List<DynamicService> GetDefaultServices()
        {
            // ReSharper disable SuggestUseVarKeywordEvident
            List<DynamicService> dss = new List<DynamicService>();
            // ReSharper restore SuggestUseVarKeywordEvident


            #region Default AddResource Service
            DynamicService newDs = new DynamicService();
            newDs.Name = "AddResourceService";
            ServiceAction sa = new ServiceAction();
            sa.Name = "AddResource";
            sa.ActionType = enActionType.InvokeManagementDynamicService;
            sa.SourceMethod = "AddResource";
            sa.SourceName = "ManagementDynamicService";

            ServiceActionInput input = new ServiceActionInput();
            input.Name = "ResourceXml";
            input.Source = "ResourceXml";

            Validator validator = new Validator();
            validator.ValidatorType = enValidationType.Required;
            input.Validators.Add(validator);

            sa.ServiceActionInputs.Add(input);

            input = new ServiceActionInput();
            input.Name = "Roles";
            input.Source = "Roles";

            validator = new Validator();
            validator.ValidatorType = enValidationType.Required;
            input.Validators.Add(validator);

            sa.ServiceActionInputs.Add(input);

            newDs.Actions.Add(sa);

            if(newDs.Compile())
            {

                dss.Add(newDs);
            }
            #endregion

            #region Default AddResource Service

            DynamicService deployResourceDynamicService = new DynamicService();
            deployResourceDynamicService.Name = "DeployResourceService";

            ServiceAction deployResourceServiceAction = new ServiceAction();
            deployResourceServiceAction.Name = "DeployResource";
            deployResourceServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            deployResourceServiceAction.SourceMethod = "DeployResource";
            deployResourceServiceAction.SourceName = "ManagementDynamicService";

            ServiceActionInput deployResourceInput = new ServiceActionInput();
            deployResourceInput.Name = "ResourceXml";
            deployResourceInput.Source = "ResourceXml";

            Validator deployResourceValidator = new Validator();
            deployResourceValidator.ValidatorType = enValidationType.Required;
            deployResourceInput.Validators.Add(deployResourceValidator);

            deployResourceServiceAction.ServiceActionInputs.Add(deployResourceInput);

            deployResourceInput = new ServiceActionInput();
            deployResourceInput.Name = "Roles";
            deployResourceInput.Source = "Roles";

            validator = new Validator();
            validator.ValidatorType = enValidationType.Required;
            deployResourceInput.Validators.Add(validator);

            deployResourceServiceAction.ServiceActionInputs.Add(deployResourceInput);
            deployResourceDynamicService.Actions.Add(deployResourceServiceAction);

            if(deployResourceDynamicService.Compile())
            {
                dss.Add(deployResourceDynamicService);
            }

            #endregion

            #region Default CompileResource Service
            DynamicService compileService = new DynamicService();
            compileService.Name = "CompileResourceService";
            ServiceAction serviceAction = new ServiceAction();
            serviceAction.Name = "CompileResource";
            serviceAction.ActionType = enActionType.InvokeManagementDynamicService;
            serviceAction.SourceMethod = "CompileResource";
            serviceAction.SourceName = "ManagementDynamicService";

            ServiceActionInput serviceActionInput = new ServiceActionInput();
            serviceActionInput.Name = "ResourceXml";
            serviceActionInput.Source = "ResourceXml";

            Validator valid = new Validator();
            valid.ValidatorType = enValidationType.Required;
            serviceActionInput.Validators.Add(valid);

            serviceAction.ServiceActionInputs.Add(serviceActionInput);

            compileService.Actions.Add(serviceAction);

            if(compileService.Compile())
            {

                dss.Add(compileService);
            }
            #endregion

            #region Find Services For Binding

            #endregion

            #region List Services For Binding Service

            DynamicService findServicesBinder = new DynamicService();
            findServicesBinder.Name = "ListResourcesForBindingService";
            ServiceAction findServiceActionBinder = new ServiceAction();
            findServiceActionBinder.Name = "FindResourceForBinding";
            findServiceActionBinder.ActionType = enActionType.InvokeManagementDynamicService;
            findServiceActionBinder.SourceMethod = "FindResourceForBinding";
            findServiceActionBinder.SourceName = "ManagementDynamicService";

            ServiceActionInput findServiceActionInputBinder = new ServiceActionInput();
            findServiceActionInputBinder.Name = "ResourceName";
            findServiceActionInputBinder.Source = "ResourceName";
            findServiceActionInputBinder.IsRequired = false;

            Validator findValidBinder = new Validator();
            findValidBinder.ValidatorType = enValidationType.Required;
            findServiceActionInputBinder.Validators.Add(valid);

            findServiceActionBinder.ServiceActionInputs.Add(findServiceActionInputBinder);

            findServiceActionInputBinder = new ServiceActionInput();
            findServiceActionInputBinder.Name = "ResourceType";
            findServiceActionInputBinder.Source = "ResourceType";

            findValidBinder = new Validator();
            findValidBinder.ValidatorType = enValidationType.Required;
            findServiceActionInputBinder.Validators.Add(valid);

            findServiceActionBinder.ServiceActionInputs.Add(findServiceActionInputBinder);

            findServiceActionInputBinder = new ServiceActionInput();
            findServiceActionInputBinder.Name = "Roles";
            findServiceActionInputBinder.Source = "Roles";

            findValidBinder = new Validator();
            findValidBinder.ValidatorType = enValidationType.Required;
            findServiceActionInputBinder.Validators.Add(valid);

            findServiceActionBinder.ServiceActionInputs.Add(findServiceActionInputBinder);

            findServicesBinder.Actions.Add(findServiceActionBinder);

            if(findServicesBinder.Compile())
            {

                dss.Add(findServicesBinder);
            }
            #endregion

            #region Interrogate Plugin
            //public string InterogatePlugin(string assemblyLocation, string assemblyName, string method, string args)
            DynamicService pluingInterrogatorServicesBinder = new DynamicService();
            pluingInterrogatorServicesBinder.Name = "InterogatePluginService";
            ServiceAction pluingInterrogatorServiceActionBinder = new ServiceAction();
            pluingInterrogatorServiceActionBinder.Name = "InterrogatePlugin";
            pluingInterrogatorServiceActionBinder.ActionType = enActionType.InvokeManagementDynamicService;
            pluingInterrogatorServiceActionBinder.SourceMethod = "InterrogatePlugin";
            pluingInterrogatorServiceActionBinder.SourceName = "ManagementDynamicService";

            //ServiceActionInput pluingInterrogatorServiceActionInputBinder = new ServiceActionInput();
            ServiceActionInput pluingInterrogatorServiceActionInputBinder = new ServiceActionInput();
            pluingInterrogatorServiceActionInputBinder.Name = "AssemblyLocation";
            pluingInterrogatorServiceActionInputBinder.Source = "AssemblyLocation";
            pluingInterrogatorServiceActionInputBinder.IsRequired = false;

            Validator pluingInterrogatorValidBinder = new Validator();
            pluingInterrogatorValidBinder.ValidatorType = enValidationType.Required;
            pluingInterrogatorServiceActionInputBinder.Validators.Add(valid);

            pluingInterrogatorServiceActionBinder.ServiceActionInputs.Add(pluingInterrogatorServiceActionInputBinder);

            pluingInterrogatorServiceActionInputBinder = new ServiceActionInput();
            pluingInterrogatorServiceActionInputBinder.Name = "AssemblyName";
            pluingInterrogatorServiceActionInputBinder.Source = "AssemblyName";

            pluingInterrogatorValidBinder = new Validator();
            pluingInterrogatorValidBinder.ValidatorType = enValidationType.Required;
            pluingInterrogatorServiceActionInputBinder.Validators.Add(valid);

            pluingInterrogatorServiceActionBinder.ServiceActionInputs.Add(pluingInterrogatorServiceActionInputBinder);

            pluingInterrogatorServiceActionInputBinder = new ServiceActionInput();
            pluingInterrogatorServiceActionInputBinder.Name = "Method";
            pluingInterrogatorServiceActionInputBinder.Source = "Method";

            pluingInterrogatorValidBinder = new Validator();
            pluingInterrogatorValidBinder.ValidatorType = enValidationType.Required;
            pluingInterrogatorServiceActionInputBinder.Validators.Add(valid);

            pluingInterrogatorServiceActionBinder.ServiceActionInputs.Add(pluingInterrogatorServiceActionInputBinder);

            pluingInterrogatorServiceActionInputBinder = new ServiceActionInput();
            pluingInterrogatorServiceActionInputBinder.Name = "Args";
            pluingInterrogatorServiceActionInputBinder.Source = "Args";

            pluingInterrogatorValidBinder = new Validator();
            pluingInterrogatorValidBinder.ValidatorType = enValidationType.Required;
            pluingInterrogatorServiceActionInputBinder.Validators.Add(valid);

            pluingInterrogatorServiceActionBinder.ServiceActionInputs.Add(pluingInterrogatorServiceActionInputBinder);

            pluingInterrogatorServicesBinder.Actions.Add(pluingInterrogatorServiceActionBinder);

            if(pluingInterrogatorServicesBinder.Compile())
            {

                dss.Add(pluingInterrogatorServicesBinder);
            }
            #endregion Interrogate Plugin

            #region Reload Resource

            DynamicService reloadResourceServicesBinder = new DynamicService();
            reloadResourceServicesBinder.Name = "ReloadResourceService";
            ServiceAction reloadResourceServiceActionBinder = new ServiceAction();
            reloadResourceServiceActionBinder.Name = "ReloadResource";
            reloadResourceServiceActionBinder.ActionType = enActionType.InvokeManagementDynamicService;
            reloadResourceServiceActionBinder.SourceMethod = "ReloadResource";
            reloadResourceServiceActionBinder.SourceName = "ManagementDynamicService";

            ServiceActionInput reloadResourceServiceActionInputBinder = new ServiceActionInput();
            reloadResourceServiceActionInputBinder.Name = "ResourceName";
            reloadResourceServiceActionInputBinder.Source = "ResourceName";
            reloadResourceServiceActionInputBinder.IsRequired = false;

            Validator reloadResourceValidBinder = new Validator();
            reloadResourceValidBinder.ValidatorType = enValidationType.Required;
            reloadResourceServiceActionInputBinder.Validators.Add(valid);

            reloadResourceServiceActionBinder.ServiceActionInputs.Add(reloadResourceServiceActionInputBinder);

            reloadResourceServiceActionInputBinder = new ServiceActionInput();
            reloadResourceServiceActionInputBinder.Name = "ResourceType";
            reloadResourceServiceActionInputBinder.Source = "ResourceType";

            reloadResourceValidBinder = new Validator();
            reloadResourceValidBinder.ValidatorType = enValidationType.Required;
            reloadResourceServiceActionInputBinder.Validators.Add(valid);

            reloadResourceServiceActionBinder.ServiceActionInputs.Add(reloadResourceServiceActionInputBinder);

            reloadResourceServicesBinder.Actions.Add(reloadResourceServiceActionBinder);

            if(reloadResourceServicesBinder.Compile())
            {
                dss.Add(reloadResourceServicesBinder);
            }

            #endregion Reload Resource

            #region Get Resource

            DynamicService getResourceServicesBinder = new DynamicService();
            getResourceServicesBinder.Name = "GetResourceService";
            ServiceAction getResourceServiceActionBinder = new ServiceAction();
            getResourceServiceActionBinder.Name = "GetResource";
            getResourceServiceActionBinder.ActionType = enActionType.InvokeManagementDynamicService;
            getResourceServiceActionBinder.SourceMethod = "GetResource";
            getResourceServiceActionBinder.SourceName = "ManagementDynamicService";

            ServiceActionInput getResourceServiceActionInputBinder = new ServiceActionInput();
            getResourceServiceActionInputBinder.Name = "ResourceName";
            getResourceServiceActionInputBinder.Source = "ResourceName";
            getResourceServiceActionInputBinder.IsRequired = false;

            Validator getResourceValidBinder = new Validator();
            getResourceValidBinder.ValidatorType = enValidationType.Required;
            getResourceServiceActionInputBinder.Validators.Add(valid);

            getResourceServiceActionBinder.ServiceActionInputs.Add(getResourceServiceActionInputBinder);

            getResourceServiceActionInputBinder = new ServiceActionInput();
            getResourceServiceActionInputBinder.Name = "ResourceType";
            getResourceServiceActionInputBinder.Source = "ResourceType";

            getResourceValidBinder = new Validator();
            getResourceValidBinder.ValidatorType = enValidationType.Required;
            getResourceServiceActionInputBinder.Validators.Add(valid);

            getResourceServiceActionBinder.ServiceActionInputs.Add(getResourceServiceActionInputBinder);

            findServiceActionInputBinder = new ServiceActionInput();
            findServiceActionInputBinder.Name = "Roles";
            findServiceActionInputBinder.Source = "Roles";

            findValidBinder = new Validator();
            findValidBinder.ValidatorType = enValidationType.Required;
            findServiceActionInputBinder.Validators.Add(valid);

            getResourceServiceActionBinder.ServiceActionInputs.Add(findServiceActionInputBinder);

            getResourceServicesBinder.Actions.Add(getResourceServiceActionBinder);

            if(getResourceServicesBinder.Compile())
            {
                dss.Add(getResourceServicesBinder);
            }

            #endregion Get Resource

            #region Default FindResource Service

            DynamicService findServices = new DynamicService();
            findServices.Name = "FindResourceService";
            ServiceAction findServiceAction = new ServiceAction();
            findServiceAction.Name = "FindResource";
            findServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findServiceAction.SourceMethod = "FindResource";
            findServiceAction.SourceName = "ManagementDynamicService";

            ServiceActionInput findServiceActionInput = new ServiceActionInput();
            findServiceActionInput.Name = "ResourceName";
            findServiceActionInput.Source = "ResourceName";
            findServiceActionInput.IsRequired = false;

            Validator findValid = new Validator();
            findValid.ValidatorType = enValidationType.Required;
            findServiceActionInput.Validators.Add(valid);

            findServiceAction.ServiceActionInputs.Add(findServiceActionInput);

            findServiceActionInput = new ServiceActionInput();
            findServiceActionInput.Name = "ResourceType";
            findServiceActionInput.Source = "ResourceType";

            findValid = new Validator();
            findValid.ValidatorType = enValidationType.Required;
            findServiceActionInput.Validators.Add(valid);

            findServiceAction.ServiceActionInputs.Add(findServiceActionInput);

            findServiceActionInput = new ServiceActionInput();
            findServiceActionInput.Name = "Roles";
            findServiceActionInput.Source = "Roles";

            findValid = new Validator();
            findValid.ValidatorType = enValidationType.Required;
            findServiceActionInput.Validators.Add(valid);

            findServiceAction.ServiceActionInputs.Add(findServiceActionInput);

            findServices.Actions.Add(findServiceAction);

            if(findServices.Compile())
            {

                dss.Add(findServices);
            }
            #endregion

            #region Delete Resource
            DynamicService deleteResourceService = new DynamicService();
            deleteResourceService.Name = "DeleteResourceService";

            ServiceAction deleteResourceAction = new ServiceAction();
            deleteResourceAction.Name = "DeleteResource";
            deleteResourceAction.ActionType = enActionType.InvokeManagementDynamicService;
            deleteResourceAction.SourceMethod = "DeleteResource";
            deleteResourceAction.SourceName = "ManagementDynamicService";

            ServiceActionInput deleteResourceActionInput = new ServiceActionInput();
            deleteResourceActionInput.Name = "ResourceName";
            deleteResourceActionInput.Source = "ResourceName";
            deleteResourceActionInput.IsRequired = false;

            Validator deleteResourceValid = new Validator();
            deleteResourceValid.ValidatorType = enValidationType.Required;
            deleteResourceActionInput.Validators.Add(deleteResourceValid);

            deleteResourceAction.ServiceActionInputs.Add(deleteResourceActionInput);

            deleteResourceActionInput = new ServiceActionInput();
            deleteResourceActionInput.Name = "ResourceType";
            deleteResourceActionInput.Source = "ResourceType";

            deleteResourceValid = new Validator();
            deleteResourceValid.ValidatorType = enValidationType.Required;
            deleteResourceActionInput.Validators.Add(deleteResourceValid);

            deleteResourceAction.ServiceActionInputs.Add(deleteResourceActionInput);

            deleteResourceActionInput = new ServiceActionInput();
            deleteResourceActionInput.Name = "Roles";
            deleteResourceActionInput.Source = "Roles";

            deleteResourceValid = new Validator();
            deleteResourceValid.ValidatorType = enValidationType.Required;
            deleteResourceActionInput.Validators.Add(deleteResourceValid);

            deleteResourceAction.ServiceActionInputs.Add(deleteResourceActionInput);

            deleteResourceService.Actions.Add(deleteResourceAction);

            if(deleteResourceService.Compile())
            {
                dss.Add(deleteResourceService);
            }


            #endregion Delete Resource

            #region Find All Resources
            DynamicService findAllService = new DynamicService();
            findAllService.Name = "FindResourcesService";

            ServiceAction findAllServiceAction = new ServiceAction();
            findAllServiceAction.Name = "FindResources";
            findAllServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findAllServiceAction.SourceMethod = "FindResources";
            findAllServiceAction.SourceName = "ManagementDynamicService";

            ServiceActionInput findAllServicesActionInput = new ServiceActionInput();
            findAllServicesActionInput.Name = "ResourceName";
            findAllServicesActionInput.Source = "ResourceName";
            findAllServicesActionInput.IsRequired = false;

            Validator findAllServicesValid = new Validator();
            findAllServicesValid.ValidatorType = enValidationType.Required;
            findAllServicesActionInput.Validators.Add(findAllServicesValid);

            findAllServiceAction.ServiceActionInputs.Add(findAllServicesActionInput);

            findAllServicesActionInput = new ServiceActionInput();
            findAllServicesActionInput.Name = "ResourceType";
            findAllServicesActionInput.Source = "ResourceType";

            findAllServicesValid = new Validator();
            findAllServicesValid.ValidatorType = enValidationType.Required;
            findAllServicesActionInput.Validators.Add(findAllServicesValid);

            findAllServiceAction.ServiceActionInputs.Add(findAllServicesActionInput);

            findAllServicesActionInput = new ServiceActionInput();
            findAllServicesActionInput.Name = "Roles";
            findAllServicesActionInput.Source = "Roles";

            findAllServicesValid = new Validator();
            findAllServicesValid.ValidatorType = enValidationType.Required;
            findAllServicesActionInput.Validators.Add(findAllServicesValid);

            findAllServiceAction.ServiceActionInputs.Add(findAllServicesActionInput);

            findAllService.Actions.Add(findAllServiceAction);

            if(findAllService.Compile())
            {

                dss.Add(findAllService);
            }


            #endregion Find All Resources

            #region Find current logged in user
            DynamicService findServerUsernameService = new DynamicService();
            findServerUsernameService.Name = "FindServerUsernameService";

            ServiceAction findServerUsernameServiceAction = new ServiceAction();
            findServerUsernameServiceAction.Name = "FindServerUsername";
            findServerUsernameServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findServerUsernameServiceAction.SourceMethod = "FindServerUsername";
            findServerUsernameServiceAction.SourceName = "ManagementDynamicService";

            findServerUsernameService.Actions.Add(findServerUsernameServiceAction);

            if(findServerUsernameService.Compile())
            {

                dss.Add(findServerUsernameService);
            }
            #endregion Find current logged in user

            //For future reference: Update the region below to get all internal services
            #region Find Supported Database Servers
            DynamicService findAllSupportedDatabaseServersService = new DynamicService();
            findAllSupportedDatabaseServersService.Name = "FindSupportedDatabaseServersService";

            ServiceAction findAllSupportedDatabaseServersServiceAction = new ServiceAction();
            findAllSupportedDatabaseServersServiceAction.Name = "FindSupportedDatabaseServers";
            findAllSupportedDatabaseServersServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findAllSupportedDatabaseServersServiceAction.SourceMethod = "FindSupportedDatabaseServers";
            //findAllSupportedDatabaseServersServiceAction.SourceName = "ManagementDynamicService";

            findAllSupportedDatabaseServersService.Actions.Add(findAllSupportedDatabaseServersServiceAction);

            if(findAllSupportedDatabaseServersService.Compile())
            {

                dss.Add(findAllSupportedDatabaseServersService);
            }
            #endregion Find All SQLSERVER Databases

            #region Find all logical drives
            DynamicService findDriveService = new DynamicService();
            findDriveService.Name = "FindDriveService";

            ServiceAction findDriveServiceAction = new ServiceAction();
            findDriveServiceAction.Name = "FindDrive";
            findDriveServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findDriveServiceAction.SourceMethod = "FindDrive";
            findDriveServiceAction.SourceName = "ManagementDynamicService";

            ServiceActionInput findDriveServiceActionInput = new ServiceActionInput();
            findDriveServiceActionInput.Name = "Domain";
            findDriveServiceActionInput.Source = "Domain";
            findDriveServiceActionInput.IsRequired = false;

            Validator findDriveServiceValid = new Validator();
            findDriveServiceValid.ValidatorType = enValidationType.Required;
            findDriveServiceActionInput.Validators.Add(findDriveServiceValid);

            findDriveServiceAction.ServiceActionInputs.Add(findDriveServiceActionInput);

            findDriveServiceActionInput = new ServiceActionInput();
            findDriveServiceActionInput.Name = "Username";
            findDriveServiceActionInput.Source = "Username";
            findDriveServiceActionInput.IsRequired = false;

            findDriveServiceValid = new Validator();
            findDriveServiceValid.ValidatorType = enValidationType.Required;
            findDriveServiceActionInput.Validators.Add(findDriveServiceValid);

            findDriveServiceAction.ServiceActionInputs.Add(findDriveServiceActionInput);

            findDriveServiceActionInput = new ServiceActionInput();
            findDriveServiceActionInput.Name = "Password";
            findDriveServiceActionInput.Source = "Password";
            findDriveServiceActionInput.IsRequired = false;

            findDriveServiceValid = new Validator();
            findDriveServiceValid.ValidatorType = enValidationType.Required;
            findDriveServiceActionInput.Validators.Add(findDriveServiceValid);

            findDriveServiceAction.ServiceActionInputs.Add(findDriveServiceActionInput);

            findDriveService.Actions.Add(findDriveServiceAction);

            if(findDriveService.Compile())
            {

                dss.Add(findDriveService);
            }
            #endregion Find all logical drives

            #region Check credentials
            DynamicService checkCredentialsService = new DynamicService();
            checkCredentialsService.Name = "CheckCredentialsService";

            ServiceAction checkCredentialsServiceAction = new ServiceAction();
            checkCredentialsServiceAction.Name = "CheckCredentials";
            checkCredentialsServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            checkCredentialsServiceAction.SourceMethod = "CheckCredentials";
            checkCredentialsServiceAction.SourceName = "ManagementDynamicService";

            ServiceActionInput checkCredentialsServiceActionInput = new ServiceActionInput();
            checkCredentialsServiceActionInput.Name = "Domain";
            checkCredentialsServiceActionInput.Source = "Domain";

            checkCredentialsServiceAction.ServiceActionInputs.Add(checkCredentialsServiceActionInput);

            checkCredentialsServiceActionInput = new ServiceActionInput();
            checkCredentialsServiceActionInput.Name = "Username";
            checkCredentialsServiceActionInput.Source = "Username";

            checkCredentialsServiceAction.ServiceActionInputs.Add(checkCredentialsServiceActionInput);

            checkCredentialsServiceActionInput = new ServiceActionInput();
            checkCredentialsServiceActionInput.Name = "Password";
            checkCredentialsServiceActionInput.Source = "Password";

            checkCredentialsServiceAction.ServiceActionInputs.Add(checkCredentialsServiceActionInput);

            checkCredentialsService.Actions.Add(checkCredentialsServiceAction);

            if(checkCredentialsService.Compile())
            {

                dss.Add(checkCredentialsService);
            }
            #endregion Check credentials

            #region Check Permissions
            DynamicService checkPermissionsService = new DynamicService();
            checkPermissionsService.Name = "CheckPermissionsService";

            ServiceAction checkPermissionsServiceAction = new ServiceAction();
            checkPermissionsServiceAction.Name = "CheckPermissions";
            checkPermissionsServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            checkPermissionsServiceAction.SourceMethod = "CheckPermissions";
            checkPermissionsServiceAction.SourceName = "ManagementDynamicService";

            ServiceActionInput checkPermissionsServiceActionInput = new ServiceActionInput();
            checkPermissionsServiceActionInput.Name = "Path";
            checkPermissionsServiceActionInput.Source = "Path";
            checkPermissionsServiceActionInput.IsRequired = true;

            Validator checkPermissionsServiceValidator = new Validator();
            checkPermissionsServiceValidator.ValidatorType = enValidationType.Required;
            checkPermissionsServiceActionInput.Validators.Add(checkPermissionsServiceValidator);

            checkPermissionsServiceAction.ServiceActionInputs.Add(checkPermissionsServiceActionInput);

            checkPermissionsServiceActionInput = new ServiceActionInput();
            checkPermissionsServiceActionInput.Name = "Username";
            checkPermissionsServiceActionInput.Source = "Username";
            checkPermissionsServiceActionInput.IsRequired = false;

            checkPermissionsServiceAction.ServiceActionInputs.Add(checkPermissionsServiceActionInput);

            checkPermissionsServiceActionInput = new ServiceActionInput();
            checkPermissionsServiceActionInput.Name = "Password";
            checkPermissionsServiceActionInput.Source = "Password";
            checkPermissionsServiceActionInput.IsRequired = false;

            checkPermissionsServiceAction.ServiceActionInputs.Add(checkPermissionsServiceActionInput);

            checkPermissionsService.Actions.Add(checkPermissionsServiceAction);

            if(checkPermissionsService.Compile())
            {

                dss.Add(checkPermissionsService);
            }
            #endregion Check Permissions

            #region Find sub-directories and files in given directory
            DynamicService findDirectoryService = new DynamicService();
            findDirectoryService.Name = "FindDirectoryService";

            ServiceAction findDirectoryServiceAction = new ServiceAction();
            findDirectoryServiceAction.Name = "FindDirectory";
            findDirectoryServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findDirectoryServiceAction.SourceMethod = "FindDirectory";
            findDirectoryServiceAction.SourceName = "ManagementDynamicService";

            ServiceActionInput findDirectoryServiceActionInput = new ServiceActionInput();
            findDirectoryServiceActionInput.Name = "DirectoryPath";
            findDirectoryServiceActionInput.Source = "DirectoryPath";
            findDirectoryServiceActionInput.IsRequired = true;

            Validator findDirectoryServiceValid = new Validator();
            findDirectoryServiceValid.ValidatorType = enValidationType.Required;
            findDirectoryServiceActionInput.Validators.Add(findDirectoryServiceValid);

            findDirectoryServiceAction.ServiceActionInputs.Add(findDirectoryServiceActionInput);

            findDirectoryServiceActionInput = new ServiceActionInput();
            findDirectoryServiceActionInput.Name = "Domain";
            findDirectoryServiceActionInput.Source = "Domain";
            findDirectoryServiceActionInput.IsRequired = false;

            findDirectoryServiceValid = new Validator();
            findDirectoryServiceValid.ValidatorType = enValidationType.Required;
            findDirectoryServiceActionInput.Validators.Add(findDirectoryServiceValid);

            findDirectoryServiceAction.ServiceActionInputs.Add(findDirectoryServiceActionInput);

            findDirectoryServiceActionInput = new ServiceActionInput();
            findDirectoryServiceActionInput.Name = "Username";
            findDirectoryServiceActionInput.Source = "Username";
            findDirectoryServiceActionInput.IsRequired = false;

            findDirectoryServiceValid = new Validator();
            findDirectoryServiceValid.ValidatorType = enValidationType.Required;
            findDirectoryServiceActionInput.Validators.Add(findDirectoryServiceValid);

            findDirectoryServiceAction.ServiceActionInputs.Add(findDirectoryServiceActionInput);

            findDirectoryServiceActionInput = new ServiceActionInput();
            findDirectoryServiceActionInput.Name = "Password";
            findDirectoryServiceActionInput.Source = "Password";
            findDirectoryServiceActionInput.IsRequired = false;

            findDirectoryServiceValid = new Validator();
            findDirectoryServiceValid.ValidatorType = enValidationType.Required;
            findDirectoryServiceActionInput.Validators.Add(findDirectoryServiceValid);

            findDirectoryServiceAction.ServiceActionInputs.Add(findDirectoryServiceActionInput);

            findDirectoryService.Actions.Add(findDirectoryServiceAction);

            if(findDirectoryService.Compile())
            {

                dss.Add(findDirectoryService);
            }
            #endregion Find sub-directories and files in given directory

            #region Find Supported Database Servers
            // ReSharper disable SuggestUseVarKeywordEvident
            DynamicService findAllSqlDatabasesService = new DynamicService();
            findAllSqlDatabasesService.Name = "FindSqlDatabasesService";


            ServiceAction findAllSqlDatabasesServiceAction = new ServiceAction();

            findAllSqlDatabasesServiceAction.Name = "FindSqlDatabases";
            findAllSqlDatabasesServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findAllSqlDatabasesServiceAction.SourceMethod = "FindSqlDatabases";
            //findAllSupportedDatabaseServersServiceAction.SourceName = "ManagementDynamicService";

            ServiceActionInput findAllSqlDatabasesServiceActionInput = new ServiceActionInput();
            findAllSqlDatabasesServiceActionInput.Name = "ServerName";
            findAllSqlDatabasesServiceActionInput.Source = "ServerName";
            findAllSqlDatabasesServiceActionInput.IsRequired = true;

            Validator findAllSqlDatabasesServiceValidator = new Validator();
            findAllSqlDatabasesServiceValidator.ValidatorType = enValidationType.Required;
            findAllSqlDatabasesServiceActionInput.Validators.Add(findAllSqlDatabasesServiceValidator);
            // ReSharper restore SuggestUseVarKeywordEvident

            findAllSqlDatabasesServiceAction.ServiceActionInputs.Add(findAllSqlDatabasesServiceActionInput);

            findAllSqlDatabasesServiceActionInput = new ServiceActionInput();
            findAllSqlDatabasesServiceActionInput.Name = "Username";
            findAllSqlDatabasesServiceActionInput.Source = "Username";
            findAllSqlDatabasesServiceActionInput.IsRequired = false;

            findAllSqlDatabasesServiceValidator = new Validator();
            findAllSqlDatabasesServiceValidator.ValidatorType = enValidationType.Required;
            findAllSqlDatabasesServiceActionInput.Validators.Add(findAllSqlDatabasesServiceValidator);

            findAllSqlDatabasesServiceAction.ServiceActionInputs.Add(findAllSqlDatabasesServiceActionInput);

            findAllSqlDatabasesServiceActionInput = new ServiceActionInput();
            findAllSqlDatabasesServiceActionInput.Name = "Password";
            findAllSqlDatabasesServiceActionInput.Source = "Password";
            findAllSqlDatabasesServiceActionInput.IsRequired = false;

            findAllSqlDatabasesServiceValidator = new Validator();
            findAllSqlDatabasesServiceValidator.ValidatorType = enValidationType.Required;
            findAllSqlDatabasesServiceActionInput.Validators.Add(findAllSqlDatabasesServiceValidator);

            findAllSqlDatabasesServiceAction.ServiceActionInputs.Add(findAllSqlDatabasesServiceActionInput);

            findAllSqlDatabasesService.Actions.Add(findAllSqlDatabasesServiceAction);

            if(findAllSqlDatabasesService.Compile())
            {

                dss.Add(findAllSqlDatabasesService);
            }
            #endregion Find All SQLSERVER Databases

            #region Find SQLSERVER database schema

            // ReSharper disable SuggestUseVarKeywordEvident
            DynamicService findSqlDatabaseSchemaService = new DynamicService();
            findSqlDatabaseSchemaService.Name = "FindSqlDatabaseSchemaService";

            ServiceAction findSqlDatabaseSchemaServiceAction = new ServiceAction();
            findSqlDatabaseSchemaServiceAction.Name = "FindSqlDatabaseSchema";
            findSqlDatabaseSchemaServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findSqlDatabaseSchemaServiceAction.SourceMethod = "FindSqlDatabaseSchema";
            //findAllSupportedDatabaseServersServiceAction.SourceName = "ManagementDynamicService";

            ServiceActionInput findSqlDatabaseSchemaServiceActionInput = new ServiceActionInput();
            findSqlDatabaseSchemaServiceActionInput.Name = "ServerName";
            findSqlDatabaseSchemaServiceActionInput.Source = "ServerName";
            findSqlDatabaseSchemaServiceActionInput.IsRequired = true;

            Validator findSqlDatabaseSchemaServiceValidator = new Validator();
            findSqlDatabaseSchemaServiceValidator.ValidatorType = enValidationType.Required;
            findSqlDatabaseSchemaServiceActionInput.Validators.Add(findSqlDatabaseSchemaServiceValidator);
            // ReSharper restore SuggestUseVarKeywordEvident

            findSqlDatabaseSchemaServiceAction.ServiceActionInputs.Add(findSqlDatabaseSchemaServiceActionInput);

            findSqlDatabaseSchemaServiceActionInput = new ServiceActionInput();
            findSqlDatabaseSchemaServiceActionInput.Name = "DatabaseName";
            findSqlDatabaseSchemaServiceActionInput.Source = "DatabaseName";
            findSqlDatabaseSchemaServiceActionInput.IsRequired = true;

            findSqlDatabaseSchemaServiceValidator = new Validator();
            findSqlDatabaseSchemaServiceValidator.ValidatorType = enValidationType.Required;
            findSqlDatabaseSchemaServiceActionInput.Validators.Add(findSqlDatabaseSchemaServiceValidator);

            findSqlDatabaseSchemaServiceAction.ServiceActionInputs.Add(findSqlDatabaseSchemaServiceActionInput);

            findSqlDatabaseSchemaServiceActionInput = new ServiceActionInput();
            findSqlDatabaseSchemaServiceActionInput.Name = "Username";
            findSqlDatabaseSchemaServiceActionInput.Source = "Username";
            findSqlDatabaseSchemaServiceActionInput.IsRequired = false;

            findSqlDatabaseSchemaServiceAction.ServiceActionInputs.Add(findSqlDatabaseSchemaServiceActionInput);

            findSqlDatabaseSchemaServiceActionInput = new ServiceActionInput();
            findSqlDatabaseSchemaServiceActionInput.Name = "Password";
            findSqlDatabaseSchemaServiceActionInput.Source = "Password";
            findSqlDatabaseSchemaServiceActionInput.IsRequired = false;

            findSqlDatabaseSchemaServiceAction.ServiceActionInputs.Add(findSqlDatabaseSchemaServiceActionInput);

            findSqlDatabaseSchemaService.Actions.Add(findSqlDatabaseSchemaServiceAction);

            if(findSqlDatabaseSchemaService.Compile())
            {

                dss.Add(findSqlDatabaseSchemaService);
            }
            #endregion Find SQLSERVER database schema

            #region Execute SQLSERVER function/procedure

            // ReSharper disable InconsistentNaming
            DynamicService CallProcedureService = new DynamicService();
            CallProcedureService.Name = "CallProcedureService";

            ServiceAction CallProcedureServiceAction = new ServiceAction();
            CallProcedureServiceAction.Name = "CallProcedure";
            CallProcedureServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            CallProcedureServiceAction.SourceMethod = "CallProcedure";
            //findAllSupportedDatabaseServersServiceAction.SourceName = "ManagementDynamicService";

            ServiceActionInput CallProcedureServiceActionInput = new ServiceActionInput();
            CallProcedureServiceActionInput.Name = "ServerName";
            CallProcedureServiceActionInput.Source = "ServerName";
            CallProcedureServiceActionInput.IsRequired = true;

            Validator CallProcedureServiceValidator = new Validator();
            CallProcedureServiceValidator.ValidatorType = enValidationType.Required;
            CallProcedureServiceActionInput.Validators.Add(CallProcedureServiceValidator);
            // ReSharper restore InconsistentNaming

            CallProcedureServiceAction.ServiceActionInputs.Add(CallProcedureServiceActionInput);

            CallProcedureServiceActionInput = new ServiceActionInput();
            CallProcedureServiceActionInput.Name = "DatabaseName";
            CallProcedureServiceActionInput.Source = "DatabaseName";
            CallProcedureServiceActionInput.IsRequired = true;

            CallProcedureServiceValidator = new Validator();
            CallProcedureServiceValidator.ValidatorType = enValidationType.Required;
            CallProcedureServiceActionInput.Validators.Add(CallProcedureServiceValidator);

            CallProcedureServiceAction.ServiceActionInputs.Add(CallProcedureServiceActionInput);

            CallProcedureServiceActionInput = new ServiceActionInput();
            CallProcedureServiceActionInput.Name = "Procedure";
            CallProcedureServiceActionInput.Source = "Procedure";
            CallProcedureServiceActionInput.IsRequired = true;

            CallProcedureServiceValidator = new Validator();
            CallProcedureServiceValidator.ValidatorType = enValidationType.Required;
            CallProcedureServiceActionInput.Validators.Add(CallProcedureServiceValidator);

            CallProcedureServiceAction.ServiceActionInputs.Add(CallProcedureServiceActionInput);

            CallProcedureServiceActionInput = new ServiceActionInput();
            CallProcedureServiceActionInput.Name = "Parameters";
            CallProcedureServiceActionInput.Source = "Parameters";
            CallProcedureServiceActionInput.IsRequired = false;

            CallProcedureServiceValidator = new Validator();
            CallProcedureServiceValidator.ValidatorType = enValidationType.Required;
            CallProcedureServiceActionInput.Validators.Add(CallProcedureServiceValidator);

            CallProcedureServiceAction.ServiceActionInputs.Add(CallProcedureServiceActionInput);

            // Travis.Frisinger : Added for future extension
            CallProcedureServiceActionInput = new ServiceActionInput();
            CallProcedureServiceActionInput.Name = "Mode";
            CallProcedureServiceActionInput.Source = "Mode";
            CallProcedureServiceActionInput.IsRequired = false;

            CallProcedureServiceAction.ServiceActionInputs.Add(CallProcedureServiceActionInput);

            CallProcedureServiceActionInput = new ServiceActionInput();
            CallProcedureServiceActionInput.Name = "Username";
            CallProcedureServiceActionInput.Source = "Username";
            CallProcedureServiceActionInput.IsRequired = false;

            CallProcedureServiceAction.ServiceActionInputs.Add(CallProcedureServiceActionInput);

            CallProcedureServiceActionInput = new ServiceActionInput();
            CallProcedureServiceActionInput.Name = "Password";
            CallProcedureServiceActionInput.Source = "Password";
            CallProcedureServiceActionInput.IsRequired = false;

            CallProcedureServiceAction.ServiceActionInputs.Add(CallProcedureServiceActionInput);


            //CallProcedureServiceActionInput = new ServiceActionInput();
            //CallProcedureServiceActionInput.Name = "DBType";
            //CallProcedureServiceActionInput.Source = "DBType";
            //CallProcedureServiceActionInput.IsRequired = false;
            //CallProcedureServiceAction.ServiceActionInputs.Add(CallProcedureServiceActionInput);

            // End amendments

            CallProcedureService.Actions.Add(CallProcedureServiceAction);

            if(CallProcedureService.Compile())
            {

                dss.Add(CallProcedureService);
            }
            #endregion Execute SQLSERVER function/procedure

            #region Default FindDependency Service
            DynamicService findDependency = new DynamicService();
            findDependency.Name = "FindDependencyService";
            ServiceAction findDependencyAction = new ServiceAction();
            findDependencyAction.Name = "FindDependency";
            findDependencyAction.ActionType = enActionType.InvokeManagementDynamicService;
            findDependencyAction.SourceMethod = "FindDependencies";
            findDependencyAction.SourceName = "ManagementDynamicService";

            ServiceActionInput findDependencyActionInput = new ServiceActionInput();
            findDependencyActionInput.Name = "ResourceName";
            findDependencyActionInput.Source = "ResourceName";
            findDependencyActionInput.IsRequired = false;

            Validator findValidDep = new Validator();
            findValidDep.ValidatorType = enValidationType.Required;
            findDependencyActionInput.Validators.Add(findValidDep);

            findDependencyAction.ServiceActionInputs.Add(findDependencyActionInput);

            findDependency.Actions.Add(findDependencyAction);

            if(findDependency.Compile())
            {

                dss.Add(findDependency);
            }

            #endregion

            #region Find Registered Assemblies (GAC)
            DynamicService registeredAssemblyService = new DynamicService();
            registeredAssemblyService.Name = "RegisteredAssemblyService";
            ServiceAction registeredAssemblyAction = new ServiceAction();
            registeredAssemblyAction.Name = "RegisteredAssembly";
            registeredAssemblyAction.ActionType = enActionType.InvokeManagementDynamicService;
            registeredAssemblyAction.SourceMethod = "RegisteredAssembly";
            registeredAssemblyAction.SourceName = "ManagementDynamicService";

            registeredAssemblyService.Actions.Add(registeredAssemblyAction);

            if(registeredAssemblyService.Compile())
            {

                dss.Add(registeredAssemblyService);
            }
            #endregion Find Registered Assemblies (GAC)

            // Travis.Frisinger
            #region Plugin MetaData Discovery Service

            DynamicService pluginMetaDataService = new DynamicService();
            pluginMetaDataService.Name = "PluginRegistryService";
            ServiceAction pluginMetaDataAction = new ServiceAction();
            pluginMetaDataAction.Name = "PluginRegistry";
            pluginMetaDataAction.ActionType = enActionType.InvokeManagementDynamicService;
            pluginMetaDataAction.SourceMethod = "PluginMetaDataRegistry";
            pluginMetaDataAction.SourceName = "ManagementDynamicService";

            ServiceActionInput pluginMetaDataInput = new ServiceActionInput();
            pluginMetaDataInput.Name = "AssemblyLocation";
            pluginMetaDataInput.Source = "AssemblyLocation";
            pluginMetaDataInput.IsRequired = false;

            pluginMetaDataAction.ServiceActionInputs.Add(pluginMetaDataInput);


            ServiceActionInput pluginMetaDataNamespace = new ServiceActionInput();
            pluginMetaDataNamespace.Name = "NameSpace";
            pluginMetaDataNamespace.Source = "NameSpace";
            pluginMetaDataNamespace.IsRequired = false;

            pluginMetaDataAction.ServiceActionInputs.Add(pluginMetaDataNamespace);


            ServiceActionInput pluginMetaDataInputPLevel = new ServiceActionInput();
            pluginMetaDataInputPLevel.Name = "ProtectionLevel";
            pluginMetaDataInputPLevel.Source = "ProtectionLevel";
            pluginMetaDataInputPLevel.IsRequired = false;

            pluginMetaDataAction.ServiceActionInputs.Add(pluginMetaDataInputPLevel);



            ServiceActionInput pluginMetaDataInputSig = new ServiceActionInput();
            pluginMetaDataInputSig.Name = "MethodName";
            pluginMetaDataInputSig.Source = "MethodName";
            pluginMetaDataInputSig.IsRequired = false;

            pluginMetaDataAction.ServiceActionInputs.Add(pluginMetaDataInputSig);

            pluginMetaDataService.Actions.Add(pluginMetaDataAction);

            if(pluginMetaDataService.Compile())
            {

                dss.Add(pluginMetaDataService);
            }
            #endregion

            #region Find MachineName
            DynamicService findMachineNameService = new DynamicService();
            findMachineNameService.Name = "FindMachineNameService";
            ServiceAction findMachineNameAction = new ServiceAction();
            findMachineNameAction.Name = "FindMachineName";
            findMachineNameAction.ActionType = enActionType.InvokeManagementDynamicService;
            findMachineNameAction.SourceMethod = "FindMachineName";
            findMachineNameAction.SourceName = "ManagementDynamicService";

            findMachineNameService.Actions.Add(findMachineNameAction);

            if(findMachineNameService.Compile())
            {

                dss.Add(findMachineNameService);
            }
            #endregion Find MachineName

            #region Find Network Computers
            DynamicService findNetworkComputersService = new DynamicService();
            findNetworkComputersService.Name = "FindNetworkComputersService";
            ServiceAction findNetworkComputersAction = new ServiceAction();
            findNetworkComputersAction.Name = "FindNetworkComputers";
            findNetworkComputersAction.ActionType = enActionType.InvokeManagementDynamicService;
            findNetworkComputersAction.SourceMethod = "FindNetworkComputers";
            findNetworkComputersAction.SourceName = "ManagementDynamicService";

            findNetworkComputersService.Actions.Add(findNetworkComputersAction);

            if(findNetworkComputersService.Compile())
            {

                dss.Add(findNetworkComputersService);
            }
            #endregion Find Network Computers

            #region UpdateWorkspaceItemService

            // 2012.10.18 - 5782: TWR

            var workspaceItemAction = new ServiceAction { Name = "UpdateWorkspaceItem", ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = "UpdateWorkspaceItem", SourceName = "ManagementDynamicService" };

            var workspaceItemInput = new ServiceActionInput { Name = "ItemXml", Source = "ItemXml" };
            workspaceItemInput.Validators.Add(new Validator { ValidatorType = enValidationType.Required });
            workspaceItemAction.ServiceActionInputs.Add(workspaceItemInput);

            workspaceItemInput = new ServiceActionInput { Name = "Roles", Source = "Roles" };
            workspaceItemInput.Validators.Add(new Validator { ValidatorType = enValidationType.Required });
            workspaceItemAction.ServiceActionInputs.Add(workspaceItemInput);

            var workspaceItemService = new DynamicService { Name = "UpdateWorkspaceItemService" };
            workspaceItemService.Actions.Add(workspaceItemAction);

            if(workspaceItemService.Compile())
            {
                dss.Add(workspaceItemService);
            }

            #endregion

            #region GetLatestService

            // 2012.10.18 - 5782: TWR

            var getLatestAction = new ServiceAction { Name = "GetLatest", ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = "GetLatest", SourceName = "ManagementDynamicService" };

            var getLatestInput = new ServiceActionInput { Name = "EditedItemsXml", Source = "EditedItemsXml" };
            getLatestInput.Validators.Add(new Validator { ValidatorType = enValidationType.Required });
            getLatestAction.ServiceActionInputs.Add(getLatestInput);

            var getLatestService = new DynamicService { Name = "GetLatestService" };
            getLatestService.Actions.Add(getLatestAction);

            if(getLatestService.Compile())
            {
                dss.Add(getLatestService);
            }

            #endregion

            #region FindResourcesByID

            // PBI 6597: TWR - added

            var findResourcesByIDAction = new ServiceAction { Name = "FindResourcesByID", ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = "FindResourcesByID", SourceName = "ManagementDynamicService" };

            var findResourcesByIDInput1 = new ServiceActionInput { Name = "GuidCsv", Source = "GuidCsv" };
            findResourcesByIDInput1.Validators.Add(new Validator { ValidatorType = enValidationType.Required });
            findResourcesByIDAction.ServiceActionInputs.Add(findResourcesByIDInput1);

            var findResourcesByIDInput2 = new ServiceActionInput { Name = "Type", Source = "Type" };
            findResourcesByIDInput2.Validators.Add(new Validator { ValidatorType = enValidationType.Required });
            findResourcesByIDAction.ServiceActionInputs.Add(findResourcesByIDInput2);

            var findResourcesByIDService = new DynamicService { Name = "FindResourcesByID" };
            findResourcesByIDService.Actions.Add(findResourcesByIDAction);

            if(findResourcesByIDService.Compile())
            {
                dss.Add(findResourcesByIDService);
            }

            #endregion

            #region FindSourcesByType

            // PBI 6597: TWR - added

            var findSourcesByTypeAction = new ServiceAction { Name = "FindSourcesByType", ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = "FindSourcesByType", SourceName = "ManagementDynamicService" };

            var findSourcesByTypeInput1 = new ServiceActionInput { Name = "Type", Source = "Type" };
            findSourcesByTypeInput1.Validators.Add(new Validator { ValidatorType = enValidationType.Required });
            findSourcesByTypeAction.ServiceActionInputs.Add(findSourcesByTypeInput1);

            var findSourcesByTypeService = new DynamicService { Name = "FindSourcesByType" };
            findSourcesByTypeService.Actions.Add(findSourcesByTypeAction);

            if(findSourcesByTypeService.Compile())
            {
                dss.Add(findSourcesByTypeService);
            }

            #endregion

            #region InvokeService

            var invokeServiceAction = new ServiceAction { Name = "InvokeService", ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = "InvokeService", SourceName = "ManagementDynamicService" };

            var invokeServiceActionInputService = new ServiceActionInput { Name = "Name", Source = "Name" };
            invokeServiceActionInputService.Validators.Add(new Validator { ValidatorType = enValidationType.Required });
            invokeServiceAction.ServiceActionInputs.Add(invokeServiceActionInputService);

            var invokeServiceActionInputAction = new ServiceActionInput { Name = "Action", Source = "Action" };
            invokeServiceActionInputAction.Validators.Add(new Validator { ValidatorType = enValidationType.Required });
            invokeServiceAction.ServiceActionInputs.Add(invokeServiceActionInputAction);

            var invokeServiceActionInputArgs = new ServiceActionInput { Name = "Args", Source = "Args" };
            invokeServiceActionInputArgs.Validators.Add(new Validator { ValidatorType = enValidationType.Required });
            invokeServiceAction.ServiceActionInputs.Add(invokeServiceActionInputArgs);

            var invokeServiceDynamicService = new DynamicService { Name = "InvokeService" };
            invokeServiceDynamicService.Actions.Add(invokeServiceAction);

            if(invokeServiceDynamicService.Compile())
            {
                dss.Add(invokeServiceDynamicService);
            }

            #endregion

            return dss;
        }

        #endregion

        #endregion
    }
    #endregion

}
