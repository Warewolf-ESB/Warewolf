
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



namespace Dev2.Session {
    public interface IDebugSession {

        /// <summary>
        /// Start a debug session
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        DebugTO InitDebugSession(DebugTO to);

        /// <summary>
        /// Save debug session data
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        DebugTO PersistDebugSession(DebugTO to);

    }
}
