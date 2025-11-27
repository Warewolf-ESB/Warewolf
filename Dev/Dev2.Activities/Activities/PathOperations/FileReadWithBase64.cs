#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Common.X6;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.PathOperations;
using Dev2.Util;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Storage;


namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("FileFolder-Read", "Read File", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "File, FTP, FTPS & SFTP", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_File_Read_File")]
    public class FileReadWithBase64 : DsfAbstractFileActivity, IPathInput, IEquatable<FileReadWithBase64>
    {

        public FileReadWithBase64()
            : base("Read File")
        {
            InputPath = string.Empty;
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name = nameof(InputPath),
                    Type = StateVariable.StateType.Input,
                    Value = InputPath
                },
                new StateVariable
                {
                    Name = nameof(Result),
                    Type = StateVariable.StateType.Output,
                    Value = Result
                }
            };
        }

        protected override bool AssignEmptyOutputsToRecordSet => true;
        protected override IList<OutputTO> TryExecuteConcreteAction(IDSFDataObject context, out ErrorResultTO error, int update)
        {

            IList<OutputTO> outputs = new List<OutputTO>();

            error = new ErrorResultTO();
            var colItr = new WarewolfListIterator();

            //get all the possible paths for all the string variables
            var inputItr = new WarewolfIterator(context.Environment.Eval(InputPath, update));
            colItr.AddVariableToIterateOn(inputItr);
            
            var userItr = new WarewolfIterator(context.Environment.Eval(Username, update));
            colItr.AddVariableToIterateOn(userItr);

            var passItr = new WarewolfIterator(context.Environment.Eval(DecryptedPassword, update));
            colItr.AddVariableToIterateOn(passItr);

            var privateKeyItr = new WarewolfIterator(context.Environment.Eval(PrivateKeyFile ?? string.Empty, update));
            colItr.AddVariableToIterateOn(privateKeyItr);

            outputs.Add(DataListFactory.CreateOutputTO(Result));

            if (context.IsDebugMode())
            {
                AddDebugInputItem(InputPath, "Input Path", context.Environment, update);
                AddDebugInputItemUserNamePassword(context.Environment, update);
                if (!string.IsNullOrEmpty(PrivateKeyFile))
                {
                    AddDebugInputItem(PrivateKeyFile, "Private Key File", context.Environment, update);
                }
                if (IsResultBase64)
                {
                    AddDebugInputItem(IsResultBase64.ToString(), "Result As Base64", context.Environment, update);
                }
            }

            while (colItr.HasMoreData())
            {
                var broker = ActivityIOFactory.CreateOperationsBroker();
                var ioPath = ActivityIOFactory.CreatePathFromString(colItr.FetchNextValue(inputItr),
                                                                                colItr.FetchNextValue(userItr),
                                                                                colItr.FetchNextValue(passItr),
                                                                                true, colItr.FetchNextValue(privateKeyItr));
                var endpoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ioPath);
                try
                {
                    if (IsResultBase64)
                    {
                        var result = broker.GetBytes(endpoint);
                        outputs[0].OutputStrings.Add(result.ToBase64String());
                    }
                    else
                    {
                        var result = broker.Get(endpoint);
                        outputs[0].OutputStrings.Add(result);
                    }
                }
                catch (Exception e)
                {
                    outputs[0].OutputStrings.Add(null);
                    error.AddError(e.Message);
                    break;
                }

            }

            return outputs;
        }

        #region Properties

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

        /// <summary>
        /// Gets or sets the is result base64.
        /// </summary>
        [Inputs("Is Result Base64")]
        [FindMissing]
        public bool IsResultBase64
        {
            get;
            set;
        }

        #endregion Properties

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


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(InputPath);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        #endregion

        public bool Equals(FileReadWithBase64 other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && string.Equals(InputPath, other.InputPath);
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

            return Equals((FileReadWithBase64)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (InputPath != null ? InputPath.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Serializes the File Read With Base64 activity to X6 JSON format
        /// </summary>
        /// <param name="cell">The X6 cell to populate</param>
        public override void ToX6Json(Cell cell)
        {
            base.ToX6Json(cell);

            cell.data[Constants.TYPE] = Constants.FILEREADWITHBASE64;
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_FILEREAD;
            cell.data[Constants.UNIQUEID] = UniqueID;
            cell.shape = Constants.RECT;

            // Add File Read With Base64 specific data
            cell.data["InputPath"] = InputPath ?? string.Empty;
            cell.data["Username"] = Username ?? string.Empty;
            cell.data["Password"] = Password ?? string.Empty;
            cell.data["PrivateKeyFile"] = PrivateKeyFile ?? string.Empty;
            cell.data["Result"] = Result ?? string.Empty;
            cell.data["IsResultBase64"] = IsResultBase64;
        }

        /// <summary>
        /// Deserializes the File Read With Base64 activity from X6 JSON format
        /// </summary>
        /// <param name="cell">The X6 cell containing File Read With Base64 data</param>
        public override void FromX6Json(Cell cell)
        {
            if (cell == null || cell.data == null) return;

            // Call base implementation for common properties (OnError handling, etc.)
            base.FromX6Json(cell);

            try
            {
                // Deserialize DisplayName
                if (cell.data.TryGetValue(Constants.DISPLAYNAME, out var displayNameObj) && displayNameObj is string displayName)
                {
                    DisplayName = displayName;
                }

                // Deserialize UniqueID
                if (cell.data.TryGetValue(Constants.UNIQUEID, out var uniqueIdObj) && uniqueIdObj is string uniqueId)
                {
                    UniqueID = uniqueId;
                }

                if (cell.data.TryGetValue(Constants.PROPERTIES, out var propertiesObj))
                {
                    Dictionary<string, object> properties = null;

                    if (propertiesObj is Dictionary<string, object> dict)
                    {
                        properties = dict;
                    }
                    else
                    {
                        var propertiesJson = propertiesObj?.ToString();
                        if (!string.IsNullOrEmpty(propertiesJson))
                        {
                            try
                            {
                                properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(propertiesJson);
                            }
                            catch (JsonException)
                            {
                                // Ignore and use direct properties
                            }
                        }
                    }

                    if (properties != null)
                    {
                        // Only use properties from the nested object if not already set from direct access
                        if (string.IsNullOrEmpty(InputPath) && properties.TryGetValue("InputPath", out var inputPathProp))
                            InputPath = inputPathProp?.ToString() ?? string.Empty;

                        if (string.IsNullOrEmpty(Username) && properties.TryGetValue("Username", out var usernameProp))
                            Username = usernameProp?.ToString() ?? string.Empty;

                        if (string.IsNullOrEmpty(Password) && properties.TryGetValue("Password", out var passwordProp))
                            Password = passwordProp?.ToString() ?? string.Empty;

                        if (string.IsNullOrEmpty(PrivateKeyFile) && properties.TryGetValue("PrivateKeyFile", out var privateKeyProp))
                            PrivateKeyFile = privateKeyProp?.ToString() ?? string.Empty;

                        if (string.IsNullOrEmpty(Result) && properties.TryGetValue("Result", out var resultProp))
                            Result = resultProp?.ToString() ?? string.Empty;

                        if (properties.TryGetValue("IsResultBase64", out var isResultBase64Prop))
                        {
                            if (isResultBase64Prop is bool boolValue)
                            {
                                IsResultBase64 = boolValue;
                            }
                            else if (bool.TryParse(isResultBase64Prop?.ToString(), out bool parsedValue))
                            {
                                IsResultBase64 = parsedValue;
                            }
                        }
                    }
                }

                // Also try to get properties directly from data (not nested in properties object)
                if (string.IsNullOrEmpty(InputPath) && cell.data.TryGetValue("InputPath", out var directInputPath))
                    InputPath = directInputPath?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(Username) && cell.data.TryGetValue("Username", out var directUsername))
                    Username = directUsername?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(Password) && cell.data.TryGetValue("Password", out var directPassword))
                    Password = directPassword?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(PrivateKeyFile) && cell.data.TryGetValue("PrivateKeyFile", out var directPrivateKey))
                    PrivateKeyFile = directPrivateKey?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(Result) && cell.data.TryGetValue("Result", out var directResult))
                    Result = directResult?.ToString() ?? string.Empty;

                // Deserialize IsResultBase64 from direct properties
                if (cell.data.TryGetValue("IsResultBase64", out var directIsResultBase64))
                {
                    if (directIsResultBase64 is bool boolValue)
                    {
                        IsResultBase64 = boolValue;
                    }
                    else if (bool.TryParse(directIsResultBase64?.ToString(), out bool parsedValue))
                    {
                        IsResultBase64 = parsedValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"Error deserializing File Read With Base64 data from X6 JSON: {ex.Message}", ex, GlobalConstants.WarewolfError);
            }
        }
    }
}
