
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Meta
{
    internal abstract class MetaTypeElement
    {
        protected readonly Type sourceType;
        private string _dynamicAssemblyName;

        public string DynamicAssemblyName { get { return _dynamicAssemblyName; } }

        protected MetaTypeElement(Type sourceType, string dynamicAssemblyName)
        {
            this.sourceType = sourceType;
            _dynamicAssemblyName = dynamicAssemblyName;
        }

        internal bool CanBeImplementedExplicitly
        {
            get { return sourceType != null && sourceType.IsInterface; }
        }

        internal abstract void SwitchToExplicitImplementation();
    }
}
