/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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

        public override bool IsSource => false;
        public override bool IsService => true;

        public override bool IsFolder => false;

        public override bool IsReservedService => false;

        public override bool IsServer => false;
        public override bool IsResourceVersion => false;
    }
}
