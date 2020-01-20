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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Warewolf.Options;

namespace Warewolf.Data
{
    public class NamedInt : INamedInt, IEquatable<NamedInt>, INotifyPropertyChanged
    {
        protected string _name;
        protected int _value;

        public NamedInt()
        {
            Name = "";
            Value = 0;
        }

        public NamedInt(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public override string ToString() => Name;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static bool operator ==(NamedInt left, NamedInt right) => Equals(left, right);
        public static bool operator !=(NamedInt left, NamedInt right) => !Equals(left, right);

        public bool Equals(NamedInt other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(_name, other._name) && _value == other._value;
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
            return Equals((NamedInt)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name?.GetHashCode() ?? 0) * 397) ^ (_value.GetHashCode());
            }
        }


        private static ConcurrentDictionary<Type, IEnumerable<INamedInt>> _cachedNamedInts = new ConcurrentDictionary<Type, IEnumerable<INamedInt>>();
        public static IEnumerable<INamedInt> GetAll(Type type)
        {
            if (!_cachedNamedInts.ContainsKey(type))
            {
                try
                {
                    var enums = Enum.GetValues(type);
                    var result = new List<(int, INamedInt)>();
                    bool hadIndex = false;
                    bool hadNoIndex = false;
                    
                    for (int i = 0; i < enums.Length; i++)
                    {
                        var entry = enums.GetValue(i);
                        var enumName = Enum.GetName(type, entry);
                        var actualEnum = type.GetMember(enumName)[0];
                        var index = -1;
                        if (actualEnum.GetCustomAttributes(typeof(IndexAttribute), false).FirstOrDefault() is IndexAttribute indexAttribute)
                        {
                            hadIndex = true;
                            index = indexAttribute.Get();
                        } 
                        else
                        {
                            index = i;
                            hadNoIndex = true;
                        }
                        if (index < 0 || (hadIndex && hadNoIndex))
                        {
                            throw new IndexAttributeException($"mixed use of enum IndexAttributes in {type}");
                        }
                        string displayValue = "";
                        if (actualEnum.GetCustomAttributes(typeof(DecisionTypeDisplayValue), false).FirstOrDefault() is DecisionTypeDisplayValue displayValueAttribute)
                        {
                            displayValue = displayValueAttribute.Get();
                        }

                        result.Add((index, new NamedInt
                        {
                            Name = displayValue,
                            Value = (int)entry,
                        }));
                    }
                    var r = result
                        .OrderBy(o => o.Item1)
                        .Select(o => o.Item2);
                    _ = _cachedNamedInts.TryAdd(type, r);
                    return r;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                return _cachedNamedInts[type];
            }
        }
    }
}
