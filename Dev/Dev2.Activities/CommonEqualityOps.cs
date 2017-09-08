using System;
using Warewolf.Security.Encryption;

namespace Dev2
{
    public static class CommonEqualityOps
    {

        public static bool IsSourceEqual<T>(T source1, T source2)
            where T : IEquatable<T>
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

        public static bool PassWordsCompare(string pass1, string pass2)
        {
            string temPpassword;
            try
            {
                temPpassword = DpapiWrapper.DecryptIfEncrypted(pass1);
            }
            catch (Exception)
            {
                // 
                temPpassword = pass1;
            }

            string temPpassword1;
            try
            {
                temPpassword1 = DpapiWrapper.DecryptIfEncrypted(pass2);
            }
            catch (Exception)
            {
                // 
                temPpassword1 = pass2;
            }

            return string.Equals(temPpassword, temPpassword1);
        } 
    }
}
