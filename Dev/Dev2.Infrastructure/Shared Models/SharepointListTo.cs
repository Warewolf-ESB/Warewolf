using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class SharepointListTo : ISharepointListTo
    {
        public string FullName { get; set; }
        public List<ISharepointFieldTo> Fields { get; set; }

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return FullName;
        }

        #endregion
    }

    public class SharepointFieldTo : ISharepointFieldTo
    {
        public string Name { get; set; }
        public string InternalName { get; set; }
        public SharepointFieldType Type { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public bool IsRequired { get; set; }
        public bool IsEditable { get; set; }

        public string GetFieldType()
        {
            switch(Type)
            {
                case SharepointFieldType.Boolean:
                    return "Boolean";
                case SharepointFieldType.Currency:
                    return "Currency";
                case SharepointFieldType.DateTime:
                    return "DateTime";
                case SharepointFieldType.Number:
                case SharepointFieldType.Integer:
                    return "Integer";
                case SharepointFieldType.Text:
                case SharepointFieldType.Note:
                    return "Text";
                default:
                    return "Text";
            }
        }
    }
}