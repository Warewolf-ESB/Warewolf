/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Util
{
    /// <summary>
    ///     Used to iterate the IO Mapping for ForEach and External Services ... DB / Plugin Services
    /// </summary>
    public class Dev2ActivityIOIteration
    {
        /// <summary>
        ///     Iterates the mapping.
        /// </summary>
        /// <param name="newInputs">The new inputs.</param>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        public string IterateMapping(string newInputs, int idx)
        {
            if (newInputs == null) return null;
            return newInputs.Replace("(*)", "(" + idx + ")");
        }
    }
}