#region Change Log
//  Author:         Sameer Chunilall
//  Date:           2010-01-24
//  Log No:         9299
//  Description:    The Validator type contains metadata on the type of validation that needs to occur on an input
//                  The supported type of validation are Required, Regex and RequiredAndRegex.
//                  This type is wrapped within the Unlimited.Framework.DynamicServices.Input type
//                  
//                  
//                  
#endregion

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.DynamicServices {
    #region Using Directives

    #endregion

                                   #region Validator Class - Represents a validator that can validate any Service Action Input
                                   /// <summary>
    /// Provides a representation of a validator.
    /// Describes the types of validation that can occur
    /// </summary>
    public class Validator : DynamicServiceObjectBase{

        public Validator() : base( enDynamicServiceObjectType.Validator) {

        }

        #region Public Properties
        /// <summary>
        /// The type of validation required
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enValidationType ValidatorType { get; set; }
        /// <summary>
        /// The regular expression to evaluate - applies only if the validation type is Regex or RequiredAndRegex 
        /// otherwise ignored
        /// </summary>
        public string RegularExpression { get; set; }
        #endregion

        public override bool Compile() {
            if ( (ValidatorType == enValidationType.Regex || ValidatorType == enValidationType.RequiredAndRegex)
                && string.IsNullOrEmpty(RegularExpression)) {
                WriteCompileError(Resources.CompilerError_MissingRegularExpression);
                return false;
            }

            return true;
        }
    }
    #endregion
}