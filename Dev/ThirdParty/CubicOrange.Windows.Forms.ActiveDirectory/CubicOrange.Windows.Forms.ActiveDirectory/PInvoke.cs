
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
using System.Runtime.InteropServices;

namespace CubicOrange.Windows.Forms.ActiveDirectory
{
    internal class PInvoke
    {
        /// <summary>
        /// The GlobalLock function locks a global memory object and returns a pointer to the first byte of the object's memory block.
        /// GlobalLock function increments the lock count by one.
        /// Needed for the clipboard functions when getting the data from IDataObject
        /// </summary>
        /// <param name="hMem"></param>
        /// <returns></returns>
        [DllImport("Kernel32.dll")]
        public static extern IntPtr GlobalLock(IntPtr hMem);

        /// <summary>
        /// The GlobalUnlock function decrements the lock count associated with a memory object.
        /// </summary>
        /// <param name="hMem"></param>
        /// <returns></returns>
        [DllImport("Kernel32.dll")]
        public static extern bool GlobalUnlock(IntPtr hMem);

        /// <summary>
        /// Frees the specified storage medium.
        /// The ReleaseStgMedium function calls the appropriate method or function to release the specified storage medium. 
        /// Use this function during data transfer operations where storage medium structures are parameters, 
        /// such as IDataObject::GetData or IDataObject::SetData. 
        /// In addition to identifying the type of the storage medium, this structure specifies the appropriate Release method 
        /// for releasing the storage medium when it is no longer needed.
        /// </summary>
        /// <param name="pMedium">Pointer to the storage medium that is to be freed.</param>
        [DllImport("Ole32.dll")]
        public static extern void ReleaseStgMedium(ref STGMEDIUM pMedium);
    }
}
