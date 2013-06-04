#region Change Log
//  Author:         Sameer Chunilall
//  Date:           2010-06-01
//  Log No:         9299
//  Description:    Stores a the sequence of
//                  actions that need to execute if the
//                  data element in the ServiceActionCases
//                  object which contains a generic list of objects
//                  of this type matches  the Regular Expression (Regex)
//                  defined here
#endregion

namespace Dev2.DynamicServices {
    #region Using Directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion

    /// <summary>
    /// Represents a case contains sequence of actions that need to executed.
    /// </summary>
    public class ServiceActionCase : DynamicServiceObjectBase {
        private bool _isDefault = false;

        /// <summary>
        /// The Regular expression to match
        /// </summary>
        public string Regex { get; set; }
        /// <summary>
        /// The parent object in the XML hierarchy
        /// This is used to build the object graph
        /// </summary>
        public dynamic Parent { get; set; }
        /// <summary>
        /// Sequence of actions that need to be executed if this case is evaluated.
        /// this list can have an infinite number of nested switch type actions.
        /// </summary>
        public List<ServiceAction> Actions { get; set; }
        /// <summary>
        /// Indicates that this case is the default case
        /// </summary>
        public bool IsDefault {
            get {
                return _isDefault;
            }
            set {
                _isDefault = value;
            }
        }

        #region Constructor
        /// <summary>
        /// Initialize the actions generic list
        /// </summary>
        public ServiceActionCase() : base(enDynamicServiceObjectType.ServiceActionCase){
            Actions = new List<ServiceAction>();
        }
        #endregion

        #region Public Methods

        #endregion
    }
}
