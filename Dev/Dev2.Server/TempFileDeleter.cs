/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using System;
using System.IO;
using System.Linq;

namespace Dev2
{
    public interface ITempFileDeleter : IDisposable
    {
        int DaysToKeepTempFiles { get; set; }

        void Start();
    }
    public class TempFileDeleter : ITempFileDeleter
    {
        readonly IDirectory _directoryWrapper;
        private ITimer _timer;
        readonly ITimerFactory _timerFactory;

        public TempFileDeleter(IDirectory directoryWrapper, ITimerFactory timerFactory)
        {
            _directoryWrapper = directoryWrapper;
            _timerFactory = timerFactory;
        }

        public int DaysToKeepTempFiles { get; set; }

        public void Dispose()
        {
            if (_timer is null)
            {
                return;
            }

            _timer.Dispose();
        }

        public void Start()
        {
            _timer = _timerFactory.New(DeleteTempFiles, null, 1000, GlobalConstants.NetworkComputerNameQueryFreq);
        }

        void DeleteTempFiles(object state)
        {
            var tempPath = EnvironmentVariables.DebugItemTempPath;
            DeleteTempFiles(tempPath);
            var schedulerTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), GlobalConstants.SchedulerDebugPath);
            DeleteTempFiles(schedulerTempPath);
        }

        void DeleteTempFiles(string tempPath)
        {
            if (_directoryWrapper.Exists(tempPath))
            {
                var files = _directoryWrapper.GetFileInfos(tempPath);
                var filesToDelete = files.Where(info =>
                {
                    var maxDaysToKeepTimeSpan = new TimeSpan(DaysToKeepTempFiles, 0, 0, 0);
                    var time = DateTime.Now.Subtract(info.CreationTime);
                    return time > maxDaysToKeepTimeSpan;
                }).ToList();

                foreach (var fileInfo in filesToDelete)
                {
                    fileInfo.Delete();
                }
            }
        }
    }
}
