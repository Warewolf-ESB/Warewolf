#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.PathOperations;
using Dev2.Util;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Security.Encryption;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.PathOperations
{
    public abstract class DsfAbstractMultipleFilesActivity : DsfAbstractFileActivity,IEquatable<DsfAbstractMultipleFilesActivity>
    {
        protected DsfAbstractMultipleFilesActivity(string displayName)
            : base(displayName)
        {
            InputPath = string.Empty;
            OutputPath = string.Empty;
            Overwrite = false;
            DestinationPassword = string.Empty;
            DestinationUsername = string.Empty;
            DestinationPrivateKeyFile = string.Empty;
        }

        protected IWarewolfListIterator ColItr;

        protected override IList<OutputTO> TryExecuteConcreteAction(IDSFDataObject context, out ErrorResultTO error, int update)
        {
            IList<OutputTO> outputs = new List<OutputTO>();
  
            error = new ErrorResultTO();
            ColItr = new WarewolfListIterator();

            //get all the possible paths for all the string variables          
            var inputItr = new WarewolfIterator(context.Environment.Eval(InputPath, update));
            ColItr.AddVariableToIterateOn(inputItr);

           
            var outputItr = new WarewolfIterator(context.Environment.Eval(OutputPath, update));
            ColItr.AddVariableToIterateOn(outputItr);
            
            var passItr = new WarewolfIterator(context.Environment.Eval(DecryptedPassword,update));
            ColItr.AddVariableToIterateOn(passItr);

            var privateKeyItr = new WarewolfIterator(context.Environment.Eval(PrivateKeyFile, update));
            ColItr.AddVariableToIterateOn(privateKeyItr);
            
            var despassItr = new WarewolfIterator(context.Environment.Eval(DecryptedDestinationPassword,update));
            ColItr.AddVariableToIterateOn(despassItr);

            var destPrivateKeyItr = new WarewolfIterator(context.Environment.Eval(DestinationPrivateKeyFile, update));
            ColItr.AddVariableToIterateOn(destPrivateKeyItr);

            AddItemsToIterator(context.Environment, update);

            outputs.Add(DataListFactory.CreateOutputTO(Result));

            if(context.IsDebugMode())
            {
                AddDebugInputItem(new DebugEvalResult(InputPath, "Source Path", context.Environment, update));
                AddDebugInputItemUserNamePassword(context.Environment, update);
                if(!string.IsNullOrEmpty(PrivateKeyFile))
                {
                    AddDebugInputItem(new DebugEvalResult(PrivateKeyFile, "Source Private Key File", context.Environment, update));
                }
                AddDebugInputItem(new DebugEvalResult(OutputPath, "Destination Path", context.Environment, update));
                AddDebugInputItemDestinationUsernamePassword(context.Environment, DestinationPassword, DestinationUsername, update);
                if(!string.IsNullOrEmpty(DestinationPrivateKeyFile))
                {
                    AddDebugInputItem(new DebugEvalResult(DestinationPrivateKeyFile, "Destination Private Key File", context.Environment, update));
                }
                AddDebugInputItem(new DebugItemStaticDataParams(Overwrite.ToString(), "Overwrite"));
                AddDebugInputItems(context.Environment, update);
            }

            while(ColItr.HasMoreData())
            {
                var hasError = false;
                IActivityIOPath src = null;
                IActivityIOPath dst = null;
                try
                {
                    src = ActivityIOFactory.CreatePathFromString(ColItr.FetchNextValue(inputItr),
                                                                                 Username,
                                                                                 ColItr.FetchNextValue(passItr),
                                                                                 true, ColItr.FetchNextValue(privateKeyItr));

                }
                catch(IOException ioException)
                {
                    error.AddError("Source: " + ioException.Message);
                    hasError = true;
                }
                try
                {
                    dst = ActivityIOFactory.CreatePathFromString(ColItr.FetchNextValue(outputItr),
                                                                                     DestinationUsername,
                                                                                     ColItr.FetchNextValue(despassItr),
                                                                                     true, ColItr.FetchNextValue(destPrivateKeyItr));
                }
                catch(IOException ioException)
                {
                    error.AddError("Destination:" + ioException.Message);
                    hasError = true;
                }

                if(hasError)
                {
                    outputs[0].OutputStrings.Add(null);
                    MoveRemainingIterators();
                    continue;
                }
                var scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
                var dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

                try
                {
                    var broker = GetOperationBroker?.Invoke();
                    var result = ExecuteBroker(broker, scrEndPoint, dstEndPoint);
                    outputs[0].OutputStrings.Add(result);

                }
                catch(Exception e)
                {
                    error.AddError(e.Message);
                    outputs[0].OutputStrings.Add(null);
                }
            }

            return outputs;

        }

        protected virtual void AddItemsToIterator(IExecutionEnvironment environment, int update)
        {
        }

        protected virtual void AddDebugInputItems(IExecutionEnvironment environment, int update)
        {
        }

        protected abstract string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint);
        protected abstract void MoveRemainingIterators();

        [JsonIgnore]
        internal Func<IActivityOperationsBroker> GetOperationBroker = () => ActivityIOFactory.CreateOperationsBroker();
        string _destPassword;

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this is overwrite.
        /// </summary>
        [Inputs("Overwrite")]
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Inputs("Input Path")]
        [FindMissing]
        public string InputPath { get; set; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        [Inputs("Output Path")]
        [FindMissing]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the destination file/folder user name
        /// </summary>
        [Inputs("Destination Username"), FindMissing]
        public string DestinationUsername { get; set; }

        [Inputs("Destination Private Public Key File"), FindMissing]
        public string DestinationPrivateKeyFile { get; set; }

        /// <summary>
        /// Gets or sets the destination file/folder password
        /// </summary>
        [Inputs("Destination Password"), FindMissing]
        public string DestinationPassword {
            get { return _destPassword; }
            set
            {
                if (DataListUtil.ShouldEncrypt(value))
                {
                    try
                    {
                        _destPassword = DpapiWrapper.Encrypt(value);
                    }
                    catch (Exception)
                    {
                        _destPassword = value;
                    }
                }
                else
                {
                    _destPassword = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        
        protected string DecryptedDestinationPassword => DataListUtil.NotEncrypted(DestinationPassword) ? DestinationPassword : DpapiWrapper.Decrypt(DestinationPassword);

        #endregion Properties

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {

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
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if(itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(InputPath, OutputPath);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        #endregion

        public bool Equals(DsfAbstractMultipleFilesActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && string.Equals(DestinationPassword, other.DestinationPassword) && Overwrite == other.Overwrite && string.Equals(InputPath, other.InputPath) && string.Equals(OutputPath, other.OutputPath) && string.Equals(DestinationUsername, other.DestinationUsername) && string.Equals(DestinationPrivateKeyFile, other.DestinationPrivateKeyFile);
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

            return Equals((DsfAbstractMultipleFilesActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (DestinationPassword != null ? DestinationPassword.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Overwrite.GetHashCode();
                hashCode = (hashCode * 397) ^ (InputPath != null ? InputPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutputPath != null ? OutputPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DestinationUsername != null ? DestinationUsername.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DestinationPrivateKeyFile != null ? DestinationPrivateKeyFile.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
