#region Change Log
//  Author:         Sameer Chunilall
//  Date:           2010-01-24
//  Log No:         9299
//  Description:    Represents a Business Rule that only tmp in a boolean value.
//                  e.g. 
//                      Customer GovID Valid?   - true/false
//                      Customer Blacklisted?   - true/false
//                      Customer Over 18        - true/false
//                  
#endregion

using Dev2.DynamicServices;

namespace Dev2.DynamicServices {
    #region Using Directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using Dev2;
    using System.Text.RegularExpressions;
    using System.Collections;
    
    using System.Diagnostics;
    #endregion

    #region Biz Rule Class - Represents a business rule
    /// <summary>
    /// Represents a business rule that must be evaluated
    /// </summary>
    public class BizRule : DynamicServiceObjectBase {
        public BizRule() : base(enDynamicServiceObjectType.BizRule) {}
        /// <summary>
        /// The Name of the service that must be called for this rule ie the Biz rule data source
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// The CSharp Expression that must be evaluated to determine the tmp of the biz rule
        /// </summary>
        public string Expression { get; set; }
        /// <summary>
        /// The Columns from the service request that need to be evaluated
        /// </summary>
        public string[] ExpressionColumns { get; set; }
        /// <summary>
        /// The instance of the service that needs to be executed as the data source for the biz rule
        /// </summary>
        public DynamicService Service { get; set; }
        /// <summary>
        /// Compiles this object
        /// </summary>
        /// <returns></returns>
        public override bool Compile() {

            base.Compile();

            if(string.IsNullOrEmpty(this.ServiceName)){
                WriteCompileError(Resources.CompilerError_MissingServiceName);
            }

            if (string.IsNullOrEmpty(this.Expression)) {
                WriteCompileError(Resources.CompilerError_MissingExpression);
            }

            if (this.ExpressionColumns == null) {
                WriteCompileError(Resources.CompilerError_MissingExpressionColumns);
            }

            if (this.Service == null) {
                WriteCompileError(Resources.CompilerError_ServiceNotFound);
            }

            return this.IsCompiled;
        }
    }
    #endregion
}