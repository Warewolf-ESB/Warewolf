#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Warewolf.Studio.ViewModels
{
    public class ManageSqliteSourceViewModel : DatabaseSourceViewModelBase
    {
        public ManageSqliteSourceViewModel(IAsyncWorker asyncWorker)
              : base(asyncWorker, "SqliteDatabase")
        {
        }

        public ManageSqliteSourceViewModel(IManageDatabaseSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : base(updateManager, requestServiceNameViewModel, aggregator, asyncWorker, "SqliteDatabase")
        {
            HeaderText = Resources.Languages.Core.SqlServerSourceServerNewHeaderLabel;
            Header = Resources.Languages.Core.SqlServerSourceServerNewHeaderLabel;
        }

        public ManageSqliteSourceViewModel(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator, IDbSource dbSource, IAsyncWorker asyncWorker)
            : base(updateManager, aggregator, dbSource, asyncWorker, "SqliteDatabase")
        {
            VerifyArgument.IsNotNull("sqliteServerSource", dbSource);
        }

        public override string Name
        {
            get => ResourceName;
            set => ResourceName = value;
        }

        public override void FromModel(IDbSource source)
        {
            ResourceName = source.Name;
            ServerName = ComputerNames.FirstOrDefault(name => string.Equals(source.ServerName, name.Name, StringComparison.CurrentCultureIgnoreCase));
            if (ServerName != null)
            {
                EmptyServerName = ServerName.Name ?? source.ServerName;
            }
            AuthenticationType = source.AuthenticationType;
            UserName = source.UserName;
            Password = source.Password;
            Path = source.Path;
            TestConnection();
            DatabaseName = source.DbName;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        protected override IDbSource ToNewDbSource() => new DbSourceDefinition
        {
            AuthenticationType = AuthenticationType,
            ServerName = GetServerName(),
            Password = Password,
            Path = Path,
            UserName = UserName,
            Type = enSourceType.SQLiteDatabase,
            Name = ResourceName,
            DbName = DatabaseName,
            Id = GetDbSourceId()
        };
        private Guid GetDbSourceId()
        {
            if (DbSource?.Id == null)
            {
                if (SelectedGuid != Guid.Empty)
                {
                    return SelectedGuid;
                }
                else
                {
                    return Guid.NewGuid();
                }
            }
            else
            {
                return DbSource.Id;
            }
        }

        protected override IDbSource ToDbSource()
        {
            if (DbSource == null)
            {
                return ToNewDbSource();
            }
            else
            {
                return new DbSourceDefinition
                {
                    AuthenticationType = AuthenticationType,
                    ServerName = GetServerName(),
                    Password = Password,
                    UserName = UserName,
                    Type = enSourceType.SQLiteDatabase,
                    Path = Path,
                    Name = ResourceName,
                    DbName = DatabaseName,
                    Id = GetDbSourceId()
                };

            }
        }

        protected override IDbSource ToSourceDefinition() => new DbSourceDefinition
        {
            AuthenticationType = DbSource.AuthenticationType,
            DbName = DbSource.DbName,
            Id = DbSource.Id,
            Name = DbSource.Name,
            Password = DbSource.Password,
            Path = DbSource.Path,
            ServerName = DbSource.ServerName,
            UserName = DbSource.UserName,
            Type = enSourceType.SQLiteDatabase
        };

        public override IDbSource ToModel()
        {

            Item = ToDbSource();
            return Item;

        }
    }
}
