#region Change Log
//  Author:         Sameer Chunilall
//  Date:           2010-06-01
//  Log No:         9299
//  Description:    Container object that is the immediate child of a 
//                  switch type action. 
//                  This object stores the list of cases that need to evaluated
//                  against the DataElement using the regular expressions that 
#endregion

namespace Dev2.DynamicServices {
    #region Using Directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion

    /// <summary>
    /// Container object that holds all cases that need to be evaluated for
    /// a switch type action
    /// </summary>
    public class ServiceActionCases : DynamicServiceObjectBase {
        /// <summary>
        /// The element name of value that will be used to match against the regular expressions 
        /// in the cases list.
        /// The element could come from either the source request data or the preceding 
        /// actions tmp data
        /// </summary>
        public string DataElementName { get; set; }

        /// <summary>
        /// Used to store the value calculated at service execution time
        /// that will be used to branch the execution path
        /// like a typical switch would
        /// </summary>
        public string DataElementValue { get; set; }
       
        /// <summary>
        /// Flag used to indicate whether to retrieve the data value from the preceding action 
        /// or from the source request
        /// </summary>
        public bool CascadeSource { get; set; }
        /// <summary>
        /// List of all cases that make up the switch type action.
        /// </summary>
        public List<ServiceActionCase> Cases { get; set; }
        /// <summary>
        /// Contains the default case if the Data value retrieved using the DataElementName
        /// does not match any case. If this is null then the switch will perform any actions.
        /// </summary>
        public ServiceActionCase DefaultCase { get; set; }
        /// <summary>
        /// The name of the action that wraps this object.
        /// </summary>
        public string RelativeActionName { get; set; }
        /// <summary>
        /// The parent object in the XML Hierarchy.
        /// This is used to build the object graph
        /// </summary>
        public dynamic Parent { get; set; }

        public ServiceActionCases() : base(enDynamicServiceObjectType.ServiceActionCases) {
            Cases = new List<ServiceActionCase>();
        }
    }
}
