using Dev2.Common.Interfaces;

namespace Dev2.TO
{
    public class SharepointReadListTo : ISharepointReadListTo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SharepointReadListTo(string variableName, string fieldName)
        {
            FieldName = fieldName;
            VariableName = variableName;
        }

        public string FieldName { get; set; }
        public string VariableName { get; set; }

    }
}