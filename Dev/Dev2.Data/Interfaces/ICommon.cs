using System.Collections.Generic;
using System.IO;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.PathOperations;
using Ionic.Zip;
using Ionic.Zlib;
using Warewolf.Storage;

namespace Dev2.Data.Interfaces
{
    public interface ICommon
    {
        void ValidateEndPoint(IActivityIOOperationsEndPoint endPoint, Dev2CRUDOperationTO args);
        void ExtractFile(Dev2UnZipOperationTO args, ZipFile zip, string extractFromPath);
        void AppendToTemp(Stream originalFileStream, string temp);
        CompressionLevel ExtractZipCompressionLevel(string lvl);
        bool IsNotFtpTypePath(IActivityIOPath src);
        void ValidateSourceAndDestinationPaths(IActivityIOOperationsEndPoint src,IActivityIOOperationsEndPoint dst);
        bool IsUncFileTypePath(IActivityIOPath src);
        void AddMissingFileDirectoryParts(IActivityIOOperationsEndPoint src,IActivityIOOperationsEndPoint dst);
        string CreateTmpDirectory();
        void CreateObjectInputs(IExecutionEnvironment outerEnvironment, IEnumerable<IDev2Definition> inputObjectList, ExecutionEnvironment env, int update);
        void CreateScalarInputs(IExecutionEnvironment outerEnvironment, IEnumerable<IDev2Definition> inputScalarList, ExecutionEnvironment env, int update);
        void CreateRecordSetsInputs(IExecutionEnvironment outerEnvironment, IRecordSetCollection inputRecSets, IList<IDev2Definition> inputs, ExecutionEnvironment env, int update);
        IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection);
        IList<IDev2Definition> GenerateDefsFromDataListForDebug(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection);

    }
}