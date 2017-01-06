/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Services.Security
{
    [Flags]
    public enum AuthorizationContext
    {
        [Reason("You are not authorized.")]
        None = 0,

        [Reason("You are not authorized to view this resource.")]
        View = 1,

        [Reason("You are not authorized to execute this resource.")]
        Execute = 2,

        [Reason("You are not authorized to add, update, delete or save this resource.")]
        Contribute = 4,

        [Reason("You are not authorized to deploy to this server.")]
        DeployTo = 8,

        [Reason("You are not authorized to deploy from this server.")]
        DeployFrom = 16,

        Administrator = 32,

        Any = 64
    }
}
