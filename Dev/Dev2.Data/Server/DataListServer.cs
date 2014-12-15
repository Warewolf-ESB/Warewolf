
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
using System.Reflection;
using Dev2.Data.DataListCache;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Server.DataList
{
    public sealed class DataListServer : IDataListServer
    {
        #region Instance Fields
        private readonly object _translatorGuard;
        private readonly Dictionary<DataListFormat, IDataListTranslator> _translators;
        private readonly IDataListPersistenceProvider _persistence;
        private readonly IDataListIDProvider _idProvider = new DataListIDProvider();
        #endregion

        public IDataListIDProvider IDProvider { get { return _idProvider; } }

        #region Constructor
        public DataListServer(IDataListPersistenceProvider pProvider)
        {
            _persistence = pProvider;
            _translatorGuard = new object();
            _translators = new Dictionary<DataListFormat, IDataListTranslator>();
        }
        #endregion

        #region Translation Handling
        public void AddTranslators(Assembly translatorAssembly)
        {
            if(translatorAssembly == null) throw new ArgumentNullException("translatorAssembly");
            Type[] allTypes = translatorAssembly.GetTypes();
            AddTranslatorsImpl(allTypes, false);
        }

        public void AddTranslators(Type[] translatorTypes)
        {
            if(translatorTypes == null) throw new ArgumentNullException("translatorTypes");
            AddTranslatorsImpl(translatorTypes, true);
        }

        private void AddTranslatorsImpl(Type[] translatorTypes, bool fromExternal)
        {
            string interfaceName = typeof(IDataListTranslator).Name;
            List<IDataListTranslator> translators = new List<IDataListTranslator>();

            for(int i = 0; i < translatorTypes.Length; i++)
            {
                Type current = translatorTypes[i];

                Type interfaceImpl = current.GetInterface(interfaceName);

                if(interfaceImpl == null)
                {
                    if(fromExternal) throw new ArgumentException("Type \"" + current.Name + "\" does not implement interface \"" + interfaceName + "\".");
                    translatorTypes[i] = null;
                }
                else
                {
                    ConstructorInfo defaultCtor = current.GetConstructor(Type.EmptyTypes);

                    if(defaultCtor == null)
                    {
                        throw new ArgumentException("Type \"" + current.Name + "\" does not implement a public parameterless constructor.");
                    }

                    IDataListTranslator instance = Activator.CreateInstance(current) as IDataListTranslator;

                    if(instance == null) throw new ArgumentException("Type \"" + current.Name + "\" does not implement interface \"" + typeof(IDataListTranslator).FullName + "\".");
                    translators.Add(instance);
                }
            }


            foreach(IDataListTranslator t in translators)
            {
                AddTranslatorImpl(t, fromExternal);
            }
        }

        public void AddTranslator(IDataListTranslator translator)
        {
            if(translator == null) throw new ArgumentNullException("translator");
            AddTranslatorImpl(translator, true);
        }

        private void AddTranslatorImpl(IDataListTranslator translator, bool fromExternal)
        {
            DataListFormat format = translator.Format;

            lock(_translatorGuard)
            {
                IDataListTranslator existing;
                if(_translators.TryGetValue(format, out existing))
                {
                    if(fromExternal)
                    {
                        throw new InvalidOperationException("A IDataListTranslator has already been added for format \"" + format + "\".");
                    }
                }
                else _translators.Add(format, translator);
            }
        }

        public IDataListTranslator GetTranslator(DataListFormat format)
        {
            if(format == null) throw new ArgumentNullException("format");
            IDataListTranslator result;

            lock(_translatorGuard)
            {
                if(!_translators.TryGetValue(format, out result))
                {
                    return null;
                }
            }

            return result;
        }

        public bool ContainsTranslator(DataListFormat format)
        {
            if(format == null) throw new ArgumentNullException("format");
            lock(_translatorGuard) return _translators.ContainsKey(format);
        }

        public bool RemoveTranslator(DataListFormat format)
        {
            if(format == null) throw new ArgumentNullException("format");
            lock(_translatorGuard) return _translators.Remove(format);
        }

        public IList<DataListFormat> FetchTranslatorTypes()
        {
            return (_translators.Keys.ToList());
        }
        #endregion

        public IBinaryDataList ReadDatalist(Guid id, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _persistence.ReadDatalist(id, errors);
        }

        public bool WriteDataList(Guid id, IBinaryDataList data, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _persistence.WriteDataList(id, data, errors);
        }

        public bool DeleteDataList(Guid id, bool onlyIfNotPersisted)
        {
            return _persistence.DeleteDataList(id, onlyIfNotPersisted);
        }

    }
}
