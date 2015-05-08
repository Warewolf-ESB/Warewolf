
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
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Activities.PathOperations;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.PathOperations;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

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

        IWarewolfIterator _archPassItr;
        IWarewolfIterator _compresItr;
        IWarewolfIterator _archNameItr;

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

        protected override void AddDebugInputItems(IExecutionEnvironment environment)
        {
            AddDebugInputItem(CompressionRatio, "Compression Ratio",environment);
        }

        protected override void AddItemsToIterator(IExecutionEnvironment environment)
        {
            _compresItr = new WarewolfIterator(environment.Eval(CompressionRatio));
            ColItr.AddVariableToIterateOn(_compresItr);

            _archNameItr = new WarewolfIterator(environment.Eval(ArchiveName));
            ColItr.AddVariableToIterateOn(_archNameItr);

            _archPassItr = new WarewolfIterator(environment.Eval(ArchivePassword));
            ColItr.AddVariableToIterateOn(_archPassItr);

        }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint)
        {

            Dev2ZipOperationTO zipTo = ActivityIOFactory.CreateZipTO(ColItr.FetchNextValue(_compresItr),
                                                                         ColItr.FetchNextValue(_archPassItr),
                                                                         ColItr.FetchNextValue(_archNameItr),
                                                                         Overwrite);

            return broker.Zip(scrEndPoint, dstEndPoint, zipTo);
        }

        protected override void MoveRemainingIterators()
        {
            ColItr.FetchNextValue(_compresItr);
            ColItr.FetchNextValue(_archPassItr);
            ColItr.FetchNextValue(_archNameItr);
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach(Tuple<string, string> t in updates)
            {
                if(t.Item1 == ArchiveName)
                {
                    ArchiveName = t.Item2;
                }

                if(t.Item1 == ArchivePassword)
                {
                    ArchivePassword = t.Item2;
                }

                if(t.Item1 == CompressionRatio)
                {
                    CompressionRatio = t.Item2;
                }

                if(t.Item1 == InputPath)
                {
                    InputPath = t.Item2;
                }

                if(t.Item1 == OutputPath)
                {
                    OutputPath = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if(itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
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
