/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Common
{
    [Serializable]
    public class PluginConstructor : IPluginConstructor, IEquatable<PluginConstructor>
    {
        public PluginConstructor()
        {
            Inputs = new List<IConstructorParameter>();
        }

        public IList<IConstructorParameter> Inputs { get; set; }
        public string ReturnObject { get; set; }
        public string ConstructorName { get; set; }

        public string GetIdentifier() => ConstructorName;


        public bool IsExistingObject { get; set; }


        public bool Equals(PluginConstructor other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (GetHashCode() == other.GetHashCode())
            {
                return true;
            }

            return string.Equals(ConstructorName, other.ConstructorName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((PluginConstructor)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Inputs?.GetHashCode() ?? 0) * 397) ^ (ConstructorName?.GetHashCode() ?? 0);
            }
        }
        public static bool operator ==(PluginConstructor left, PluginConstructor right) => Equals(left, right);

        public static bool operator !=(PluginConstructor left, PluginConstructor right) => !Equals(left, right);

        public override string ToString() => ConstructorName;

        public Guid ID { get; set; }
    }
}