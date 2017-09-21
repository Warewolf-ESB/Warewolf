using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
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
        public void ValidateEndPoint(IActivityIOOperationsEndPoint endPoint, IDev2CRUDOperationTO args)
        {
            if (endPoint.IOPath?.Path.Trim().Length == 0)
            {
                throw new Exception(ErrorResource.SourceCannotBeAnEmptyString);
            }

            if (endPoint.PathExist(endPoint.IOPath) && !args.Overwrite)
            {
                throw new Exception(ErrorResource.DestinationDirectoryExist);
            }
        }

        public void ExtractFile(IDev2UnZipOperationTO args, ZipFile zip, string extractFromPath)
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
                                           ? ExtractExistingFileAction.OverwriteSilently
                                           : ExtractExistingFileAction.DoNotOverwrite);
                        }
                        catch (BadPasswordException bpe)
                        {
                            throw new Exception(ErrorResource.InvalidArchivePassword, bpe);
                        }
                    }
                }
            }
        }

        public void AppendToTemp(Stream originalFileStream, string temp)
        {
            const int bufferSize = 1024 * 1024;
            var buffer = new char[bufferSize];

            using (var writer = new StreamWriter(temp, true))
            {
                using (var reader = new StreamReader(originalFileStream))
                {
                    int bytesRead;
                    while ((bytesRead = reader.ReadBlock(buffer, 0, bufferSize)) != 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

        public CompressionLevel ExtractZipCompressionLevel(string lvl)
        {
            var lvls = Enum.GetValues(typeof(CompressionLevel));
            var pos = 0;
            //19.09.2012: massimo.guerrera - Changed to default instead of none
            CompressionLevel clvl = CompressionLevel.Default;

            while (pos < lvls.Length && lvls.GetValue(pos).ToString() != lvl)
            {
                pos++;
            }

            if (pos < lvls.Length)
            {
                clvl = (CompressionLevel)lvls.GetValue(pos);
            }

            return clvl;
        }

        public bool IsNotFtpTypePath(IActivityIOPath src)
        {
            return
                !src.Path.ToUpper().StartsWith("ftp://".ToUpper())
                && !src.Path.ToUpper().StartsWith("ftps://".ToUpper())
                && !src.Path.ToUpper().StartsWith("sftp://".ToUpper());
        }

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
                if (!Path.IsPathRooted(dst.IOPath.Path) && IsNotFtpTypePath(dst.IOPath) && IsUncFileTypePath(dst.IOPath))
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

        public bool IsUncFileTypePath(IActivityIOPath src)
        {
            return src.Path.StartsWith(@"\\");
        }

        public void AddMissingFileDirectoryParts(IActivityIOOperationsEndPoint src,
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
                if (!Path.IsPathRooted(dst.IOPath.Path) && IsNotFtpTypePath(dst.IOPath) && IsUncFileTypePath(dst.IOPath))
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

            if (destinationParts.OrderBy(i => i).SequenceEqual(sourceParts.OrderBy(i => i)))
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

        public string CreateTmpDirectory()
        {
            try
            {
                var tmpDir = GlobalConstants.TempLocation;
                var di = Directory.CreateDirectory(tmpDir + "\\" + Guid.NewGuid());

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
            }
        }

        public void CreateScalarInputs(IExecutionEnvironment outerEnvironment, IEnumerable<IDev2Definition> inputScalarList, IExecutionEnvironment env, int update)
        {
            foreach (var dev2Definition in inputScalarList)
            {
                if (!string.IsNullOrEmpty(dev2Definition.Name))
                {
                    env.AssignDataShape("[[" + dev2Definition.Name + "]]");
                }
                if (!dev2Definition.IsRecordSet)
                {
                    if (!string.IsNullOrEmpty(dev2Definition.RawValue))
                    {
                        var warewolfEvalResult = outerEnvironment.Eval(dev2Definition.RawValue, update);
                        if (warewolfEvalResult.IsWarewolfAtomListresult)
                        {
                            ScalarAtomList(warewolfEvalResult, env, dev2Definition);
                        }
                        else
                        {
                            ScalarAtom(warewolfEvalResult, env, dev2Definition);
                        }
                    }
                }
            }
        }

        public void CreateRecordSetsInputs(IExecutionEnvironment outerEnvironment, IRecordSetCollection inputRecSets, IList<IDev2Definition> inputs, IExecutionEnvironment env, int update)
        {
            foreach (var recordSetDefinition in inputRecSets.RecordSets)
            {
                var outPutRecSet = inputs.FirstOrDefault(definition => definition.IsRecordSet && DataListUtil.ExtractRecordsetNameFromValue(definition.MapsTo) == recordSetDefinition.SetName);
                if (outPutRecSet != null)
                {
                    var emptyList = new List<string>();
                    foreach (var dev2ColumnDefinition in recordSetDefinition.Columns)
                    {
                        if (dev2ColumnDefinition.IsRecordSet)
                        {
                            var defn = "[[" + dev2ColumnDefinition.RecordSetName + "()." + dev2ColumnDefinition.Name + "]]";


                            if (string.IsNullOrEmpty(dev2ColumnDefinition.RawValue))
                            {
                                if (!emptyList.Contains(defn))
                                {
                                    emptyList.Add(defn);
                                    continue;
                                }
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
            }
        }

        public IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            if (!string.IsNullOrEmpty(dataList))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(dataList);

                XmlNodeList tmpRootNl = xDoc.ChildNodes;
                XmlNodeList nl = tmpRootNl[0].ChildNodes;

                for (int i = 0; i < nl.Count; i++)
                {
                    XmlNode tmpNode = nl[i];

                    var ioDirection = DataListUtil.GetDev2ColumnArgumentDirection(tmpNode);

                    if (DataListUtil.CheckIODirection(dev2ColumnArgumentDirection, ioDirection))
                    {
                        var jsonAttribute = false;
                        var xmlAttribute = tmpNode.Attributes?["IsJson"];
                        if (xmlAttribute != null)
                        {
                            bool.TryParse(xmlAttribute.Value, out jsonAttribute);
                        }
                        if (tmpNode.HasChildNodes && !jsonAttribute)
                        {
                            // it is a record set, make it as such
                            string recordsetName = tmpNode.Name;
                            // now extract child node defs
                            XmlNodeList childNl = tmpNode.ChildNodes;
                            for (int q = 0; q < childNl.Count; q++)
                            {
                                var xmlNode = childNl[q];
                                if (xmlNode == null)
                                {
                                    continue;
                                }

                                var fieldIoDirection = DataListUtil.GetDev2ColumnArgumentDirection(xmlNode);
                                if (DataListUtil.CheckIODirection(dev2ColumnArgumentDirection, fieldIoDirection))
                                {
                                    result.Add(DataListFactory.CreateDefinition(xmlNode.Name, "", "", recordsetName, false, "",
                                                                                false, "", false));
                                }
                            }
                        }
                        else
                        {
                            // scalar value, make it as such
                            var name = jsonAttribute ? "@" + tmpNode.Name : tmpNode.Name;
                            var dev2Definition = DataListFactory.CreateDefinition(name, "", "", false, "", false, "");
                            dev2Definition.IsObject = jsonAttribute;
                            result.Add(dev2Definition);
                        }
                    }
                }
            }

            return result;
        }

        private static bool IsObject(XmlNode tmpNode)
        {
            XmlAttribute isObjectAttribute = tmpNode.Attributes?["IsJson"];

            if (isObjectAttribute != null)
            {
                if (bool.TryParse(isObjectAttribute.Value, out bool isObject))
                {
                    return isObject;
                }
            }
            return false;
        }

        private bool IsArray(XmlNode tmpNode)
        {
            XmlAttribute isObjectAttribute = tmpNode.Attributes?["IsArray"];

            if (isObjectAttribute != null)
            {
                if (bool.TryParse(isObjectAttribute.Value, out bool isArray))
                {
                    return isArray;
                }
            }
            return false;
        }

        public IList<IDev2Definition> GenerateDefsFromDataListForDebug(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            if (!string.IsNullOrEmpty(dataList))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(dataList);

                XmlNodeList tmpRootNl = xDoc.ChildNodes;
                XmlNodeList nl = tmpRootNl[0].ChildNodes;

                for (int i = 0; i < nl.Count; i++)
                {
                    XmlNode tmpNode = nl[i];

                    var ioDirection = DataListUtil.GetDev2ColumnArgumentDirection(tmpNode);
                    var isObject = IsObject(tmpNode);
                    var isArray = IsArray(tmpNode);
                    if (DataListUtil.CheckIODirection(dev2ColumnArgumentDirection, ioDirection) && tmpNode.HasChildNodes && !isObject)
                    {
                        result.Add(DataListFactory.CreateDefinition("", "", "", tmpNode.Name, false, "",
                                                                            false, "", false));
                    }
                    else if (tmpNode.HasChildNodes && !isObject)
                    {
                        // it is a record set, make it as such
                        string recordsetName = tmpNode.Name;
                        // now extract child node defs
                        XmlNodeList childNl = tmpNode.ChildNodes;
                        for (int q = 0; q < childNl.Count; q++)
                        {
                            var xmlNode = childNl[q];
                            var fieldIoDirection = DataListUtil.GetDev2ColumnArgumentDirection(xmlNode);
                            if (DataListUtil.CheckIODirection(dev2ColumnArgumentDirection, fieldIoDirection))
                            {
                                result.Add(DataListFactory.CreateDefinition(xmlNode.Name, "", "", recordsetName, false, "",
                                                                            false, "", false));
                            }
                        }
                    }
                    else if (DataListUtil.CheckIODirection(dev2ColumnArgumentDirection, ioDirection))
                    {
                        // scalar value, make it as such
                        var dev2Definition = isObject ? DataListFactory.CreateDefinition("@" + tmpNode.Name, "", "", false, "", false, "", false, isArray) : DataListFactory.CreateDefinition(tmpNode.Name, "", "", false, "", false, "");
                        result.Add(dev2Definition);
                    }

                }
            }

            return result;
        }



        private void AtomListInputs(CommonFunctions.WarewolfEvalResult warewolfEvalResult, IDev2Definition dev2ColumnDefinition, IExecutionEnvironment env)
        {
            var recsetResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            DataListUtil.GetRecordsetIndexType(dev2ColumnDefinition.Value);
            if (recsetResult != null)
            {
                var correctRecSet = "[[" + dev2ColumnDefinition.RecordSetName + "(*)." + dev2ColumnDefinition.Name + "]]";

                env.EvalAssignFromNestedStar(correctRecSet, recsetResult, 0);
            }
        }

        private void ScalarAtomList(CommonFunctions.WarewolfEvalResult warewolfEvalResult, IExecutionEnvironment env, IDev2Definition dev2Definition)
        {
            if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult data && data.Item.Any())
            {
                env.AssignWithFrame(new AssignValue("[[" + dev2Definition.Name + "]]", ExecutionEnvironment.WarewolfAtomToString(data.Item.Last())), 0);
            }
        }
        private void ScalarAtom(CommonFunctions.WarewolfEvalResult warewolfEvalResult, IExecutionEnvironment env, IDev2Definition dev2Definition)
        {
            if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult data)
            {
                env.AssignWithFrame(new AssignValue("[[" + dev2Definition.Name + "]]", ExecutionEnvironment.WarewolfAtomToString(data.Item)), 0);
            }
        }

        private void AtomInputs(CommonFunctions.WarewolfEvalResult warewolfEvalResult, IDev2Definition dev2ColumnDefinition, IExecutionEnvironment env)
        {
            var recsetResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            if (dev2ColumnDefinition.IsRecordSet)
            {
                if (recsetResult != null)
                {
                    var correctRecSet = "[[" + dev2ColumnDefinition.RecordSetName + "(*)." + dev2ColumnDefinition.Name + "]]";

                    env.AssignWithFrame(new AssignValue(correctRecSet, PublicFunctions.AtomtoString(recsetResult.Item)), 0);
                }
            }
        }
    }
}
