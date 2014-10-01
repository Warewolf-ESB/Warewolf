
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace System
{
    public static class Deserializer
    {
        #region Instance Fields
        private static Type _serializableEntityType = typeof(SerializableEntity);
        private static List<Type> _typeStore = new List<Type>();
        private static string[] _typeDefinitions = new string[0];
        private static uint _tdCount = 0;
        #endregion

        #region Load Handling
        internal static Exception PrepareLoad(string path)
        {
            Exception result = null;

            try
            {
                if (path != null)
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None)))
                    {
                        _tdCount = reader.ReadUInt32();
                        _typeDefinitions = new string[_tdCount];
                        _typeStore = new List<Type>((int)_tdCount);

                        for (uint i = 0; i < _tdCount; i++)
                        {
                            _typeDefinitions[i] = reader.ReadString();
                            _typeStore.Add(Type.GetType(_typeDefinitions[i]));
                        }
                    }
                }
            }
            catch (Exception e)
            {

                _tdCount = 0;
                _typeDefinitions = new string[0];
                _typeStore = new List<Type>();
                result = e;
            }

            return result;
        }

        internal static void ConfirmLoad(Assembly[] definedAssemblies)
        {
            List<Type> allTypes = new List<Type>();
            for (int i = 0; i < definedAssemblies.Length; i++) allTypes.AddRange(definedAssemblies[i].GetTypes());
            Type[] rawTypes = allTypes.ToArray();
            List<string> sTypes = new List<string>();

            for (int i = 0; i < rawTypes.Length; i++)
                if (rawTypes[i].IsSubclassOf(_serializableEntityType))
                    sTypes.Add(rawTypes[i].AssemblyQualifiedName);

            _tdCount = (uint)sTypes.Count;
            _typeDefinitions = sTypes.ToArray();
            _typeStore = new List<Type>((int)_tdCount);
            for (uint i = 0; i < _tdCount; i++) _typeStore.Add(Type.GetType(_typeDefinitions[i]));
        }
        #endregion

        #region Save Handling
        internal static Exception Save(string path)
        {
            Exception result = null;

            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
                {
                    writer.Write(_tdCount);
                    for (uint i = 0; i < _tdCount; i++) writer.Write(_typeDefinitions[i]);
                }
            }
            catch (Exception e)
            {
                result = e;
            }

            return result;
        }
        #endregion

        #region Type Header Handling
        public static uint GetTypeHeader(Type headerFor)
        {
            int index = _typeStore.IndexOf(headerFor);
            if (index == -1) return uint.MaxValue;
            return (uint)index;
        }

        public static Type GetTypeFromHeader(uint header)
        {
            return _typeStore[(int)header];
        }
        #endregion

        #region Deserialization Handling
        public static T ByHeader<T>(IByteReaderBase reader) where T : class
        {
            Type headerType = GetTypeFromHeader(reader.ReadUInt32());
            return (T)Activator.CreateInstance(headerType, reader);
        }
        #endregion
    }
}
