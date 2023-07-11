﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    [Serializable]
    public class DllListing : IFileListing
    {
        public DllListing(IDllListingModel selectedDll)
        {
            Name = selectedDll.Name;
            FullName = selectedDll.FullName;
            IsDirectory = selectedDll.IsDirectory;
            ClsId = selectedDll.ClsId;
            Is32Bit = selectedDll.Is32Bit;
        }

        public DllListing()
        {
        }

        public bool Equals(IFileListing other)
        {
            var equals = true;
            equals &= string.Equals(Name, other.Name);
            equals &= string.Equals(FullName, other.FullName);
            equals &= IsDirectory == other.IsDirectory;

            return equals;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
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
            return Equals((DllListing)obj);
        }
        
        public override int GetHashCode()
        {
            unchecked
			{



                //var hashCode = Name?.Length ?? 0;
                //hashCode += (Children?.ToString().Length ?? 0);
                //hashCode += (FullName?.Length ?? 0);
                //hashCode += IsDirectory.ToString().Length;
                //return hashCode;

                var child = Children?.Count ?? 0;
                var hashCode = Name?.GetDeterministicHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (child.ToString().GetDeterministicHashCode());                
                hashCode = (hashCode * 397) ^ (FullName?.GetDeterministicHashCode() ?? 0);                
                hashCode = (hashCode * 397) ^ (IsDirectory ? IsDirectory.ToString().GetDeterministicHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(DllListing left, DllListing right) => Equals(left, right);

        public static bool operator !=(DllListing left, DllListing right) => !Equals(left, right);

        public string Name { get; set; }
        public ICollection<IFileListing> Children { get; set; }
        public string FullName { get; set; }
        public bool IsDirectory { get; set; }
        public bool Is32Bit { get; set; }
        public string ClsId { get; set; }

    }



    public static class FindHashCode
    {
        public static int GetDeterministicHashCode(this string str)
        {
            unchecked
            {
                int hash1 = 352654597;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }

}
