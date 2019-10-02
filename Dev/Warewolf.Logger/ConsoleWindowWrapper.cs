/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Warewolf.Common;

namespace Warewolf.Logger
{
    public interface IConsoleWindowWrapper
    {
    }

    public class ConsoleWindowWrapper : IConsoleWindowWrapper
    {
        public ConsoleWindowWrapper()
        {
            _ = new ConsoleWindow();
        }
    }

    public interface IConsoleWindowFactory
    {
        IConsoleWindowWrapper New();
    }

    public class ConsoleWindowFactory : IConsoleWindowFactory
    {
        public IConsoleWindowWrapper New()
        {
           return new ConsoleWindowWrapper();
        }
    }
}
