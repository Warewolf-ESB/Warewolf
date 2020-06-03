/*
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
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Converters.Graph.DataTable;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Ouput;
using Unlimited.Framework.Converters.Graph.Poco;
using Unlimited.Framework.Converters.Graph.String.Json;
using Unlimited.Framework.Converters.Graph.String.Xml;
using Warewolf.Core;

namespace Dev2.Tests.Activities.ActivityComparerTests.ComDll
{
    [TestClass]
    public class DsfComDllActivityEqualityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_UniqueID_Equals_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfComDllActivity() { UniqueID = uniqueId };
            var activity1 = new DsfComDllActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfComDllActivity() { UniqueID = uniqueId };
            var activity1 = new DsfComDllActivity() { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_DisplayName_Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfComDllActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity1 = new DsfComDllActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Equals_DisplayName_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfComDllActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity1 = new DsfComDllActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Equals_DisplayName_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfComDllActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var activity1 = new DsfComDllActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Method_DisplayName_IsNot_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a"
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "b"
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "aaa",
                Method = pluginActiona
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Method_DisplayName_Same_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a"
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a"
            };

            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Method_DifferentIsObject_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = false
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "aaa",
                Method = pluginActiona
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Method_SameIsObject_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Method_DifferentOutputVariable_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]"
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[b]]"
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Method_SameOutputVariable_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]"
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]"
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Method_DifferentInputs_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                   new ServiceInput()
                }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(), new ServiceInput()
                }
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "aaa",
                Method = pluginActiona
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Method_SameInputs_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
                }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_OutputDescription_SameFormat_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
                }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            IOutputDescription description = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML
            };

            IOutputDescription description1 = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_OutputDescription_DifferentFormat_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
               {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            IOutputDescription description = new OutputDescription()
            {
                Format = OutputFormats.Unknown
            };

            IOutputDescription description1 = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_OutputDescription_SameDataSourceShapes_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
                }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            IOutputDescription description = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new JsonPath("a","a")
                        }
                    }
                }
            };

            IOutputDescription description1 = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new JsonPath("a","a")
                        }
                    }
                }
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_OutputDescription_DifferentDataSourceShapes_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
               {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            IOutputDescription description = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new JsonPath("a","a")
                        }
                    }
                }
            };

            IOutputDescription description1 = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new DataTablePath("a","a")
                        }
                    }
                }
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_OutputDescription_SameJsonPathsDifferentNames_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
                }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            IOutputDescription description = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new JsonPath("B","B")
                        }
                    }
                }
            };

            IOutputDescription description1 = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new JsonPath("a","a")
                        }
                    }
                }
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_OutputDescription_SameDataTableDifferentNames_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
               {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            IOutputDescription description = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new DataTablePath("B","B")
                        }
                    }
                }
            };

            IOutputDescription description1 = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new DataTablePath("a","a")
                        }
                    }
                }
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_OutputDescription_SameStringPathDifferentNames_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
                }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            IOutputDescription description = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new StringPath()
                            {
                                ActualPath = "a",
                                DisplayPath = "a",
                                OutputExpression = "a",
                                SampleData = "Data"

                            }
                        }
                    }
                }
            };

            IOutputDescription description1 = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new StringPath()
                            {
                                ActualPath = "a",
                                DisplayPath = "a",
                                OutputExpression = "a",
                                SampleData = "Data"

                            },
                        }
                    }
                }
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_OutputDescription_SamePocoPathDifferentNames_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
               {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            IOutputDescription description = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new PocoPath("a","a","a","a")
                        }
                    }
                }
            };

            IOutputDescription description1 = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new PocoPath("a","a","a","a")
                        }
                    }
                }
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_OutputDescription_SameXmlPathDifferentNames_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
               {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            IOutputDescription description = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new XmlPath("a","a","a","a")
                        }
                    }
                }
            };

            IOutputDescription description1 = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML,
                DataSourceShapes = new List<IDataSourceShape>()
                {
                    new DataSourceShape()
                    {
                        Paths = new List<IPath>()
                        {
                            new XmlPath("a","a","a","a")
                        }
                    }
                }
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_NameSpace_SameAssemblyLocation_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
               {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem()
            {
                AssemblyLocation = "a"
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_NameSpace_SameAssemblyName_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
                }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a"
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_NameSpace_SameFullName_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
               {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_NameSpace_SameMethodName_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
               {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod"
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_NameSpace_SameJsonObject_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
               {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod",
                JsonObject = "{}"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod",
                JsonObject = "{}"
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_NameSpace_DifferentAssemblyName_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
               {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "b"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a"
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_NameSpace_DifferentFullName_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
               {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "b"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_NameSpace_DifferentMethodName_Object_Is_not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
                }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod"
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_NameSpace_DifferentJsonObject_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                   new ServiceInput(){Name = "a"},
                   new ServiceInput(){Name = "b"},
                }
            };
            IPluginAction pluginActiona = new PluginAction()
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput(){Name = "a"},
                    new ServiceInput(){Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod",
                JsonObject = "{\"name\":\"name\"}"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem()
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod",
                JsonObject = "{}"
            };

            var uniqueId = Guid.NewGuid().ToString();

            var activity = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfComDllActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

    }
}