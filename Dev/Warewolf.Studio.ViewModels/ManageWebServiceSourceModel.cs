﻿using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Warewolf.Studio.ViewModels
{
    public class ManageWebServiceSourceModel : IManageWebServiceSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;

        public ManageWebServiceSourceModel(IStudioUpdateManager updateRepository, string serverName)
        {
            _updateRepository = updateRepository;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", StringComparison.Ordinal));
            }

        }

        #region Implementation of IManageWebServiceSourceModel

        public void TestConnection(IWebServiceSource resource)
        {
            _updateRepository.TestConnection(resource);
        }

        public void Save(IWebServiceSource resource)
        {
            _updateRepository.Save(resource);
        }

        public string ServerName { get; set; }

        #endregion
    }
}
