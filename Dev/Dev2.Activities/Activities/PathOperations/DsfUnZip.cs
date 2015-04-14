
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
using Dev2.Activities;
using Dev2.Activities.PathOperations;
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
    public class DsfUnZip : DsfAbstractMultipleFilesActivity, IUnZip, IPathOverwrite, IPathOutput, IPathInput,
                            IDestinationUsernamePassword
    {
        public DsfUnZip()
            : base("Unzip")
        {
            ArchivePassword = string.Empty;
        }

        WarewolfIterator _archPassItr;

        #region Properties
        /// <summary>
        /// Gets or sets the archive password.
        /// </summary>      
        [Inputs("Archive Password")]
        [FindMissing]
        public string ArchivePassword { get; set; }
        #endregion Properties

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach(Tuple<string, string> t in updates)
            {
                if(t.Item1 == ArchivePassword)
                {
                    ArchivePassword = t.Item2;
                }

                if(t.Item1 == Overwrite.ToString())
                {
                    bool tmpOverwrite;
                    bool.TryParse(t.Item2, out tmpOverwrite);
                    Overwrite = tmpOverwrite;
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

        protected override void AddItemsToIterator(IExecutionEnvironment environment)
        {
            _archPassItr = new WarewolfIterator(environment.Eval(ArchivePassword));
            ColItr.AddVariableToIterateOn(_archPassItr);
        }

        protected override void AddDebugInputItems(IExecutionEnvironment environment)
        {
            AddDebugInputItemPassword("Archive Password", ArchivePassword);
        }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint)
        {
            Dev2UnZipOperationTO zipTo =
                       ActivityIOFactory.CreateUnzipTO(ColItr.FetchNextValue(_archPassItr),
                                                       Overwrite);
            return broker.UnZip(scrEndPoint, dstEndPoint, zipTo);
        }

        protected override void MoveRemainingIterators()
        {
            ColItr.FetchNextValue(_archPassItr);
        }

        #region GetForEachInputs/Outputs
        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(ArchivePassword, Overwrite.ToString(), OutputPath, InputPath);
        }
        #endregion
    }
}
