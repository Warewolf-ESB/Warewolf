﻿using Dev2.Common.Interfaces.DB;
using System.Collections.Generic;

namespace Dev2.Comparer
{
    internal class ServiceOutputMappingComparer : IEqualityComparer<IServiceOutputMapping>
    {
        public bool Equals(IServiceOutputMapping x, IServiceOutputMapping y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Equals(y);
        }
        
        public int GetHashCode(IServiceOutputMapping obj)
        {
            return obj.GetHashCode();
        }
    }
}