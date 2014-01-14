using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using Dev2.Common;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.PathOperations;

namespace Dev2.Session
{
    internal class Dev2StudioSessionBroker : IDev2StudioSessionBroker
    {

        #region Static Conts
        private string _rootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        const string _savePath = @"Warewolf\DebugData\PersistSettings.dat";
        private string _debugPersistPath;
        private static readonly DataListFormat binaryFormat = DataListFormat.CreateFormat(GlobalConstants._BINARY);
        // the settings lock object
        private readonly static object _settingsLock = new object();
        private readonly static object _initLock = new object();
        #endregion

        private IActivityIOPath _debugPath;
        private IActivityIOOperationsEndPoint _debugOptsEndPoint;
        private readonly IDictionary<string, DebugTO> _debugPersistSettings = new ConcurrentDictionary<string, DebugTO>();

        /// <summary>
        /// Init the Debug Session
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public DebugTO InitDebugSession(DebugTO to)
        {
            DebugTO tmp;

            to.Error = string.Empty;

            // Bootstrap the operations
            if(to.BaseSaveDirectory != null)
            {
                BootstrapPersistence(to.BaseSaveDirectory);
                InitPersistSettings();

            }
            else if(to.BaseSaveDirectory == null && _debugPersistSettings.Count == 0)
            {
                BootstrapPersistence(_rootPath);
                InitPersistSettings();
            }

            if(to.BaseSaveDirectory == null)
            {
                // set the save location
                to.BaseSaveDirectory = _rootPath;
            }


            if(to.DataList != null)
            {
                to.DataListHash = to.DataList.GetHashCode(); // set incoming DL hash
            }
            else
            {
                to.DataListHash = -1; // default value
            }

            var svrCompiler = DataListFactory.CreateServerDataListCompiler();
            ErrorResultTO errors;

            if(_debugPersistSettings.TryGetValue(to.WorkflowID, out tmp))
            {

                var convertData = tmp.XmlData;
                var mergeGuid = svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), Encoding.UTF8.GetBytes(convertData), to.DataList ?? "", out errors);
                tmp.XmlData = svrCompiler.ConvertFrom(null, mergeGuid, enTranslationDepth.Data, DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), out errors).FetchAsString();
                to.XmlData = tmp.RememberInputs
                                 ? (tmp.XmlData ?? "<DataList></DataList>")
                                 : (to.XmlData ?? "<DataList></DataList>");

                to.BinaryDataList = svrCompiler.FetchBinaryDataList(null, mergeGuid, out errors);
            }
            else
            {
                // if no XML data copy over the DataList
                to.XmlData = to.XmlData != null && to.XmlData == string.Empty
                                 ? (to.DataList ?? "<DataList></DataList>")
                                 : (to.XmlData ?? "<DataList></DataList>");

                var createGuid = svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), Encoding.UTF8.GetBytes(to.XmlData), to.DataList, out errors);

                to.BinaryDataList = to.BinaryDataList = svrCompiler.FetchBinaryDataList(null, createGuid, out errors);
            }



            return to;
        }

        /// <summary>
        /// Save the debug session data
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public DebugTO PersistDebugSession(DebugTO to)
        {

            lock(_settingsLock)
            {

                if(to.DataList != null) to.DataListHash = to.DataList.GetHashCode(); // set incoming hash //2013.01.22: Ashley Lewis - Added condition for Bug 7837
                to.Error = string.Empty;

                if(to.RememberInputs)
                {
                    // update the current TO
                    _debugPersistSettings[to.WorkflowID] = to;
                }
                else
                {
                    // no longer relavent, remove it
                    DebugTO tmp;

                    if(_debugPersistSettings.TryGetValue(to.WorkflowID, out tmp))
                    {
                        _debugPersistSettings.Remove(to.WorkflowID);
                    }
                }

                List<SaveDebugTO> settingList = new List<SaveDebugTO>();

                // build the list
                foreach(string key in _debugPersistSettings.Keys)
                {
                    DebugTO tmp;

                    if(key.Length > 0 && _debugPersistSettings.TryGetValue(key, out tmp))
                    {
                        SaveDebugTO that = tmp.CopyToSaveDebugTO();
                        settingList.Add(that);
                    }
                }

                // push to disk
                using(Stream s = File.Open(_debugPersistPath, FileMode.Truncate))
                {
                    XmlSerializer bf = new XmlSerializer(typeof(List<SaveDebugTO>));
                    bf.Serialize(s, settingList);
                }
            }

            return to;
        }

        public string Serialize(IBinaryDataList datalist, enTranslationTypes typeOf, out string error)
        {
            string result = string.Empty;
            error = string.Empty;

            var compiler = DataListFactory.CreateDataListCompiler();

            if(typeOf == enTranslationTypes.XML)
            {
                BinaryFormatter bf = new BinaryFormatter();
                using(MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, datalist);

                    ErrorResultTO errors;
                    Guid pushID = compiler.ConvertTo(binaryFormat, ms.ToArray(), string.Empty, out errors);

                    if(errors.HasErrors())
                    {
                        error = errors.FetchErrors()[0];
                    }
                    else
                    {
                        // now extract into XML
                        result = compiler.ConvertFrom(pushID, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), enTranslationDepth.Data, out errors);
                        if(errors.HasErrors())
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

            var compiler = DataListFactory.CreateDataListCompiler();

            if(typeOf == enTranslationTypes.XML)
            {
                ErrorResultTO errors;


                Guid resultID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), data,
                                                    targetShape, out errors);
                if(errors.HasErrors())
                {
                    error = errors.FetchErrors()[0]; // take the first error ;)
                }
                else
                {
                    result = compiler.FetchBinaryDataList(resultID, out errors);
                    if(errors.HasErrors())
                    {
                        error = errors.FetchErrors()[0]; // take the first error ;)
                    }
                }
            }

            return result;
        }

        public string GetXMLForInputs(IBinaryDataList binaryDataList)
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            return compiler.ConvertFrom(binaryDataList.UID, DataListFormat.CreateFormat(GlobalConstants._XML_Inputs_Only), enTranslationDepth.Data, out errors);
        }

        #region Private Method

        private void BootstrapPersistence(string baseDir)
        {

            lock(_initLock)
            {
                if(_debugPath == null)
                {
                    if(baseDir != null)
                    {
                        _rootPath = baseDir;
                    }

                    if(_rootPath.EndsWith("\\"))
                    {
                        _debugPersistPath = _rootPath + _savePath;
                    }
                    else
                    {
                        _debugPersistPath = _rootPath + "\\" + _savePath;
                    }

                    _debugPath = ActivityIOFactory.CreatePathFromString(_debugPersistPath, "", "");
                    _debugOptsEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(_debugPath);
                }
            }
        }


        /// <summary>
        /// Boot strap the Session
        /// </summary>
        private void InitPersistSettings()
        {

            lock(_settingsLock)
            {

                if(!_debugOptsEndPoint.PathExist(_debugPath))
                {
                    Dev2PutRawOperationTO args = new Dev2PutRawOperationTO(WriteType.Overwrite, "");
                    ActivityIOFactory.CreateOperationsBroker().PutRaw(_debugOptsEndPoint, args);
                }
                else
                {
                    // fetch from disk
                    using(Stream s = _debugOptsEndPoint.Get(_debugPath))
                    {
                        if(s.Length > 0)
                        {
                            XmlSerializer bf = new XmlSerializer(typeof(List<SaveDebugTO>));

                            try
                            {
                                List<SaveDebugTO> settings = (List<SaveDebugTO>)bf.Deserialize(s);

                                // now push back into the Dictonary
                                foreach(SaveDebugTO dto in settings)
                                {
                                    if(dto.ServiceName.Length > 0)
                                    {
                                        DebugTO tmp = new DebugTO();
                                        tmp.CopyFromSaveDebugTO(dto);
                                        string error;

                                        tmp.BinaryDataList = DeSerialize(tmp.XmlData, tmp.DataList,
                                                                         enTranslationTypes.XML, out error);
                                        _debugPersistSettings[dto.WorkflowID] = tmp;
                                    }
                                }
                            }
                            catch(Exception e)
                            {
                                ServerLogger.LogError(e);
                            }
                        }
                        else
                        {
                            ServerLogger.LogError("No debug data stream [ " + _debugPath + " ] ");
                        }

                        s.Close();
                        s.Dispose();
                    }
                }
            }
        }

        #endregion

    }
}
