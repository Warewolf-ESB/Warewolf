/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    /// <summary>
    ///     Defines the requirements for a debug writer
    /// </summary>
    public interface IDebugWriter
    {
        /// <summary>
        ///     Writes the given state.
        ///     <remarks>
        ///         This must implement the one-way (fire and forget) message exchange pattern.
        ///     </remarks>
        /// </summary>
        /// <param name="debugState">The state to be written.</param>
        void Write(IDebugState debugState);
    }
}