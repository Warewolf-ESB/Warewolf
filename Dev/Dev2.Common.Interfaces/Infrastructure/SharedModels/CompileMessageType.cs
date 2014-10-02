
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.ComponentModel;

namespace Dev2.Common.Interfaces.Infrastructure.SharedModels
{
    /// <summary>
    /// Compile time message types
    /// </summary>
    public enum CompileMessageType
    {
        [Description("Mapping out of date")]
        MappingChange,

        [Description("Resource has been deleted")]
        ResourceDeleted,

        [Description("Resource has been saved")]
        ResourceSaved,

        [Description("IsRequired mapping has changed")]
        MappingIsRequiredChanged
    }
}
