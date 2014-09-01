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
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public ICollection<string> CompilerErrors { get; set; }

        public enDynamicServiceObjectType ObjectType { get; set; }

        public enApprovalState ApprovalState { get; set; }

        public StringBuilder ResourceDefinition { get; set; }

        private int _versionNo = 1;
        public int VersionNo
        {
            get
            {
                return _versionNo;
            }
            set
            {
                _versionNo = value;
            }
        }

        public string IconPath { get; set; }
        public string Comment { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public string OutputSpecification { get; set; }
        public string DataListSpecification { get; set; }
        public string HelpLink { get; set; }

        public bool IsCompiled
        {
            get
            {

                if(CompilerErrors == null)
                {
                    return false;
                }

                return CompilerErrors.Count <= 0;
            }
        }
        #endregion

        #region Public Methods
        public bool IsUserInRole(string userRoles, string resourceRoles)
        {

            if(string.IsNullOrEmpty(userRoles))
            {
                return false;
            }

            if(string.IsNullOrEmpty(resourceRoles))
            {
                return true;
            }

            string[] user = userRoles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string[] res = resourceRoles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            if(user.Contains("Domain Admins"))
            {
                return true;
            }

            if(!user.Any())
            {
                return false;
            }

            if(!res.Any())
            {
                return false;
            }

            return user.Intersect(res).Any();
        }

        public virtual bool Compile()
        {
            if(string.IsNullOrEmpty(Name))
            {
                string objectName = GetType().Name;
                _errorMsg = string.Format(Resources.CompilerError_MissingName, objectName, objectName);
                WriteCompileError(_errorMsg);
            }

            return IsCompiled;
        }

        public virtual void WriteCompileError(string traceMsg)
        {
            string objectName = GetType().Name;

            if(string.IsNullOrEmpty(Name))
            {

                traceMsg = string.Format(traceMsg, objectName, objectName);
            }
            else
            {
                traceMsg = string.Format(traceMsg, objectName, Name);
            }
            CompilerErrors.Add(traceMsg);

            WriteOutput(traceMsg);

        }

        public virtual void WriteCompileWarning(string traceMsg)
        {
            WriteOutput(traceMsg);
        }

        public virtual string GetCompilerErrors()
        {
            StringBuilder result = new StringBuilder();

            if(CompilerErrors.Count > 0)
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
                    Enum.GetName(typeof(enDynamicServiceObjectType), ObjectType), Name));
            }
            return result.ToString();
        }

        #endregion

        #region Private Methods
        private void WriteOutput(string traceMsg)
        {
            ServerLogger.LogMessage(traceMsg);
            Console.WriteLine(traceMsg);
        }
        #endregion
    }
}
