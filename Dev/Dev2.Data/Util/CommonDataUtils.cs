#pragma warning disable
 using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Utils;
using Dev2.Common.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.PathOperations.Extension;
using Dev2.DataList.Contract;
using Ionic.Zip;
using Ionic.Zlib;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;

namespace Dev2.Data.Util
{

    public class CommonDataUtils : ICommon
    {
        readonly IDirectory _directory;
        public CommonDataUtils()
            : this(new DirectoryWrapper())
        {
        }
        public CommonDataUtils(IDirectory directory)
        {
            _directory = directory;
        }

        public void ValidateEndPoint(IActivityIOOperationsEndPoint endPoint, IDev2CRUDOperationTO args)
        {
            var path = endPoint.IOPath?.Path;
            if (path is null || path.Trim().Length == 0)
            {
                throw new Exception(ErrorResource.SourceCannotBeAnEmptyString);
            }

            if (endPoint.PathExist(endPoint.IOPath) && !args.Overwrite)
            {
                throw new Exception(ErrorResource.DestinationDirectoryExist);
            }
        }

        public void ExtractFile(IDev2UnZipOperationTO args, IIonicZipFileWrapper zip, string extractFromPath)
        {
            if (zip != null)
            {
                using (zip)
                {
                    if (!string.IsNullOrEmpty(args.ArchivePassword))
                    {
                        zip.Password = args.ArchivePassword;
                    }

                    foreach (var ze in zip)
                    {
                        try
                        {
                            ze.Extract(extractFromPath,
                                       args.Overwrite
                                           ? FileOverwrite.Yes
                                           : FileOverwrite.No);
                        }
                        catch (BadPasswordException bpe)
                        {
                            throw new Exception(ErrorResource.InvalidArchivePassword, bpe);
                        }
                    }
                }
            }
        }

        public static string TempFile(string extension)
        {
            var path = System.IO.Path.GetTempPath();
            var guid = Guid.NewGuid().ToString();
            return $"{path}/{guid}.{extension}";
        }

        public CompressionLevel ExtractZipCompressionLevel(string lvl)
        {
            var lvls = Enum.GetValues(typeof(CompressionLevel));

            for (var pos = 0; pos < lvls.Length; pos++)
            {
                if (lvls.GetValue(pos).ToString() == lvl)
                {
                    return (CompressionLevel)lvls.GetValue(pos);
                }
            }
            return CompressionLevel.Default;
        }

        public static bool IsNotFtpTypePath(IActivityIOPath src) => !src.Path.ToLower().StartsWith("ftp://".ToLower())
                && !src.Path.ToLower().StartsWith("ftps://".ToLower())
                && !src.Path.ToLower().StartsWith("sftp://".ToLower());

        public void ValidateSourceAndDestinationPaths(IActivityIOOperationsEndPoint src,
                                                        IActivityIOOperationsEndPoint dst)
        {
            if (src.IOPath.Path.Trim().Length == 0)
            {
                throw new Exception(ErrorResource.SourceCannotBeAnEmptyString);
            }

            var sourceParts = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                       StringSplitOptions.RemoveEmptyEntries).ToList();

            if (dst.IOPath.Path.Trim().Length == 0)
            {
                dst.IOPath.Path = src.IOPath.Path;
            }
            else
            {
                // TODO: verify if this condition is possible, UNC paths start with @"\\" but @"\\file" is always rooted
                if (!Path.IsPathRooted(dst.IOPath.Path) && IsNotFtpTypePath(dst.IOPath) && IsUncFileTypePath(dst.IOPath.Path))
                {

                    var lastPart = sourceParts.Last();

                    dst.IOPath.Path =
                        Path.Combine(src.PathIs(dst.IOPath) == enPathType.Directory
                                         ? src.IOPath.Path
                                         : src.IOPath.Path.Replace(lastPart, ""), dst.IOPath.Path);
                }
            }

            var destinationParts = dst.IOPath.Path.Split(dst.PathSeperator().ToCharArray(),
                                                       StringSplitOptions.RemoveEmptyEntries).ToList();

            while (destinationParts.Count > sourceParts.Count)
            {
                destinationParts.Remove(destinationParts.Last());
            }

            if (destinationParts.OrderBy(i => i).SequenceEqual(
                 sourceParts.OrderBy(i => i)))
            {
                throw new Exception(ErrorResource.DestinationDirectoryCannotBeAChild);
            }
        }

        public static bool IsUncFileTypePath(string path) => path.StartsWith(@"\\");

        public void AddMissingFileDirectoryParts(IActivityIOOperationsEndPoint src,
                                                 IActivityIOOperationsEndPoint dst)
        {
            AddMissingFileDirectoryPartsImpl.Execute(src, dst);
        }

        static class AddMissingFileDirectoryPartsImpl
        {
            static internal void Execute(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst)
            {
                var sourceParts = VerifyAndCleanInputs(src, dst);

                if (IsDestinationSubdirectoryOfSource(dst, sourceParts))
                {
                    if (dst.PathIs(dst.IOPath) == enPathType.Directory)
                    {
                        var strings = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                                 StringSplitOptions.RemoveEmptyEntries);
                        var lastPart = strings.Last();
                        dst.IOPath.Path = src.PathIs(src.IOPath) == enPathType.Directory
                                              ? Path.Combine(dst.IOPath.Path, lastPart)
                                              : dst.IOPath.Path.Replace(lastPart, "");
                    }
                }
                else
                {
                    if (dst.PathIs(dst.IOPath) == enPathType.Directory && src.PathIs(src.IOPath) == enPathType.Directory)
                    {
                        var strings = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                                 StringSplitOptions.RemoveEmptyEntries);
                        var lastPart = strings.Last();
                        dst.IOPath.Path = dst.Combine(lastPart);
                    }
                }
            }

            static List<string> VerifyAndCleanInputs(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst)
            {
                if (src.IOPath.Path.Trim().Length == 0)
                {
                    throw new Exception(ErrorResource.SourceCannotBeAnEmptyString);
                }
                var sourceParts = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                        StringSplitOptions.RemoveEmptyEntries).ToList();

                if (dst.IOPath.Path.Trim().Length == 0)
                {
                    dst.IOPath.Path = src.IOPath.Path;
                }
                else
                {
                    // TODO: verify if this condition is possible, UNC paths start with @"\\" but @"\\file" is always rooted
                    if (!Path.IsPathRooted(dst.IOPath.Path) && IsNotFtpTypePath(dst.IOPath) && IsUncFileTypePath(dst.IOPath.Path))
                    {
                        var lastPart = sourceParts.Last();
                        dst.IOPath.Path =
                            Path.Combine(src.PathIs(dst.IOPath) == enPathType.Directory
                                             ? src.IOPath.Path
                                             : src.IOPath.Path.Replace(lastPart, ""), dst.IOPath.Path);
                    }
                }

                return sourceParts;
            }

            static bool IsDestinationSubdirectoryOfSource(IActivityIOOperationsEndPoint dst, List<string> sourceParts)
            {
                var destinationParts = dst.IOPath.Path.Split(dst.PathSeperator().ToCharArray(),
                                                         StringSplitOptions.RemoveEmptyEntries).ToList();

                while (destinationParts.Count > sourceParts.Count)
                {
                    destinationParts.Remove(destinationParts.Last());
                }
                return destinationParts.OrderBy(i => i).SequenceEqual(sourceParts.OrderBy(i => i));
            }
        }


        public string CreateTmpDirectory()
        {
            try
            {
                var tmpDir = GlobalConstants.TempLocation;
                var di = _directory.CreateDirectory(tmpDir + "\\" + Guid.NewGuid());

                return di.FullName;
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }

        }

        public void CreateObjectInputs(IExecutionEnvironment outerEnvironment, IEnumerable<IDev2Definition> inputObjectList, IExecutionEnvironment env, int update)
        {
            foreach (var dev2Definition in inputObjectList)
            {
                if (!string.IsNullOrEmpty(dev2Definition.RawValue))
                {
                    CreateObjectInputs(outerEnvironment, dev2Definition, env, update);
                }
            }
        }

        static void CreateObjectInputs(IExecutionEnvironment outerEnvironment, IDev2Definition dev2Definition, IExecutionEnvironment env, int update)
        {
            if (DataListUtil.RemoveLanguageBrackets(dev2Definition.RawValue).StartsWith("@"))
            {
                var jVal = outerEnvironment.EvalJContainer(dev2Definition.RawValue);
                env.AddToJsonObjects(DataListUtil.AddBracketsToValueIfNotExist(dev2Definition.Name), jVal);
            }
            else
            {
                var result = outerEnvironment.Eval(dev2Definition.RawValue, update);
                if (result.IsWarewolfAtomListresult)
                {
                    if (result is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult data && data.Item.Any())
                    {
                        env.AssignWithFrame(new AssignValue(DataListUtil.AddBracketsToValueIfNotExist(dev2Definition.Name), ExecutionEnvironment.WarewolfAtomToString(data.Item.Last())), 0);
                    }
                }
                else
                {
                    if (result is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult data)
                    {
                        env.AssignWithFrame(new AssignValue(DataListUtil.AddBracketsToValueIfNotExist(dev2Definition.Name), ExecutionEnvironment.WarewolfAtomToString(data.Item)), 0);
                    }
                }
            }
        }

        public void CreateScalarInputs(IExecutionEnvironment outerEnvironment, IEnumerable<IDev2Definition> inputScalarList, IExecutionEnvironment env, int update)
        {
            foreach (var dev2Definition in inputScalarList)
            {
                CreateScalarInputs(outerEnvironment, dev2Definition, env, update);
            }
        }

        static void CreateScalarInputs(IExecutionEnvironment outerEnvironment, IDev2Definition dev2Definition, IExecutionEnvironment env, int update)
        {
            void ScalarAtomList(CommonFunctions.WarewolfEvalResult warewolfEvalResult)
            {
                if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult data && data.Item.Any())
                {
                    env.AssignWithFrame(new AssignValue("[[" + dev2Definition.Name + "]]", ExecutionEnvironment.WarewolfAtomToString(data.Item.Last())), 0);
                }
            }
            void ScalarAtom(CommonFunctions.WarewolfEvalResult warewolfEvalResult)
            {
                if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult data)
                {
                    env.AssignWithFrame(new AssignValue("[[" + dev2Definition.Name + "]]", ExecutionEnvironment.WarewolfAtomToString(data.Item)), 0);
                }
            }

            if (!string.IsNullOrEmpty(dev2Definition.Name))
            {
                env.AssignDataShape("[[" + dev2Definition.Name + "]]");
            }
            if (!dev2Definition.IsRecordSet && !string.IsNullOrEmpty(dev2Definition.RawValue))
            {
                var warewolfEvalResult = outerEnvironment.Eval(dev2Definition.RawValue, update);
                if (warewolfEvalResult.IsWarewolfAtomListresult)
                {
                    ScalarAtomList(warewolfEvalResult);
                }
                else
                {
                    ScalarAtom(warewolfEvalResult);
                }
            }
        }

        public void CreateRecordSetsInputs(IExecutionEnvironment outerEnvironment, IRecordSetCollection inputRecSets, IList<IDev2Definition> inputs, IExecutionEnvironment env, int update)
        {
            foreach (var recordSetDefinition in inputRecSets.RecordSets)
            {
                CreateRecordSetsInputsImpl.CreateRecordSetsInputs(outerEnvironment, recordSetDefinition, inputs, env, update);
            }
        }

        static class CreateRecordSetsInputsImpl
        {
            static internal void CreateRecordSetsInputs(IExecutionEnvironment outerEnvironment, IRecordSetDefinition recordSetDefinition, IList<IDev2Definition> inputs, IExecutionEnvironment env, int update)
            {
                var outPutRecSet = inputs.FirstOrDefault(definition => definition.IsRecordSet && DataListUtil.ExtractRecordsetNameFromValue(definition.MapsTo) == recordSetDefinition.SetName);
                if (outPutRecSet != null)
                {
                    CreateRecordSetsInputs(outerEnvironment, recordSetDefinition, env, update);
                }
            }

            static void CreateRecordSetsInputs(IExecutionEnvironment outerEnvironment, IRecordSetDefinition recordSetDefinition, IExecutionEnvironment env, int update)
            {
                var emptyList = new List<string>();
                foreach (var dev2ColumnDefinition in recordSetDefinition.Columns)
                {
                    if (dev2ColumnDefinition.IsRecordSet)
                    {
                        var defn = "[[" + dev2ColumnDefinition.RecordSetName + "()." + dev2ColumnDefinition.Name + "]]";


                        if (string.IsNullOrEmpty(dev2ColumnDefinition.RawValue) && !emptyList.Contains(defn))
                        {
                            emptyList.Add(defn);
                            continue;
                        }

                        var warewolfEvalResult = outerEnvironment.Eval(dev2ColumnDefinition.RawValue, update);

                        if (warewolfEvalResult.IsWarewolfAtomListresult)
                        {
                            AtomListInputs(warewolfEvalResult, dev2ColumnDefinition, env);
                        }
                        if (warewolfEvalResult.IsWarewolfAtomResult)
                        {
                            AtomInputs(warewolfEvalResult, dev2ColumnDefinition, env);
                        }
                    }
                }
                foreach (var defn in emptyList)
                {
                    env.AssignDataShape(defn);
                }
            }
            static void AtomListInputs(CommonFunctions.WarewolfEvalResult warewolfEvalResult, IDev2Definition dev2ColumnDefinition, IExecutionEnvironment env)
            {
                // TODO: why is this called but the return never used? can we remove this?
                DataListUtil.GetRecordsetIndexType(dev2ColumnDefinition.Value);

                if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult)
                {
                    var correctRecSet = "[[" + dev2ColumnDefinition.RecordSetName + "(*)." + dev2ColumnDefinition.Name + "]]";

                    env.EvalAssignFromNestedStar(correctRecSet, recsetResult, 0);
                }
            }

            static void AtomInputs(CommonFunctions.WarewolfEvalResult warewolfEvalResult, IDev2Definition dev2ColumnDefinition, IExecutionEnvironment env)
            {
                var recsetResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                if (dev2ColumnDefinition.IsRecordSet && recsetResult != null)
                {
                    var correctRecSet = "[[" + dev2ColumnDefinition.RecordSetName + "(*)." + dev2ColumnDefinition.Name + "]]";
                    env.AssignWithFrame(new AssignValue(correctRecSet, PublicFunctions.AtomtoString(recsetResult.Item)), 0);
                }
            }
        }

        public IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection) => GenerateDefsFromDataList(dataList, dev2ColumnArgumentDirection, false, null);

        public IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, bool includeNoneDirection, ISearch searchParameters)
        {
            return new GenerateDefsFromXmlNodeListImpl(dataList, dev2ColumnArgumentDirection, includeNoneDirection, searchParameters).Execute();
        }

        public IList<IDev2Definition> GenerateDefsFromDataListForDebug(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            return new GenerateDefsFromXmlNodeListForDebugImpl(dataList, dev2ColumnArgumentDirection, false, null).Execute();
        }

        abstract class GenerateDefsFromXmlCommon
        {
            protected readonly IList<IDev2Definition> _result = new List<IDev2Definition>();
            protected readonly string _dataList;
            protected readonly enDev2ColumnArgumentDirection _dev2ColumnArgumentDirection;
            protected readonly bool _includeNoneDirection;
            protected readonly ISearch _searchParameters;

            protected GenerateDefsFromXmlCommon(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, bool includeNoneDirection, ISearch searchParameters)
            {
                this._dataList = dataList;
                this._dev2ColumnArgumentDirection = dev2ColumnArgumentDirection;
                this._includeNoneDirection = includeNoneDirection;
                this._searchParameters = searchParameters;
            }

            internal IList<IDev2Definition> Execute()
            {
                if (!string.IsNullOrEmpty(_dataList))
                {
                    var xDoc = new XmlDocument();
                    xDoc.LoadXml(_dataList);

                    var tmpRootNl = xDoc.ChildNodes;
                    var nl = tmpRootNl[0].ChildNodes;

                    for (int i = 0; i < nl.Count; i++)
                    {
                        GenerateDefsFromXmlNodeList(nl, i);
                    }
                }

                return _result;
            }

            protected abstract void GenerateDefsFromXmlNodeList(XmlNodeList nl, int i);
        }

        class GenerateDefsFromXmlNodeListImpl : GenerateDefsFromXmlCommon
        {
            public GenerateDefsFromXmlNodeListImpl(string dataList, enDev2ColumnArgumentDirection enDev2ColumnArgumentDirection, bool includeNoneDirection, ISearch searchParameters)
                : base(dataList, enDev2ColumnArgumentDirection, includeNoneDirection, searchParameters)
            {
            }

            protected override void GenerateDefsFromXmlNodeList(XmlNodeList nl, int i)
            {
                var tmpNode = nl[i];

                var ioDirection = DataListUtil.GetDev2ColumnArgumentDirection(tmpNode);

                bool ioDirectionMatch = DataListUtil.CheckIODirection(_dev2ColumnArgumentDirection, ioDirection, _includeNoneDirection);
                var wordMatch = true;
                if (_searchParameters != null)
                {
                    wordMatch = SearchUtils.FilterText(tmpNode.Name, _searchParameters);
                }
                if (ioDirectionMatch && wordMatch)
                {
                    GenerateDefsFromXmlNodeList(_dev2ColumnArgumentDirection, _includeNoneDirection, _result, tmpNode);
                }
            }

            static void GenerateDefsFromXmlNodeList(enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, bool includeNoneDirection, IList<IDev2Definition> result, XmlNode tmpNode)
            {
                var isJson = false;
                var xmlAttribute = tmpNode.Attributes?["IsJson"];
                if (xmlAttribute != null)
                {
                    bool.TryParse(xmlAttribute.Value, out isJson);
                }

                var isRecordset = tmpNode.HasChildNodes && !isJson;
                if (isRecordset)
                {
                    var recordsetName = tmpNode.Name;
                    var childNl = tmpNode.ChildNodes;
                    for (int q = 0; q < childNl.Count; q++)
                    {
                        var xmlNode = childNl[q];
                        // is it possible for their to be childnodes that are null?
                        if (xmlNode == null)
                        {
                            continue;
                        }

                        var fieldIoDirection = DataListUtil.GetDev2ColumnArgumentDirection(xmlNode);
                        if (DataListUtil.CheckIODirection(dev2ColumnArgumentDirection, fieldIoDirection, includeNoneDirection))
                        {
                            result.Add(DataListFactory.CreateDefinition_Recordset(xmlNode.Name, "", "", recordsetName, false, "",
                                                                        false, "", false));
                        }
                    }
                }
                else
                {
                    var name = isJson ? "@" + tmpNode.Name : tmpNode.Name;

                    var dev2Definition = new Dev2Definition(name, "", "", false, "", false, "")
                    {
                        IsObject = isJson
                    };
                    result.Add(dev2Definition);
                }
            }
        }

        private class GenerateDefsFromXmlNodeListForDebugImpl : GenerateDefsFromXmlCommon
        {
            public GenerateDefsFromXmlNodeListForDebugImpl(string dataList, enDev2ColumnArgumentDirection enDev2ColumnArgumentDirection, bool includeNoneDirection, ISearch searchParameters)
                : base(dataList, enDev2ColumnArgumentDirection, includeNoneDirection, searchParameters)
            {
            }
            protected override void GenerateDefsFromXmlNodeList(XmlNodeList nl, int i)
            {
                var tmpNode = nl[i];

                var ioDirection = DataListUtil.GetDev2ColumnArgumentDirection(tmpNode);
                var isObject = IsObject(tmpNode);
                var isArray = IsArray(tmpNode);
                if (DataListUtil.CheckIODirection(_dev2ColumnArgumentDirection, ioDirection, false) && tmpNode.HasChildNodes && !isObject)
                {
                    _result.Add(DataListFactory.CreateDefinition_Recordset("", "", "", tmpNode.Name, false, "", false, "", false));
                }
                else if (tmpNode.HasChildNodes && !isObject)
                {
                    var recordsetName = tmpNode.Name;
                    var childNl = tmpNode.ChildNodes;
                    for (int q = 0; q < childNl.Count; q++)
                    {
                        var xmlNode = childNl[q];
                        var fieldIoDirection = DataListUtil.GetDev2ColumnArgumentDirection(xmlNode);
                        if (DataListUtil.CheckIODirection(_dev2ColumnArgumentDirection, fieldIoDirection, false))
                        {
                            _result.Add(DataListFactory.CreateDefinition_Recordset(xmlNode.Name, "", "", recordsetName, false, "",
                                                                        false, "", false));
                        }
                    }
                }
                else
                {
                    if (DataListUtil.CheckIODirection(_dev2ColumnArgumentDirection, ioDirection, false))
                    {
                        var dev2Definition = isObject
                            ? DataListFactory.CreateDefinition_JsonArray("@" + tmpNode.Name, "", "", false, "", false, "", false, isArray)
                            : new Dev2Definition(tmpNode.Name, "", "", false, "", false, "");
                        _result.Add(dev2Definition);
                    }
                }
            }
            static bool IsObject(XmlNode tmpNode)
            {
                var isObjectAttribute = tmpNode.Attributes?["IsJson"];

                if (isObjectAttribute != null && bool.TryParse(isObjectAttribute.Value, out bool _isObject))
                {
                    return _isObject;
                }
                return false;
            }

            static bool IsArray(XmlNode tmpNode)
            {
                var isObjectAttribute = tmpNode.Attributes?["IsArray"];

                if (isObjectAttribute != null && bool.TryParse(isObjectAttribute.Value, out bool _isArray))
                {
                    return _isArray;
                }
                return false;
            }

        }
    }
}