
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.Core.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;

namespace Dev2.Studio.Core.InterfaceImplementors
{
     //<summary>
     //A service for querying resource dependencies.
     //</summary>
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

            return resourceModel.Environment.ResourceRepository.GetDependenciesXml(resourceModel);
            }

        #endregion

        #region GetUniqueDependencies

         //<summary>
         //Gets a list of unique dependencies for the given <see cref="IResourceModel"/>.
         //</summary>
         //<param name="resourceModel">The resource model to be queried.</param>
         //<returns>A list of <see cref="IResourceModel"/>'s.</returns>
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
        /// <param name="environmentModel"></param>
        /// <param name="getDependsOnMe"></param>
        /// <returns>
        /// A list of resource name string's.
        /// </returns>
        public List<string> GetDependanciesOnList(List<IContextualResourceModel> resourceModels,IEnvironmentModel environmentModel,bool getDependsOnMe = false)
        {
            if(!resourceModels.Any() || environmentModel == null)
            {
                return new List<string>();
            }

            var result =  environmentModel.ResourceRepository.GetDependanciesOnList(resourceModels, environmentModel, getDependsOnMe);

            return result;
        }

        #endregion

    }
}
