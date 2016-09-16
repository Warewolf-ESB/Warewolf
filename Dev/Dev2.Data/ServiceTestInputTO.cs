using Dev2.Common.Interfaces;

namespace Dev2.Data
{
    public class ServiceTestInputTO : IServiceTestInput
    {
        public string Variable { get; set; }
        public string Value { get; set; }
        public bool EmptyIsNull { get; set; }

        #region Implementation of ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}