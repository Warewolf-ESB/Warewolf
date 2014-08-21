using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.Hosting;
using Dev2.Util;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetVersion : IEsbManagementEndpoint
    {
        const string PayloadStart = "<XamlDefinition>";
        const string PayloadEnd = "</XamlDefinition>";
        const string AltPayloadStart = "<Actions>";
        const string AltPayloadEnd = "</Actions>";
        #region Implementation of ISpookyLoadable<string>
        private IServerVersionRepository _serverExplorerRepository;
        IResourceCatalog _resourceCatalog   ;

        public string HandlesType()
        {
            return "GetVersion";
        }

        #endregion

        #region Implementation of IEsbManagementEndpoint

        /// <summary>
        /// Executes the service
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="theWorkspace">The workspace.</param>
        /// <returns></returns>
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                var res = new ExecuteMessage { HasError = false };
                if (values == null)
                {
                    throw new ArgumentNullException("values");
                }
                if (!values.ContainsKey("versionInfo"))
                {
// ReSharper disable NotResolvedInText
                    throw new ArgumentNullException("No resourceId was found in the incoming data");
// ReSharper restore NotResolvedInText
                }
                var version = serializer.Deserialize<IVersionInfo>(values["versionInfo"]);
                var result = ServerVersionRepo.GetVersion(version);
                var resource = Resources.GetResource(theWorkspace.ID, version.ResourceId);
                if (resource != null && resource.ResourceType == ResourceType.DbSource)
                {
                    res.Message.Append(result);
                }
                else
                {
                    var startIdx = result.IndexOf(PayloadStart, 0, false);

                    if (startIdx >= 0)
                    {
                        // remove beginning junk
                        startIdx += PayloadStart.Length;
                        result = result.Remove(0, startIdx);

                        startIdx = result.IndexOf(PayloadEnd, 0, false);

                        if (startIdx > 0)
                        {
                            var len = result.Length - startIdx;
                            result = result.Remove(startIdx, len);

                            res.Message.Append(result.Unescape());
                        }
                    }
                    else
                    {
                        // handle services ;)
                        startIdx = result.IndexOf(AltPayloadStart, 0, false);
                        if (startIdx >= 0)
                        {
                            // remove begging junk
                            startIdx += AltPayloadStart.Length;
                            result = result.Remove(0, startIdx);

                            startIdx = result.IndexOf(AltPayloadEnd, 0, false);

                            if (startIdx > 0)
                            {
                                var len = result.Length - startIdx;
                                result = result.Remove(startIdx, len);

                                res.Message.Append(result.Unescape());
                            }
                        }
                        else
                        {
                            // send the entire thing ;)
                            res.Message.Append(result);
                        }
                    }
                }

                Dev2XamlCleaner dev2XamlCleaner = new Dev2XamlCleaner();
                res.Message = dev2XamlCleaner.StripNaughtyNamespaces(res.Message);


                return serializer.SerializeToBuilder(res);

            }
            catch (Exception e)
            {
                IExplorerRepositoryResult error = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
                return serializer.SerializeToBuilder(error);
            }
        }

        /// <summary>
        /// Creates the service entry.
        /// </summary>
        /// <returns></returns>
        public DynamicService CreateServiceEntry()
        {
            var serviceAction = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            var serviceEntry = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };
            serviceEntry.Actions.Add(serviceAction);

            return serviceEntry;
        }

        #endregion
        public IServerVersionRepository ServerVersionRepo
        {
            get { return _serverExplorerRepository ?? new ServerVersionRepository(new VersionStrategy(), ResourceCatalog.Instance, new DirectoryWrapper(), EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID), new FileWrapper()); }
            set { _serverExplorerRepository = value; }
        }

        public IResourceCatalog Resources
        {
            get { return _resourceCatalog ?? ResourceCatalog.Instance; }
            set { _resourceCatalog = value; }
        }
    }
}
