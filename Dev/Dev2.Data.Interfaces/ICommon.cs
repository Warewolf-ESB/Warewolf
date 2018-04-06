using System.Collections.Generic;
using System.IO;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Interfaces.Enums;
using Ionic.Zip;
using Ionic.Zlib;
using Warewolf.Storage.Interfaces;

namespace Dev2.Data.Interfaces
{
    public interface ICommon
    {
        void ValidateEndPoint(IActivityIOOperationsEndPoint endPoint, IDev2CRUDOperationTO args);
        void ExtractFile(IDev2UnZipOperationTO args, ZipFile zip, string extractFromPath);
        void AppendToTemp(Stream originalFileStream, string temp);
        CompressionLevel ExtractZipCompressionLevel(string lvl);
        bool IsNotFtpTypePath(IActivityIOPath src);
        void ValidateSourceAndDestinationPaths(IActivityIOOperationsEndPoint src,IActivityIOOperationsEndPoint dst);
        bool IsUncFileTypePath(IActivityIOPath src);
        void AddMissingFileDirectoryParts(IActivityIOOperationsEndPoint src,IActivityIOOperationsEndPoint dst);
        string CreateTmpDirectory();
        void CreateObjectInputs(IExecutionEnvironment outerEnvironment, IEnumerable<IDev2Definition> inputObjectList, IExecutionEnvironment env, int update);
        void CreateScalarInputs(IExecutionEnvironment outerEnvironment, IEnumerable<IDev2Definition> inputScalarList, IExecutionEnvironment env, int update);
        void CreateRecordSetsInputs(IExecutionEnvironment outerEnvironment, IRecordSetCollection inputRecSets, IList<IDev2Definition> inputs, IExecutionEnvironment env, int update);
        IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection);
        IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, bool includeNoneDirection);
        IList<IDev2Definition> GenerateDefsFromDataListForDebug(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection);

    }
}