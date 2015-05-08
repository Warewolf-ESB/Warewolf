
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
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.ExtMethods;
using Dev2.Data;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
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
    /// Purpose : To provide an activity that can read a folder's contents via FTP, FTPS and file system
    /// </summary>
    public class DsfFolderRead : DsfAbstractFileActivity, IPathInput
    {

        public DsfFolderRead()
            : base("Read Folder")
        {
            InputPath = string.Empty;
        }

        protected override IList<OutputTO> ExecuteConcreteAction(NativeActivityContext context, out ErrorResultTO allErrors)
        {
            IsNotCertVerifiable = true;

            allErrors = new ErrorResultTO();
            IList<OutputTO> outputs = new List<OutputTO>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            var colItr = new WarewolfListIterator();

            //get all the possible paths for all the string variables
            var inputItr = new WarewolfIterator(dataObject.Environment.Eval(InputPath));
            colItr.AddVariableToIterateOn(inputItr);

            var unameItr = new WarewolfIterator(dataObject.Environment.Eval(Username));
            colItr.AddVariableToIterateOn(unameItr);

            var passItr = new WarewolfIterator(dataObject.Environment.Eval(Password));
            colItr.AddVariableToIterateOn(passItr);

            if(dataObject.IsDebugMode())
            {
                AddDebugInputItem(InputPath, "Input Path", dataObject.Environment);
                AddDebugInputItem(new DebugItemStaticDataParams(GetReadType().GetDescription(), "Read"));
                AddDebugInputItemUserNamePassword(dataObject.Environment);
            }

            while(colItr.HasMoreData())
            {


                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
                IActivityIOPath ioPath = ActivityIOFactory.CreatePathFromString(colItr.FetchNextValue(inputItr),
                                                                                colItr.FetchNextValue(unameItr),
                                                                                colItr.FetchNextValue(passItr),
                                                                                true);
                IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ioPath);

                try
                {
                    IList<IActivityIOPath> listOfDir = broker.ListDirectory(endPoint, GetReadType());
                    if(DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) != enRecordsetIndexType.Numeric)
                    {
                        if(DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
                        {
                            string recsetName = DataListUtil.ExtractRecordsetNameFromValue(Result);
                            string fieldName = DataListUtil.ExtractFieldNameFromValue(Result);

                            int indexToUpsertTo = 1;
                            if(listOfDir != null)
                            {
                                foreach(IActivityIOPath pa in listOfDir)
                                {
                                    string fullRecsetName = DataListUtil.CreateRecordsetDisplayValue(recsetName, fieldName,
                                        indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                                    outputs.Add(DataListFactory.CreateOutputTO(DataListUtil.AddBracketsToValueIfNotExist(fullRecsetName), pa.Path));
                                    indexToUpsertTo++;
                                }
                            }
                        }
                        else if(DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Blank)
                        {
                            if(listOfDir != null)
                            {
                                foreach(IActivityIOPath pa in listOfDir)
                                {
                                    outputs.Add(DataListFactory.CreateOutputTO(Result, pa.Path));
                                }
                            }
                        }
                    }
                    else
                    {
                        if(listOfDir != null)
                        {
                            string xmlList = string.Join(",", listOfDir.Select(c => c.Path));
                            outputs.Add(DataListFactory.CreateOutputTO(Result));
                            outputs.Last().OutputStrings.Add(xmlList);
                        }
                    }
                }
                catch(Exception e)
                {
                    outputs.Add(DataListFactory.CreateOutputTO(null));
                    allErrors.AddError(e.Message);
                    break;
                }
            }

            return outputs;

        }

        #region Properties

        /// <summary>
        /// Gets or sets the files option.
        /// </summary>
        [Inputs("Files")]
        [FindMissing]
        public bool IsFilesSelected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the folders otion.
        /// </summary>
        [Inputs("Folders")]
        [FindMissing]
        public bool IsFoldersSelected
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the files and folders option.
        /// </summary>
        [Inputs("Files & Folders")]
        [FindMissing]
        public bool IsFilesAndFoldersSelected
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Inputs("Input Path")]
        [FindMissing]
        public string InputPath
        {
            get;
            set;
        }


        #endregion Properties

        #region Private Methods

        private ReadTypes GetReadType()
        {
            if(IsFoldersSelected)
            {
                return ReadTypes.Folders;
            }

            if(IsFilesSelected)
            {
                return ReadTypes.Files;
            }
            return ReadTypes.FilesAndFolders;
        }

        #endregion

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null && updates.Count == 1)
            {
                InputPath = updates[0].Item2;
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
            return GetForEachItems(InputPath);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
