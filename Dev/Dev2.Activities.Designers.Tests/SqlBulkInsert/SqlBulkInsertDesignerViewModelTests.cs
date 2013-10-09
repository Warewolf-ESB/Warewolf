using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.SqlBulkInsert
{
    [TestClass]
    public class SqlBulkInsertDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Constructor")]
        public void SqlBulkInsertDesignerViewModel_Constructor_InitializesProperties()
        {
            //------------Setup for test--------------------------
            //var sqlBulkInsertDesignerViewModel = new SqlBulkInsertDesignerViewModel();
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }


        static ModelItem CreateModelItem(IEnumerable<DataColumnMapping> items)
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfSqlBulkInsertActivity());

            // ReSharper disable PossibleNullReferenceException
            var modelItemCollection = modelItem.Properties["InputMappings"].Collection;
            foreach(var dto in items)
            {
                modelItemCollection.Add(dto);
            }
            // ReSharper restore PossibleNullReferenceException

            return modelItem;
        }
 
    }
}
