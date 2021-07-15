﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;

namespace Warewolf.OS.IO
{
    public interface IFileSystemWatcher : IDisposable
    {
        string Path { get; set; }
        string Filter { get; set; }
        bool EnableRaisingEvents { get; set; }
        NotifyFilters NotifyFilter { get; set; }

        event FileSystemEventHandler Created;
        event FileSystemEventHandler Changed;
        event FileSystemEventHandler Deleted;
        event RenamedEventHandler Renamed;
        event ErrorEventHandler Error;
    }
}
