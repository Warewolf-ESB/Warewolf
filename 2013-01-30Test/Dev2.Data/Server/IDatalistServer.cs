using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList.Contract
{
    public interface IDataListServer
    {
        IDataListIDProvider IDProvider { get; }

        #region Translator Handling
        void AddTranslators(Assembly translatorAssembly);
        void AddTranslators(Type[] translatorTypes);
        void AddTranslator(IDataListTranslator translator);
        IDataListTranslator GetTranslator(DataListFormat format);
        bool ContainsTranslator(DataListFormat format);
        bool RemoveTranslator(DataListFormat format);
        IList<DataListFormat> FetchTranslatorTypes();
        #endregion

        #region Persistence Handleing

        IBinaryDataList ReadDatalist(Guid id, out ErrorResultTO errors);

        bool WriteDataList(Guid id, IBinaryDataList data, out ErrorResultTO errors);

        bool PersistChildChain(Guid id);

        void DeleteDataList(Guid id, bool onlyIfNotPersisted);
        #endregion
    }
}
