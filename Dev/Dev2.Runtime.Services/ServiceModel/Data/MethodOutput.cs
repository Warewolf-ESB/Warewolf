
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
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public class MethodOutput : IDev2Definition
    {
        #region Properties

        // ReSharper disable UnusedAutoPropertyAccessor.Local

        public string Name { get; private set; }

        public string MapsTo { get; private set; }

        public string Value { get; private set; }

        public bool IsRecordSet { get; private set; }

        public string RecordSetName { get; private set; }

        public bool IsEvaluated { get; private set; }

        public string DefaultValue { get; private set; }

        public bool IsRequired { get; private set; }

        public string RawValue { get; private set; }

        public bool EmptyToNull { get; private set; }

        // ReSharper restore UnusedAutoPropertyAccessor.Local

        #endregion

        #region Methods

        public XElement ToXml()
        {
            return new XElement("");
        }

        #endregion


    }
}
