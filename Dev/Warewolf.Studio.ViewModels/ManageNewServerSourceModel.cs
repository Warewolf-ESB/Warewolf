﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class ManageNewServerSourceModel: IManageServerSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageNewServerSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", StringComparison.Ordinal));
            }

        }

        #region Implementation of IManageServerSourceModel

        public IList<string> GetComputerNames()
        {
            return _queryProxy.GetComputerNames();
        }

        public void TestConnection(IServerSource resource)
        {
            _updateRepository.TestConnection(resource);
        }

        public void Save(IServerSource resource)
        {
            _updateRepository.Save(resource);
        }

        public string ServerName { get; set; }



        #endregion
    }
}
