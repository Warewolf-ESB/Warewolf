
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

namespace Dev2.Integration.Tests.Mocks
{
    /// <summary>
    /// This class is specifically for testing the serialization of data Objects 
    /// in the Unlimited Framework
    /// </summary>
    internal class DataObject
    {
        public string DataObjectName { get; set; }
        private string datathing;
        public string DataobjectType
        {
            get
            {
                if(datathing == null)
                    throw new Exception();
                else
                    return datathing;
            }
            set
            {
                if(value == String.Empty)
                    throw new ArgumentNullException();
                else
                    datathing = value;
            }
        }
        internal List<DataObjectChild> Datachildren { get; set; }
        internal dynamic data
        {
            get
            {
                return "this as string";
            }
        }

        internal DataObject()
        {
            Datachildren = new List<DataObjectChild>() { new DataObjectChild ("dto1"), 
                                                         new DataObjectChild("dto2"), 
                                                         new DataObjectChild("dto3")
                                                        };
        }
    }

    internal class DataObjectChild
    {
        internal string child { get; set; }
        internal List<string> childattributes { get; set; }

        internal DataObjectChild()
        {
            child = "dtochild";
            childattributes = new List<string>() { "CanDoWork", "CanAcceptConnection", "IsMultithreaded" };
        }
        internal DataObjectChild(string childname)
        {
            child = childname;
            childattributes = new List<string>() { "CanDoWork", "CanAcceptConnection", "IsMultithreaded" };
        }

    }
}
