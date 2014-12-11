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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;

namespace Dev2.DynamicServices.Objects.Base
{
    public abstract class DynamicServiceObjectBase : IDynamicServiceObject
    {
        #region Private Fields

        private string _errorMsg = string.Empty;

        #endregion

        #region Constructors

        protected DynamicServiceObjectBase()
        {
            CompilerErrors = new List<string>();
        }

        protected DynamicServiceObjectBase(enDynamicServiceObjectType objectType)
            : this()
        {
            ObjectType = objectType;
        }

        #endregion

        #region Public Properties

        private int _versionNo = 1;
        public string DisplayName { get; set; }
        public enApprovalState ApprovalState { get; set; }
        public string IconPath { get; set; }
        public string Tags { get; set; }
        public string OutputSpecification { get; set; }
        public StringBuilder DataListSpecification { get; set; }
        public string Name { get; set; }

        public ICollection<string> CompilerErrors { get; set; }

        public enDynamicServiceObjectType ObjectType { get; set; }

        public StringBuilder ResourceDefinition { get; set; }

        public int VersionNo
        {
            get { return _versionNo; }
            set { _versionNo = value; }
        }

        public string Comment { get; set; }
        public string Category { get; set; }
        public string HelpLink { get; set; }

        public bool IsCompiled
        {
            get
            {
                if (CompilerErrors == null)
                {
                    return false;
                }

                return CompilerErrors.Count <= 0;
            }
        }

        #endregion

        #region Public Methods

        public virtual bool Compile()
        {
            if (string.IsNullOrEmpty(Name))
            {
                string objectName = GetType().Name;
                _errorMsg = string.Format(Resources.Languages.Services.CompilerError_MissingName, objectName, objectName);
                WriteCompileError(_errorMsg);
            }

            return IsCompiled;
        }

        public virtual void WriteCompileError(string traceMsg)
        {
            string objectName = GetType().Name;

            traceMsg = string.Format(traceMsg, objectName, string.IsNullOrEmpty(Name) ? objectName : Name);
            CompilerErrors.Add(traceMsg);

            WriteOutput(traceMsg);
        }

        public virtual void WriteCompileWarning(string traceMsg)
        {
            WriteOutput(traceMsg);
        }

        public bool IsUserInRole(string userRoles, string resourceRoles)
        {
            if (string.IsNullOrEmpty(userRoles))
            {
                return false;
            }

            if (string.IsNullOrEmpty(resourceRoles))
            {
                return true;
            }

            string[] user = userRoles.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
            string[] res = resourceRoles.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

            if (user.Contains("Domain Admins"))
            {
                return true;
            }

            if (!user.Any())
            {
                return false;
            }

            if (!res.Any())
            {
                return false;
            }

            return user.Intersect(res).Any();
        }

        public virtual string GetCompilerErrors()
        {
            var result = new StringBuilder();

            if (CompilerErrors.Count > 0)
            {
                CompilerErrors.ToList().ForEach(c =>
                {
                    string errorData = string.Format("<CompilerError>{0}</CompilerError>", c);
                    result.Append(errorData);
                });
            }
            else
            {
                result.Append(string.Format("<CompilerMessage>Build of {0} '{1}' Succeeded</CompilerMessage>",
                    Enum.GetName(typeof (enDynamicServiceObjectType), ObjectType), Name));
            }
            return result.ToString();
        }

        #endregion

        #region Private Methods

        private void WriteOutput(string traceMsg)
        {
            Dev2Logger.Log.Info(traceMsg);
            Console.WriteLine(traceMsg);
        }

        #endregion
    }
}