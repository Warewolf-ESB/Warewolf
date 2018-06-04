﻿using Dev2.Common.Interfaces.Core;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Tests.Activities.ActivityComparerTests.Sharepoint
{
    [TestClass]
    public class SharepointSearchToEqualityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_EmptyTos_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo();
            var listTo1 = new SharepointSearchTo();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_FieldNames__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo(){FieldName = "a"};
            var listTo1 = new SharepointSearchTo() { FieldName = "a" }; ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentFieldNames_Object_Is_Not_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "a" };
            var listTo1 = new SharepointSearchTo { FieldName = "Adfdf" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentFieldNames_Object_Is_Not_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "a" };
            var listTo1 = new SharepointSearchTo { FieldName = "A" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_SearchType__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo() { FieldName = "a", SearchType = "A" };
            var listTo1 = new SharepointSearchTo() { FieldName = "a", SearchType = "A" }; ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentSearchType_Object_Is_Not_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", SearchType = "sss" };
            var listTo1 = new SharepointSearchTo { FieldName = "A", SearchType = "dfdf" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentSearchType_Object_Is_Not_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", SearchType = "a"};
            var listTo1 = new SharepointSearchTo { FieldName = "A" , SearchType = "mkmk"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_InternalName__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo() { FieldName = "a", InternalName = "A" };
            var listTo1 = new SharepointSearchTo() { FieldName = "a", InternalName = "A" }; ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentInternalName_Object_Is_Not_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", InternalName = "sss" };
            var listTo1 = new SharepointSearchTo { FieldName = "A", InternalName = "dfdf" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentInternalName_Object_Is_Not_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", InternalName = "a" };
            var listTo1 = new SharepointSearchTo { FieldName = "A", InternalName = "mkmk" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_From__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo() { FieldName = "a", From = "A" };
            var listTo1 = new SharepointSearchTo() { FieldName = "a", From = "A" }; ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentFrom_Object_Is_Not_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", From = "sss" };
            var listTo1 = new SharepointSearchTo { FieldName = "A", From = "dfdf" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentTo_Object_Is_Not_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", To = "a" };
            var listTo1 = new SharepointSearchTo { FieldName = "A", To = "mkmk" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_To__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo() { FieldName = "a", To = "A" };
            var listTo1 = new SharepointSearchTo() { FieldName = "a", To = "A" }; ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentTo_Object_Is_Not_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", To = "sss" };
            var listTo1 = new SharepointSearchTo { FieldName = "A", To = "dfdf" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentValueToMatch_Object_Is_Not_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", ValueToMatch = "a" };
            var listTo1 = new SharepointSearchTo { FieldName = "A", ValueToMatch = "mkmk" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_ValueToMatch__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo() { FieldName = "a", ValueToMatch = "A" };
            var listTo1 = new SharepointSearchTo() { FieldName = "a", ValueToMatch = "A" }; ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentValueToMatch_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", ValueToMatch = "sss" };
            var listTo1 = new SharepointSearchTo { FieldName = "A", ValueToMatch = "dfdf" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentFrom_Object_Is_Not_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", From = "a" };
            var listTo1 = new SharepointSearchTo { FieldName = "A", From = "mkmk" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsSearchCriteriaEnabled_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A",};
            var listTo1 = new SharepointSearchTo { FieldName = "A",  };
            //---------------Assert Precondition----------------
            Assert.IsTrue(listTo.Equals(listTo1));
            //---------------Execute Test ----------------------
            listTo.IsSearchCriteriaEnabled = true;
            listTo1.IsSearchCriteriaEnabled = false;
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsSearchCriteriaEnabled_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", };
            var listTo1 = new SharepointSearchTo { FieldName = "A",  };
            //---------------Assert Precondition----------------
            Assert.IsTrue(listTo.Equals(listTo1));
            //---------------Execute Test ----------------------
            listTo.IsSearchCriteriaEnabled = true;
            listTo1.IsSearchCriteriaEnabled = true;
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Inserted_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A",};
            var listTo1 = new SharepointSearchTo { FieldName = "A",  };
            //---------------Assert Precondition----------------
            Assert.IsTrue(listTo.Equals(listTo1));
            //---------------Execute Test ----------------------
            listTo.Inserted = true;
            listTo1.Inserted = false;
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Inserted_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", };
            var listTo1 = new SharepointSearchTo { FieldName = "A",  };
            //---------------Assert Precondition----------------
            Assert.IsTrue(listTo.Equals(listTo1));
            //---------------Execute Test ----------------------
            listTo.Inserted = true;
            listTo1.Inserted = true;
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsSearchCriteriaFocused_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", };
            var listTo1 = new SharepointSearchTo { FieldName = "A",  };
            //---------------Assert Precondition----------------
            Assert.IsTrue(listTo.Equals(listTo1));
            //---------------Execute Test ----------------------
            listTo.IsSearchCriteriaFocused = true;
            listTo1.IsSearchCriteriaFocused = false;
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsSearchCriteriaFocused_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointSearchTo { FieldName = "A", };
            var listTo1 = new SharepointSearchTo { FieldName = "A", };
            //---------------Assert Precondition----------------
            Assert.IsTrue(listTo.Equals(listTo1));
            //---------------Execute Test ----------------------
            listTo.IsSearchCriteriaFocused = true;
            listTo1.IsSearchCriteriaFocused = true;
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        public void SavedSource_Null_Object_Is_NotEqual()
        {
            //---------------Set up test pack-------------------
            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition
            {
                Id = Guid.NewGuid(),
                Server = "A"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(sharePointServiceSourceDefinition.Equals(null), "Equals operator can't compare to null.");
        }

        [TestMethod]
        public void SavedSource_Itself_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition
            {
                Id = Guid.NewGuid(),
                Server = "A"
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharePointServiceSourceDefinition.Equals(sharePointServiceSourceDefinition), "Equals operator can't compare to itself.");
        }

        [TestMethod]
        public void SavedSource_DifferentType_Is_NotEqual()
        {
            //---------------Set up test pack-------------------
            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition
            {
                Id = Guid.NewGuid(),
                Server = "A"
            };
            object differentObject = new SharepointSearchTo
            {
                InternalName = Guid.NewGuid().ToString(),
                FieldName = "A"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(sharePointServiceSourceDefinition.Equals(differentObject), "Equals operator can't compare to differently typed object.");
        }

        [TestMethod]
        public void EqualsOperator_WithEqualObjects_AreEqual()
        {
            var firstEmailServiceSourceDefinition = new SharePointServiceSourceDefinition
            {
                Name = "Test",
                Server = "companyweb",
                AuthenticationType = AuthenticationType.Windows,
                UserName = "Me@dev2.io",
                Password = "me too"
            };

            var secondEmailServiceSourceDefinition = new SharePointServiceSourceDefinition
            {
                Name = "Test",
                Server = "companyweb",
                AuthenticationType = AuthenticationType.Windows,
                UserName = "Me@dev2.io",
                Password = "me too"
            };
            Assert.IsTrue(firstEmailServiceSourceDefinition == secondEmailServiceSourceDefinition, "Equals operator doesnt work.");
        }

        [TestMethod]
        public void NotEqualsOperator_WithNotEqualObjects_AreNotEqual()
        {
            var firstEmailServiceSourceDefinition = new SharePointServiceSourceDefinition
            {
                Name = "Test",
                Server = "companyweb",
                AuthenticationType = AuthenticationType.Windows,
                UserName = "Me@dev2.io",
                Password = "me too"
            };

            var secondEmailServiceSourceDefinition = new SharePointServiceSourceDefinition
            {
                Name = "Different Test",
                Server = "differentcompanyweb",
                AuthenticationType = AuthenticationType.Anonymous
            };
            Assert.IsTrue(firstEmailServiceSourceDefinition != secondEmailServiceSourceDefinition, "Not equals operator doesnt work.");
        }
    }
}