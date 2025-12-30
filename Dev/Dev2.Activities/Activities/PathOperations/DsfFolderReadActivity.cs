#pragma warning disable
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.PathOperations;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Storage;
using Dev2.Common.X6;
using Dev2.WorkflowConverters;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("FileFolder-ReadFolder", "Read Folder", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "File, FTP, FTPS & SFTP", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_File_Read_Folder")]
    public class DsfFolderReadActivity : DsfAbstractFileActivity, IPathInput, IEquatable<DsfFolderReadActivity>
    {
        public DsfFolderReadActivity()
                : base("Folder Read")
        {
            InputPath = string.Empty;
        }

        protected override IList<OutputTO> TryExecuteConcreteAction(IDSFDataObject context, out ErrorResultTO error, int update)
        {
            IsNotCertVerifiable = true;

            error = new ErrorResultTO();
            IList<OutputTO> outputs = new List<OutputTO>();

            using (var colItr = new WarewolfListIterator())
            {
                //get all the possible paths for all the string variables
                var inputItr = new WarewolfIterator(context.Environment.Eval(InputPath, update));
                colItr.AddVariableToIterateOn(inputItr);

                var unameItr = new WarewolfIterator(context.Environment.Eval(Username, update));
                colItr.AddVariableToIterateOn(unameItr);

                var passItr = new WarewolfIterator(context.Environment.Eval(DecryptedPassword, update));
                colItr.AddVariableToIterateOn(passItr);

                var privateKeyItr = new WarewolfIterator(context.Environment.Eval(PrivateKeyFile, update));
                colItr.AddVariableToIterateOn(privateKeyItr);

                if (context.IsDebugMode())
                {
                    AddDebugInputItem(InputPath, "Input Path", context.Environment, update);
                    AddDebugInputItem(new DebugItemStaticDataParams(GetReadType().GetDescription(), "Read"));
                    AddDebugInputItemUserNamePassword(context.Environment, update);
                    if (!string.IsNullOrEmpty(PrivateKeyFile))
                    {
                        AddDebugInputItem(PrivateKeyFile, "Private Key File", context.Environment, update);
                    }
                }

                while (colItr.HasMoreData())
                {
                    var broker = ActivityIOFactory.CreateOperationsBroker();
                    var ioPath = ActivityIOFactory.CreatePathFromString(colItr.FetchNextValue(inputItr),
                                                                                    colItr.FetchNextValue(unameItr),
                                                                                    colItr.FetchNextValue(passItr),
                                                                                    true, colItr.FetchNextValue(privateKeyItr));
                    var endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ioPath);

                    try
                    {
                        ExecuteConcreteAction(outputs, broker, endPoint);
                    }
                    catch (Exception e)
                    {
                        outputs.Add(DataListFactory.CreateOutputTO(null));
                        error.AddError(e.Message);
                        break;
                    }
                }

                return outputs;
            }
        }

        void ExecuteConcreteAction(IList<OutputTO> outputs, IActivityOperationsBroker broker, IActivityIOOperationsEndPoint endPoint)
        {
            var listOfDir = broker.ListDirectory(endPoint, GetReadType());
            if (DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) != enRecordsetIndexType.Numeric)
            {
                if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
                {
                    ExecuteForListOfDir(listOfDir, outputs);
                }
                else
                {
                    AddOutputsForListOfDir(listOfDir, outputs);
                }
            }
            else
            {
                if (listOfDir != null)
                {
                    var xmlList = string.Join(",", listOfDir.Select(c => c.Path));
                    outputs.Add(DataListFactory.CreateOutputTO(Result));
                    outputs.Last().OutputStrings.Add(xmlList);
                }
            }
        }

        void AddOutputsForListOfDir(IList<IActivityIOPath> listOfDir, IList<OutputTO> outputs)
        {
            if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Blank && listOfDir != null)
            {
                foreach (IActivityIOPath pa in listOfDir)
                {
                    outputs.Add(DataListFactory.CreateOutputTO(Result, pa.Path));
                }
            }
        }

        void ExecuteForListOfDir(IList<IActivityIOPath> listOfDir, IList<OutputTO> outputs)
        {
            var recsetName = DataListUtil.ExtractRecordsetNameFromValue(Result);
            var fieldName = DataListUtil.ExtractFieldNameFromValue(Result);

            var indexToUpsertTo = 1;
            if (listOfDir != null)
            {
                foreach (IActivityIOPath pa in listOfDir)
                {
                    var fullRecsetName = DataListUtil.CreateRecordsetDisplayValue(recsetName, fieldName,
                        indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                    outputs.Add(DataListFactory.CreateOutputTO(DataListUtil.AddBracketsToValueIfNotExist(fullRecsetName), pa.Path));
                    indexToUpsertTo++;
                }
            }
        }

        protected override bool AssignEmptyOutputsToRecordSet => false;

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
        
        [Inputs("Input Path")]
        [FindMissing]
        public string InputPath
        {
            get;
            set;
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name = "InputPath",
                    Type = StateVariable.StateType.Input,
                    Value = InputPath
                },
                new StateVariable
                {
                    Name = "IsFilesSelected",
                    Type = StateVariable.StateType.Input,
                    Value = IsFilesSelected.ToString()
                },
                new StateVariable
                {
                    Name = "IsFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = IsFoldersSelected.ToString()
                },
                new StateVariable
                {
                    Name = "IsFilesAndFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = IsFilesAndFoldersSelected.ToString()
                },
                new StateVariable
                {
                    Name = "Username",
                    Type = StateVariable.StateType.Input,
                    Value = Username
                },
                new StateVariable
                {
                    Name = "PrivateKeyFile",
                    Type = StateVariable.StateType.Input,
                    Value = PrivateKeyFile
                },
                new StateVariable
                {
                    Name="Result",
                    Type=StateVariable.StateType.Output,
                    Value= Result
                }
            };
        }

        ReadTypes GetReadType()
        {
            if (IsFoldersSelected)
            {
                return ReadTypes.Folders;
            }

            if (IsFilesSelected)
            {
                return ReadTypes.Files;
            }
            return ReadTypes.FilesAndFolders;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if (updates != null && updates.Count == 1)
            {
                InputPath = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if (itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(InputPath);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        public bool Equals(DsfFolderReadActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other)
                && IsFilesSelected == other.IsFilesSelected
                && IsFoldersSelected == other.IsFoldersSelected
                && IsFilesAndFoldersSelected == other.IsFilesAndFoldersSelected
                && string.Equals(InputPath, other.InputPath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfFolderReadActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ IsFilesSelected.GetHashCode();
                hashCode = (hashCode * 397) ^ IsFoldersSelected.GetHashCode();
                hashCode = (hashCode * 397) ^ IsFilesAndFoldersSelected.GetHashCode();
                hashCode = (hashCode * 397) ^ (InputPath != null ? InputPath.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new Dictionary<string, object>();

            base.ToX6Json(cell);

            cell.shape = Constants.DSFFOLDERREADACTIVITY;
            cell.data[Constants.TYPE] = Constants.DSFFOLDERREADACTIVITY.ToLower();
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_FOLDERREAD;
            cell.data[Constants.UNIQUEID] = UniqueID;

            // File operation specific properties
            cell.data.TryAdd(Constants.FOLDERREAD_INPUTPATH, InputPath);
            cell.data.TryAdd(Constants.RESULT, Result);
            
            // Read type selectors
            cell.data.TryAdd(Constants.FOLDERREAD_ISFILESSELECTED, IsFilesSelected);
            cell.data.TryAdd(Constants.FOLDERREAD_ISFOLDERSSELECTED, IsFoldersSelected);
            cell.data.TryAdd(Constants.FOLDERREAD_ISFILESANDFOLDERSSELECTED, IsFilesAndFoldersSelected);
        }

        public override void FromX6Json(Cell cell)
        {
            if (cell == null || cell.data == null) return;

            base.FromX6Json(cell);

            if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) 
                DisplayName = displayName;
            if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId)) 
                UniqueID = uniqueId;

            // File operation specific properties
            if (cell.data.TryGetString(Constants.FOLDERREAD_INPUTPATH, out var inputPath)) 
                InputPath = inputPath;
            if (cell.data.TryGetString(Constants.RESULT, out var result)) 
                Result = result;

            // Read type selectors
            if (cell.data.TryGetBool(Constants.FOLDERREAD_ISFILESSELECTED, out var isFilesSelected)) 
                IsFilesSelected = isFilesSelected;
            if (cell.data.TryGetBool(Constants.FOLDERREAD_ISFOLDERSSELECTED, out var isFoldersSelected)) 
                IsFoldersSelected = isFoldersSelected;
            if (cell.data.TryGetBool(Constants.FOLDERREAD_ISFILESANDFOLDERSSELECTED, out var isFilesAndFoldersSelected)) 
                IsFilesAndFoldersSelected = isFilesAndFoldersSelected;

            // Defensive initialization
            InputPath ??= string.Empty;
        }
    }
}