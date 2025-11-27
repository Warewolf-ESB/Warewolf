#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
    public class DsfFileRead : DsfAbstractFileActivity, IPathInput,IEquatable<DsfFileRead>
    {

        public DsfFileRead()
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
            }

            while (colItr.HasMoreData())
            {
                var broker = ActivityIOFactory.CreateOperationsBroker();
                var ioPath = ActivityIOFactory.CreatePathFromString(colItr.FetchNextValue(inputItr),
                                                                                Username,
                                                                                colItr.FetchNextValue(passItr),
                                                                                true, colItr.FetchNextValue(privateKeyItr));
                var endpoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ioPath);
                try
                {
                    var result = broker.Get(endpoint);
                    outputs[0].OutputStrings.Add(result);
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

        public bool Equals(DsfFileRead other)
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

            return Equals((DsfFileRead) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (InputPath != null ? InputPath.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Serializes the File Read activity to X6 JSON format
        /// </summary>
        /// <param name="cell">The X6 cell to populate</param>
        public override void ToX6Json(Cell cell)
        {
            base.ToX6Json(cell);

            cell.data[Constants.TYPE] = Constants.DSFFILEREAD;
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_FILEREAD;
            cell.data[Constants.UNIQUEID] = UniqueID;
            cell.shape = Constants.RECT;

            // Add File Read specific data
            cell.data["InputPath"] = InputPath ?? string.Empty;
            cell.data["Username"] = Username ?? string.Empty;
            cell.data["Password"] = Password ?? string.Empty;
            cell.data["PrivateKeyFile"] = PrivateKeyFile ?? string.Empty;
            cell.data["Result"] = Result ?? string.Empty;
        }

        /// <summary>
        /// Deserializes the File Read activity from X6 JSON format
        /// </summary>
        /// <param name="cell">The X6 cell containing File Read data</param>
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
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"Error deserializing File Read data from X6 JSON: {ex.Message}", ex, GlobalConstants.WarewolfError);
            }
        }
    }
}
