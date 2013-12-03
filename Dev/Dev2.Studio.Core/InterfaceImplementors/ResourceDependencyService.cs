using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DynamicServices;
using Dev2.Studio.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Unlimited.Framework;

namespace Dev2.Studio.Core.InterfaceImplementors
{
    /// <summary>
    /// A service for querying resource dependencies.
    /// </summary>
    [Export(typeof(IResourceDependencyService))]
    public class ResourceDependencyService : IResourceDependencyService
    {
        #region GetDependenciesXml

        /// <summary>
        /// Gets the dependencies XML for the given <see cref="IResourceModel"/>.
        /// </summary>
        /// <param name="resourceModel">The resource model to be queried.</param>
        /// <returns>The dependencies XML.</returns>
        public string GetDependenciesXml(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null)
            {
                return string.Empty;
            }

            dynamic request = new UnlimitedObject();
            request.Service = "FindDependencyService";
            request.ResourceName = resourceModel.ResourceName;
            request.GetDependsOnMe = true;
            var workspaceID = resourceModel.Environment.Connection.WorkspaceID;

            var result = resourceModel.Environment.Connection.ExecuteCommand(request.XmlString, workspaceID, GlobalConstants.NullDataListID);

            if (result == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, request.Service));
            }


            return result;
        }

        #endregion

        #region GetUniqueDependencies

        /// <summary>
        /// Gets a list of unique dependencies for the given <see cref="IResourceModel"/>.
        /// </summary>
        /// <param name="resourceModel">The resource model to be queried.</param>
        /// <returns>A list of <see cref="IResourceModel"/>'s.</returns>
        public List<IResourceModel> GetUniqueDependencies(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null || resourceModel.Environment == null || resourceModel.Environment.ResourceRepository == null)
            {
                return new List<IResourceModel>();
            }

            var xml = XElement.Parse(GetDependenciesXml(resourceModel));
            var nodes = from node in xml.DescendantsAndSelf("node") // this is case-sensitive!
                        select node.Attribute("id")
                            into idAttr
                            where idAttr != null
                            select idAttr.Value;

            var resources = from r in resourceModel.Environment.ResourceRepository.All()
                            join n in nodes on r.ResourceName equals n
                            select r;

            var returnList = resources.ToList().Distinct().ToList();
            return returnList;
        }

        public bool HasDependencies(IContextualResourceModel resourceModel)
        {
            var uniqueList = GetUniqueDependencies(resourceModel);
            uniqueList.RemoveAll(res => res.ID == resourceModel.ID);
            return uniqueList.Count > 0;
        }

        #endregion

        #region GetDependanciesOnList

        /// <summary>
        /// Gets a list of dependencies for the given ResourceModel's.
        /// </summary>
        /// <param name="resourceModels">The resource models to get dependancies for.</param>
        /// <returns>A list of resource name string's.</returns>
        public List<string> GetDependanciesOnList(List<IContextualResourceModel> resourceModels,IEnvironmentModel environmentModel,bool getDependsOnMe = false)
        {
            if(!resourceModels.Any() || environmentModel == null)
            {
                return new List<string>();
            }
            List<string> resourceNames = resourceModels.Select(contextualResourceModel => contextualResourceModel.ResourceName).ToList();

            dynamic request = new UnlimitedObject();
            request.Service = "GetDependanciesOnListService";
            request.ResourceNames = JsonConvert.SerializeObject(resourceNames);
            request.GetDependsOnMe = getDependsOnMe;
            var workspaceID = environmentModel.Connection.WorkspaceID;

            var result = environmentModel.Connection.ExecuteCommand(request.XmlString, workspaceID, GlobalConstants.NullDataListID);    

            List<string> deserializeObject = new List<string>();
            try
            {
                deserializeObject = JsonConvert.DeserializeObject<List<string>>(result);           
            }
            catch(Exception exception)
            {
                throw;
            }                        

            if (result == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, request.Service));
            }

            return deserializeObject;
        }

        #endregion

    }
}
