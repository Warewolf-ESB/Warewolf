using System;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Framework;

namespace Dev2.DynamicServices {
    public abstract class DynamicServiceObjectBase : IDynamicServiceObject {

        #region Private Fields
        private string _errorMsg = string.Empty;
        private string _warningMsg = string.Empty;
        #endregion

        #region Constructors
        public DynamicServiceObjectBase() {
            CompilerErrors = new List<string>();
        }

        public DynamicServiceObjectBase(enDynamicServiceObjectType objectType) : this (){
            ObjectType = objectType;
            ApprovalState = enApprovalState.Pending;
        }
        #endregion


        #region Public Properties
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public ICollection<string> CompilerErrors { get; set; }

        public enDynamicServiceObjectType ObjectType { get; set; }

        public enApprovalState ApprovalState { get; set; }

        public string ResourceDefinition { get; set; }

        private int _versionNo = 1;
        public int VersionNo {
            get {
                return _versionNo;
            }
            set {
                _versionNo = value;
            }
        }

        public string AuthorRoles { get; set; }
        public string SignOffRoles { get; set; }
        public string Comment { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public string OutputSpecification { get; set;}
        public string DataListSpecification { get; set; }

        public string HelpLink { get; set; }

        public bool IsCompiled {
            get {

                if (CompilerErrors == null) {
                    return false;                    
                }

                if (CompilerErrors.Count > 0) {
                    return false;
                }

                return true;
            }
        }
        #endregion

        #region Public Methods
        public bool IsUserInRole(string userRoles, string resourceRoles) {
            if (string.IsNullOrEmpty(userRoles)) {
                return false;
            }

            if (string.IsNullOrEmpty(resourceRoles)) {
                return true;
            }

            string[] user = userRoles.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string[] res = resourceRoles.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            if (user.Contains("Domain Admins")) {
                return true;
            }

            if (!user.Any()) {
                return false;
            }

            if (!res.Any()) {
                return false;
            }

            return user.Intersect(res).Any();
        }

        public virtual bool Compile() {
            if (string.IsNullOrEmpty(this.Name)) {
                string objectName = this.GetType().Name;
                _errorMsg = string.Format(Resources.CompilerError_MissingName, objectName, objectName);
                WriteCompileError(_errorMsg);
            }
          
            return IsCompiled;
        }

        public virtual void WriteCompileError(string traceMsg) {
            string objectName = this.GetType().Name;

            if (string.IsNullOrEmpty(this.Name)) {
                
                traceMsg = string.Format(traceMsg, objectName,objectName);
            }
            else {
                traceMsg = string.Format(traceMsg, objectName, this.Name);
            }
            CompilerErrors.Add(traceMsg);

            WriteOutput(traceMsg);

        }

        public virtual void WriteCompileWarning(string traceMsg) {
            WriteOutput(traceMsg);
        }

        public virtual dynamic GetCompilerErrors() {
            dynamic returnData = new UnlimitedObject();

            if (CompilerErrors.Count > 0) {
                CompilerErrors.ToList().ForEach(c => {
                    string errorData = string.Format("<CompilerError>{0}</CompilerError>", c);
                    dynamic error = new UnlimitedObject();
                    error.Load(errorData);
                    returnData.AddResponse(error);
                });
            }
            else {
                returnData.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(
                    string.Format("<CompilerMessage>Build of {0} '{1}' Succeeded</CompilerMessage>", 
                    Enum.GetName(typeof(enDynamicServiceObjectType), this.ObjectType), this.Name)));
            }
            return returnData;
        }

        #endregion

        #region Private Methods
        private void WriteOutput(string traceMsg) {
            TraceWriter.WriteTrace(traceMsg);
            Console.WriteLine(traceMsg);
        }
        #endregion

        public string IconPath {
            get;
            set;
        }

        /// <summary>
        /// When set to false will disable editing of all activity properties except those listed in the remarks section
        /// </summary>
        /// <remarks>
        /// Activity Properties out of scope:
        /// 1. IsSimulationEnabled
        /// 2. SimulationOutput
        /// 3. DisplayName
        /// 4. Categories
        /// </remarks>
        public bool IsActivityEditable {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        public string ErrorHandler {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        public string TooltipText {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        public int DefaultSizeHeight {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        public int DefaultSizeWidth {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        public string CategoryColour {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        public string Description {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        /// <summary>
        /// When set to false prevents a service from being part of another application
        /// </summary>
        public bool IsRedistributable {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        /// <summary>
        /// Used to determine if a service is an extension of an application. If it is it will be visible as a service. If it is not it will still be usable by other services that share the same application id
        /// </summary>
        public bool IsUsable {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        /// <summary>
        /// The Id of the application that this service is a part of. The application Id will be overwritten every time an application is redistributed
        /// </summary>
        /// <remarks></remarks>
        public Guid ApplicationId {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        /// <summary>
        /// The original application that created this service
        /// </summary>
        /// <remarks>The original appilcation id can never be overwritten and will stay with a service for its lifetime if it is part of an application</remarks>
        public Guid OriginalApplicationId {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        public Guid LicenseId {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        /// <summary>
        /// The unique identifier of the server - services will be bound to a server. If the service does not require  a license then you will be allowed to deploy the service to another server
        /// </summary>
        public Guid ServerId {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }

        /// <summary>
        /// If true will allow the service to be accesible from through the web server
        /// </summary>
        public bool IsWebAccessEnabled {
            get {
                throw new System.NotImplementedException();
            }
            set {
            }
        }


    }
}
