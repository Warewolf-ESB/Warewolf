
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

        bool DeleteDataList(Guid id, bool onlyIfNotPersisted);

        #endregion
    }
}
