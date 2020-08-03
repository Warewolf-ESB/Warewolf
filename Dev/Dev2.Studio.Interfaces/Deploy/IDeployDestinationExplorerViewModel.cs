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

namespace Dev2.Studio.Interfaces.Deploy
{

    public delegate void ServerSate(object sender, IServer server);
    public interface IDeployDestinationExplorerViewModel:IExplorerViewModel    
    {
        event ServerSate ServerStateChanged;
        Version MinSupportedVersion{get;}
        Version ServerVersion { get; }
        bool DeployTests { get; set; }  
        bool DeployTriggers { get; set; }
    }
}