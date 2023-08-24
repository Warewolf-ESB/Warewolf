/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using System.IO;

namespace Warewolf.Testing
{
    /// <summary>
    /// These variables are only for setting up test cases for our test suit. 
    /// For any other purpose, consider EnvironmentVariables.
    /// </summary>
    public static class EnvironmentVariablesForTesting
    {
        public static string PersistencePathForTests
        {
            get => Path.Combine(Config.UserDataPath, "PersistenceTests");
        }
    }
}
