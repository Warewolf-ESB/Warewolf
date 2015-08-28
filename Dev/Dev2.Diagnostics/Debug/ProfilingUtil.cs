
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Diagnostics;
// ReSharper disable ObjectCreationAsStatement

namespace Dev2.Diagnostics.Debug
{
    /// <summary>
    /// Provides useful mechanisms for performance testing.
    /// </summary>
    public static class ProfilingUtil
    {
        private static readonly Stopwatch Watch = new Stopwatch();
        private static ProfileRegion _currentRegion;

        /// <summary>
        /// Begins a profiled region.
        /// </summary>
        public static void BeginRegion()
        {
            if (_currentRegion == null)
            {
                new ProfileRegion();
            }
            else
            {
                new ProfileRegion(_currentRegion);
            }
        }

        /// <summary>
        /// Ends a profiled region.
        /// </summary>
        /// <returns>The total number of milliseconds that elapsed between begin and end region calls.</returns>
        public static long EndRegion()
        {
            if (_currentRegion == null) return 0L;
            return _currentRegion.Pop();
        }

        private sealed class ProfileRegion
        {
            private readonly ProfileRegion _parent;
            private List<ProfileRegion> _children;
            private readonly long _start;
            private long _end;

            public ProfileRegion()
            {
                _currentRegion = this;
                Watch.Reset();
                Watch.Start();
            }

            public ProfileRegion(ProfileRegion parent)
            {
                _parent = parent;
                (_parent._children ?? (_parent._children = new List<ProfileRegion>())).Add(this);
                _currentRegion = this;
                _start = Watch.ElapsedMilliseconds;
            }

            public long Pop()
            {
                _end = Watch.ElapsedMilliseconds;
                if ((_currentRegion = _parent) == null) Watch.Stop();
                return _end - _start;
            }
        }
    }
}
