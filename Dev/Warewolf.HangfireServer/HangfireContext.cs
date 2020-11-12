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

namespace HangfireServer
{
    public interface IHangfireContext
    {
        bool Verbose { get; }
    }

    public class HangfireContext : IHangfireContext
    {
        private IArgs _options;
        public bool Verbose => _options.ShowConsole;

        public HangfireContext(IArgs args)
        {
            try
            {
                _options = args;
            }
            catch (Exception)
            {
            }
        }
    }
}