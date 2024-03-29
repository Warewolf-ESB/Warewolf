#pragma warning disable
﻿using System;
using System.Collections.Generic;

namespace Dev2.Comparer
{
    internal class GatherSystemInformationTOComparer : IEqualityComparer<GatherSystemInformationTO>
    {
#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(GatherSystemInformationTO x, GatherSystemInformationTO y)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (x == null && y == null)
            {
                return true;
            }

            if ((x != null && y == null) || (x == null && y != null))
            {
                return false;
            }

            var expressionsAreEqual = Common.CommonEqualityOps.CollectionEquals(x.Expressions, y.Expressions, StringComparer.Ordinal);
            return string.Equals(x.WatermarkTextVariable, y.WatermarkTextVariable)
                && string.Equals(x.WatermarkText, y.WatermarkText)
                && string.Equals(x.Result, y.Result)
                && expressionsAreEqual
                && x.EnTypeOfSystemInformation.Equals(y.EnTypeOfSystemInformation)
                && x.Inserted.Equals(y.Inserted)
                && x.IndexNumber.Equals(y.IndexNumber);
        }
        public int GetHashCode(GatherSystemInformationTO obj) => 1;
    }
}
