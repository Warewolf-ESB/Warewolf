/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Text;
using System.Xml.Serialization;
using Dev2.Common;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.PathOperations;

namespace Dev2.Session
{
    internal class Dev2StudioSessionBroker : IDev2StudioSessionBroker
    {
        #region Static Conts

        private const string SavePath = @"Warewolf\DebugData\PersistSettings.dat";
        // the settings lock object
        private static readonly object SettingsLock = new object();
        private static readonly object InitLock = new object();
        private string _debugPersistPath;
        private string _rootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        #endregion

        private readonly IDictionary<string, DebugTO> _debugPersistSettings =
            new ConcurrentDictionary<string, DebugTO>();

        private IActivityIOOperationsEndPoint _debugOptsEndPoint;
        private IActivityIOPath _debugPath;

        /// <summary>
        ///     Init the Debug Session
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public DebugTO InitDebugSession(DebugTO to)
        {
            DebugTO tmp;

            to.Error = string.Empty;

            // Bootstrap the operations
            if (to.BaseSaveDirectory != null)
            {
                BootstrapPersistence(to.BaseSaveDirectory);
                InitPersistSettings();
            }
            else if (to.BaseSaveDirectory == null && _debugPersistSettings.Count == 0)
            {
                BootstrapPersistence(_rootPath);
                InitPersistSettings();
            }

            if (to.BaseSaveDirectory == null)
            {
                // set the save location
                to.BaseSaveDirectory = _rootPath;
            }


            if (to.DataList != null)
            {
                to.DataListHash = to.DataList.GetHashCode(); // set incoming DL hash
            }
            else
            {
                to.DataListHash = -1; // default value
            }

            lock(SettingsLock)
            {
                if (_debugPersistSettings.TryGetValue(to.WorkflowID, out tmp))
                {
                    to.XmlData = tmp.RememberInputs
                        ? (tmp.XmlData)
                        : (to.XmlData ?? "<DataList></DataList>");
                    tmp.CleanUp();
                }
                else
                {
                    // if no XML data copy over the DataList
                    to.XmlData = to.XmlData != null && to.XmlData == string.Empty
                        ? (to.DataList ?? "<DataList></DataList>")
                        : (to.XmlData ?? "<DataList></DataList>");                    
                }
                to.BinaryDataList = new DataListModel();
                to.BinaryDataList.Create(to.XmlData, to.DataList);
            }

            if (tmp != null) tmp.CleanUp();
            return to;
        }

        /// <summary>
        ///     Save the debug session data
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public DebugTO PersistDebugSession(DebugTO to)
        {
            lock (SettingsLock)
            {
                if (to.DataList != null)
                    to.DataListHash = to.DataList.GetHashCode();
                        // set incoming hash //2013.01.22: Ashley Lewis - Added condition for Bug 7837
                to.Error = string.Empty;

                if (to.RememberInputs)
                {
                    // update the current TO
                    _debugPersistSettings[to.WorkflowID] = to;
                }
                else
                {
                    // no longer relavent, remove it
                    DebugTO tmp;

                    if (_debugPersistSettings.TryGetValue(to.WorkflowID, out tmp))
                    {
                         _debugPersistSettings[to.WorkflowID].CleanUp();
                        _debugPersistSettings.Remove(to.WorkflowID);
                    }
                }

                var settingList = new List<SaveDebugTO>();

                // build the list
                foreach (string key in _debugPersistSettings.Keys)
                {
                    DebugTO tmp;

                    if (key.Length > 0 && _debugPersistSettings.TryGetValue(key, out tmp))
                    {
                        SaveDebugTO that = tmp.CopyToSaveDebugTO();
                        settingList.Add(that);
                    }
                }

                // push to disk
                using (Stream s = File.Open(_debugPersistPath, FileMode.Truncate))
                {
                    var bf = new XmlSerializer(typeof (List<SaveDebugTO>));
                    bf.Serialize(s, settingList);
                }
            }

            return to;
        }
        protected const string RootTag = "DataList";
        public string GetXMLForInputs(IDataListModel dataListModel)
        {
            StringBuilder result = new StringBuilder("<" + RootTag + ">");
            foreach(var item in dataListModel.Scalars.Where(scalar => scalar.IODirection==enDev2ColumnArgumentDirection.Input || scalar.IODirection==enDev2ColumnArgumentDirection.Both))
            {
                DoScalarAppending(result, item);    
            }
            foreach (var recordSet in dataListModel.RecordSets.Where(scalar => scalar.IODirection == enDev2ColumnArgumentDirection.Input || scalar.IODirection == enDev2ColumnArgumentDirection.Both))
            {
                DoRecordSetAppending(recordSet, result);
            }
            result.Append("</" + RootTag + ">");
            return result.ToString();
        }

        internal static void DoRecordSetAppending(IRecordSet recordSet, StringBuilder result)
        {
            var cnt = recordSet.Columns.Max(pair => pair.Key);

            for (var i = 1; i <= cnt; i++)
            {
                var rowData = recordSet.Columns[i];

                result.Append("<");
                result.Append(recordSet.Name);
                result.Append(">");
                foreach (var col in rowData)
                {

                    var fName = col.Name;



                    result.Append("<");
                    result.Append(fName);
                    result.Append(">");
                    try
                    {
                        result.Append(col.Value);
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch (Exception)
                    {
                    }
                    result.Append("</");
                    result.Append(fName);
                    result.Append(">");
                }

                result.Append("</");
                result.Append(recordSet.Name);
                result.Append(">");
            }
        }

        internal static void DoScalarAppending(StringBuilder result, IScalar val)
        {

            var fName = val.Name;
            result.Append("<");
            result.Append(fName);
            result.Append(">");
            try
            {
                result.Append(val.Value);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
            result.Append("</");
            result.Append(fName);
            result.Append(">");
        }
        #region Private Method

        private void BootstrapPersistence(string baseDir)
        {
            lock (InitLock)
            {
                if (_debugPath == null)
                {
                    if (baseDir != null)
                    {
                        _rootPath = baseDir;
                    }

                    if (_rootPath.EndsWith("\\"))
                    {
                        _debugPersistPath = _rootPath + SavePath;
                    }
                    else
                    {
                        _debugPersistPath = _rootPath + "\\" + SavePath;
                    }

                    _debugPath = ActivityIOFactory.CreatePathFromString(_debugPersistPath, "", "");
                    _debugOptsEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(_debugPath);
                }
            }
        }


        /// <summary>
        ///     Boot strap the Session
        /// </summary>
        private void InitPersistSettings()
        {
            lock (SettingsLock)
            {
                if (!_debugOptsEndPoint.PathExist(_debugPath))
                {
                    var args = new Dev2PutRawOperationTO(WriteType.Overwrite, "");
                    ActivityIOFactory.CreateOperationsBroker().PutRaw(_debugOptsEndPoint, args);
                }
                else
                {
                    // fetch from disk
                    var filesToCleanup = new List<string>();
                    using (Stream s = _debugOptsEndPoint.Get(_debugPath, filesToCleanup))
                    {
                        if (s.Length > 0)
                        {
                            var bf = new XmlSerializer(typeof (List<SaveDebugTO>));

                            try
                            {
                                var settings = (List<SaveDebugTO>) bf.Deserialize(s);
                                _debugPersistSettings.Values.ToList().ForEach(a=>a.CleanUp());
                                _debugPersistSettings.Clear();
                                // now push back into the Dictionary
                                foreach (SaveDebugTO dto in settings)
                                {
                                    if (dto.ServiceName.Length > 0)
                                    {
                                        var tmp = new DebugTO();
                                        tmp.CopyFromSaveDebugTO(dto);
                                        _debugPersistSettings[dto.WorkflowID] = tmp;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Dev2Logger.Log.Error(e);
                            }
                        }
                        else
                        {
                            Dev2Logger.Log.Error("No debug data stream [ " + _debugPath + " ] ");
                        }

                        s.Close();
                        s.Dispose();
                        filesToCleanup.ForEach(File.Delete);
                    }
                }
            }
        }

        #endregion

        public void Dispose()
        {
            _debugPersistSettings.Values.ToList().ForEach(a=>a.CleanUp());
        }
    }
}