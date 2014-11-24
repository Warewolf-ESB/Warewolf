
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
using Caliburn.Micro;
using Dev2.Data.ServiceModel;
using Dev2.Network;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Newtonsoft.Json;

namespace Dev2.Webs.Callbacks
{
    public class ConnectCallbackHandler : WebsiteCallbackHandler
    {
        #region CTOR

        public ConnectCallbackHandler()
            : this(EnvironmentRepository.Instance)
        {
        }

        public ConnectCallbackHandler(IEnvironmentRepository currentEnvironmentRepository)
            : this(EventPublishers.Aggregator, currentEnvironmentRepository)
        {
        }

        public ConnectCallbackHandler(IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository)
            : base(eventPublisher, currentEnvironmentRepository)
        {
        }

        #endregion

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            Save(jsonObj, environmentModel);
        }

        #region Save

        /// <summary>
        /// Saves the specified connection - method provided for testing.
        /// </summary>
        /// <param name="jsonObj"></param>
        /// <param name="defaultEnvironment">The environment where the connection will be saved - must ALWAYS be .</param>
        /// <exception cref="System.ArgumentNullException">connectionID</exception>
        public void Save(dynamic jsonObj, IEnvironmentModel defaultEnvironment)
        {
            if(jsonObj == null)
            {
                throw new ArgumentNullException();
            }
            Connection newConnection = JsonConvert.DeserializeObject<Connection>(jsonObj.ToString());

            var resourceId = newConnection.ResourceID;
            ServerProxy connection;
            if(newConnection.AuthenticationType == AuthenticationType.Windows || newConnection.AuthenticationType == AuthenticationType.Anonymous)
            {
                connection = new ServerProxy(new Uri(newConnection.WebAddress));
            }
            else
            {
                //
                // NOTE: Public needs to drop through to User for the rest of the framework to pick up properly behind the scenes ;)
                //
                connection = new ServerProxy(newConnection.WebAddress, newConnection.UserName, newConnection.Password);
            }
            var environmentModel = CurrentEnvironmentRepository.Get(resourceId);
            if(environmentModel == null)
            {
                var newEnvironment = new EnvironmentModel(resourceId, connection) { Name = newConnection.ResourceName, Category = newConnection.ResourcePath };
                CurrentEnvironmentRepository.Save(newEnvironment);
            }
            else
            {
                environmentModel.Connection = connection;
                environmentModel.Name = newConnection.ResourceName;
                environmentModel.Category = newConnection.ResourcePath;
            }
        }

        #endregion
    }
}
