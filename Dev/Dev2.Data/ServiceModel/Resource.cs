
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;

// ReSharper disable CheckNamespace
namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public class Resource : ResourceBase
    {
       
        public Resource(IResource copy)
            : base(copy)
        {
        }

        public Resource(XElement xml)
            : base(xml)
        {
        }

        public Resource()
        {
        }

        public override bool IsSource
        {
            get
            {
                return false;
            }
        }
        public override bool IsService
        {
            get
            {
                return true;
            }
        }
        public override bool IsFolder
        {
            get
            {
                return false;
            }
        }
        public override bool IsReservedService
        {
            get
            {
                return false;
            }
        }
        public override bool IsServer
        {
            get
            {
                return false;
            }
        }
        public override bool IsResourceVersion
        {
            get
            {
                return false;
            }
        }
    }
}
