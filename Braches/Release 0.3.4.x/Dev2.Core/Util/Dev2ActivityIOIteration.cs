namespace Dev2.Util
{
    /// <summary>
    /// Used to iterate the IO Mapping for ForEach and External Services ... DB / Plugin Services
    /// </summary>
    public class Dev2ActivityIOIteration
    {
        /// <summary>
        /// Iterates the mapping.
        /// </summary>
        /// <param name="newInputs">The new inputs.</param>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        public string IterateMapping(string newInputs, int idx)
        {
            if(newInputs == null) return null;
            return newInputs.Replace("(*)", "(" + idx + ")");
        }

    }
}
