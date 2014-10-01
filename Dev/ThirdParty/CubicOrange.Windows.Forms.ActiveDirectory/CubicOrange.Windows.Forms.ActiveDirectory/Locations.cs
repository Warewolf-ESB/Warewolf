
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
using System.Text;

namespace CubicOrange.Windows.Forms.ActiveDirectory
{
    /// <summary>
    /// Indicates the scope the DirectoryObjectPickerDialog searches for objects.
    /// </summary>
    [Flags]
    public enum Locations
    {
        /// <summary>
        /// No locations.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// The target computer (down-level).
        /// </summary>
        LocalComputer = 0x0001,

        /// <summary>
        /// A domain to which the target computer is joined (down-level and up-level).
        /// </summary>
        JoinedDomain = 0x0002,

        /// <summary>
        /// All Windows 2000 domains in the enterprise to which the target computer belongs (up-level).
        /// </summary>
        EnterpriseDomain = 0x0004,

        /// <summary>
        /// A scope containing objects from all domains in the enterprise (up-level). 
        /// </summary>
        GlobalCatalog = 0x0008,

        /// <summary>
        /// All domains external to the enterprise, but trusted by the domain to which the target computer 
        /// is joined (down-level and up-level).
        /// </summary>
        ExternalDomain = 0x0010,

        /// <summary>
        /// The workgroup to which the target computer is joined (down-level). 
        /// </summary>
        /// <remarks>
        /// <para>
        /// Applies only if the target computer is not 
        /// joined to a domain. The only type of object that can be selected from a workgroup is a computer.
        /// </para>
        /// </remarks>
        Workgroup = 0x0020,

        /// <summary>
        /// Enables the user to enter a scope (down-level and up-level). 
        /// </summary>
        /// <remarks>
        /// <para>
        /// If not specified, the dialog box restricts the user to the scopes in the locations drop-down list.
        /// </para>
        /// </remarks>
        UserEntered = 0x0040,

        /// <summary>
        /// All locations.
        /// </summary>
        All = 0x007F
    }
}
