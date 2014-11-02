/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.PathOperations;
using Dev2.Server.Datalist;

namespace Dev2.Session
{
    internal class Dev2StudioSessionBroker : IDev2StudioSessionBroker
    {
        #region Static Conts

        private const string SavePath = @"Warewolf\DebugData\PersistSettings.dat";
        private static readonly DataListFormat BinaryFormat = DataListFormat.CreateFormat(GlobalConstants._BINARY);
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

            IEnvironmentModelDataListCompiler svrCompiler = DataListFactory.CreateServerDataListCompiler();
            ErrorResultTO errors;

            if (_debugPersistSettings.TryGetValue(to.WorkflowID, out tmp))
            {
                string convertData = tmp.XmlData;
                Guid mergeGuid = svrCompiler.ConvertTo(null,
                    DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), Encoding.UTF8.GetBytes(convertData),
                    new StringBuilder(to.DataList), out errors);
                tmp.XmlData =
                    svrCompiler.ConvertFrom(null, mergeGuid, enTranslationDepth.Data,
                        DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), out errors)
                        .FetchAsString()
                        .ToString();
                to.XmlData = tmp.RememberInputs
                    ? (tmp.XmlData)
                    : (to.XmlData ?? "<DataList></DataList>");

                to.BinaryDataList = svrCompiler.FetchBinaryDataList(null, mergeGuid, out errors);
            }
            else
            {
                // if no XML data copy over the DataList
                to.XmlData = to.XmlData != null && to.XmlData == string.Empty
                    ? (to.DataList ?? "<DataList></DataList>")
                    : (to.XmlData ?? "<DataList></DataList>");

                Guid createGuid = svrCompiler.ConvertTo(null,
                    DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), Encoding.UTF8.GetBytes(to.XmlData),
                    new StringBuilder(to.DataList), out errors);

                to.BinaryDataList = to.BinaryDataList = svrCompiler.FetchBinaryDataList(null, createGuid, out errors);
            }


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

        public string Serialize(IBinaryDataList datalist, enTranslationTypes typeOf, out string error)
        {
            string result = string.Empty;
            error = string.Empty;

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            if (typeOf == enTranslationTypes.XML)
            {
                var bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, datalist);

                    ErrorResultTO errors;
                    Guid pushID = compiler.ConvertTo(BinaryFormat, ms.ToArray(), new StringBuilder(), out errors);

                    if (errors.HasErrors())
                    {
                        error = errors.FetchErrors()[0];
                    }
                    else
                    {
                        // now extract into XML
                        result =
                            compiler.ConvertFrom(pushID, DataListFormat.CreateFormat(GlobalConstants._Studio_XML),
                                enTranslationDepth.Data, out errors).ToString();
                        if (errors.HasErrors())
                        {
                            error = errors.FetchErrors()[0];
                        }
                    }
                    ms.Close();
                }
            }

            return result;
        }

        public IBinaryDataList DeSerialize(string data, string targetShape, enTranslationTypes typeOf, out string error)
        {
            error = string.Empty;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            if (typeOf == enTranslationTypes.XML)
            {
                ErrorResultTO errors;


                Guid resultID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML),
                    data.ToStringBuilder(),
                    new StringBuilder(targetShape), out errors);
                if (errors.HasErrors())
                {
                    error = errors.FetchErrors()[0]; // take the first error ;)
                }
                else
                {
                    result = compiler.FetchBinaryDataList(resultID, out errors);
                    if (errors.HasErrors())
                    {
                        error = errors.FetchErrors()[0]; // take the first error ;)
                    }
                }
            }

            return result;
        }

        public string GetXMLForInputs(IBinaryDataList binaryDataList)
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            return
                compiler.ConvertFrom(binaryDataList.UID, DataListFormat.CreateFormat(GlobalConstants._XML_Inputs_Only),
                    enTranslationDepth.Data, out errors).ToString();
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

                                // now push back into the Dictionary
                                foreach (SaveDebugTO dto in settings)
                                {
                                    if (dto.ServiceName.Length > 0)
                                    {
                                        var tmp = new DebugTO();
                                        tmp.CopyFromSaveDebugTO(dto);
                                        string error;

                                        tmp.BinaryDataList = DeSerialize(tmp.XmlData, tmp.DataList,
                                            enTranslationTypes.XML, out error);
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
    }
}