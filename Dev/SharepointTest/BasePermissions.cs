using Newtonsoft.Json.Linq;

namespace Warewolf.SharePoint
{
    public class BasePermissions
    {
        public static BasePermissions ParseFromJson(JObject data)
        {
            var permissions = new BasePermissions { _low = (uint)data["Low"], _high = (uint)data["High"] };
            return permissions;
        }

        public bool Has(PermissionKind perm)
        {
            if (perm == PermissionKind.EmptyMask)
                return true;
            if (perm == PermissionKind.FullMask)
            {
                if ((_high & short.MaxValue) == short.MaxValue)
                    return _low == ushort.MaxValue;
                return false;
            }
            var permLow = (int)perm - 1;
            var permHigh = 1U;
            if (permLow >= 0 && permLow < 32)
                return 0 != (_low & (permHigh << permLow));
            if (permLow >= 32 && permLow < 64)
                return 0 != (_high & (permHigh << permLow - 32));
            return false;
        }



        private uint _high;
        private uint _low;
    }
}
