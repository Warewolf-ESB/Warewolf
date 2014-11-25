
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Services.Security
{
    public enum AuthorizationContext
    {
        [Reason("You are not authorized.")]
        None,

        [Reason("You are not authorized to view this resource.")]
        View,

        [Reason("You are not authorized to execute this resource.")]
        Execute,

        [Reason("You are not authorized to add, update, delete or save this resource.")]
        Contribute,

        [Reason("You are not authorized to deploy to this server.")]
        DeployTo,

        [Reason("You are not authorized to deploy from this server.")]
        DeployFrom,

        Administrator,

        Any
    }
}
