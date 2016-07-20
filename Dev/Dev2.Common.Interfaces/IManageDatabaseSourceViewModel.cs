/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Common.Interfaces
{
    public interface IManageDatabaseSourceViewModel
    {
        /// <summary>
        /// The Database Server Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        NameValue ServerType { get; set; }
        /// <summary>
        ///  Windows or user or publlic
        /// </summary>
        AuthenticationType AuthenticationType { get; set; }

        /// <summary>
        /// The Database Server Name
        /// </summary>
        ComputerName ServerName { get; set; }

        /// <summary>
        /// The Database Server Name
        /// </summary>
        string EmptyServerName { get; set; }

        /// <summary>
        /// The Database that the source is reading from
        /// </summary>
        string DatabaseName { get; set; }
        /// <summary>
        /// User Name
        /// </summary>
        string UserName { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        string Password { get; set; }
        /// <summary>
        /// Test if connection is successful
        /// </summary>
        ICommand TestCommand { get; set; }

        /// <summary>
        /// Cancel a test that has started
        /// </summary>
        ICommand CancelTestCommand { get; set; }
        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        string TestMessage { get; }

        /// <summary>
        /// Command for save/ok
        /// </summary>
        ICommand OkCommand { get; set; }

        /// <summary>
        /// Header text that is used on the view
        /// </summary>
        string HeaderText { get; set; }

        /// <summary>
        /// List of database names for the user to choose from based on the server entered
        /// </summary>
        IList<string> DatabaseNames { get; set; }

        /// <summary>
        /// Has test passed
        /// </summary>
        bool TestPassed { get; set; }

        /// <summary>
        /// has test failed
        /// </summary>
        bool TestFailed { get; set; }
        /// <summary>
        /// IsTesting
        /// </summary>
        bool Testing { get; }
        /// <summary>
        /// Database Types avaialable 
        /// </summary>
        IList<NameValue> Types { get; set; }
        /// <summary>
        /// The name of the resource
        /// </summary>
        // ReSharper disable UnusedMemberInSuper.Global
        string ResourceName { get; set; }
        // ReSharper restore UnusedMemberInSuper.Global

        /// <summary>
        /// The authentications Type
        /// </summary>
        bool UserAuthenticationSelected { get; }

        IList<ComputerName> ComputerNames { get; set; }
        bool CanSelectWindows { get; set; }
        bool CanSelectServer { get; set; }
    }

    public interface IManageDatabaseSourceModel
    {
        IList<string> GetComputerNames();
        IList<string> TestDbConnection(IDbSource resource);
        void Save(IDbSource toDbSource);
        string ServerName { get; }
    }
}
