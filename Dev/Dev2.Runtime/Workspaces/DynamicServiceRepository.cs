using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.DynamicServices;
using Unlimited.Framework;

namespace Dev2.Workspaces
{
    /// <summary>
    /// A <see cref="IDynamicServiceObject" /> repository
    /// </summary>
    public class DynamicServiceRepository : IDynamicServiceRepository
    {
        //
        // TODO: Finalize implementation - need to refactor DynamicServicesHost --> AddWorkflowActivity, AddBizRule, AddDynamicService, AddSource
        // 
        readonly ConcurrentDictionary<string, IDynamicServiceObject> _items = new ConcurrentDictionary<string, IDynamicServiceObject>();

        #region ServerRepository

        static IDynamicServiceRepository _serverRepository;

        /// <summary>
        /// Gets the singleton instance of the server <see cref="IDynamicServiceRepository"/>
        /// </summary>
        public static IDynamicServiceRepository ServerRepository
        {
            get
            {
                if(_serverRepository == null)
                {
                    _serverRepository = new DynamicServiceRepository
                    {
                        SourcePath = Path.Combine(GlobalConstants.ApplicationPath, Guid.Empty.ToString())
                    };
                    _serverRepository.Load(null);
                }
                return _serverRepository;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the items in the repository.
        /// </summary>
        public ICollection<IDynamicServiceObject> Items
        {
            get
            {
                return _items.Values;
            }
        }

        /// <summary>
        /// Gets or sets the path of the folder containing the repository's files.
        /// </summary>
        public string SourcePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the path of the folder containing the repository's version control files.
        /// </summary>
        public string VersionPath
        {
            get
            {
                return string.Format("{0}\\VersionControl", SourcePath);
            }
        }

        #endregion

        #region File Handling

        #region Read

        /// <summary>
        /// Reads service definition (*.XML) files from the repository's <see cref="SourcePath"/>.
        /// </summary>
        /// <returns>A list of services in <see cref="SourcePath"/>.</returns>
        public IList<IDynamicServiceObject> Read()
        {
            EnsurePath();

            var result = new List<IDynamicServiceObject>();
            foreach(var xml in Directory.EnumerateFiles(SourcePath, "*.xml").Select(File.ReadAllText))
            {
                result.AddRange(GenerateObjectGraphFromString(xml));
            }
            return result;
        }

        #endregion

        #region Write

        /// <summary>
        /// Writes the given service object to the repository's <see cref="SourcePath" />.
        /// </summary>
        /// <param name="dso">The dynamic service object to be written.</param>
        public void Write(IDynamicServiceObject dso)
        {
            if(dso == null)
            {
                return;
            }

            EnsurePath();

            var sourcePath = GetFileName(dso);
            WriteVersion(sourcePath);

            File.WriteAllText(sourcePath, dso.ResourceDefinition, Encoding.UTF8);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes the given service object from the repository's <see cref="SourcePath" />.
        /// </summary>
        /// <param name="dso">The dynamic service object to be deleted.</param>
        public void Delete(IDynamicServiceObject dso)
        {
            if(dso == null)
            {
                return;
            }

            EnsurePath();

            var sourcePath = GetFileName(dso);
            WriteVersion(sourcePath);
            if(File.Exists(sourcePath))
            {
                File.Delete(sourcePath);
            }
        }

        #endregion

        #region WriteVersion

        /// <summary>
        /// Writes the next version to the <see cref="VersionPath"/>.
        /// </summary>
        /// <param name="sourcePath">The path to the service object in the <see cref="SourcePath"/>.</param>
        void WriteVersion(string sourcePath)
        {
            if(File.Exists(sourcePath))
            {
                var sourceName = Path.GetFileNameWithoutExtension(sourcePath);
                var extension = Path.GetExtension(sourcePath);  // includes period
                var searchPattern = string.Format("{0}*{1}", sourceName, extension);
                var count = Directory.GetFiles(VersionPath, searchPattern).Count();
                var versionFileName = string.Format("{0}.V{1}{2}", sourceName, ++count, extension);
                File.Copy(sourcePath, Path.Combine(VersionPath, versionFileName), true);
            }
        }

        #endregion

        #region EnsurePath

        void EnsurePath()
        {
            if(string.IsNullOrEmpty(SourcePath))
            {
                // ReSharper disable NotResolvedInText
                throw new ArgumentNullException("SourcePath");
                // ReSharper restore NotResolvedInText
            }
            if(!Directory.Exists(VersionPath))
            {
                Directory.CreateDirectory(VersionPath);
            }
        }

        #endregion

        #region GetFileName

        string GetFileName(IDynamicServiceObject dso)
        {
            return Path.Combine(SourcePath, dso.Name + ".xml");
        }

        #endregion

        #endregion

        #region Load

        /// <summary>
        /// Loads the services in the <see cref="ServerRepository"/>
        /// before adding the repository's files.
        /// </summary>
        public void Load()
        {
            Load(Read());
        }

        /// <summary>
        /// Loads the <see cref="ServerRepository" /> into this repository, 
        /// and then overwrites entries with the given workspace repository.
        /// </summary>
        /// <param name="workspaceRepository">The workspace repository to be used.</param>
        public void Load(IEnumerable<IDynamicServiceObject> workspaceRepository)
        {
            _items.Clear();

            foreach(var service in ServerRepository.Items)
            {
                _items[service.Name] = ServerRepository.Get(service.Name);
            }

            if(workspaceRepository != null)
            {
                foreach(var service in workspaceRepository)
                {
                    _items[service.Name] = service;
                }
            }
        }

        #endregion

        #region Load(IDynamicServiceObject dso)

        /// <summary>
        /// Loads a copy of the given service from the server repository.
        /// </summary>
        /// <param name="dso">The <see cref="IDynamicServiceObject" /> to be loaded.</param>
        public void Load(IDynamicServiceObject dso)
        {
            if(ServerRepository == null || dso == null)
            {
                return;
            }

            var dsoServer = ServerRepository.Get(dso.Name);
            if(dsoServer != null)
            {
                var dsoThis = DeepCopy(dsoServer);
                _items[dso.Name] = dsoThis;
                Write(dsoThis);
            }
        }

        #endregion

        #region Commit

        /// <summary>
        /// Commits (and compiles) a copy of the given service to the server repository
        /// and then removes it from this repository.
        /// </summary>
        /// <param name="dso">The <see cref="IDynamicServiceObject" /> to be committed.</param>
        public void Commit(IDynamicServiceObject dso)
        {
            if(ServerRepository == null || dso == null)
            {
                return;
            }

            Compile(dso);
            var dsoServer = DeepCopy(dso);
            ServerRepository.Replace(dsoServer);
            Remove(dso);
        }

        #endregion

        #region Discard

        /// <summary>
        /// Discards the given service and loads the one from the server repository.
        /// </summary>
        /// <param name="dso">The <see cref="IDynamicServiceObject" /> to be discarded.</param>
        public void Discard(IDynamicServiceObject dso)
        {
            if(dso == null)
            {
                return;
            }

            Remove(dso);

            if(ServerRepository != null)
            {
                var dsoServer = ServerRepository.Get(dso.Name);
                if(dsoServer != null)
                {
                    _items[dso.Name] = dsoServer;
                }
            }
        }

        #endregion

        #region Add

        /// <summary>
        /// Adds the specified service to the repository.
        /// </summary>
        /// <param name="dso">The <see cref="IDynamicServiceObject" /> to be added.</param>
        /// <exception cref="DuplicateException">If <paramref name="dso" /> exists.</exception>
        public void Add(IDynamicServiceObject dso)
        {
            if(dso == null)
            {
                return;
            }

            if(!_items.TryAdd(dso.Name, dso))
            {
                throw new DuplicateException(dso);
            }
            Write(dso);
        }

        #endregion

        #region Replace

        /// <summary>
        /// Replaces the service with the given ID in the repository.
        /// </summary>
        /// <param name="dso">The <see cref="IDynamicServiceObject" /> to be replaced.</param>
        public void Replace(IDynamicServiceObject dso)
        {
            if(dso == null)
            {
                return;
            }

            _items[dso.Name] = dso;
            Write(dso);
        }

        #endregion

        #region Remove

        /// <summary>
        /// Removes the specified service from the repository.
        /// </summary>
        /// <param name="dso">The <see cref="IDynamicServiceObject" /> to be removed.</param>
        public void Remove(IDynamicServiceObject dso)
        {
            if(dso == null)
            {
                return;
            }

            IDynamicServiceObject result;
            if(_items.TryRemove(dso.Name, out result))
            {
                Delete(dso);
            }
        }

        #endregion

        #region Get

        /// <summary>
        /// Gets the service with the given ID.
        /// </summary>
        /// <param name="serviceID">The service ID to be queried.</param>
        /// <returns>
        /// The service with the given ID or null if not found.
        /// </returns>
        public IDynamicServiceObject Get(string serviceID)
        {
            IDynamicServiceObject dso;
            _items.TryGetValue(serviceID, out dso);
            return dso;
        }

        #endregion

        #region Static Helper Methods

        #region Compile

        static void Compile(IDynamicServiceObject dso)
        {
            if(!dso.Compile())
            {
                throw new CompilerException(dso);
            }
        }

        #endregion

        #region GenerateObjectGraphFromString

        /// <summary>
        /// 	Generates and object graphs for each type contained in the domain specific xml language
        /// </summary>
        /// <param name="serviceDefinitionsXml"> The string containing the domain specific language code. </param>
        /// <returns> A list of all object graphs that were built </returns>
        public static List<DynamicServiceObjectBase> GenerateObjectGraphFromString(string serviceDefinitionsXml)
        {
            if(string.IsNullOrEmpty(serviceDefinitionsXml))
            {
                throw new ArgumentNullException("serviceDefinitionsXml");
            }

            //This will store the return data of this method
            //which represents the services that were successfully loaded
            var objectsLoaded = new List<DynamicServiceObjectBase>();

            var dslObject = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(serviceDefinitionsXml);

            #region Create MetatData about this resource

            var authorRoles = string.Empty;
            var comment = string.Empty;
            string helpLink = null;
            var unitTestTarget = string.Empty;
            var category = string.Empty;
            var tags = string.Empty;
            var dataList = string.Empty;


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
                try
                {
                    dataList = dslObject.DataList.XmlString;
                }
                catch(Exception)
                {
                    // nothing, init it as such
                    dataList = "<ADL></ADL>";
                }
            }

            #endregion

            #region Create and Hydrate BizRules then add them to the service directory

            //Retrieve a list of UnlimitedObjects that 
            //each contain an individual BizRule node from the
            //Service Definition file
            var bizRules = dslObject.BizRule;
            //We check if a list is being returned as this
            //will be the case when loading complex xml 
            //elements that have attributes
            //as in the case of the service definition file
            //All classes are hydrated in this way so these
            //comments will not be repeated later on in this source file
            if(bizRules is List<UnlimitedObject>)
            {
                //Iterate the bizrule collection of UnlimitedObjects and 
                //Hydrate an instance of the BizRule class each time
                foreach(var bizrule in bizRules)
                {
                    var br = new BizRule { Name = bizrule.Name, ServiceName = bizrule.ServiceName, Expression = bizrule.Expression, ExpressionColumns = bizrule.ExpressionColumns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) };
                    //Add the newly instantiated and Hydrated bizrule class
                    //to the BizRules list of the DynamicServices service directory
                    //this.BizRules.Add(br);
                    objectsLoaded.Add(br);
                    Trace.WriteLine(string.Format("successfully parsed biz rule '{0}'", br.Name));
                }
            }

            #endregion

            #region Create and Hydrate Workflow ActivityMetaData

            var activities = dslObject.WorkflowActivityDef;
            if(activities is List<UnlimitedObject>)
            {
                foreach(var item in activities)
                {
                    var wd = new WorkflowActivityDef { AuthorRoles = authorRoles, Comment = comment, Tags = tags, Category = category, HelpLink = helpLink, ResourceDefinition = item.XmlString, DataListSpecification = dataList };

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
            var sources = dslObject.Source;
            if(sources is List<UnlimitedObject>)
            {
                foreach(var source in sources)
                {
                    var dlCheck = false;


                    try
                    {
                        if(source.Type is string)
                        {
                            enSourceType sourceType;
                            dlCheck = Enum.TryParse<enSourceType>(source.Type, out sourceType);
                        }
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                    }

                    if(dlCheck)
                    {
                        // Travis : filter out the Source in DataList issue
                        var src = new Source { AuthorRoles = authorRoles, Tags = tags, Comment = comment, Category = category, HelpLink = helpLink, ResourceDefinition = source.XmlString, DataListSpecification = dataList };

                        if(source.Name is string)
                        {
                            src.Name = source.Name;
                        }

                        if(source.Type is string)
                        {
                            enSourceType sourceType;
                            src.Type = !Enum.TryParse<enSourceType>(source.Type, out sourceType) ? enSourceType.Unknown : sourceType;
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

                        if (source.ID is string)
                        {
                            if (!string.IsNullOrEmpty(source.ID))
                            {
                                src.ID = Guid.Parse(source.ID);
                            }
                        }

                        objectsLoaded.Add(src);
                    }
                }
            }

            #endregion

            #region Build an object graph for each service in the domain specific language string

            var services = dslObject.Service;
            if(services is List<UnlimitedObject>)
            {
                foreach(var service in services)
                {
                    var ds = new DynamicService { AuthorRoles = authorRoles, Category = category, Tags = tags, Comment = comment, HelpLink = helpLink, ResourceDefinition = service.XmlString, UnitTestTargetWorkflowService = unitTestTarget, DataListSpecification = dataList };

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
                        var tmpObj = (service as UnlimitedObject);

                        try
                        {
                            var xDoc = new XmlDocument();
                            xDoc.LoadXml(tmpObj.XmlString);
                            var n = xDoc.SelectSingleNode("Service");
                            ds.Name = n.Attributes["Name"].Value;
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch
                        // ReSharper restore EmptyGeneralCatchClause
                        {
                        }
                    }

                    #region Build Actions

                    var actions = service.Action;

                    if(actions is List<UnlimitedObject>)
                    {
                        foreach(var action in actions)
                        {
                            #region Process Actions

                            var sa = new ServiceAction();


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

                                sa.ActionType = !Enum.TryParse<enActionType>(action.Type, out actionType) ? enActionType.Unknown : actionType;
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

                            //Biz Rules are special actions 
                            //so we need to treat everything else differently

                            switch(sa.ActionType)
                            {
                                case enActionType.BizRule:
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

                            var inputs = action.Input;
                            if(inputs is List<UnlimitedObject>)
                            {
                                foreach(var input in inputs)
                                {
                                    var sai = new ServiceActionInput();
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

                                    if(input.IsRequired is string)
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

                                    var validators = input.Validator;

                                    if(validators is List<UnlimitedObject>)
                                    {
                                        foreach(var validator in validators)
                                        {
                                            var v = new Validator();

                                            if(validator.Type is string)
                                            {
                                                enValidationType validatorType;
                                                v.ValidatorType = !Enum.TryParse<enValidationType>(validator.Type, out validatorType) ? enValidationType.Required : validatorType;
                                            }

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
                        }
                    }

                    #endregion Process Actions

                    #region Build CasesHolder

                    var casesParent = service.Cases;
                    var casesHolderList = new List<ServiceActionCases>();

                    if(casesParent is List<UnlimitedObject>)
                    {
                        foreach(var casesInstance in casesParent)
                        {
                            var casesObject = new ServiceActionCases();

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

                    var cases = service.Case;
                    var casesList = new List<ServiceActionCase>();

                    if(cases is List<UnlimitedObject>)
                    {
                        foreach(var caseInstance in cases)
                        {
                            var caseObject = new ServiceActionCase();
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

                    var removeAction = new List<ServiceAction>();
                    foreach(var sa in ds.Actions)
                    {
                        if(sa.Parent != null)
                        {
                            if(sa.Parent.Regex is string)
                            {
                                var sa1 = sa;
                                var caseInstance = casesList.FirstOrDefault(c => c.Regex == sa1.Parent.Regex);
                                if(caseInstance != null)
                                {
                                    caseInstance.Actions.Add(sa);
                                    removeAction.Add(sa);
                                }
                            }
                        }
                    }

                    #endregion

                    #region Map each case to CasesHolder

                    foreach(var sc in casesList)
                    {
                        if(sc.Parent != null)
                        {
                            if(sc.Parent.DataElementName is string)
                            {
                                var holder = casesHolderList.FirstOrDefault(c => c.DataElementName == sc.Parent.DataElementName);
                                if(holder != null)
                                {
                                    holder.Cases.Add(sc);
                                    var defaultCase = holder.Cases.FirstOrDefault(c => c.IsDefault);
                                    if(defaultCase != null)
                                    {
                                        holder.DefaultCase = defaultCase;
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region Map CaseHolder to Switch Action

                    foreach(var sch in casesHolderList)
                    {
                        if(sch.Parent != null)
                        {
                            if(sch.Parent.Name is string)
                            {
                                var sch1 = sch;
                                var action = ds.Actions.FirstOrDefault(c => c.Name == sch1.Parent.Name);
                                if(action != null)
                                {
                                    action.Cases = sch;
                                }
                            }
                        }
                    }

                    #endregion

                    foreach(var sa in removeAction)
                    {
                        ds.Actions.Remove(sa);
                    }

                    objectsLoaded.Add(ds);
                }
            }

            #endregion

            return objectsLoaded;
        }

        #endregion

        #region DeepCopy

        /// <summary>
        /// Creates a deep copy of the given service.
        /// </summary>
        /// <param name="dso">The service to be copied.</param>
        /// <returns>A deep copy of the given service.</returns>
        public static IDynamicServiceObject DeepCopy(IDynamicServiceObject dso)
        {
            if(dso == null)
            {
                return null;
            }
            var graph = GenerateObjectGraphFromString(dso.ResourceDefinition);
            return graph.FirstOrDefault();
        }

        #endregion

        #endregion
    }
}