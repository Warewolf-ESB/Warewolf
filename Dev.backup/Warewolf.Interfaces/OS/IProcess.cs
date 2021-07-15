﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using System.IO;

namespace Warewolf.OS
{
    public interface IProcess : IDisposable
    {
        void Kill();
        Process Unwrap();
        bool WaitForExit(int milliseconds);
        void WaitForExit();
        int Id { get; }
        bool HasExited { get; }
        StreamReader StandardOutput { get; }
        StreamReader StandardError { get; }
    }
    public interface IProcessFactory
    {
        IProcess Start(string fileName);
        IProcess Start(ProcessStartInfo startInfo);
    }

}
