using Dev2.Activities;
using Dev2.Activities.PathOperations;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.PathOperations;
using Dev2.Util;
using System;
using System.Activities;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{

    /// <summary>
    /// PBI : 1172
    /// Status : New 
    /// Purpose : To provide an activity that can zip the contents of a file/folder from FTP, FTPS and file system
    /// </summary>
    public class DsfZip : DsfAbstractMultipleFilesActivity, IZip, IPathInput, IPathOutput, IPathOverwrite,
                          IDestinationUsernamePassword
    {
        private string _compressionRatio;

        public DsfZip()
            : base("Zip")
        {
            ArchiveName = string.Empty;
            CompressionRatio = string.Empty;
            ArchivePassword = string.Empty;
        }

        IDev2DataListEvaluateIterator _archPassItr;
        IDev2DataListEvaluateIterator _compresItr;
        IBinaryDataListEntry _archiveNameEntry;
        IDev2DataListEvaluateIterator _archNameItr;
        IBinaryDataListEntry _compressionRatioEntry;

        #region Properties
        /// <summary>
        /// Gets or sets the archive password.
        /// </summary>      
        [Inputs("Archive Password")]
        [FindMissing]
        public string ArchivePassword { get; set; }

        /// <summary>
        /// Gets or sets the name of the archive.
        /// </summary>
        [Inputs("Archive Name")]
        [FindMissing]
        public string ArchiveName { get; set; }

        /// <summary>
        /// Gets or sets the compression ratio.
        /// </summary>
        [Inputs("Compession Ratio"), FindMissing]
        public string CompressionRatio
        {
            get { return _compressionRatio; }
            set { _compressionRatio = string.IsNullOrEmpty(value) ? value : value.Replace(" ", ""); }
        }
        #endregion Properties

        protected override void AddDebugInputItems(Guid executionId)
        {
            //AddDebugInputItemPassword("Archive Password", ArchivePassword);
            AddDebugInputItem(CompressionRatio, "Compression Ratio", _compressionRatioEntry, executionId);
        }

        protected override void AddItemsToIterator(Guid executionId, IDataListCompiler compiler, List<ErrorResultTO> errors)
        {
            ErrorResultTO error;
            _compressionRatioEntry = compiler.Evaluate(executionId, enActionType.User,
                                                                          CompressionRatio, false, out error);
            errors.Add(error);
            _compresItr =
                Dev2ValueObjectFactory.CreateEvaluateIterator(_compressionRatioEntry);
            ColItr.AddIterator(_compresItr);

            _archiveNameEntry = compiler.Evaluate(executionId, enActionType.User, ArchiveName, false,
                                                                      out error);
            errors.Add(error);
            _archNameItr = Dev2ValueObjectFactory.CreateEvaluateIterator(_archiveNameEntry);
            ColItr.AddIterator(_archNameItr);

            IBinaryDataListEntry archPassEntry = compiler.Evaluate(executionId, enActionType.User, ArchivePassword,
                                                                  false, out error);
            errors.Add(error);
            _archPassItr = Dev2ValueObjectFactory.CreateEvaluateIterator(archPassEntry);
            ColItr.AddIterator(_archPassItr);

        }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint)
        {

            Dev2ZipOperationTO zipTO = ActivityIOFactory.CreateZipTO(ColItr.FetchNextRow(_compresItr).TheValue,
                                                                         ColItr.FetchNextRow(_archPassItr).TheValue,
                                                                         ColItr.FetchNextRow(_archNameItr).TheValue,
                                                                         Overwrite);

            return broker.Zip(scrEndPoint, dstEndPoint, zipTO);
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach (Tuple<string, string> t in updates)
            {
                if (t.Item1 == ArchiveName)
                {
                    ArchiveName = t.Item2;
                }

                if (t.Item1 == ArchivePassword)
                {
                    ArchivePassword = t.Item2;
                }

                if (t.Item1 == CompressionRatio)
                {
                    CompressionRatio = t.Item2;
                }

                if (t.Item1 == InputPath)
                {
                    InputPath = t.Item2;
                }

                if (t.Item1 == OutputPath)
                {
                    OutputPath = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(ArchiveName, ArchivePassword, CompressionRatio, InputPath, OutputPath);
        }
        #endregion
    }
}
