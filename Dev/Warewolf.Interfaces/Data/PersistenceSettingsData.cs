/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Warewolf.Data;

namespace Warewolf.Configuration
{
    public class PersistenceSettingsData : BindableBase, IEquatable<PersistenceSettingsData>
    {
        private NamedGuidWithEncryptedPayload _persistenceDataSource = new NamedGuidWithEncryptedPayload();
        private bool? _encryptDataSource;
        private bool? _enable;
        private bool? _prepareSchemaIfNecessary;
        private string _dashboardEndpoint = "http://localhost";
        private string _dashboardPort = "5001";
        private string _dashboardName = "HangfireDashboard";
        private string _serverName = "";
        private string _persistenceScheduler = "";

        public bool Enable
        {
            get => _enable ?? false;
            set => SetProperty(ref _enable, value);
        }

        public bool PrepareSchemaIfNecessary
        {
            get => _prepareSchemaIfNecessary ?? true;
            set => SetProperty(ref _prepareSchemaIfNecessary, value);
        }

        public NamedGuidWithEncryptedPayload PersistenceDataSource
        {
            get => _persistenceDataSource;
            set => SetProperty(ref _persistenceDataSource, value);
        }

        public bool EncryptDataSource
        {
            get => _encryptDataSource ?? true;
            set => SetProperty(ref _encryptDataSource, value);
        }

        public string PersistenceScheduler
        {
            get => _persistenceScheduler;
            set => SetProperty(ref _persistenceScheduler, value);
        }

        public PersistenceSettingsData Clone()
        {
            var result = (PersistenceSettingsData) MemberwiseClone();
            result._persistenceDataSource = PersistenceDataSource.Clone();
            return result;
        }

        public string DashboardEndpoint
        {
            get => _dashboardEndpoint;
            set => SetProperty(ref _dashboardEndpoint, value);
        }

        public string DashboardPort
        {
            get => _dashboardPort;
            set => SetProperty(ref _dashboardPort, value);
        }

        public string DashboardName
        {
            get => _dashboardName;
            set => SetProperty(ref _dashboardName, value);
        }

        public string ServerName
        {
            get => _serverName;
            set => SetProperty(ref _serverName, value);
        }

        public bool Equals(PersistenceSettingsData obj)
        {
            if (obj is PersistenceSettingsData other)
            {
                var equals = PersistenceDataSource.Equals(other.PersistenceDataSource);
                equals &= EncryptDataSource.Equals(other.EncryptDataSource);
                equals &= Enable.Equals(other.Enable);
                equals &= PersistenceScheduler.Equals(other.PersistenceScheduler);
                equals &= DashboardEndpoint == other.DashboardEndpoint;
                equals &= DashboardPort == other.DashboardPort;
                equals &= DashboardName == other.DashboardName;
                equals &= ServerName == other.ServerName;
                equals &= PrepareSchemaIfNecessary == other.PrepareSchemaIfNecessary;
                return equals;
            }

            return false;
        }
    }
}