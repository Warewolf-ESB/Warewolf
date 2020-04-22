/*
*  Warewolf - Once bitten, there's no going bac
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Counters;
using Dev2.PerformanceCounters.Management;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Hooks
{
    [Binding]
    public sealed class ConnectAndLoadServerResourcesHooks
    {
        private static IServer _environmentModel;
        private const int EXPECTED_NUMBER_OF_RESOURCES = 108;

        private static WarewolfPerformanceCounterManager _performanceCounterLocater;
        private static IPrincipal _principal;

        [BeforeFeature(tags: "ConnectAndLoadServer")]
        private static void Setup()
        {
            ConnectAndLoadServer();
            Assert.IsTrue(_environmentModel.ResourceRepository.All().Count >= EXPECTED_NUMBER_OF_RESOURCES, $"This test expects {EXPECTED_NUMBER_OF_RESOURCES} resources on localhost but there are only {_environmentModel.ResourceRepository.All().Count}.");
            
            _performanceCounterLocater = BuildPerfomanceCounter();
            FeatureContext.Current.Add("performanceCounterLocater", _performanceCounterLocater);
            
            _principal = BuildPrincipal();
            FeatureContext.Current.Add("principal", _principal);
        }

        private static void ConnectAndLoadServer()
        {
            _environmentModel = ServerRepository.Instance.Source;
            _environmentModel.ConnectAsync().Wait(60000);
            if (_environmentModel.IsConnected)
            {
                _environmentModel.ResourceRepository.Load(true);
                FeatureContext.Current.Add("environmentModel", _environmentModel);
            }
            else
            {
                throw new Exception("Failed to connect to localhost Warewolf server.");
            }
        }

        private static WarewolfPerformanceCounterManager BuildPerfomanceCounter()
        {
            var _mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var _performanceCounterFactory = _mockPerformanceCounterFactory.Object;

            var register = new WarewolfPerformanceCounterRegister(new List<IPerformanceCounter>
                                                        {
                                                            new WarewolfCurrentExecutionsPerformanceCounter(_performanceCounterFactory),
                                                            new WarewolfNumberOfErrors(_performanceCounterFactory),
                                                            new WarewolfRequestsPerSecondPerformanceCounter(_performanceCounterFactory),
                                                            new WarewolfAverageExecutionTimePerformanceCounter(_performanceCounterFactory),
                                                            new WarewolfNumberOfAuthErrors(_performanceCounterFactory),
                                                            new WarewolfServicesNotFoundCounter(_performanceCounterFactory),
                                                        }, new List<IResourcePerformanceCounter>());

            return new WarewolfPerformanceCounterManager(register.Counters, new List<IResourcePerformanceCounter>(), register, new Mock<IPerformanceCounterPersistence>().Object, _performanceCounterFactory);
        }

        private static IPrincipal BuildPrincipal()
        {
            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetCurrent());
            return mockPrincipal.Object;
        }

        [AfterFeature(tags: "ConnectAndLoadServer")]
        private static void Cleanup()
        {
            FeatureContext.Current.Keys.ToList()
                .ForEach(key => FeatureContext.Current.Remove(key));
        }
    }
}
