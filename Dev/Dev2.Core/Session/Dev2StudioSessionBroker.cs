using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.PathOperations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Xml;

namespace Dev2.Session
{
    internal class Dev2StudioSessionBroker : IDev2StudioSessionBroker
    {

        #region Static Conts
        private string _rootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private string _savePath = @"Dev2\DebugData\PersistSettings.dat";
        private string _debugPersistPath; //_debugLoc + "\\persistSettings.dat";
        //private static Dev2TranslationFactory _tFactory = new Dev2TranslationFactory();
        private static readonly IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        private static readonly DataListFormat xmlFormat = DataListFormat.CreateFormat(GlobalConstants._XML);
        private static readonly DataListFormat binaryFormat = DataListFormat.CreateFormat(GlobalConstants._BINARY);
        // the settings lock object
        private readonly object _settingsLock = new object();
        private readonly object _initLock = new object();
        #endregion

        private IActivityIOPath _debugPath; //ActivityIOFactory.CreatePathFromString(_debugPersistPath);
        private IActivityIOOperationsEndPoint _debugOptsEndPoint; // = ActivityIOFactory.CreateOperationEndPointFromIOPath(_debugPath);
        private readonly IDictionary<string, DebugTO> _debugPersistSettings = new ConcurrentDictionary<string, DebugTO>();

        /// <summary>
        /// Init the Debug Session
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public DebugTO InitDebugSession(DebugTO to)
        {
            DebugTO tmp = null;

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

            if (_debugPersistSettings.TryGetValue(to.WorkflowID, out tmp))
            {
                ////Bug 8018
                //var svrCompiler = DataListFactory.CreateServerDataListCompiler(); 
                //var errors = new ErrorResultTO();
                //var mergeGuid = svrCompiler.ConvertTo(null, null, Encoding.UTF8.GetBytes(tmp.XmlData), to.DataList, out errors);
                //tmp.XmlData = svrCompiler.ConvertFrom(null, mergeGuid, enTranslationDepth.Data, null, out errors).FetchAsString();

                //2013.01.28: Ashley Lewis - Phase 1: Find invalid references in the saved session to variables that don't exist any more
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(tmp.XmlData);
                XmlNode savedDL = xDoc.SelectSingleNode(@"DataList");
                var invalidVarNodes = new List<XmlNode>();
                var invalidFieldNodes = new List<XmlNode>();
                foreach (XmlNode Vars in savedDL.ChildNodes) // Search all vars
                {
                    if (!to.DataList.Contains("<" + Vars.Name + " ") && !to.DataList.Contains("<" + Vars.Name + ">") && !to.DataList.Contains("<" + Vars.Name + "/")) invalidVarNodes.Add(Vars);// build list if invalid record sets or scalar
                    if (Vars.HasChildNodes)
                    {
                        foreach (XmlNode Fields in Vars.ChildNodes) // Search fields too
                        {
                            if (!to.DataList.Contains("<" + Fields.Name + " ") && (Fields.Name != "#text") && !to.DataList.Contains("<" + Fields.Name + ">") && !to.DataList.Contains("<" + Fields.Name + "/")) if (Fields.ParentNode != null) invalidFieldNodes.Add(Fields);// Build list of invalid fields
                        }
                    }
                }

                //2013.01.28: Ashley Lewis - Phase 2: Remove references to variables that don't exist anymore
                //Remove invalid scalar or record set references
                foreach (var varNode in invalidVarNodes) savedDL.RemoveChild(varNode);
                //Remove record set fields
                foreach (XmlNode Vars in savedDL.ChildNodes) // Search all vars
                    foreach (var fieldNode in invalidFieldNodes) // For each removed field
                        if (fieldNode.ParentNode!=null) // Confirm is a field
                            if (fieldNode.ParentNode.Name == Vars.Name) // find the recordset it was found in
                                if (Vars.InnerXml.Contains("<" + fieldNode.Name + " ") || Vars.InnerXml.Contains("<" + fieldNode.Name + ">") || Vars.InnerXml.Contains("<" + fieldNode.Name + "/")) // Confirm it exists within that recordset
                                    Vars.RemoveChild(fieldNode); // Remove it

                //2013.01.28: Ashley Lewis - Phase 3: Add new nodes to the saved session
                xDoc.LoadXml(to.DataList);
                XmlNode currentDL = xDoc.SelectSingleNode(@"DataList");
                foreach (XmlNode Vars in currentDL.ChildNodes) // Search all current vars
                {
                    if (!savedDL.InnerXml.Contains("<" + Vars.Name + " ") && !savedDL.InnerXml.Contains("<" + Vars.Name + ">") && !savedDL.InnerXml.Contains("<" + Vars.Name + "/")) savedDL.InnerXml += "<"+Vars.Name+"/>";// add if valid record sets or scalar
                    if (Vars.HasChildNodes)
                    {
                        foreach (XmlNode Fields in Vars.ChildNodes) // Search fields tmpo
                        {
                            if (!savedDL.InnerXml.Contains("<" + Fields.Name + " ") && (Fields.Name != "#text") && !savedDL.InnerXml.Contains("<" + Fields.Name + ">") && !savedDL.InnerXml.Contains("<" + Fields.Name + "/")) 
                                savedDL.SelectSingleNode(Fields.ParentNode.Name).InnerXml += "<"+Fields.Name+"/>";// Add valid fields
                        }
                    }
                }

                if (String.IsNullOrEmpty(savedDL.InnerXml)) tmp.RememberInputs = false;
                else tmp.XmlData = "<DataList>" + savedDL.InnerXml + "</DataList>";
                //End Bug 8018

                to.XmlData = !tmp.RememberInputs
                                 ? (to.DataList ?? "<DataList></DataList>")
                                 : (tmp.XmlData ?? "<DataList></DataList>");

                to.RememberInputs = tmp.RememberInputs;
            }

            // if no XML data copy over the DataList
            to.XmlData = to.XmlData != null && to.XmlData == string.Empty
                             ? (to.DataList ?? "<DataList></DataList>")
                             : (to.XmlData ?? "<DataList></DataList>");

            return to;
        }

        /// <summary>
        /// Update the Debug Session Data
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public DebugTO UpdateDebugSession(DebugTO to)
        {
            return to;
        }

        /// <summary>
        /// Save the debug session data
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public DebugTO PersistDebugSession(DebugTO to)
        {

            lock (_settingsLock)
            {

                if (to.DataList!=null) to.DataListHash = to.DataList.GetHashCode(); // set incoming hash //2013.01.22: Ashley Lewis - Added condition for Bug 7837
                to.Error = string.Empty;

                if (to.RememberInputs)
                {
                    // update the current TO
                    _debugPersistSettings[to.WorkflowID] = to;
                }
                else
                {
                    // no longer relavent, remove it
                    DebugTO tmp = null;

                    if (_debugPersistSettings.TryGetValue(to.WorkflowID, out tmp))
                    {
                        _debugPersistSettings.Remove(to.WorkflowID);
                    }
                }

                List<SaveDebugTO> settingList = new List<SaveDebugTO>();

                // build the list
                foreach (string key in _debugPersistSettings.Keys)
                {
                    DebugTO tmp = null;

                    if (key.Length > 0 && _debugPersistSettings.TryGetValue(key, out tmp))
                    {
                        SaveDebugTO that;
                        that = tmp.CopyToSaveDebugTO();
                        settingList.Add(that);
                    }
                }

                // push to disk
                Stream s = File.Open(_debugPersistPath, FileMode.Truncate);
                XmlSerializer bf = new XmlSerializer(typeof(List<SaveDebugTO>));
                bf.Serialize(s, settingList);

                s.Close();
                s.Dispose();
            }

            return to;
        }

        public string Serialize(IBinaryDataList datalist, enTranslationTypes typeOf, out string error)
        {
            string result = string.Empty;
            error = string.Empty;
            ErrorResultTO errors = new ErrorResultTO();

            if (typeOf == enTranslationTypes.XML)
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, datalist);

                Guid pushID = _compiler.ConvertTo(binaryFormat, ms.ToArray(), string.Empty, out errors);

                if (errors.HasErrors())
                {
                    error = errors.FetchErrors()[0];
                }
                else
                {
                    // now extract into XML
                    result = _compiler.ConvertFrom(pushID, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), enTranslationDepth.Data, out errors);
                    if (errors.HasErrors())
                    {
                        error = errors.FetchErrors()[0];
                    }
                }
                ms.Dispose();
            }

            return result;
        }

        public IBinaryDataList DeSerialize(string data, string targetShape, enTranslationTypes typeOf, out string error)
        {
            error = string.Empty;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();

            if (typeOf == enTranslationTypes.XML)
            {
                ErrorResultTO errors = new ErrorResultTO();


                Guid resultID = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), data,
                                                    targetShape, out errors);
                if (errors.HasErrors())
                {
                    error = errors.FetchErrors()[0]; // take the first error ;)
                }
                else
                {
                    result = _compiler.FetchBinaryDataList(resultID, out errors);
                    if (errors.HasErrors())
                    {
                        error = errors.FetchErrors()[0]; // take the first error ;)
                    }
                    // Now remove it ;)
                    _compiler.DeleteDataListByID(resultID);
                }
            }

            return result;
        }

        #region Private Method

        private void BootstrapPersistence(string baseDir)
        {

            lock (_initLock)
            {
                if (_debugPath == null)
                {
                    if (baseDir != null)
                    {
                        _rootPath = baseDir;
                    }

                    if (_rootPath.EndsWith("\\"))
                    {
                        _debugPersistPath = _rootPath + _savePath;
                    }
                    else
                    {
                        _debugPersistPath = _rootPath + "\\" + _savePath;
                    }

                    _debugPath = ActivityIOFactory.CreatePathFromString(_debugPersistPath);
                    _debugOptsEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(_debugPath);
                }
            }
        }


        /// <summary>
        /// Boot strap the Session
        /// </summary>
        private void InitPersistSettings()
        {

            lock (_settingsLock)
            {

                if (!_debugOptsEndPoint.PathExist(_debugPath))
                {
                    Dev2PutRawOperationTO args = new Dev2PutRawOperationTO(false, "", true);
                    ActivityIOFactory.CreateOperationsBroker().PutRaw(_debugOptsEndPoint, args);
                }
                else
                {
                    // fetch from disk
                    using (Stream s = _debugOptsEndPoint.Get(_debugPath))
                    {
                        if (s.Length > 0)
                        {
                            XmlSerializer bf = new XmlSerializer(typeof(List<SaveDebugTO>));

                            try
                            {
                                List<SaveDebugTO> settings = (List<SaveDebugTO>)bf.Deserialize(s);

                                // now push back into the Dictonary
                                foreach (SaveDebugTO dto in settings)
                                {
                                    if (dto.ServiceName.Length > 0)
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
                            catch (Exception e)
                            {

                                TraceWriter.WriteTrace("Debug Data Error : " + e.Message);
                            }
                        }
                        else
                        {
                            TraceWriter.WriteTrace("No debug data stream [ " + _debugPath + " ] ");
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
