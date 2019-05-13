/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Dev2.Common;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.PathOperations;

namespace Dev2.Session
{
    class Dev2StudioSessionBroker : IDev2StudioSessionBroker
    {
        const string SavePath = @"Warewolf\DebugData\PersistSettings.dat";
        static readonly object SettingsLock = new object();
        static readonly object InitLock = new object();
        string _debugPersistPath;
        string _rootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        readonly IDictionary<string, DebugTO> _debugPersistSettings = new ConcurrentDictionary<string, DebugTO>();

        IActivityIOOperationsEndPoint _debugOptsEndPoint;
        IActivityIOPath _debugPath;

        public DebugTO InitDebugSession(DebugTO to)
        {
            lock (SettingsLock)
            {
                to.Error = string.Empty;
                if (to.BaseSaveDirectory != null)
                {
                    BootstrapPersistence(to.BaseSaveDirectory);
                    InitPersistSettings();
                }
                else
                {
                    if (to.BaseSaveDirectory == null && _debugPersistSettings.Count == 0)
                    {
                        BootstrapPersistence(_rootPath);
                        InitPersistSettings();
                    }
                }
                if (to.BaseSaveDirectory == null)
                {
                    to.BaseSaveDirectory = _rootPath;
                }
                to.DataListHash = to.DataList != null ? to.DataList.GetHashCode() : -1;

                PersistBinaryDataList(to);
            }
            return to;
        }

        void InitPersistSettings()
        {
            if (!_debugOptsEndPoint.PathExist(_debugPath))
            {
                var args = new Dev2PutRawOperationTO(WriteType.Overwrite, "");
                ActivityIOFactory.CreateOperationsBroker().PutRaw(_debugOptsEndPoint, args);
            }
            else
            {
                FetchFromDisk();
            }
        }

        void PersistBinaryDataList(DebugTO to)
        {
            DebugTO tmp = null;
            try
            {
                if (_debugPersistSettings.TryGetValue(to.WorkflowID, out tmp))
                {
                    to.XmlData = tmp.RememberInputs ? tmp.XmlData : (to.XmlData ?? "<DataList></DataList>");
                    tmp.CleanUp();
                }
                else
                {
                    to.XmlData = to.RememberInputs ? to.XmlData : "<DataList></DataList>";
                }
                to.BinaryDataList = new DataListModel();
                to.BinaryDataList.Create(to.XmlData, to.DataList);
            }
            finally
            {
                tmp?.CleanUp();
            }
        }

        public DebugTO PersistDebugSession(DebugTO to)
        {
            lock (SettingsLock)
            {
                if (to.DataList != null)
                {
                    to.DataListHash = to.DataList.GetHashCode();
                }
                to.Error = string.Empty;

                if (to.RememberInputs)
                {
                    _debugPersistSettings[to.WorkflowID] = to;
                }
                else
                {
                    if (_debugPersistSettings.TryGetValue(to.WorkflowID, out DebugTO tmp))
                    {
                        _debugPersistSettings[to.WorkflowID].CleanUp();
                        _debugPersistSettings.Remove(to.WorkflowID);
                    }
                }

                var settingList = new List<SaveDebugTO>();

                foreach (string key in _debugPersistSettings.Keys)
                {
                    if (key.Length > 0 && _debugPersistSettings.TryGetValue(key, out DebugTO tmp))
                    {
                        var that = tmp.CopyToSaveDebugTO();
                        settingList.Add(that);
                    }
                }
                using (Stream s = File.Open(_debugPersistPath, FileMode.Truncate))
                {
                    var bf = new XmlSerializer(typeof(List<SaveDebugTO>));
                    bf.Serialize(s, settingList);
                }
            }

            return to;
        }
        protected const string RootTag = "DataList";

        void BootstrapPersistence(string baseDir)
        {
            lock (InitLock)
            {
                if (_debugPath == null)
                {
                    if (baseDir != null)
                    {
                        _rootPath = baseDir;
                    }

                    _debugPersistPath = _rootPath.EndsWith("\\", StringComparison.Ordinal) ? _rootPath + SavePath : _rootPath + "\\" + SavePath;

                    _debugPath = ActivityIOFactory.CreatePathFromString(_debugPersistPath, "", "");
                    _debugOptsEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(_debugPath);
                }
            }
        }

        void FetchFromDisk()
        {
            var filesToCleanup = new List<string>();
            using (var s = _debugOptsEndPoint.Get(_debugPath, filesToCleanup))
            {
                if (s.Length > 0)
                {
                    var bf = new XmlSerializer(typeof(List<SaveDebugTO>));

                    try
                    {
                        var settings = (List<SaveDebugTO>)bf.Deserialize(s);
                        _debugPersistSettings.Values.ToList().ForEach(a => a.CleanUp());
                        _debugPersistSettings.Clear();
                        PersistSettings(settings);
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                    }
                }
                else
                {
                    Dev2Logger.Error("No debug data stream [ " + _debugPath + " ] ", GlobalConstants.WarewolfError);
                }

                s.Close();
                s.Dispose();
                filesToCleanup.ForEach(File.Delete);
            }
        }

        void PersistSettings(List<SaveDebugTO> settings)
        {
            foreach (var dto in settings)
            {
                if (!string.IsNullOrEmpty(dto.ServiceName))
                {
                    var tmp = new DebugTO();
                    tmp.CopyFromSaveDebugTO(dto);
                    _debugPersistSettings[dto.WorkflowID] = tmp;
                }
            }
        }

        public void Dispose()
        {
            _debugPersistSettings.Values.ToList().ForEach(a => a.CleanUp());
        }
    }
}