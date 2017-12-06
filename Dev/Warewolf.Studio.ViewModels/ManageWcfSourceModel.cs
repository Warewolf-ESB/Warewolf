/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Runtime.ServiceModel.Data;
using System;

namespace Warewolf.Studio.ViewModels
{
    public class ManageWcfSourceModel : IWcfSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageWcfSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;
        }

        public void TestConnection(IWcfServerSource resource)
        {
            _updateRepository.TestConnection(resource);
        }

        public void Save(IWcfServerSource resource)
        {
            _updateRepository.Save(resource);
        }

        public string ServerName { get; set; }

        public IWcfServerSource FetchSource(Guid resourceID)
        {
            var xaml = _queryProxy.FetchResourceXaml(resourceID);
            var wcfsource = new WcfSource(xaml.ToXElement());

            var def = new WcfServiceSourceDefinition(wcfsource);

            return def;
        }
    }
}
