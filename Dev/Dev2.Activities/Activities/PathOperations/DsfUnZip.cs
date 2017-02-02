/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Activities.PathOperations;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.PathOperations;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    [ToolDescriptorInfo("FileFolder-UnZip", "UnZip", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "File, FTP, FTPS & SFTP", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_File_Unzip")]
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

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
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

        protected override void AddItemsToIterator(IExecutionEnvironment environment, int update)
        {
            _archPassItr = new WarewolfIterator(environment.Eval(ArchivePassword, update));
            ColItr.AddVariableToIterateOn(_archPassItr);
        }

        protected override void AddDebugInputItems(IExecutionEnvironment environment, int update)
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
