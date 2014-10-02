
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
    /// Indicates the type of objects the DirectoryObjectPickerDialog searches for.
    /// </summary>
    [Flags]
    public enum ObjectTypes
    {
        /// <summary>
        /// No object types.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Includes user objects.
        /// </summary>
        Users = 0x0001, 

        /// <summary>
        /// Includes security groups with universal scope. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// In an up-level scope, this includes distribution and security groups, with universal, global and domain local scope.
        /// </para>
        /// <para>
        /// In a down-level scope, this includes local and global groups.
        /// </para>
        /// </remarks>
        Groups = 0x0002, 
        
        /// <summary>
        /// Includes computer objects.
        /// </summary>
        Computers = 0x0004, 

        /// <summary>
        /// Includes contact objects.
        /// </summary>
        Contacts = 0x0008, 

        /// <summary>
        /// Includes built-in group objects.
        /// </summary>
        /// <summary>
        /// <para>
        /// In an up-level scope, this includes group objects with the built-in groupType flags.
        /// </para>
        /// <para>
        /// In a down-level scope, not setting this object type excludes local built-in groups.
        /// </para>
        /// </summary>
        BuiltInGroups = 0x0010, 

        /// <summary>
        /// Includes all well-known security principals. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// In an up-level scope, this includes the contents of the Well Known Security Principals container.
        /// </para>
        /// <para>
        /// In a down-level scope, this includes all well-known SIDs.
        /// </para>
        /// </remarks>
        WellKnownPrincipals = 0x0020, 

        /// <summary>
        /// All object types.
        /// </summary>
        All = 0x003F
    }
}
