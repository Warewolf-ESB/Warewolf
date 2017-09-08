using Dev2.Common.Interfaces.Data;

namespace Dev2
{
    public static class CommonSourceEquality
    {
        public static bool IsSourceEqual(IResource source1, IResource source2)
        {
            bool sourceIsSame;
            var b = source1 == null && source2 != null;
            var b1 = source2 == null && source1 != null;
            if (source1 == null && source2 == null)
            {
                sourceIsSame = true;
            }

            else if (b || b1)
            {
                sourceIsSame = false;
            }
            else
            {
                sourceIsSame = source1.Equals(source2);
            }

            return sourceIsSame;
        }
    }
}
