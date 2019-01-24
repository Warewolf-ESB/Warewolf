using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Converters.Graph.DataTable;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Framework.Converters.Graph.Ouput;
using Unlimited.Framework.Converters.Graph.Poco;
using Unlimited.Framework.Converters.Graph.String.Json;
using Unlimited.Framework.Converters.Graph.String.Xml;
using Warewolf.Core;
using Warewolf.Storage;

namespace Dev2.Tests.Activities.ActivityComparerTests.ComDll
{
    [TestClass]
    public class DsfComDllActivityEqualityTests
    {
        [TestMethod]
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
            Assert.IsTrue(equals); ;
        }

        [TestMethod]
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
        [Owner("Nkosinathi Sangweni")]
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
                UniqueID = uniqueId ,
                DisplayName = "AAA",
                Method = pluginActiona
            };
            //---------------Assert DsfComDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals); ;
        }

        [TestMethod]
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
            Assert.IsTrue(equals); ;
        }

        [TestMethod]
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
            Assert.IsFalse(equals); ;
        }

        [TestMethod]
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
            Assert.IsTrue(equals); ;
        }

        [TestMethod]
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
            Assert.IsFalse(equals); ;
        }

        [TestMethod]
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
            Assert.IsFalse(equals); ;
        }

        [TestMethod]
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
            Assert.IsTrue(equals); ;
        }

        [TestMethod]
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
            Assert.IsTrue(equals); ;
        }
        [TestMethod]
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
            Assert.IsTrue(equals); ;
        }

        [TestMethod]
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
            Assert.IsTrue(equals); ;
        }
        [TestMethod]
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
            Assert.IsTrue(equals); ;
        }

        [TestMethod]
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
            Assert.IsTrue(equals); ;
        }

        [TestMethod]
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
            Assert.IsTrue(equals); ;
        }

        [TestMethod]
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
        [Owner("Nkosinathi Sangweni")]
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

        static IEsbChannel mockEsbChannel => new Mock<IEsbChannel>().Object;
        static IDSFDataObject mockDSFDataObject = new Mock<IDSFDataObject>().Object;

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Method_IsNull_Expect_Error()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            //-----------------------Act-------------------------
            dsfComDllActivity.TestExecutionImpl(mockEsbChannel, mockDSFDataObject, "TestInput", "TestOutput", out ErrorResultTO errorResult, 0);
            //-----------------------Assert----------------------
            Assert.AreEqual(1, errorResult.FetchErrors().Count);
            Assert.AreEqual("No Method Selected", errorResult.FetchErrors()[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Method_IsNull_Expect_Error1()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            //-----------------------Act-------------------------
            
            //-----------------------Assert----------------------
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_ExecutionImpl_Inputs_IsNull_Expect_Error()
        {
            //-----------------------Arrange---------------------
            var mockPluginAction = new Mock<IPluginAction>();
            var mockComPluginSource = new Mock<ComPluginSource>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            
            var dsfComDllActivity = new TestDsfComDllActivity()
            {
                ResourceCatalog = mockResourceCatalog.Object,
                Method = mockPluginAction.Object
            };

            mockPluginAction.Setup(o => o.Method).Returns("TestMethod");
            mockResourceCatalog.Setup(o => o.GetResource<ComPluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockComPluginSource.Object);
            //-----------------------Act-------------------------
            dsfComDllActivity.TestExecutionImpl(mockEsbChannel, mockDSFDataObject, "TestInput", "TestOutput", out ErrorResultTO errorResult, 0);
            //-----------------------Assert----------------------
            Assert.AreEqual(1, errorResult.FetchErrors().Count);
            Assert.AreEqual("Object reference not set to an instance of an object.", errorResult.FetchErrors()[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_ExecutionImpl_Inputs_IsNotNull_Expect_Error()
        {
            //-----------------------Arrange---------------------
            var mockPluginAction = new Mock<IPluginAction>();
            var mockComPluginSource = new Mock<ComPluginSource>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockDsfActivity = new Mock<DsfActivity>();
            var mockServiceInput = new Mock <ICollection<IServiceInput>>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            var inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") };

            IOutputDescription description = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML
            };

            var dsfComDllActivity = new TestDsfComDllActivity()
            {
                ResourceCatalog = mockResourceCatalog.Object,
                Method = mockPluginAction.Object,
                Inputs = inputs, 
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping() },
                OutputDescription = description

            };

            mockPluginAction.Setup(o => o.Method).Returns("TestMethod");
            mockResourceCatalog.Setup(o => o.GetResource<ComPluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockComPluginSource.Object);

            var inputList = new List<MethodParameter>();
            var methodParamList = new MethodParameter { EmptyToNull = false, IsRequired = false, Name = null, Value = "TestValue", TypeName = "TestTypeName" };

            inputList.Add(methodParamList);

            var methodParameters = inputList?.Select(a => new MethodParameter { EmptyToNull = a.EmptyToNull, IsRequired = a.IsRequired, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList() ?? new List<MethodParameter>();

            var environment = new ExecutionEnvironment();
            var dataListID = Guid.NewGuid();

            mockDSFDataObject.Setup(o => o.DataListID).Returns(dataListID);
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
            //-----------------------Act-------------------------
            dsfComDllActivity.TestExecutionImpl(mockEsbChannel, mockDSFDataObject.Object, "TestInput", "TestOutput", out ErrorResultTO errorResult, 0);
            //-----------------------Assert----------------------
            Assert.AreEqual(1, errorResult.FetchErrors().Count);
            Assert.AreEqual("Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index", errorResult.FetchErrors()[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_GetHashCode_IsNotNull_Expect_True()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            //-----------------------Act-------------------------
            var hashCode = dsfComDllActivity.GetHashCode();
            //-----------------------Assert----------------------
            Assert.IsNotNull(hashCode);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Equals_IsNotNull_Expect_True()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            //-----------------------Act-------------------------
            var equals = dsfComDllActivity.Equals(dsfComDllActivity);
            //-----------------------Assert----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Equals_IsNull_Expect_False()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            //-----------------------Act-------------------------
            var equals = dsfComDllActivity.Equals(null);
            //-----------------------Assert----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_Equals_NotSame_Expect_False()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            //-----------------------Act-------------------------
            var equals = dsfComDllActivity.Equals(new TestDsfComDllActivity());
            //-----------------------Assert----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_ObjectEquals_IsNotNull_Expect_True()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            var obj = new object();
            obj = new DsfComDllActivity();
            //-----------------------Act-------------------------
            var equals = dsfComDllActivity.Equals(obj);
            //-----------------------Assert----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfComDllActivity))]
        public void DsfComDllActivity_ObjectEquals_IsNull_Expect_False()
        {
            //-----------------------Arrange---------------------
            var dsfComDllActivity = new TestDsfComDllActivity();
            var obj = new object();
            obj = null;
            //-----------------------Act-------------------------
            var equals = dsfComDllActivity.Equals(obj);
            //-----------------------Assert----------------------
            Assert.IsFalse(equals);
        }
        

    }

    class TestDsfComDllActivity : DsfComDllActivity
    {
        public void TestExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            base.ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors, update);
        }
    }
}