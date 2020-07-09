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

namespace Dev2.Tests.Activities.ActivityComparerTests.DotNet_Dll
{
    [TestClass]
    public class DsfDotNetDllActivityEqualityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfDotNetDllActivity() { UniqueID = uniqueId };
            var activity1 = new DsfDotNetDllActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfDotNetDllActivity() { UniqueID = uniqueId };
            var activity1 = new DsfDotNetDllActivity() { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfDotNetDllActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity1 = new DsfDotNetDllActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfDotNetDllActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity1 = new DsfDotNetDllActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfDotNetDllActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var activity1 = new DsfDotNetDllActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Method_DifferentName_Object_Is_Not_Equal_CaseSensitive()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "aaa",
                Method = pluginActiona
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Method_SameName_Object_Is_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals); ;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Method_DifferentIsObject_Object_Is_Not_Equal_CaseSensitive()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "aaa",
                Method = pluginActiona
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Method_SameIsObject_Object_Is_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Method_DifferentOutputVariable_Object_Is_Not_Equal_CaseSensitive()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Method_SameOutputVariable_Object_Is_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Method_DifferentInputs_Object_Is_Not_Equal_CaseSensitive()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "aaa",
                Method = pluginActiona
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Method_SameInputs_Object_Is_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals); ;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void OutputDescription_SameFormat_Object_Is_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals); ;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void OutputDescription_DifferentFormat_Object_Is_Not_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals); ;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void OutputDescription_SameDataSourceShapes_Object_Is_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals); ;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void OutputDescription_DifferentDataSourceShapes_Object_Is_Not_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals); ;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void OutputDescription_SameJsonPathsDifferentNames_Object_Is_Not_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals); ;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void OutputDescription_SameDataTableDifferentNames_Object_Is_Not_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals); ;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void OutputDescription_SameStringPathDifferentNames_Object_Is_Not_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals); ;
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void OutputDescription_SamePocoPathDifferentNames_Object_Is_Not_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals); ;
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void OutputDescription_SameXmlPathDifferentNames_Object_Is_Not_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                OutputDescription = description1
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                OutputDescription = description
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals); ;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_SameAssemblyLocation_Object_Is_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals); ;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_SameAssemblyName_Object_Is_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals); ;
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_SameFullName_Object_Is_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals); ;
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_SameMethodName_Object_Is_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals); ;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_SameJsonObject_Object_Is_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals); ;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_DifferentAssemblyName_Object_Is_Not_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_DifferentFullName_Object_Is_Not_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_DifferentMethodName_Object_Is_not_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_DifferentJsonObject_Object_Is_Not_Equal()
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
            var activity = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                Method = pluginAction,
                Namespace = namespaceItem
            };
            var activity1 = new DsfDotNetDllActivity()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                Method = pluginActiona,
                Namespace = namespaceItem1
            };
            //---------------Assert DsfDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

    }
}