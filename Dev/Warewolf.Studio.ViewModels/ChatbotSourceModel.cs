#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net.Http;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;

namespace Warewolf.Studio.ViewModels
{
    public class ManageChatbotSourceModel : IManageChatbotSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageChatbotSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", StringComparison.Ordinal));
            }
        }

        #region Implementation of IManageChatbotSourceModel

        public void TestConnection(IChatbotSource resource)
        {
            _updateRepository.TestConnection(resource);
        }

        public void Save(IChatbotSource toSource)
        {
            _updateRepository.Save(toSource);
        }

        public string ServerName { get; set; }

        public IChatbotSource FetchSource(Guid id)
        {
            var xaml = _queryProxy.FetchResourceXaml(id);
            var source = new Dev2.Data.ServiceModel.ChatbotSource(xaml.ToXElement());

            var def = new ChatbotSourceDefinition
            {
                Id = source.ResourceID,
                Name = source.ResourceName,
                Path = source.GetSavePath(),
                ApiKey = source.ApiKey,
                ModelsEndpoint = source.ModelsEndpoint,
                CompletionsEndpoint = source.CompletionsEndpoint,
                SelectedModel = source.SelectedModel
            };
            return def;
        }

        #endregion
    }
}
