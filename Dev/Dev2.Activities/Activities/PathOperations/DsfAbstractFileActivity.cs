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
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.PathOperations;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Security.Encryption;
using Warewolf.Storage.Interfaces;


namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
	public abstract class DsfAbstractFileActivity : DsfActivityAbstract<string>, IPathAuth, IResult, IPathCertVerify, IEquatable<DsfAbstractFileActivity>
	{

		string _username;
		string _password;

		protected DsfAbstractFileActivity(string displayName)
			: base(displayName)
		{
			Username = string.Empty;
			Password = string.Empty;
			Result = string.Empty;
			PrivateKeyFile = string.Empty;
		}

		protected override void OnExecute(NativeActivityContext context)
		{
			var dataObject = context.GetExtension<IDSFDataObject>();
			ExecuteTool(dataObject, 0);
		}

		protected override void ExecuteTool(IDSFDataObject dataObject, int update)
		{
			var allErrors = new ErrorResultTO();

			// Process if no errors

			if (dataObject.IsDebugMode())
			{
				InitializeDebug(dataObject);
			}
            
			try
            {
                TryExecuteTool(dataObject, update, allErrors);
            }
            catch (Exception ex)
			{
				allErrors.AddError(ex.Message);
			}
			finally
			{
				// Handle Errors
				if (allErrors.HasErrors())
				{
					foreach (var err in allErrors.FetchErrors())
					{
						dataObject.Environment.Errors.Add(err);
					}
					foreach (var region in DataListCleaningUtils.SplitIntoRegions(Result))
					{
						dataObject.Environment.Assign(region, "", update);
					}
				}

				if (dataObject.IsDebugMode())
				{
					DispatchDebugState(dataObject, StateType.Before, update);
					DispatchDebugState(dataObject, StateType.After, update);
				}
			}
		}

        private ErrorResultTO TryExecuteTool(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            ErrorResultTO errors;
            //Execute the concrete action for the specified activity
            var outputs = TryExecuteConcreteAction(dataObject, out errors, update);

            allErrors.MergeErrors(errors);

            if (outputs.Count > 0)
            {
                foreach (OutputTO output in outputs)
                {
                    if (output.OutputStrings.Count > 0)
                    {
                        ParseOutputs(dataObject, update, errors, output);
                    }
                }
                allErrors.MergeErrors(errors);
            }
            else
            {
                if (AssignEmptyOutputsToRecordSet)
                {
                    foreach (var region in DataListCleaningUtils.SplitIntoRegions(Result))
                    {
                        dataObject.Environment.Assign(region, "", update);
                    }
                }
            }
            if (dataObject.IsDebugMode() && !String.IsNullOrEmpty(Result))
            {
                AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
            }

            return errors;
        }

        private static void ParseOutputs(IDSFDataObject dataObject, int update, ErrorResultTO errors, OutputTO output)
        {
            foreach (string value in output.OutputStrings)
            {
                if (output.OutPutDescription == GlobalConstants.ErrorPayload)
                {
                    errors.AddError(value);
                }
                else
                {
                    foreach (var region in DataListCleaningUtils.SplitIntoRegions(output.OutPutDescription))
                    {
                        dataObject.Environment.Assign(region, value, update);
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected string DecryptedPassword => DataListUtil.NotEncrypted(Password) ? Password : DpapiWrapper.Decrypt(Password);
		
		protected abstract IList<OutputTO> TryExecuteConcreteAction(IDSFDataObject context, out ErrorResultTO error, int update);

		#region Properties

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		[Inputs("Password")]
		[FindMissing]
		public string Password
		{
			get { return _password; }
			set
			{
				if (DataListUtil.ShouldEncrypt(value))
				{
					try
					{
						_password = DpapiWrapper.Encrypt(value);
					}
					catch (Exception)
					{
						_password = value;
					}
				}
				else
				{
					_password = value;
				}
			}
		}

		protected abstract bool AssignEmptyOutputsToRecordSet {get;}

		/// <summary>
		/// Gets or sets the username.
		/// </summary>
		[Inputs("Username")]
		[FindMissing]
		public string Username
		{
			get { return _username; }
			set { _username = value; }
		}

		[Inputs("PrivateKeyFile")]
		[FindMissing]
		public string PrivateKeyFile { get; set; }

		/// <summary>
		/// Gets or sets the result.
		/// </summary>
		[Outputs("Result")]
		[FindMissing]
		public new string Result
		{
			get;
			set;
		}

		public override List<string> GetOutputs() => new List<string> { Result };

		/// <summary>
		/// Gets or sets a value indicating whether this instance is not cert verifiable.
		/// </summary>
		[Inputs("Is Not Certificate Verifiable")]
		public bool IsNotCertVerifiable
		{
			get;
			set;
		}


		#endregion Properties

		#region Get Debug Inputs/Outputs

		public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
		{
			foreach (IDebugItem debugInput in _debugInputs)
			{
				debugInput.FetchResultsList();
			}
			return _debugInputs;
		}

		public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
		{

			foreach (IDebugItem debugOutput in _debugOutputs)
			{
				debugOutput.FlushStringBuilder();
			}
			return _debugOutputs;
		}

		#endregion Get Inputs/Outputs

		#region Internal Methods

		internal void AddDebugInputItem(string expression, string labelText, IExecutionEnvironment environment, int update)
		{
			AddDebugInputItem(new DebugEvalResult(expression, labelText, environment, update));
		}

		#endregion

		protected void AddDebugInputItemUserNamePassword(IExecutionEnvironment environment, int update)
		{
			AddDebugInputItem(new DebugEvalResult(Username, "Username", environment, update));
			AddDebugInputItemPassword("Password", Password);
		}

		protected void AddDebugInputItemDestinationUsernamePassword(IExecutionEnvironment environment, string destinationPassword, string userName, int update)
		{
			AddDebugInputItem(new DebugEvalResult(userName, "Destination Username", environment, update));
			AddDebugInputItemPassword("Destination Password", destinationPassword);
		}

		protected void AddDebugInputItemPassword(string label, string password)
		{
			AddDebugInputItem(new DebugItemStaticDataParams(GetBlankedOutPassword(password), label));
		}

		static string GetBlankedOutPassword(string password) => "".PadRight((password ?? "").Length, '*');

		public bool Equals(DsfAbstractFileActivity other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			var passWordsCompare = CommonEqualityOps.PassWordsCompare(Password, other.Password);
			return base.Equals(other)
				&& string.Equals(Username, other.Username)
				&& passWordsCompare
				&& string.Equals(DisplayName, other.DisplayName)
				&& string.Equals(PrivateKeyFile, other.PrivateKeyFile)
				&& string.Equals(Result, other.Result)
				&& IsNotCertVerifiable == other.IsNotCertVerifiable;
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

			return Equals((DsfAbstractFileActivity)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Username != null ? Username.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (PrivateKeyFile != null ? PrivateKeyFile.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ IsNotCertVerifiable.GetHashCode();
				return hashCode;
			}
		}
	}
}
