
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Integration.Tests.Test_utils
{
    public enum ServerCommonDirectory
    {
        /// <summary>
        /// The root directory of the fabricated server, this is the same directory that contains the server executable.
        /// </summary>
        Root = 0,
        /// <summary>
        /// The plugins directory of the fabricated server.
        /// </summary>
        Plugins = 1,
        /// <summary>
        /// The css directory of the fabricated server.
        /// </summary>
        Css = 2,
        /// <summary>
        /// The scripts directory of the fabricated server.
        /// </summary>
        Scripts = 3,
        /// <summary>
        /// The services directory of the fabricated server.
        /// </summary>
        Services = 4,
        /// <summary>
        /// The sources directory of the fabricated server.
        /// </summary>
        Sources = 5,
        /// <summary>
        /// The themes directory of the fabricated server.
        /// </summary>
        Themes = 6
    }
}
